using PPDEditor;
using PPDEditor.Command.PPDSheet;
using PPDEditor.ViewModels.Docks;
using PPDEditorCommon;
using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Color = System.Drawing.Color;
using Utility = PPDEditor.Utility;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// PPDEditor のドックの ViewModel のテスト（イベント、統計、レイヤー）です。マークは描画の装置が要るので、空の譜面だけを使う。
    /// </summary>
    public sealed class PPDEditorDockTests2 : IDisposable
    {
        readonly TempFolder folder = new(setCurrentDirectory: true);

        public PPDEditorDockTests2()
        {
            AppFiles.CopyTo("PPDEditor", folder);
            Utility.ChangeLanguage("jp");
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        sealed class FakeEventHost : IEventManagerHost
        {
            public float CurrentTime { get; set; }
            public float DefaultBpm { get; set; } = 120;
            public Func<EventData, EventData> Edit { get; set; } = e => e;
            public List<string> Messages { get; } = [];
            public bool ConfirmAnswer { get; set; } = true;
            public int ShownRows { get; private set; }
            public int Refreshed { get; private set; }
            public List<int> MovieVolumes { get; } = [];

            public EventData EditEvent(EventData eventData) => Edit(eventData);

            public void ShowMessage(string text) => Messages.Add(text);

            public bool Confirm(string text, string caption, bool yesNo) => ConfirmAnswer;

            public void ShowSelectedRow() => ShownRows++;

            public void RefreshTimeline() => Refreshed++;

            public void ChangeMovieVolume(int volume) => MovieVolumes.Add(volume);
        }

        [Fact]
        public void EventManager_AddEditDeleteAndShift()
        {
            var host = new FakeEventHost();
            var viewModel = new EventManagerViewModel(host);
            var changed = 0;
            viewModel.Changed += (sender, e) => changed++;

            // 最初のイベントは設定ファイルの BPM で作る
            viewModel.AddCommand.Execute(null);
            Assert.Equal(120, viewModel.GetEvent(0).BPM);
            viewModel.AddCommand.Execute(null);
            Assert.Equal([Utility.Language["EMAlreadyAdded"]], host.Messages);

            // 今の時間のイベントを元にする
            viewModel.OnSeeked(0);
            host.CurrentTime = 2;
            host.Edit = e =>
            {
                Assert.Equal(120, e.BPM);
                e.BPM = 150;
                return e;
            };
            viewModel.AddCommand.Execute(null);
            host.Edit = _ => null;
            host.CurrentTime = 3;
            viewModel.AddCommand.Execute(null);
            Assert.Equal([0f, 2f], viewModel.Items.Select(i => i.Key));

            viewModel.SelectedIndex = 1;
            host.Edit = e => new EventData { BPM = 160 };
            viewModel.EditSelectedCommand.Execute(null);
            Assert.Equal(160, viewModel.GetEvent(2).BPM);

            viewModel.ShiftAmount = 1.5m;
            viewModel.ShiftSelectedCommand.Execute(null);
            Assert.Equal([0f, 3.5f], viewModel.GetEventsWithinTime(0, 10));

            host.ConfirmAnswer = false;
            viewModel.DeleteSelectedCommand.Execute(null);
            Assert.Equal(2, viewModel.Items.Count);
            host.ConfirmAnswer = true;
            viewModel.DeleteSelectedCommand.Execute(null);
            Assert.Single(viewModel.Items);
            Assert.Equal(-1, viewModel.SelectedIndex);

            // 行を選んでいないとき
            viewModel.EditSelectedCommand.Execute(null);
            viewModel.DeleteSelectedCommand.Execute(null);
            viewModel.ShiftSelectedCommand.Execute(null);
            Assert.Equal(Utility.Language["EMNoSelectedLine"], host.Messages.Last());
            Assert.Equal(5, changed);
        }

        [Fact]
        public void EventManager_SeekAppliesVolumesAndDisplay()
        {
            var host = new FakeEventHost();
            var viewModel = new EventManagerViewModel(host);
            var data = new EventData
            {
                DisplayState = DisplayState.Sudden,
                MovieVolumePercent = 50,
                CrossVolumePercent = 80,
                LReleaseSound = true
            };
            viewModel.AddChange(1, data);

            viewModel.OnSeeked(0.5f);
            Assert.Equal(-1, viewModel.SelectedIndex);
            Assert.Equal(-1000, viewModel.GetVolume(1));
            Assert.Equal(DisplayState.Normal, viewModel.DisplayState);

            viewModel.OnSeeked(1);
            Assert.Equal(0, viewModel.SelectedIndex);
            Assert.Equal(-2000, viewModel.GetVolume(1));
            Assert.True(viewModel.ReleaseSound(9));
            Assert.Equal(DisplayState.Sudden, viewModel.DisplayState);
            Assert.Equal([-1000, -5000], host.MovieVolumes);

            viewModel.Clear();
            Assert.Empty(viewModel.Items);
            Assert.Equal(-1000, viewModel.GetVolume(1));
            Assert.False(viewModel.ReleaseSound(9));
            Assert.Equal(DisplayState.Normal, viewModel.DisplayState);
        }

        [Fact]
        public void EventManager_InitialBpmMoveAndEvd()
        {
            var host = new FakeEventHost();
            var viewModel = new EventManagerViewModel(host)
            {
                ChangeData = new SortedList<float, EventData> { [0] = new EventData { BPM = 100 }, [2] = new EventData { BPM = 140 } }
            };

            host.ConfirmAnswer = false;
            viewModel.CheckInitialBpm(120);
            Assert.Equal(100, viewModel.GetEvent(0).BPM);
            host.ConfirmAnswer = true;
            viewModel.CheckInitialBpm(120);
            Assert.Equal(120, viewModel.ChangeData[0].BPM);

            viewModel.FocusWithTimeData(2);
            viewModel.FocusWithTimeData(9);
            Assert.Equal((1, 1), (viewModel.SelectedIndex, host.ShownRows));

            viewModel.ChangeTime(2, 4);
            viewModel.ChangeTime(-1, 5);
            Assert.Equal(1, host.Refreshed);
            Assert.NotEqual("", viewModel.GetFormattedContent(4));

            var evd = viewModel.GetEvdData();
            Assert.Equal(140, Assert.IsType<ChangeBPMEvent>(Assert.Single(evd, e => e.Time == 4)).BPM);
            Assert.Equal(4, viewModel.GetCorrectTime(4, 10));
        }

        sealed class FakeStatsSource : IStatsSource
        {
            public List<(string Name, PPDSheet Sheet)> Layers { get; } = [];
            public MarkData[] AllData { get; set; } = [];

            public IEnumerable<(string Name, PPDSheet Sheet)> GetLayers() => Layers;

            public MarkData[] GetAllData() => AllData;

            public IEnumerable<int> VisibleRows => [0, 1];

            public (float Start, float End) TimeRange => (0.5f, 3.2f);
        }

        [Fact]
        public void StatsManager_UpdatesLayersAndCharts()
        {
            var source = new FakeStatsSource
            {
                AllData = [ChartFiles.Mark(1, ButtonType.Square), ChartFiles.Mark(1.5f, ButtonType.Cross), ChartFiles.Mark(2, ButtonType.Cross)]
            };
            source.Layers.Add(("Layer0", new PPDSheet()));
            var viewModel = new StatsManagerViewModel(source);

            viewModel.UpdateCommand.Execute(null);

            Assert.Equal([Utility.Language["AllLayers"], "Layer0"], viewModel.Layers.Select(l => l.Name));
            Assert.Same(viewModel.Layers[0], viewModel.SelectedLayer);
            // 表示している 2 行と、普通、長押し、合計、難易度の平均と最大
            Assert.Equal(7, viewModel.DataRows.Count);
            Assert.Equal(Utility.Language["Sum"], viewModel.DataRows[4].Key);
            Assert.Empty(viewModel.PieItems);
            // 曲の始めから終わりまで 1 秒ごと
            Assert.Equal([0, 1, 2, 3, 4], viewModel.DifficultyPoints.Select(p => p.Key));
            Assert.Equal(3, viewModel.ChartModeNames.Length);

            viewModel.Layers[0].NormalCounts[1] = 2;
            viewModel.Layers[0].LongCounts[2] = 1;
            viewModel.ChartModeIndex = (int)ChartMode.Normal;
            Assert.Equal([2], viewModel.PieItems.Select(p => p.Value));
            viewModel.ChartModeIndex = (int)ChartMode.Long;
            Assert.Equal([1], viewModel.PieItems.Select(p => p.Value));
            viewModel.ChartModeIndex = (int)ChartMode.Both;
            Assert.Equal(2, viewModel.PieItems.Count);

            // 言語を変えたら、最初の種類に戻す
            viewModel.ChartModeIndex = 1;
            Utility.ChangeLanguage("en");
            Assert.Equal(0, viewModel.ChartModeIndex);
            Utility.ChangeLanguage("jp");
        }

        sealed class FakeLayerHost : ILayerHost
        {
            public bool ConfirmAnswer { get; set; } = true;
            public string NewName { get; set; }
            public Color? Picked { get; set; }
            public (float Bpm, float BpmOffset, int DisplayWidth) CurrentSettings { get; set; } = (130, 0.25f, 150);
            public List<PPDSheet> Applied { get; } = [];
            public int Refreshed { get; private set; }

            public bool Confirm(string text, string caption) => ConfirmAnswer;

            public string AskNewLayerName(string defaultName) => NewName ?? defaultName;

            public Color? PickColor(Color current) => Picked;

            public void ApplySheetSettings(PPDSheet sheet) => Applied.Add(sheet);

            public void RefreshTimeline() => Refreshed++;

            public void OnSheetChanged()
            {
            }
        }

        [Fact]
        public void LayerManager_CreateSelectAndReorder()
        {
            var host = new FakeLayerHost();
            var viewModel = new LayerManagerViewModel(host);
            viewModel.Initialize();
            Assert.Equal(Utility.Language["LMLayer"] + "0", viewModel.SelectedLayer.DisplayName);
            Assert.Equal((130f, 0.25f, 150), (viewModel.SelectedPpdSheet.BPM, viewModel.SelectedPpdSheet.BPMStart, viewModel.SelectedPpdSheet.DisplayWidth));

            host.NewName = "second";
            viewModel.NewLayerCommand.Execute(null);
            host.NewName = null;
            viewModel.NewLayerCommand.Execute(null);
            string[] expected = ["LMLayer2", "second", "LMLayer0"];
            Assert.Equal(expected.Select(n => n.Replace("LMLayer", Utility.Language["LMLayer"])), viewModel.DisplayLayers.Select(l => l.DisplayName));

            viewModel.SelectLastLayer();
            Assert.Equal(2, viewModel.SelectedLayerIndex);
            Assert.Single(host.Applied);
            viewModel.SelectLayerByIndex(1);
            viewModel.SelectLayerByIndex(9);
            Assert.Equal("second", viewModel.SelectedLayer.DisplayName);
            // 同じ設定のレイヤーでも、違う譜面なら反映する
            Assert.Equal(2, host.Applied.Count);

            // 表示の一番上に移す
            viewModel.MoveLayer(viewModel.Layers[0], 0);
            Assert.Equal("LMLayer0".Replace("LMLayer", Utility.Language["LMLayer"]), viewModel.DisplayLayers[0].DisplayName);
            viewModel.MoveLayer(viewModel.Layers[2], 0);
            Assert.Equal(3, viewModel.Layers.Length);

            viewModel.SetSelectedBpm(180);
            viewModel.SetSelectedBpmOffset(1);
            viewModel.SetSelectedDisplayWidth(200);
            viewModel.SetSelectedDisplayMode(DisplayLineMode.Eigth);
            viewModel.SetSelectedBeatType(DisplayBeatType.Third);
            var sheet = viewModel.SelectedPpdSheet;
            Assert.Equal((180f, 1f, 200, DisplayLineMode.Eigth, DisplayBeatType.Third), (sheet.BPM, sheet.BPMStart, sheet.DisplayWidth, sheet.DisplayMode, sheet.BeatType));
            Assert.Equal(Utility.Language["LMBPM"] + " 180", viewModel.SelectedLayer.BPM);
        }

        [Fact]
        public void LayerManager_VisibleRenameColorDuplicateDelete()
        {
            var host = new FakeLayerHost();
            var viewModel = new LayerManagerViewModel(host);
            var changed = 0;
            viewModel.Changed += (sender, e) => changed++;
            viewModel.Initialize();
            viewModel.AddLayer("hidden", 100, 0, 100, false, DisplayLineMode.Fourth, DisplayBeatType.Fourth);
            Assert.Single(viewModel.Sheets);

            var layer = viewModel.Layers[1];
            layer.ToggleVisibleCommand.Execute(null);
            Assert.Equal(2, viewModel.Sheets.Length);
            layer.SelectCommand.Execute(null);
            Assert.Same(layer, viewModel.SelectedLayer);

            layer.EndRenameCommand.Execute(null);
            layer.BeginRenameCommand.Execute(null);
            layer.EditingName = "";
            layer.EndRenameCommand.Execute(null);
            Assert.Equal(("no text", "no text", false), (layer.DisplayName, layer.PPDData.DisplayName, layer.IsEditingName));

            layer.ChangeColorCommand.Execute(null);
            host.Picked = Color.Red;
            layer.ChangeColorCommand.Execute(null);
            Assert.Equal(Color.Red, layer.MarkColor);

            layer.DuplicateCommand.Execute(null);
            var copy = viewModel.Layers[2];
            Assert.Equal("no text" + Utility.Language["LMCopy"], copy.DisplayName);
            Assert.Equal((100f, Color.Red), (copy.PPDData.BPM, copy.MarkColor));

            host.ConfirmAnswer = false;
            copy.DeleteCommand.Execute(null);
            Assert.Equal(3, viewModel.Layers.Length);
            host.ConfirmAnswer = true;
            copy.DeleteCommand.Execute(null);
            Assert.Same(layer, viewModel.SelectedLayer);
            viewModel.Layers[0].DeleteCommand.Execute(null);
            layer.DeleteCommand.Execute(null);
            Assert.Empty(viewModel.Layers);
            Assert.Null(viewModel.SelectedPpdSheet);
            Assert.Equal(-1, viewModel.SelectedLayerIndex);
            Assert.Equal(4, changed);

            viewModel.SetSelectedBpmOffset(1);
            viewModel.SetSelectedBpm(1);
            Assert.Empty(viewModel.GetAllData(true));
            viewModel.CalculateMaxID();
            Assert.Equal(1u, viewModel.IDProvider.Next());
            viewModel.Clear();
            Assert.Empty(viewModel.DisplayLayers);
        }
    }
}
