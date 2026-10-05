using PPDConfiguration;
using PPDEditor;
using PPDEditor.Models;
using PPDEditor.ViewModels;
using PPDEditor.ViewModels.Docks;
using PPDEditorCommon;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Utility = PPDEditor.Utility;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// PPDEditor のドックの ViewModel のテストです（偽物の画面やホストを使う）。
    /// </summary>
    public class PPDEditorDockTests : IDisposable
    {
        readonly TempFolder folder = new(setCurrentDirectory: true);

        public PPDEditorDockTests()
        {
            AppFiles.CopyTo("PPDEditor", folder);
            Utility.ChangeLanguage("jp");
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        [Theory]
        // 0.5 秒ごとに押すと 120
        [InlineData(new long[] { 1000, 1500 }, "500", 120)]
        [InlineData(new long[] { 0, 500, 1000, 1500 }, "500", 120)]
        [InlineData(new long[] { 0, 400, 1000 }, "600", 120)]
        public void BPMMeasure_CountsIntervals(long[] times, string interval, float bpm)
        {
            var index = 0;
            var viewModel = new BPMMeasureViewModel(() => times[index++]);

            viewModel.TapCommand.Execute(null);
            Assert.Equal(("0", "0"), (viewModel.Interval, viewModel.Bpm));
            for (int i = 1; i < times.Length; i++)
            {
                viewModel.TapCommand.Execute(null);
            }

            Assert.Equal(interval, viewModel.Interval);
            // 以前は押した回数で数えていたので、2 回押すと 240 になっていた
            Assert.Equal(bpm, float.Parse(viewModel.Bpm), 3);
            viewModel.ClearCommand.Execute(null);
            Assert.Equal(("0", "0"), (viewModel.Interval, viewModel.Bpm));
        }

        [Fact]
        public void Memo_TextChangesAndFontSize()
        {
            var savedSize = PPDStaticSetting.MemoFontSize;
            try
            {
                var viewModel = new MemoViewModel();
                var changed = 0;
                viewModel.Changed += (sender, e) => changed++;

                viewModel.MemoText = "memo";
                viewModel.IncreaseFontSizeCommand.Execute(null);
                Assert.Equal((1, 10f), (changed, viewModel.MemoFontSize));
                Assert.Equal(10 * 96 / 72.0, viewModel.FontSizePixels, 3);
                viewModel.DecreaseFontSizeCommand.Execute(null);
                Assert.Equal(9, PPDStaticSetting.MemoFontSize);

                viewModel.SetFontSize(4);
                viewModel.SetFontSize(101);
                Assert.Equal(9, viewModel.MemoFontSize);
                viewModel.SetFontSize(100);
                Assert.Equal(100, viewModel.MemoFontSize);
            }
            finally
            {
                PPDStaticSetting.MemoFontSize = savedSize;
            }
        }

        [Fact]
        public void IniFileWriter_SetsAndSavesValues()
        {
            var checkedBpm = new List<float>();
            var errors = new List<string>();
            var viewModel = new IniFileWriterViewModel(() => 12.5f, checkedBpm.Add, errors.Add);
            Assert.Equal(14, viewModel.Rows.Length);
            Assert.Equal((100f, 0f, 0f), (viewModel.BPM, viewModel.StartTime, viewModel.EndTime));
            Assert.True(viewModel.Rows[0].IsTime && viewModel.Rows[8].IsBpm && !viewModel.Rows[4].IsTime);
            Assert.Equal(Utility.Language["SFWLabel1"], viewModel.Rows[0].Name);

            viewModel.SetIni([1, 2, 3, 4], ["e", "n", "h", "x"], 150, "150-160", new MovieTrimmingData(5, 6, 7, 8));
            viewModel.LoadTimeCommand.Execute(viewModel.Rows[3]);
            viewModel.CheckBpm();
            Assert.Equal((150f, 3f, 12.5f), (viewModel.BPM, viewModel.StartTime, viewModel.EndTime));
            Assert.Equal([150f], checkedBpm);

            viewModel.Rows[1].Value = "abc";
            var path = folder.Combine("data.ini");
            using (var writer = new SettingWriter(path, false))
            {
                Assert.True(viewModel.SaveIni(writer));
            }
            var saved = new SettingReader(File.ReadAllText(path));
            Assert.Equal(("1", "0", "12.5", "150"), (saved["thumbtimestart"], saved["thumbtimeend"], saved["end"], saved["bpm"]));
            Assert.Equal(("150-160", "e", "x"), (saved["bpmstring"], saved["difficulty easy"], saved["difficulty extreme"]));
            Assert.Equal(("6", "7", "5", "8"), (saved["moviecutleft"], saved["moviecutright"], saved["moviecuttop"], saved["moviecutbottom"]));
            Assert.Equal(viewModel.SongGuid.ToString(), saved["guid"]);
            Assert.Equal(["数値変換エラーです。@" + Utility.Language["SFWLabel2"]], errors);

            var guid = viewModel.SongGuid;
            viewModel.Rows[8].Value = "x";
            Assert.Equal(100, viewModel.BPM);
            viewModel.Clear();
            Assert.NotEqual(guid, viewModel.SongGuid);
            Assert.Equal(("0", "", "100"), (viewModel.Rows[0].Value, viewModel.Rows[4].Value, viewModel.Rows[8].Value));
        }

        sealed class FakeKasiView : IKasiEditorView
        {
            public List<string> Messages { get; } = [];
            public string NewText { get; set; }
            public int ShownRows { get; private set; }
            public int Focused { get; private set; }
            public int Refreshed { get; private set; }

            public void ShowMessage(string text) => Messages.Add(text);

            public string AskNewText(string text) => NewText;

            public void ShowSelectedRow() => ShownRows++;

            public void FocusInput() => Focused++;

            public void RefreshTimeline() => Refreshed++;
        }

        [Fact]
        public void KasiEditor_AddEditAndShift()
        {
            var view = new FakeKasiView();
            var now = 1f;
            var viewModel = new KasiEditorViewModel(view, () => now);
            var changed = 0;
            viewModel.Changed += (sender, e) => changed++;

            viewModel.InputText = "first\nline";
            viewModel.AddAtCurrentTimeCommand.Execute(null);
            viewModel.AddAtCurrentTimeCommand.Execute(null);
            Assert.Equal([Utility.Language["KEAlreadyAdded"]], view.Messages);
            Assert.Equal("firstline", viewModel.Items.Single().Value);

            // 行を選んでいないときは知らせる
            viewModel.DeleteSelectedCommand.Execute(null);
            viewModel.ShiftSelectedCommand.Execute(null);
            viewModel.ChangeSelectedTextCommand.Execute(null);
            viewModel.CopySelectedToCurrentTimeCommand.Execute(null);
            Assert.Equal(5, view.Messages.Count);

            viewModel.SelectedIndex = 0;
            now = 3;
            viewModel.CopySelectedToCurrentTimeCommand.Execute(null);
            Assert.Equal([1f, 3f], viewModel.Items.Select(i => i.Key));

            viewModel.ShiftAmount = 0.5m;
            viewModel.ShiftSelectedCommand.Execute(null);
            Assert.Equal([1.5f, 3f], viewModel.Items.Select(i => i.Key));

            view.NewText = null;
            viewModel.ChangeSelectedTextCommand.Execute(null);
            view.NewText = "changed\n";
            viewModel.ChangeSelectedTextCommand.Execute(null);
            Assert.Equal("changed", viewModel.Items[0].Value);

            now = 2;
            Assert.Equal("changed", viewModel.GetCurrentKasi());
            Assert.Equal([3f], viewModel.GetKasiChangeWithinTime(2, 4));
            Assert.Equal("firstline", viewModel.GetFormattedContent(3));

            viewModel.SelectedIndex = 1;
            viewModel.DeleteSelectedCommand.Execute(null);
            Assert.Single(viewModel.Items);
            Assert.Equal(-1, viewModel.SelectedIndex);
            Assert.True(changed >= 5);

            viewModel.ClearInputCommand.Execute(null);
            Assert.Equal(("", 1), (viewModel.InputText, view.Focused));
        }

        [Fact]
        public void KasiEditor_LoadMoveAndSave()
        {
            var view = new FakeKasiView();
            var viewModel = new KasiEditorViewModel(view, () => 0);
            viewModel.SetData("1:a\n2:b");

            viewModel.FocusWithTimeData(2);
            viewModel.FocusWithTimeData(5);
            Assert.Equal((1, 1), (viewModel.SelectedIndex, view.ShownRows));

            viewModel.ChangeTime(-1, 4);
            viewModel.ChangeTime(1, 2);
            viewModel.ChangeTime(1, 4);
            Assert.Equal(1, view.Refreshed);
            Assert.Equal(string.Join(Environment.NewLine, "2:b", "4:a"), viewModel.SaveData());

            viewModel.Clear();
            Assert.Empty(viewModel.Items);
            Assert.Equal("", viewModel.GetCurrentKasi());
        }

        sealed class FakeTimeLineOperations : ITimeLineOperations
        {
            public List<string> Calls { get; } = [];
            public bool ConfirmAnswer { get; set; }
            public DateTime PresetsLoadTime { get; set; } = new(2026, 1, 1);
            public ParameterPresetFolder PresetRoot { get; set; } = new("root");
            public Dictionary<string, string> Common { get; set; }

            public void AssignID() => Calls.Add("assign");

            public void UnassignID(Func<bool> confirm) => Calls.Add("unassign:" + confirm());

            public void InterpolatePosition() => Calls.Add("position");

            public void InterpolateAngle(bool clockwise) => Calls.Add("angle:" + clockwise);

            public bool Confirm(string text, string caption) => ConfirmAnswer;

            public Dictionary<string, string> GetSelectedCommonParameters() => Common;

            public void ApplyParameters(KeyValuePair<string, string>[] parameters) => Calls.Add("apply:" + parameters.Length);

            public void UnapplyParameters(KeyValuePair<string, string>[] parameters) => Calls.Add("unapply:" + parameters.Length);
        }

        private ParameterPresetFolder CreatePresets()
        {
            Directory.CreateDirectory(Path.Combine(ParameterPresetStore.PresetDir, "Sub"));
            ParameterPresetStore.Save(Path.Combine(ParameterPresetStore.PresetDir, "Top.xml"), [new("a", "1"), new("b", "2")]);
            ParameterPresetStore.Save(Path.Combine(ParameterPresetStore.PresetDir, "Sub", "Inner.xml"), [new("c", "3")]);
            return ParameterPresetStore.Load(() => { });
        }

        [Fact]
        public void TimeLine_ForwardsMenuOperations()
        {
            var operations = new FakeTimeLineOperations { ConfirmAnswer = true };
            var viewModel = new TimeLineViewModel(operations);

            viewModel.AssignIDCommand.Execute(null);
            viewModel.UnassignIDCommand.Execute(null);
            viewModel.InterpolatePositionCommand.Execute(null);
            viewModel.InterpolateAngleClockwiseCommand.Execute(null);
            viewModel.InterpolateAngleCounterclockwiseCommand.Execute(null);

            Assert.Equal(["assign", "unassign:True", "position", "angle:True", "angle:False"], operations.Calls);
        }

        [Fact]
        public void TimeLine_PresetMenuChecksAndToggles()
        {
            var operations = new FakeTimeLineOperations { PresetRoot = CreatePresets() };
            var viewModel = new TimeLineViewModel(operations);

            viewModel.RefreshPresets();
            Assert.Equal(["Sub", "Top"], viewModel.Presets.Select(p => p.Name));
            Assert.Equal("Inner", viewModel.Presets[0].Children.Single().Name);
            var top = viewModel.Presets[1];
            Assert.False(top.IsChecked || top.IsPartial);

            operations.Common = new Dictionary<string, string> { ["a"] = "1" };
            viewModel.RefreshPresets();
            Assert.True(top.IsPartial);
            Assert.False(top.IsChecked);
            viewModel.TogglePresetCommand.Execute(top);

            operations.Common = new Dictionary<string, string> { ["a"] = "1", ["b"] = "2" };
            viewModel.RefreshPresets();
            Assert.True(top.IsChecked);
            viewModel.TogglePresetCommand.Execute(top);
            viewModel.TogglePresetCommand.Execute(viewModel.Presets[0]);
            viewModel.TogglePresetCommand.Execute(null);
            Assert.Equal(["apply:2", "unapply:2"], operations.Calls);

            // 読み込んだ時間が変わったら作り直す
            var before = viewModel.Presets[1];
            viewModel.RefreshPresets();
            Assert.Same(before, viewModel.Presets[1]);
            operations.PresetsLoadTime = operations.PresetsLoadTime.AddMinutes(1);
            viewModel.RefreshPresets();
            Assert.NotSame(before, viewModel.Presets[1]);
            Assert.Equal(2, PresetNode.GetPresets(viewModel.Presets).Count());
        }
    }
}
