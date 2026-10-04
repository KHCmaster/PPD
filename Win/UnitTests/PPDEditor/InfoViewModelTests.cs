using PPDEditor;
using PPDEditor.Command.PPDSheet;
using PPDEditor.Models;
using PPDEditor.ViewModels;
using PPDEditor.ViewModels.Docks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// 情報とパラメーターのドックのテストです。マークは描画の装置が要るので、マークを選んでいないときの動きだけを確かめる。
    /// </summary>
    public class InfoViewModelTests : IDisposable
    {
        sealed class FakeInfoEditor : IInfoEditor
        {
            public PPDSheet SelectedSheet { get; set; } = new();
            public List<string> Messages { get; } = [];
            public string PresetPath { get; set; }
            public int Edited { get; private set; }

            public Mark[] GetSelectedMarks() => [];

            public void ShowMessage(string text) => Messages.Add(text);

            public Dictionary<string, string> EditParameters(Dictionary<string, string> parameters, string[] existKeys)
            {
                Edited++;
                return null;
            }

            public string AskPresetFilePath(string initialDirectory, string fileName) => PresetPath;
        }

        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakeInfoEditor editor = new();

        public InfoViewModelTests()
        {
            AppFiles.CopyTo("PPDEditor", folder);
            Utility.ChangeLanguage("jp");
            Directory.CreateDirectory(ParameterPresetStore.PresetDir);
            ParameterPresetStore.Save(Path.Combine(ParameterPresetStore.PresetDir, "Preset.xml"), [new("a", "1")]);
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        [Fact]
        public void InfoRows_ShowMovieAndMark()
        {
            var viewModel = new InfoViewModel(editor);
            var changed = new List<string>();
            viewModel.PropertyChanged += (sender, e) => changed.Add(e.PropertyName);

            viewModel.SetMovieInfo("1280", "720", "90");
            viewModel.SetMarkInfo("100,200", "1.5", "7");

            Assert.Equal(["1280", "720", "90", "100,200", "1.5", "7"], viewModel.InfoRows.Select(r => r.Value));
            Assert.Equal(Utility.Language["Width"], viewModel.InfoRows[0].Key);
            Assert.Equal("ID", viewModel.InfoRows[5].Key);
            Assert.Equal(2, changed.Count(n => n == nameof(InfoViewModel.InfoRows)));
        }

        [Fact]
        public void Presets_LoadAndReportBroken()
        {
            File.WriteAllText(Path.Combine(ParameterPresetStore.PresetDir, "broken.xml"), "not xml");

            var viewModel = new InfoViewModel(editor);

            Assert.Equal([Utility.Language["ErrorInLoadingParameterPreset"]], editor.Messages);
            Assert.Equal("Preset", Assert.Single(viewModel.Presets).PresetName);
            Assert.Equal(["Preset"], viewModel.PresetNodes.Select(n => n.Name));
            Assert.NotEqual(default, viewModel.PresetsLoadTime);
            Assert.Same(viewModel.PresetRoot.Presets[0], viewModel.Presets[0]);
        }

        [Fact]
        public void WithoutSelectedMarks_ReportsOrDoesNothing()
        {
            var viewModel = new InfoViewModel(editor);

            viewModel.RefreshEditMenu();
            viewModel.RefreshPresetMenu();
            Assert.False(viewModel.CanCopy || viewModel.CanPaste);

            viewModel.EditParametersCommand.Execute(null);
            viewModel.ApplyParameters([new("a", "1")]);
            viewModel.UnapplyParameters([new("a", "1")]);
            viewModel.TogglePresetCommand.Execute(viewModel.PresetNodes[0]);
            viewModel.TogglePresetCommand.Execute(null);
            viewModel.CopyCommand.Execute(null);
            viewModel.PasteCommand.Execute(null);

            Assert.Equal(4, editor.Messages.Count);
            Assert.All(editor.Messages, m => Assert.Equal(Utility.Language["NoSelectedMarks"], m));
            Assert.Equal(0, editor.Edited);

            // 譜面がないときは何もしない
            editor.SelectedSheet = null;
            viewModel.EditParametersCommand.Execute(null);
            Assert.Equal(4, editor.Messages.Count);
            viewModel.UpdateParameters([]);
            Assert.Empty(viewModel.Parameters);
        }

        [Fact]
        public void SavePreset_NeedsParameters()
        {
            var viewModel = new InfoViewModel(editor);

            viewModel.SavePresetCommand.Execute(null);
            Assert.Equal([Utility.Language["NoParameters"]], editor.Messages);

            viewModel.Parameters = [new("speed", "2")];
            viewModel.SavePresetCommand.Execute(null);
            Assert.Single(viewModel.Presets);

            editor.PresetPath = folder.Combine(ParameterPresetStore.PresetDir, "Saved.xml");
            viewModel.SavePresetCommand.Execute(null);
            Assert.Equal(["Preset", "Saved"], viewModel.PresetNodes.Select(n => n.Name));
            Assert.Equal([new KeyValuePair<string, string>("speed", "2")], viewModel.Presets.Single(p => p.PresetName == "Saved").Parameters);
        }
    }
}
