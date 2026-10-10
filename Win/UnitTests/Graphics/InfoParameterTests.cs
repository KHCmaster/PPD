using PPDEditor;
using PPDEditor.Command.PPDSheet;
using PPDEditor.Models;
using PPDEditor.ViewModels.Docks;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using EditorUtility = PPDEditor.Utility;

namespace UnitTests.Graphics
{
    /// <summary>
    /// 情報のドックで、選んだマークのパラメーターを編集する（編集、コピーと貼り付け、プリセット）テストです。マークは本物を使う。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public sealed class InfoParameterTests : IDisposable
    {
        sealed class Editor(PPDSheet sheet) : IInfoEditor
        {
            public PPDSheet SelectedSheet { get; } = sheet;
            public List<string> Messages { get; } = [];
            public Dictionary<string, string> Edited { get; set; }
            public string PresetPath { get; set; }

            // エディターと同じく、選んだマーク、なければ範囲の中のマーク
            public Mark[] GetSelectedMarks() => SelectedSheet.SelectedMark is Mark mark ? [mark] : SelectedSheet.GetAreaData();

            public void ShowMessage(string text) => Messages.Add(text);

            public Dictionary<string, string> EditParameters(Dictionary<string, string> parameters, string[] existKeys) => Edited;

            public string AskPresetFilePath(string initialDirectory, string fileName) => PresetPath;
        }

        readonly EditorMarkEnvironment environment;
        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly PPDSheet sheet;
        readonly Editor editor;
        readonly Mark first;
        readonly Mark second;
        uint lastID = 100;

        public InfoParameterTests(GraphicsDevice graphics)
        {
            environment = new EditorMarkEnvironment(graphics);
            AppFiles.CopyTo("PPDEditor", folder);
            EditorUtility.ChangeLanguage("jp");
            Directory.CreateDirectory(ParameterPresetStore.PresetDir);
            ParameterPresetStore.Save(Path.Combine(ParameterPresetStore.PresetDir, "Preset.xml"), [new("p", "1")]);
            sheet = new PPDSheet { GetVisibleRows = () => [.. Enumerable.Range(0, 10)], NextID = () => ++lastID };
            first = sheet.AddMark(1, 100, 100, 0, (int)ButtonType.Circle, 1, [new("a", "1"), new("b", "2")], false);
            second = sheet.AddMark(2, 100, 100, 0, (int)ButtonType.Circle, 2, [new("a", "1")], false);
            sheet.ClearHistory();
            editor = new Editor(sheet);
        }

        public void Dispose()
        {
            folder.Dispose();
            environment.Dispose();
        }

        void SelectBoth()
        {
            sheet.FocusedMark = [-1, -1];
            sheet.RecStart = new CustomPoint(0, (int)ButtonType.Circle);
            sheet.RecEnd = new CustomPoint(3, (int)ButtonType.Circle);
        }

        void Select(Mark mark)
        {
            sheet.SelectMark(mark.Time, (int)mark.Type, 0);
        }

        [Fact]
        public void EditParameters_ChangesCommonParameters()
        {
            var viewModel = new InfoViewModel(editor);
            SelectBoth();
            viewModel.UpdateParameters(editor.GetSelectedMarks());
            // 共通のパラメーターだけを見せる
            Assert.Equal([new("a", "1")], viewModel.Parameters);

            editor.Edited = new() { ["c"] = "3" };
            viewModel.EditParametersCommand.Execute(null);

            // 共通のもので消したものは消し、新しいものを付ける（共通でないものは残す）
            Assert.Equal((null, "3", "2"), (first["a"], first["c"], first["b"]));
            Assert.Equal((null, "3"), (second["a"], second["c"]));
            Assert.Equal([new("c", "3")], viewModel.Parameters);
            // まとめて 1 回で戻る
            sheet.Undo();
            Assert.Equal(("1", null), (first["a"], first["c"]));

            // 編集をやめたら何もしない
            editor.Edited = null;
            viewModel.EditParametersCommand.Execute(null);
            Assert.Equal("1", first["a"]);
            sheet.RecStart = new CustomPoint(-1, -1);
            viewModel.EditParametersCommand.Execute(null);
            Assert.Equal([EditorUtility.Language["NoSelectedMarks"]], editor.Messages);
        }

        [Fact]
        public void CopyAndPaste()
        {
            var viewModel = new InfoViewModel(editor);
            sheet.RecStart = new CustomPoint(-1, -1);
            viewModel.RefreshEditMenu();
            Assert.False(viewModel.CanCopy || viewModel.CanPaste);

            Select(first);
            viewModel.RefreshEditMenu();
            Assert.True(viewModel.CanCopy);
            Assert.False(viewModel.CanPaste);
            viewModel.CopyCommand.Execute(null);
            Select(second);
            viewModel.RefreshEditMenu();
            Assert.True(viewModel.CanPaste);
            viewModel.PasteCommand.Execute(null);

            Assert.Equal("2", second["b"]);
            Assert.Equal([new("a", "1"), new("b", "2")], viewModel.Parameters.OrderBy(p => p.Key));
        }

        [Fact]
        public void Presets_ApplyToggleAndSave()
        {
            var viewModel = new InfoViewModel(editor);
            var preset = Assert.Single(viewModel.PresetNodes);
            SelectBoth();

            // 当てはめると印が付き、もう一度で外す
            viewModel.TogglePresetCommand.Execute(preset);
            Assert.Equal(("1", "1"), (first["p"], second["p"]));
            viewModel.RefreshPresetMenu();
            Assert.True(preset.IsChecked);
            viewModel.TogglePresetCommand.Execute(preset);
            Assert.Equal((null, null), (first["p"], second["p"]));
            viewModel.TogglePresetCommand.Execute(null);

            // 選んでいるマークのパラメーターをプリセットにする
            viewModel.UpdateParameters(editor.GetSelectedMarks());
            editor.PresetPath = Path.GetFullPath(Path.Combine(ParameterPresetStore.PresetDir, "Saved.xml"));
            viewModel.SavePresetCommand.Execute(null);
            Assert.Equal(["Preset", "Saved"], viewModel.PresetNodes.Select(n => n.Name).Order());

            // 何も選んでいないとき
            sheet.RecStart = new CustomPoint(-1, -1);
            viewModel.ApplyParameters([new("x", "1")]);
            viewModel.UnapplyParameters([new("x", "1")]);
            viewModel.UpdateParameters([]);
            viewModel.SavePresetCommand.Execute(null);
            Assert.Equal(2, editor.Messages.Count(m => m == EditorUtility.Language["NoSelectedMarks"]));
            Assert.Contains(EditorUtility.Language["NoParameters"], editor.Messages);
        }
    }
}
