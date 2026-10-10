using PPDEditor;
using PPDEditor.ViewModels;
using PPDEditor.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Xunit;
using Color = System.Drawing.Color;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// PPDEditor のメインのウィンドウの ViewModel のテストです。
    /// </summary>
    /// <remarks>
    /// プロジェクトの読み書きやゲームの画面はドックのウィンドウ（WindowUtility）を直接使うので、UI のテストで確かめる。
    /// ここではドックを作る前（WindowUtility が空）でも動く、ツールバーやメニュー、状態の表示を確かめる。
    /// </remarks>
    public class PPDEditorMainWindowViewModelTests : IDisposable
    {
        sealed class FakeMainView : IMainWindowView
        {
            public List<string> Messages { get; } = [];
            public Func<object, bool> Dialog { get; set; } = _ => false;
            public List<DockKind> Toggled { get; } = [];
            public HashSet<DockKind> Shown { get; } = [];
            public List<bool> FixedValues { get; } = [];
            public List<string> Layouts { get; } = [];

            public void ShowMessage(string text) => Messages.Add(text);

            public bool Confirm(string text, string caption, bool warning = false) => true;

            public bool? AskSave(string text, string caption, bool okCancel) => null;

            public string OpenFile(string filter, string fileName) => null;

            public string SaveFile(string filter, string fileName, string initialDirectory = null) => null;

            public bool ShowDialog(object viewModel) => Dialog(viewModel);

            public void ShowInExplorer(string path)
            {
            }

            public void ToggleDock(DockKind kind) => Toggled.Add(kind);

            public bool IsDockShown(DockKind kind) => Shown.Contains(kind);

            public void SetDocksFixed(bool value) => FixedValues.Add(value);

            public void LoadLayout(string path) => Layouts.Add(path);

            public void ShowSaveDone()
            {
            }

            public (Rect Bounds, bool Maximized) Placement => (new Rect(0, 0, 800, 600), false);

            public void Close()
            {
            }
        }

        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakeMainView view = new();
        readonly MainWindowViewModel viewModel;
        readonly (string AuthorName, bool FixDockPanel) savedSettings = (PPDStaticSetting.AuthorName, PPDStaticSetting.FixDockPanel);

        public PPDEditorMainWindowViewModelTests()
        {
            AppFiles.CopyTo("PPDEditor", folder);
            Utility.ChangeLanguage("jp");
            Assert.Null(WindowUtility.LayerManager);
            viewModel = new MainWindowViewModel(view);
        }

        public void Dispose()
        {
            (PPDStaticSetting.AuthorName, PPDStaticSetting.FixDockPanel) = savedSettings;
            folder.Dispose();
        }

        [Fact]
        public void Startup_ListsMenus()
        {
            Assert.Equal(8, viewModel.DisplayModes.Length);
            Assert.Equal(Utility.Language["Displaylevel1"], viewModel.DisplayModes[0].Text);
            Assert.Equal(4, viewModel.BeatTypes.Length);
            Assert.Equal(16, viewModel.Docks.Length);
            Assert.Equal(DockKind.GameWindow, viewModel.Docks[0].Kind);
            Assert.Equal("PPDEditor", viewModel.WindowTitle);
            Assert.False(viewModel.IsProjectLoaded);
            Assert.Equal(0, viewModel.AvailableDifficultyCount);
        }

        [Fact]
        public void Docks_ToggleAndShowState()
        {
            view.Shown.Add(DockKind.Memo);

            viewModel.ToggleDockCommand.Execute(viewModel.Docks[1]);
            viewModel.RefreshDockStates();
            viewModel.LoadViewLayoutCommand.Execute(new FileMenuItem("800x450", @"view\view800x450.xml"));

            Assert.Equal([DockKind.TimeLine], view.Toggled);
            Assert.True(viewModel.Docks.Single(d => d.Kind == DockKind.Memo).IsShown);
            Assert.False(viewModel.Docks[0].IsShown);
            Assert.Equal([@"view\view800x450.xml"], view.Layouts);

            viewModel.FixDockPanel = true;
            Assert.Equal([true], view.FixedValues);
            Assert.True(PPDStaticSetting.FixDockPanel);
        }

        [Fact]
        public void Toolbar_ParsesValuesWithDefaults()
        {
            Assert.Equal((100f, 0f, 240), (viewModel.BPM, viewModel.BPMOffset, viewModel.DisplayWidth));

            viewModel.BPM = 180;
            viewModel.BPM = 600;
            viewModel.BPM = 0;
            viewModel.BPMOffset = 0.5f;
            viewModel.DisplayWidth = 300;
            Assert.Equal(("180", "0.5", "300"), (viewModel.BpmText, viewModel.BpmOffsetText, viewModel.DisplayWidthText));

            viewModel.BpmText = "x";
            viewModel.BpmOffsetText = "x";
            viewModel.DisplayWidthText = "x";
            Assert.Equal((100f, 0f, 240), (viewModel.BPM, viewModel.BPMOffset, viewModel.DisplayWidth));

            viewModel.DisplayMode = DisplayLineMode.Sixteenth;
            viewModel.DisplayMode = (DisplayLineMode)99;
            Assert.Equal(DisplayLineMode.Sixteenth, viewModel.DisplayMode);
            viewModel.BeatType = DisplayBeatType.Third;
            viewModel.BeatType = (DisplayBeatType)(-1);
            Assert.Equal(DisplayBeatType.Third, viewModel.BeatType);

            viewModel.InitializeToolbar();
            Assert.Equal((0, 2, 0.5f, false), (viewModel.DisplayModeIndex, viewModel.SpeedScale, viewModel.Farness, viewModel.IsAreaSelectionEnabled));
        }

        [Theory]
        [InlineData(0, 0.5f)]
        [InlineData(1, 0.75f)]
        [InlineData(2, 1f)]
        [InlineData(3, 1.25f)]
        [InlineData(4, 1.5f)]
        [InlineData(5, 1.75f)]
        [InlineData(6, 2f)]
        [InlineData(-1, 1f)]
        public void SpeedScale_ToFloat(int index, float expected)
        {
            viewModel.SpeedScale = index;

            Assert.Equal(expected, viewModel.SpeedScaleAsFloat);
        }

        [Theory]
        [InlineData(GridSizeChoice.Px10, 10)]
        [InlineData(GridSizeChoice.Px20, 20)]
        [InlineData(GridSizeChoice.Px50, 50)]
        public void SetGridSize_ShowsGrid(GridSizeChoice choice, int size)
        {
            viewModel.SetGridSizeCommand.Execute(choice);

            Assert.Equal((size, size), (viewModel.Grid.Width, viewModel.Grid.Height));
            Assert.Equal(choice, viewModel.GridSize);
            Assert.True(viewModel.DisplayGrid);
        }

        [Fact]
        public void EditGridAndFarness_ApplyOnlyOnOk()
        {
            viewModel.EditGridCommand.Execute(null);
            viewModel.EditFarnessCommand.Execute(null);
            Assert.False(viewModel.DisplayGrid);

            view.Dialog = dialog =>
            {
                switch (dialog)
                {
                    case GridSettingViewModel grid:
                        grid.Width = "30";
                        grid.OffsetX = "4";
                        grid.PickColor = _ => Color.Blue;
                        grid.ChooseColorCommand.Execute(null);
                        break;
                    case FarnessSettingViewModel farness:
                        Assert.Equal(50, farness.Percent);
                        farness.Percent = 80;
                        break;
                }
                return true;
            };
            viewModel.Farness = 0.5f;
            viewModel.EditGridCommand.Execute(null);
            viewModel.EditFarnessCommand.Execute(null);

            Assert.Equal((30, 4, Color.Blue), (viewModel.Grid.Width, viewModel.Grid.OffsetX, viewModel.Grid.GridColor));
            Assert.Equal(GridSizeChoice.Custom, viewModel.GridSize);
            Assert.True(viewModel.DisplayGrid);
            Assert.Equal(0.8f, viewModel.Farness, 4);
            Assert.Equal(100, new FarnessSettingViewModel(3).Percent);
        }

        [Fact]
        public void StatusTexts_FollowStateAndLanguage()
        {
            Assert.Equal(Utility.Language["AreaSelectionDiabled"], viewModel.AreaSelectionText);
            Assert.Equal("", viewModel.CopyBufferText);

            viewModel.ChangeAreaSelect(true);
            viewModel.ChangeCopyBuffer(true);
            Assert.Equal(Utility.Language["AreaSelectionEnabled"], viewModel.AreaSelectionText);
            Assert.Equal(Utility.Language["ExistCopyBuffer"], viewModel.CopyBufferText);

            // 言語を変えたら、表示し直す
            var changed = new List<string>();
            viewModel.PropertyChanged += (sender, e) => changed.Add(e.PropertyName);
            var textChanged = 0;
            viewModel.DisplayModes[0].PropertyChanged += (sender, e) => textChanged++;
            Utility.ChangeLanguage("en");
            try
            {
                Assert.Contains(nameof(MainWindowViewModel.AreaSelectionText), changed);
                Assert.Contains(nameof(MainWindowViewModel.CopyBufferText), changed);
                Assert.Equal(1, textChanged);
                Assert.Equal(Utility.Language["AreaSelectionEnabled"], viewModel.AreaSelectionText);
            }
            finally
            {
                Utility.ChangeLanguage("jp");
            }
        }

        [Fact]
        public void OnLoaded_AsksAuthorNameWhenEmpty()
        {
            PPDStaticSetting.AuthorName = "";
            view.Dialog = dialog =>
            {
                ((AuthorNameViewModel)dialog).AuthorName = "new author";
                return true;
            };

            viewModel.OnLoaded();
            Assert.Equal("new author", PPDStaticSetting.AuthorName);

            // 作者名があるときは聞かない
            view.Dialog = _ => throw new InvalidOperationException();
            viewModel.OnLoaded();
        }

        [Fact]
        public void OnLoaded_Cancelled_KeepsEmpty()
        {
            PPDStaticSetting.AuthorName = "";

            viewModel.OnLoaded();

            Assert.Equal("", PPDStaticSetting.AuthorName);
        }

        [Fact]
        public void WithoutProject_DifficultyMenus()
        {
            viewModel.AvailableDifficulty = AvailableDifficulty.Base | AvailableDifficulty.Hard;
            Assert.Equal(2, viewModel.AvailableDifficultyCount);

            // プロジェクトを開いていないときは、ほかの難易度のイベントを読み込めない
            Assert.False(viewModel.ImportEventCommand.CanExecute(AvailableDifficulty.Hard));

            // 同じ難易度を選んだときは、タイトルだけ変える
            viewModel.ChangeDifficultyCommand.Execute(AvailableDifficulty.Base);
            Assert.Equal("(Base) - PPDEditor", viewModel.WindowTitle);
            Assert.False(viewModel.ProjectMenusEnabled);
        }
    }
}
