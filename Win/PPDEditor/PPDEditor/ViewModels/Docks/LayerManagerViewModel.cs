using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDEditor.Command.PPDSheet;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Color = System.Drawing.Color;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// ViewModel から使う、エディターと画面の機能です。
    /// </summary>
    public interface ILayerHost
    {
        bool Confirm(string text, string caption);

        /// <summary>
        /// 新しいレイヤーの名前を入力させます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string AskNewLayerName(string defaultName);

        /// <summary>
        /// マークの色を選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        Color? PickColor(Color current);

        /// <summary>
        /// 新しいレイヤーの BPM、BPM のずれ、表示の幅です（今の設定）。
        /// </summary>
        (float Bpm, float BpmOffset, int DisplayWidth) CurrentSettings { get; }

        /// <summary>
        /// 選んだレイヤーの BPM などを、タイムラインとメインのウィンドウに反映します。
        /// </summary>
        void ApplySheetSettings(PPDSheet sheet);

        /// <summary>
        /// タイムラインを描き直します。
        /// </summary>
        void RefreshTimeline();

        /// <summary>
        /// 譜面を変えたことをタイムラインに知らせます（保存していない変更の印）。
        /// </summary>
        void OnSheetChanged();
    }

    /// <summary>
    /// レイヤー（譜面を重ねるもの）の一覧です。追加、複製、削除、並べ替え、選択を扱います。
    /// </summary>
    /// <remarks>
    /// <see cref="Layers"/> は保存する順（下のレイヤーが先）、<see cref="DisplayLayers"/> は表示する順（後ろのレイヤーほど上）。
    /// </remarks>
    public partial class LayerManagerViewModel(ILayerHost host) : DockViewModel
    {
        readonly List<LayerViewModel> layers = [];
        PPDSheet lastSheet;
        float lastBpm;
        float lastBpmOffset;
        int lastDisplayWidth;
        DisplayBeatType lastBeatType;

        public IDProvider IDProvider { get; private set; } = new(0);

        /// <summary>
        /// 表示する順のレイヤー（後ろのレイヤーほど上）です。
        /// </summary>
        public ObservableCollection<LayerViewModel> DisplayLayers { get; } = [];

        /// <summary>
        /// 保存する順のレイヤーです。
        /// </summary>
        public LayerViewModel[] Layers
        {
            get { return [.. layers]; }
        }

        public LayerViewModel SelectedLayer
        {
            get { return layers.FirstOrDefault(l => l.IsSelected); }
        }

        public PPDSheet SelectedPpdSheet
        {
            get { return SelectedLayer?.PPDData; }
        }

        public int SelectedLayerIndex
        {
            get { return layers.IndexOf(SelectedLayer); }
        }

        /// <summary>
        /// 表示しているレイヤーの譜面です。
        /// </summary>
        public PPDSheet[] Sheets
        {
            get { return [.. layers.Where(l => l.DisplayVisible).Select(l => l.PPDData)]; }
        }

        internal ILayerHost Host
        {
            get { return host; }
        }

        /// <summary>
        /// 最初のレイヤーを 1 つ作ります。
        /// </summary>
        public void Initialize()
        {
            var layer = CreateLayer(Texts["LMLayer"] + 0);
            layers.Add(layer);
            layer.IsSelected = true;
            Relocate();
        }

        public void Clear()
        {
            IDProvider = new IDProvider(0);
            layers.Clear();
            Relocate();
        }

        [RelayCommand]
        private void NewLayer()
        {
            var name = host.AskNewLayerName(Texts["LMLayer"] + layers.Count);
            if (name != null)
            {
                layers.Add(CreateLayer(name));
                Relocate();
                OnChanged();
            }
        }

        public void AddLayer(string name, float bpm, float bpmOffset, int displayWidth, bool visible, DisplayLineMode displayMode, DisplayBeatType beatType)
        {
            var layer = CreateLayer(name, bpm, bpmOffset, displayWidth, displayMode, beatType);
            layer.DisplayVisible = visible;
            layers.Add(layer);
            Relocate();
        }

        private LayerViewModel CreateLayer(string name)
        {
            var (bpm, bpmOffset, displayWidth) = host.CurrentSettings;
            return CreateLayer(name, bpm, bpmOffset, displayWidth, DisplayLineMode.Fourth, DisplayBeatType.Fourth);
        }

        private LayerViewModel CreateLayer(string name, float bpm, float bpmOffset, int displayWidth, DisplayLineMode displayMode, DisplayBeatType beatType)
        {
            var layer = new LayerViewModel(this, name)
            {
                BPM = Texts["LMBPM"] + " " + bpm
            };
            layer.PPDData.BPM = bpm;
            layer.PPDData.BPMStart = bpmOffset;
            layer.PPDData.DisplayWidth = displayWidth;
            layer.PPDData.DisplayMode = displayMode;
            layer.PPDData.BeatType = beatType;
            layer.PPDData.DisplayDataChanged += (sender, e) => ApplySelectedSettings();
            layer.PPDData.CommandChanged += (sender, e) => host.OnSheetChanged();
            return layer;
        }

        private void Relocate()
        {
            DisplayLayers.Clear();
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                DisplayLayers.Add(layers[i]);
            }
        }

        /// <summary>
        /// レイヤーを選びます。
        /// </summary>
        internal void Select(LayerViewModel layer)
        {
            foreach (var other in layers)
            {
                other.IsSelected = other == layer;
            }
            ApplySelectedSettings();
        }

        public void SelectLastLayer()
        {
            if (layers.Count > 0)
            {
                Select(layers[^1]);
            }
        }

        public void SelectLayerByIndex(int index)
        {
            if (index >= 0 && index < layers.Count)
            {
                Select(layers[index]);
            }
        }

        /// <summary>
        /// レイヤーを、表示する順で <paramref name="displayIndex"/> の場所（一番上が 0 の、行の間の番号）に移します。
        /// </summary>
        public void MoveLayer(LayerViewModel layer, int displayIndex)
        {
            var index = layers.Count - displayIndex;
            var currentIndex = layers.IndexOf(layer);
            if (currentIndex < index)
            {
                index--;
            }
            if (currentIndex != index)
            {
                layers.RemoveAt(currentIndex);
                layers.Insert(index, layer);
                Relocate();
            }
        }

        internal void Delete(LayerViewModel layer)
        {
            if (!host.Confirm(Texts["LMDeleteConfirm"], Texts["LMConfirm"]))
            {
                return;
            }
            var index = layers.IndexOf(layer);
            layers.Remove(layer);
            Relocate();
            index = Math.Clamp(index - 1, 0, Math.Max(layers.Count - 1, 0));
            if (layers.Count != 0)
            {
                Select(layers[index]);
            }
            host.RefreshTimeline();
            OnChanged();
        }

        internal void Duplicate(LayerViewModel source)
        {
            var index = layers.IndexOf(source);
            var layer = CreateLayer(source.DisplayName + Texts["LMCopy"]);
            layers.Insert(index + 1, layer);
            Relocate();
            host.RefreshTimeline();
            SortedList<float, Mark>[] data = source.PPDData.Data;
            for (int i = 0; i < data.Length; i++)
            {
                foreach (Mark mk in data[i].Values)
                {
                    if (mk is ExMark exmk)
                    {
                        layer.PPDData.AddExMark(exmk.Time, exmk.EndTime, exmk.Position.X, exmk.Position.Y, exmk.Rotation, (int)exmk.Type, exmk.ID, [.. exmk.Parameters]);
                    }
                    else
                    {
                        layer.PPDData.AddMark(mk.Time, mk.Position.X, mk.Position.Y, mk.Rotation, (int)mk.Type, mk.ID, [.. mk.Parameters]);
                    }
                }
            }
            layer.PPDData.BPM = source.PPDData.BPM;
            layer.PPDData.BPMStart = source.PPDData.BPMStart;
            layer.PPDData.DisplayWidth = source.PPDData.DisplayWidth;
            layer.PPDData.DisplayMode = source.PPDData.DisplayMode;
            layer.PPDData.BeatType = source.PPDData.BeatType;
            layer.BPM = source.BPM;
            layer.MarkColor = source.MarkColor;
            OnChanged();
        }

        // 選んだレイヤーの BPM などが変わったら、タイムラインとメインのウィンドウに反映する
        private void ApplySelectedSettings()
        {
            var sheet = SelectedPpdSheet;
            if (sheet != null && (sheet != lastSheet || sheet.BPM != lastBpm || sheet.BPMStart != lastBpmOffset
                || sheet.DisplayWidth != lastDisplayWidth || sheet.BeatType != lastBeatType))
            {
                host.ApplySheetSettings(sheet);
                lastSheet = sheet;
                lastBpm = sheet.BPM;
                lastBpmOffset = sheet.BPMStart;
                lastDisplayWidth = sheet.DisplayWidth;
                lastBeatType = sheet.BeatType;
            }
        }

        /// <summary>
        /// すべてのレイヤー（<paramref name="any"/> が false のときは表示しているレイヤー）のマークを、時間の順にまとめます。同じボタンで同じ時間のマークは先のレイヤーのものにします。
        /// </summary>
        public MarkData[] GetAllData(bool any)
        {
            var dest = new SortedList<float, MarkData>[10];
            for (int i = 0; i < dest.Length; i++)
            {
                dest[i] = [];
            }
            foreach (var layer in layers.Where(l => l.DisplayVisible || any))
            {
                SortedList<float, Mark>[] source = layer.PPDData.Data;
                for (int j = 0; j < source.Length; j++)
                {
                    foreach (Mark mk in source[j].Values)
                    {
                        if (!dest[j].ContainsKey(mk.Time))
                        {
                            dest[j].Add(mk.Time, mk is ExMark exmk ? exmk.Convert() : mk.Convert());
                        }
                    }
                }
            }
            return Utility.GetSortedData<MarkData>(dest);
        }

        public void CalculateMaxID()
        {
            uint max = 0;
            foreach (MarkData markData in GetAllData(true))
            {
                max = Math.Max(max, markData.ID);
            }
            IDProvider = new IDProvider(max);
        }

        // 選んでいるレイヤーの設定を変える（メインのウィンドウの値を変えたとき）
        public void SetSelectedBpm(float value)
        {
            var layer = SelectedLayer;
            if (layer != null)
            {
                layer.BPM = Texts["LMBPM"] + " " + value;
                layer.PPDData.BPM = value;
            }
        }

        public void SetSelectedBpmOffset(float value)
        {
            SelectedPpdSheet?.BPMStart = value;
        }

        public void SetSelectedDisplayWidth(int value)
        {
            SelectedPpdSheet?.DisplayWidth = value;
        }

        public void SetSelectedDisplayMode(DisplayLineMode value)
        {
            SelectedPpdSheet?.DisplayMode = value;
        }

        public void SetSelectedBeatType(DisplayBeatType value)
        {
            SelectedPpdSheet?.BeatType = value;
        }
    }

    /// <summary>
    /// 1 つのレイヤーです（譜面と、名前、表示するか、マークの色など）。
    /// </summary>
    public partial class LayerViewModel : ObservableObject
    {
        readonly LayerManagerViewModel owner;
        string bpm = "BPM";

        [ObservableProperty]
        private string displayName;

        [ObservableProperty]
        private bool displayVisible = true;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private Color markColor = PPDEditorSkin.Skin.TimeLineMarkColor;

        /// <summary>
        /// 名前を編集しているかです。
        /// </summary>
        [ObservableProperty]
        private bool isEditingName;

        [ObservableProperty]
        private string editingName = "";

        internal LayerViewModel(LayerManagerViewModel owner, string name)
        {
            this.owner = owner;
            displayName = name;
            PPDData.DisplayName = name;
        }

        public PPDSheet PPDData { get; } = new();

        public LanguageTexts Texts
        {
            get { return LanguageTexts.Instance; }
        }

        /// <summary>
        /// BPM の表示です（「BPM 120」など）。
        /// </summary>
        public string BPM
        {
            get { return bpm; }
            set { SetProperty(ref bpm, value); }
        }

        partial void OnDisplayVisibleChanged(bool value)
        {
            owner.Host.RefreshTimeline();
        }

        [RelayCommand]
        private void ToggleVisible()
        {
            DisplayVisible = !DisplayVisible;
        }

        [RelayCommand]
        private void Select()
        {
            owner.Select(this);
        }

        [RelayCommand]
        private void BeginRename()
        {
            EditingName = DisplayName;
            IsEditingName = true;
        }

        /// <summary>
        /// 名前の編集を終えます（空のときは「no text」にする）。
        /// </summary>
        [RelayCommand]
        private void EndRename()
        {
            if (!IsEditingName)
            {
                return;
            }
            DisplayName = EditingName == "" ? "no text" : EditingName;
            PPDData.DisplayName = DisplayName;
            IsEditingName = false;
        }

        [RelayCommand]
        private void Delete()
        {
            owner.Delete(this);
        }

        [RelayCommand]
        private void Duplicate()
        {
            owner.Duplicate(this);
        }

        [RelayCommand]
        private void ChangeColor()
        {
            var color = owner.Host.PickColor(MarkColor);
            if (color.HasValue)
            {
                MarkColor = color.Value;
                owner.Host.RefreshTimeline();
            }
        }
    }
}
