using PPDEditor;
using PPDEditor.ViewModels;
using PPDEditor.ViewModels.Dialogs;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Color = System.Drawing.Color;
using Utility = PPDEditor.Utility;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// ダイアログの ViewModel から使う、偽物のウィンドウです（メッセージは覚えるだけ）。
    /// </summary>
    sealed class FakeDialogView : IDialogView
    {
        public List<string> Messages { get; } = [];
        public string Folder { get; set; }
        public string File { get; set; }

        public void ShowMessage(string text) => Messages.Add(text);

        public string SelectFolder(string initialDirectory) => Folder;

        public string OpenFile(string filter) => File;

        public string SaveFile(string filter) => File;
    }

    /// <summary>
    /// PPDEditor のダイアログの ViewModel のテストです。言語ファイルを一時フォルダーにコピーして、日本語で動かす。
    /// </summary>
    public class PPDEditorDialogTests : IDisposable
    {
        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakeDialogView view = new();

        public PPDEditorDialogTests()
        {
            AppFiles.CopyTo("PPDEditor", folder);
            Utility.ChangeLanguage("jp");
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        private static bool Ok(DialogViewModel viewModel)
        {
            var closed = false;
            viewModel.CloseRequested += ok => closed = ok;
            viewModel.OkCommand.Execute(null);
            return closed;
        }

        [Fact]
        public void Suggest_NeedsSelection()
        {
            var viewModel = new SuggestViewModel([new("a", "1"), new("b", "2")]);
            Assert.False(viewModel.OkCommand.CanExecute(null));

            viewModel.SelectedSuggest = viewModel.Suggests[1];

            Assert.True(viewModel.OkCommand.CanExecute(null));
            Assert.True(Ok(viewModel));
        }

        [Fact]
        public void SwapLine_ListsTenLines()
        {
            var viewModel = new SwapLineViewModel { First = 1, Second = 3 };

            Assert.Equal(10, viewModel.Lines.Length);
            Assert.Equal(Utility.Language["SwapLineBox1"], viewModel.Lines[0]);
            Assert.Same(LanguageTexts.Instance, viewModel.Texts);
        }

        [Fact]
        public void CopyDifficulty_ExcludesCurrent()
        {
            var viewModel = new CopyDifficultyViewModel(AvailableDifficulty.Base);

            Assert.Equal([AvailableDifficulty.Easy, AvailableDifficulty.Normal, AvailableDifficulty.Hard, AvailableDifficulty.Extreme], viewModel.Difficulties);
            Assert.Equal(AvailableDifficulty.Easy, viewModel.DestDifficulty);
            Assert.Equal(AvailableDifficulty.Base, viewModel.CurrentDifficulty);
        }

        [Fact]
        public void SavePosAndAngle_ValidatesName()
        {
            var viewModel = new SavePosAndAngleViewModel("位置") { View = view };
            var rejected = 0;
            viewModel.FileNameRejected += (sender, e) => rejected++;

            Assert.False(Ok(viewModel));
            Assert.Equal([Utility.Language["PAALSNameError1"]], view.Messages);

            folder.Write(Path.Combine("posdat", "used.txt"), "");
            viewModel.FileName = "used";
            Assert.False(Ok(viewModel));
            Assert.Equal(1, rejected);
            Assert.Equal(Utility.Language["PAALSNameError2"], view.Messages[1]);

            viewModel.FileName = "new";
            Assert.True(Ok(viewModel));
            Assert.Equal("位置", viewModel.Caption);
        }

        [Fact]
        public void MovieCut_ParsesAndReportsBadNumbers()
        {
            var viewModel = new MovieCutViewModel(new MovieTrimmingData(1, 2, 3, 4)) { View = view };
            Assert.Equal(("2", "3", "1", "4"), (viewModel.Left, viewModel.Right, viewModel.Top, viewModel.Bottom));

            viewModel.Right = "x";
            viewModel.Top = "10.5";

            Assert.True(Ok(viewModel));
            Assert.Equal((10.5f, 2f, 0f, 4f), (viewModel.TrimmingData.Top, viewModel.TrimmingData.Left, viewModel.TrimmingData.Right, viewModel.TrimmingData.Bottom));
            Assert.Single(view.Messages);
        }

        [Fact]
        public void MovieCut_WithoutData_StartsAtZero()
        {
            var viewModel = new MovieCutViewModel(null);

            Assert.Equal(("0", "0", "0", "0"), (viewModel.Left, viewModel.Right, viewModel.Top, viewModel.Bottom));
        }

        [Fact]
        public void ModPublish_ValidatesFileNameAndFolder()
        {
            var viewModel = new ModPublishViewModel { View = view };
            Assert.Equal(folder.Combine("publish_mod"), viewModel.PublishFolder);
            Assert.True(Directory.Exists(folder.Combine("publish_mod")));

            Assert.False(Ok(viewModel));
            viewModel.ModFileName = "bad|name";
            Assert.False(Ok(viewModel));
            Assert.Equal(2, view.Messages.Count);

            view.Folder = null;
            viewModel.BrowseFolderCommand.Execute(null);
            Assert.Equal(folder.Combine("publish_mod"), viewModel.PublishFolder);
            view.Folder = folder.Combine("other");
            viewModel.BrowseFolderCommand.Execute(null);
            viewModel.ModFileName = "good";
            Assert.True(Ok(viewModel));
            Assert.True(Directory.Exists(folder.Combine("other")));
        }

        [Fact]
        public void SoundChangeEdit_SlotsAndPreview()
        {
            var viewModel = new SoundChangeEditViewModel([0, 2, 1, 0, 0, 0, 0, 0, 0, 0], ["", @"C:\s\a.wav", @"C:\s\b.ogg"]);
            var played = new List<string>();
            viewModel.PlaySound += played.Add;

            Assert.Equal(["", "a", "b"], viewModel.SoundNames);
            viewModel.Slots[0].SelectedIndex = 1;
            Assert.Equal([1, 2, 1, 0, 0, 0, 0, 0, 0, 0], viewModel.Change);

            viewModel.PlayCommand.Execute(null);
            viewModel.PreviewIndex = 2;
            viewModel.PlayCommand.Execute(null);
            Assert.Equal([@"C:\s\b.ogg"], played);
        }

        [Fact]
        public void CommandSelect_ReadsScriptsInFolders()
        {
            folder.Write(Path.Combine("Commands", "Tools", "Align.fsml"), "");
            folder.Write(Path.Combine("Commands", "Top.FSML"), "");
            folder.Write(Path.Combine("Commands", "readme.txt"), "");

            var viewModel = new CommandSelectViewModel();

            Assert.Equal(["Tools", "Top"], viewModel.Nodes.Select(n => n.Name));
            var script = Assert.Single(viewModel.Nodes[0].Children);
            Assert.Equal(@"Tools\Align", script.FullPath);
            viewModel.SelectedNode = viewModel.Nodes[0];
            Assert.False(viewModel.OkCommand.CanExecute(null));
            viewModel.SelectedNode = script;
            Assert.True(viewModel.OkCommand.CanExecute(null));
            Assert.Equal(Path.Combine("Commands", "Tools", "Align"), viewModel.SelectedScriptPath);
        }

        [Fact]
        public void CommandSelect_WithoutFolder_CreatesCommandsFolder()
        {
            var viewModel = new CommandSelectViewModel();

            Assert.Empty(viewModel.Nodes);
            // 以前は「Command」という違う名前のフォルダーを作っていた
            Assert.True(Directory.Exists(folder.Combine("Commands")));
            Assert.False(Directory.Exists(folder.Combine("Command")));
        }

        [Fact]
        public void EditTimeLineRow_MovesAndApplies()
        {
            var manager = new TimeLineRowManager();
            var viewModel = new EditTimeLineRowViewModel(manager);
            Assert.Equal(Enumerable.Range(0, 10), viewModel.Rows.Select(r => r.RowIndex));
            Assert.False(viewModel.MoveUpCommand.CanExecute(null));
            Assert.Equal(Utility.Language["Square"], viewModel.Rows[0].Name);

            viewModel.SelectedRow = viewModel.Rows[0];
            Assert.False(viewModel.MoveUpCommand.CanExecute(null));
            viewModel.MoveDownCommand.Execute(null);
            Assert.True(viewModel.MoveUpCommand.CanExecute(null));
            viewModel.SelectedRow = viewModel.Rows[9];
            Assert.False(viewModel.MoveDownCommand.CanExecute(null));
            viewModel.MoveUpCommand.Execute(null);
            viewModel.Rows.First(r => r.RowIndex == 3).IsVisible = false;

            viewModel.Apply(manager);

            Assert.Equal([1, 0, 2, 3, 4, 5, 6, 7, 9, 8], manager.RowOrders);
            Assert.False(manager.RowVisibilities[3]);
            Assert.True(manager.RowVisibilities[4]);
        }

        [Fact]
        public void EditParameter_AddRemoveAndDuplicateKeys()
        {
            var viewModel = new EditParameterViewModel(new Dictionary<string, string> { ["Key1"] = "a" }, ["Key2"]) { View = view };

            viewModel.AddCommand.Execute(null);
            Assert.Equal("Key3", viewModel.Items[1].Key);
            viewModel.Items[1].Key = "Key1";
            Assert.False(Ok(viewModel));
            Assert.Equal([Utility.Language["DuplicateKeyError"]], view.Messages);

            viewModel.RemoveCommand.Execute(new List<object> { viewModel.Items[1] });
            viewModel.Items.Add(new ParameterItem());
            Assert.True(Ok(viewModel));
            Assert.Equal(new Dictionary<string, string> { ["Key1"] = "a", [""] = "" }, viewModel.Parameters);
        }

        [Fact]
        public void GridSetting_ParsesAndPicksColor()
        {
            var viewModel = new GridSettingViewModel(new SquareGrid { Width = 10, Height = 20, OffsetX = 3, OffsetY = 4, GridColor = Color.FromArgb(255, 255, 255) });
            Assert.Equal(("10", "20", "3", "4"), (viewModel.Width, viewModel.Height, viewModel.OffsetX, viewModel.OffsetY));
            Assert.Equal("#FFFFFF", viewModel.ColorText);
            Assert.False(viewModel.IsDarkColor);

            viewModel.Width = "900";
            viewModel.Height = "abc";
            viewModel.OffsetX = "-7";
            viewModel.ChooseColorCommand.Execute(null);
            viewModel.PickColor = _ => null;
            viewModel.ChooseColorCommand.Execute(null);
            viewModel.PickColor = _ => Color.FromArgb(0x10, 0x20, 0x30);
            viewModel.ChooseColorCommand.Execute(null);

            var grid = viewModel.Grid;
            Assert.Equal((5, 5, -7, 4), (grid.Width, grid.Height, grid.OffsetX, grid.OffsetY));
            Assert.Equal("#102030", viewModel.ColorText);
            Assert.True(viewModel.IsDarkColor);
        }
    }
}
