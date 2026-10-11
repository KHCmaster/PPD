using PPDEditor;
using PPDEditor.ViewModels;
using PPDEditor.ViewModels.Dialogs;
using PPDEditorCommon;
using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Keys = System.Windows.Forms.Keys;
using Utility = PPDEditor.Utility;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// PPDEditor のダイアログの ViewModel のテスト（イベント、発行、マージ、設定）です。
    /// </summary>
    public class PPDEditorDialogTests2 : IDisposable
    {
        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakeDialogView view = new();

        public PPDEditorDialogTests2()
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
        public void ImportEvent_ImportsAndDeletesOnCopy()
        {
            var current = new SortedList<float, EventData> { [0] = new EventData() };
            var other = new SortedList<float, EventData> { [0] = new EventData { BPM = 1 }, [2] = new EventData { BPM = 200 } };
            var viewModel = new ImportEventViewModel(current, other);
            Assert.False(viewModel.ImportCommand.CanExecute(null));
            Assert.False(viewModel.DeleteCommand.CanExecute(null));
            Assert.Equal(Utility.Language["Time(Sec)"], viewModel.TimeHeader);
            Assert.Equal("<=" + Utility.Language["Import"], viewModel.ImportText);

            // 同じ時間のイベントはインポートしない
            viewModel.SelectedReadOnlyEvent = viewModel.ReadOnlyEvents[0];
            viewModel.ImportCommand.Execute(null);
            Assert.Equal(100, viewModel.ChangeData[0].BPM);
            viewModel.SelectedReadOnlyEvent = viewModel.ReadOnlyEvents[1];
            viewModel.ImportCommand.Execute(null);
            Assert.Equal([0f, 2f], viewModel.CurrentEvents.Select(e => e.Key));

            viewModel.SelectedCurrentEvent = viewModel.CurrentEvents[0];
            viewModel.DeleteCommand.Execute(null);
            Assert.Equal([2f], viewModel.ChangeData.Keys);
            // 元の一覧は変えない
            Assert.Single(current);

            EventData shown = null;
            viewModel.ShowEventRequested += e => shown = e;
            viewModel.ShowEventCommand.Execute(null);
            Assert.Null(shown);
            viewModel.ShowEventCommand.Execute(viewModel.ReadOnlyEvents[1]);
            Assert.Equal(200, shown.BPM);
            Assert.NotSame(other[2], shown);
        }

        [Fact]
        public void Publish_SelectsAvailableDifficultiesAndChecks()
        {
            folder.Write(Path.Combine("project", "Easy.ppd"), "same");
            folder.Write(Path.Combine("project", "Hard.ppd"), "same");
            var viewModel = new PublishViewModel(AvailableDifficulty.Easy | AvailableDifficulty.Hard, "song", folder.Combine("project")) { View = view };

            Assert.Equal((true, false, true, false), (viewModel.CanEasy, viewModel.CanNormal, viewModel.CanHard, viewModel.CanExtreme));
            Assert.Equal(AvailableDifficulty.Easy | AvailableDifficulty.Hard, viewModel.PublishDifficulty);
            Assert.Equal(folder.Combine("publish"), viewModel.PublishFolderPath);

            // 同じ内容の譜面は発行できない
            Assert.False(Ok(viewModel));
            Assert.Single(view.Messages);

            viewModel.Easy = false;
            viewModel.Hard = false;
            Assert.False(viewModel.OkCommand.CanExecute(null));
            viewModel.Hard = true;
            viewModel.PublishFolderName = "";
            Assert.False(viewModel.OkCommand.CanExecute(null));

            view.Folder = folder.Combine("elsewhere");
            viewModel.BrowseFolderCommand.Execute(null);
            Assert.Equal(folder.Combine("elsewhere"), viewModel.PublishFolderPath);
        }

        [Fact]
        public void Publish_ValidChart_Closes()
        {
            Directory.CreateDirectory(folder.Combine("project"));
            ChartFiles.WritePack(folder.Combine("project", "Normal.ppd"), [ChartFiles.Mark(1, ButtonType.Square)], []);
            var viewModel = new PublishViewModel(AvailableDifficulty.Normal, "song", folder.Combine("project")) { View = view };

            Assert.True(Ok(viewModel));
            Assert.Empty(view.Messages);
        }

        [Fact]
        public void Merge_SelectsProjectsAndBaseDifficulty()
        {
            folder.Write("v2.ppdproj", "[version]2\n");
            Directory.CreateDirectory(folder.Combine("v2", "Hard_layers"));
            Directory.CreateDirectory(folder.Combine("v2", "Easy_layers"));
            folder.Write("v1.ppdproj", "[bpm]120\n");
            folder.Write("empty.ppdproj", "[version]2\n");
            var viewModel = new MergeViewModel { View = view };
            Assert.False(viewModel.OkCommand.CanExecute(null));

            view.File = folder.Combine("v2.ppdproj");
            viewModel.BrowseProjectCommand.Execute(viewModel.Slots[2]);
            Assert.Equal([AvailableDifficulty.Easy, AvailableDifficulty.Hard], viewModel.Slots[2].Difficulties);
            Assert.Equal(AvailableDifficulty.Easy, viewModel.Slots[2].SelectedDifficulty);
            Assert.Equal([AvailableDifficulty.Hard], viewModel.BaseDifficulties);
            Assert.Equal(AvailableDifficulty.Hard, viewModel.BaseInfoProjectDifficulty);

            view.File = folder.Combine("v1.ppdproj");
            viewModel.BrowseProjectCommand.Execute(viewModel.Slots[0]);
            Assert.Equal([AvailableDifficulty.Base], viewModel.Slots[0].Difficulties);
            Assert.Equal(1, viewModel.GetMergeInfo(AvailableDifficulty.Easy).ProjectVersion);
            Assert.Equal(AvailableDifficulty.Easy | AvailableDifficulty.Hard, viewModel.MergeDifficulty);
            // 選んでいた基にする難易度はそのまま
            Assert.Equal(AvailableDifficulty.Hard, viewModel.BaseInfoProjectDifficulty);

            view.File = folder.Combine("empty.ppdproj");
            viewModel.BrowseProjectCommand.Execute(viewModel.Slots[1]);
            Assert.Equal([Utility.Language["NoAvailableDifficulty"]], view.Messages);
            // 難易度のないプロジェクトはマージする難易度に入れない
            Assert.Equal(AvailableDifficulty.Easy | AvailableDifficulty.Hard, viewModel.MergeDifficulty);

            Assert.False(viewModel.OkCommand.CanExecute(null));
            view.File = folder.Combine("merged.ppdproj");
            viewModel.BrowseDestCommand.Execute(null);
            Assert.True(viewModel.OkCommand.CanExecute(null));
            var info = viewModel.GetMergeInfo(AvailableDifficulty.Hard);
            Assert.Equal((folder.Combine("v2.ppdproj"), 2, AvailableDifficulty.Easy), (info.ProjectPath, info.ProjectVersion, info.Difficulty));
            Assert.Null(viewModel.GetMergeInfo(AvailableDifficulty.None));

            view.File = null;
            viewModel.BrowseProjectCommand.Execute(viewModel.Slots[2]);
            viewModel.BrowseProjectCommand.Execute(viewModel.Slots[0]);
            Assert.Empty(viewModel.BaseDifficulties);
            Assert.Null(viewModel.BaseInfoProjectDifficulty);
            Assert.Equal("", viewModel.Slots[0].ProjectPath);
        }

        [Fact]
        public void EventEdit_ReadsAndBuildsEvent()
        {
            var source = new EventData
            {
                BPM = 150,
                BPMRapidChange = true,
                DisplayState = DisplayState.Hidden,
                MoveState = MoveState.Stop,
                NoteType = NoteType.AC,
                SlideScale = 2,
                MovieVolumePercent = 40,
                CrossVolumePercent = 70,
                LKeepPlaying = true,
                UpReleaseSound = true
            };
            var viewModel = new EventEditViewModel(source);

            Assert.True(viewModel.IsEditable);
            Assert.Equal(("150", "150", "2"), (viewModel.BeforeBpm, viewModel.Bpm, viewModel.SlideScale));
            Assert.Equal(InitializeOrderMode.Normal, viewModel.OrderMode);
            Assert.Equal(Utility.Language["EEFLabel2"], viewModel.Sounds[0].Label);
            Assert.Equal(Utility.Language["SMLabel1"], viewModel.InitializeOrder[0].Name);

            var copy = viewModel.EventData;
            Assert.Equal((40, 70, true, true), (copy.MovieVolumePercent, copy.CrossVolumePercent, copy.LKeepPlaying, copy.UpReleaseSound));
            Assert.Equal((150f, true, DisplayState.Hidden, MoveState.Stop, NoteType.AC, 2f), (copy.BPM, copy.BPMRapidChange, copy.DisplayState, copy.MoveState, copy.NoteType, copy.SlideScale));

            viewModel.Bpm = "abc";
            viewModel.SlideScale = "x";
            Assert.True(viewModel.IsBpmInvalid);
            Assert.Equal((100f, 1f), (viewModel.EventData.BPM, viewModel.EventData.SlideScale));
        }

        [Fact]
        public void EventEdit_InitializeOrderModes()
        {
            var viewModel = new EventEditViewModel(new EventData(), isReadOnly: true);
            Assert.False(viewModel.IsEditable);
            viewModel.SelectedOrderIndex = 1;
            Assert.False(viewModel.MoveUpCommand.CanExecute(null));

            viewModel.OrderMode = InitializeOrderMode.AC;
            Assert.Equal(ButtonType.Triangle, viewModel.EventData.InitializeOrder[0]);
            Assert.Equal(InitializeOrderMode.AC, new EventEditViewModel(viewModel.EventData).OrderMode);

            viewModel.OrderMode = InitializeOrderMode.Custom;
            viewModel.SelectedOrderIndex = 1;
            viewModel.MoveUpCommand.Execute(null);
            Assert.Equal(0, viewModel.SelectedOrderIndex);
            Assert.False(viewModel.MoveUpCommand.CanExecute(null));
            viewModel.SelectedOrderIndex = 9;
            Assert.False(viewModel.MoveDownCommand.CanExecute(null));
            viewModel.SelectedOrderIndex = 8;
            viewModel.MoveDownCommand.Execute(null);

            var order = viewModel.EventData.InitializeOrder;
            Assert.Equal([ButtonType.Square, ButtonType.Triangle], order.Take(2));
            Assert.Equal([ButtonType.L, ButtonType.R], order.Skip(8));
            Assert.Equal(InitializeOrderMode.Custom, new EventEditViewModel(viewModel.EventData).OrderMode);
            viewModel.OrderMode = InitializeOrderMode.Normal;
            Assert.Equal(ButtonType.Square, viewModel.EventData.InitializeOrder[0]);
        }

        /// <summary>
        /// 設定の ViewModel は、OK のときにエディター全体の設定（静的なフィールド）を変えるので、テストの後で元に戻す。
        /// </summary>
        private static void KeepStaticSettings(Action test)
        {
            var saved = (PPDStaticSetting.AuthorName, PPDStaticSetting.MovieLatency, PPDStaticSetting.HideToggleArrow, PPDStaticSetting.HideToggleRectangle,
                PPDStaticSetting.EnableToChangeMarkTypeAndTime, PPDStaticSetting.Moves, PPDStaticSetting.Angles);
            try
            {
                test();
            }
            finally
            {
                (PPDStaticSetting.AuthorName, PPDStaticSetting.MovieLatency, PPDStaticSetting.HideToggleArrow, PPDStaticSetting.HideToggleRectangle,
                    PPDStaticSetting.EnableToChangeMarkTypeAndTime, PPDStaticSetting.Moves, PPDStaticSetting.Angles) = saved;
            }
        }

        private static ShortcutManager CreateShortcuts()
        {
            var manager = new ShortcutManager();
            manager.RegisterShortcut(new ShortcutInfo(Keys.Z, false, true, false, ShortcutType.Undo));
            manager.RegisterShortcut(new ShortcutInfo(Keys.Y, true, true, true, ShortcutType.Redo));
            manager.RegisterShortcut(new ShortcutInfo(Keys.F1, false, false, false, Path.Combine("Commands", "Tools", "Align")));
            return manager;
        }

        [Fact]
        public void Setting_ShowsAndAppliesValues()
        {
            KeepStaticSettings(() =>
            {
                PPDStaticSetting.AuthorName = "author";
                PPDStaticSetting.MovieLatency = 0.5f;
                PPDStaticSetting.HideToggleArrow = true;
                PPDStaticSetting.Moves = [1, 2, 3, 4, 5, 6, 7, 8];
                PPDStaticSetting.Angles = [8, 7, 6, 5, 4, 3, 2, 1];
                var manager = CreateShortcuts();
                var viewModel = new SettingViewModel(manager) { View = view };

                Assert.Equal(("author", "0.5", false, true), (viewModel.AuthorName, viewModel.MovieLatency, viewModel.ShowToggleArrow, viewModel.ShowToggleRectangle));
                Assert.False(viewModel.ShouldRestart);
                Assert.Equal("Ctrl+Z", viewModel.Rows.Single(r => r.ShortcutType == ShortcutType.Undo).Key);
                Assert.Equal("Ctrl+Shift+Alt+Y", viewModel.Rows.Single(r => r.ShortcutType == ShortcutType.Redo).Key);
                var custom = viewModel.Rows.Last();
                Assert.Equal(("Tools:Align", "F1", ShortcutType.Custom), (custom.Command, custom.Key, custom.ShortcutType));
                Assert.Equal(Utility.Language["Undo"], viewModel.Rows.Single(r => r.ShortcutType == ShortcutType.Undo).Command);

                viewModel.AuthorName = "new";
                viewModel.MovieLatency = "abc";
                viewModel.ShowToggleArrow = true;
                viewModel.ShowToggleRectangle = false;
                viewModel.EnableToChangeMarkTypeAndTime = true;
                viewModel.Moves[0] = 10;
                Assert.True(viewModel.ShouldRestart);
                Assert.True(Ok(viewModel));
                viewModel.Apply();

                Assert.Equal(("new", 0f, false, true, true), (PPDStaticSetting.AuthorName, PPDStaticSetting.MovieLatency, PPDStaticSetting.HideToggleArrow, PPDStaticSetting.HideToggleRectangle, PPDStaticSetting.EnableToChangeMarkTypeAndTime));
                Assert.Equal(10, PPDStaticSetting.Moves[0]);
                Assert.Equal(8, PPDStaticSetting.Angles[0]);
                Assert.Equal(ShortcutType.Undo, manager.GetShortcut(Keys.Z, true, false, false).ShortcutType);
                Assert.Equal(Path.Combine("Commands", "Tools", "Align"), manager.GetShortcut(Keys.F1, false, false, false).ScriptPath);
            });
        }

        [Fact]
        public void Setting_EditKeysAndCommands()
        {
            KeepStaticSettings(() =>
            {
                var manager = CreateShortcuts();
                var viewModel = new SettingViewModel(manager) { View = view, SelectCommand = () => null };
                var keyInputs = 0;
                viewModel.KeyInputRequested += (sender, e) => keyInputs++;
                Assert.False(viewModel.DeleteShortcutCommand.CanExecute(null));

                // 選んだコマンドにキーを割り当てる
                var undo = viewModel.Rows.Single(r => r.ShortcutType == ShortcutType.Undo);
                viewModel.SelectedRow = undo;
                Assert.Equal((1, "Ctrl+Z"), (keyInputs, viewModel.EditingKey));
                Assert.False(viewModel.RemoveCommandCommand.CanExecute(null));
                viewModel.AssignKey(Keys.U, true, false, false);
                Assert.Equal("Shift+U", undo.Key);
                viewModel.CancelKeyInput();
                Assert.Equal("", viewModel.EditingKey);

                // 同じキーを 2 つに割り当てると OK できない
                viewModel.Rows.Single(r => r.ShortcutType == ShortcutType.Redo).Key = "Shift+U";
                Assert.False(Ok(viewModel));
                Assert.Contains(Utility.Language["SameShortcutKeyAssigned"], view.Messages.Single());
                viewModel.DeleteShortcutCommand.Execute(null);
                Assert.Equal("", undo.Key);

                // コマンドを足して消す
                viewModel.AddCommandCommand.Execute(null);
                viewModel.SelectCommand = () => Path.Combine("Commands", "Other");
                viewModel.AddCommandCommand.Execute(null);
                Assert.Equal("Other", viewModel.SelectedRow.Command);
                Assert.True(viewModel.RemoveCommandCommand.CanExecute(null));
                viewModel.RemoveCommandCommand.Execute(null);
                Assert.DoesNotContain(viewModel.Rows, r => r.Command == "Other");

                viewModel.AssignKey(Keys.Q, false, false, false);
                viewModel.Apply();
                Assert.Null(manager.GetShortcut(Keys.Z, true, false, false));
                Assert.Equal(ShortcutType.Redo, manager.GetShortcut(Keys.U, false, true, false).ShortcutType);
            });
        }
    }
}
