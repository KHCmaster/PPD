using PPDEditor.ViewModels.Dialogs;
using PPDEditor.Command.PPDSheet;
using PPDEditor.Controls;
using PPDEditor.Forms;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PPDEditor
{
    public partial class LayerManager : DockView
    {
        string BPMtext = "BPM";
        string BPMOffsettext = "BPM offset";
        string Displaywidthtext = "タイムラインの幅";
        string layer = "レイヤー";
        string copy = " のコピー";
        LayerDisplay lastld;
        float lastBPM;
        float lastBPMOffset;
        int lastDisplayWidth;
        DisplayLineMode lastDisplayMode;
        DisplayBeatType lastBeatType;

        // ドラッグで並べ替えるときの状態
        bool mousedown;
        bool dragging;
        Point mousedownpos;
        int movecount;

        readonly List<LayerDisplay> layers;

        IDProvider idProvider;

        public LayerManager()
        {
            InitializeComponent();

            idProvider = new IDProvider(0);
            layers = [];
        }
        public void Initialize()
        {
            var ld = CreateLayerDisplay(layer + 0);
            layers.Add(ld);
            ld.Selected = true;
            Relocate();
        }
        public void SetLang()
        {
            BPMtext = Utility.Language["LMBPM"];
            BPMOffsettext = Utility.Language["LMBPMOffset"];
            Displaywidthtext = Utility.Language["LMDisplayWidth"];
            layer = Utility.Language["LMLayer"];
            copy = Utility.Language["LMCopy"];
            this.新規レイヤーMenuItem.Header = Utility.Language["LMMenu1"];
        }
        private void 新規レイヤーMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = new NewLayerViewModel(layer + layers.Count);
            if (WpfDialog.ShowDialog(new NewLayerWindow(viewModel)))
            {
                var ld = CreateLayerDisplay(viewModel.LayerName);
                AddLayerToPanel(ld);
                ContentChanged();
            }
        }

        private void Relocate()
        {
            // 後ろのレイヤーほど上に並べる
            layersPanel.Children.Clear();
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                layersPanel.Children.Add(layers[i]);
            }
        }

        private void AddLayerToPanel(LayerDisplay ld)
        {
            layers.Add(ld);
            Relocate();
        }

        private LayerDisplay CreateLayerDisplay(string layername)
        {
            return CreateLayerDisplay(layername,
                WindowUtility.MainForm.BPM,
                WindowUtility.MainForm.BPMOffset,
                WindowUtility.MainForm.DisplayWidth, DisplayLineMode.Fourth, DisplayBeatType.Fourth);
        }

        private LayerDisplay CreateLayerDisplay(string layername, float bpm, float bpmoffset, int displaywidth, DisplayLineMode displayMode, DisplayBeatType beatType)
        {
            var ld = new LayerDisplay();
            ld.SetLang();
            ld.DisplayName = layername;
            ld.BPM = BPMtext + " " + bpm;
            ld.PPDData.BPM = bpm;
            ld.PPDData.BPMStart = bpmoffset;
            ld.PPDData.DisplayWidth = displaywidth;
            ld.PPDData.DisplayName = layername;
            ld.PPDData.DisplayMode = displayMode;
            ld.PPDData.BeatType = beatType;
            ld.MouseLeftButtonDown += Ld_MouseLeftButtonDown;
            ld.MouseMove += Ld_MouseMove;
            ld.MouseLeftButtonUp += Ld_MouseLeftButtonUp;
            ld.LostMouseCapture += Ld_LostMouseCapture;
            ld.SelectStateChanged += Ld_SelectStateChanged;
            ld.Deleted += Ld_Deleted;
            ld.Duplicated += Ld_Duplicated;
            ld.VisibleStateChanged += Ld_VisibleStateChanged;
            ld.PPDData.DisplayDataChanged += PPDData_DisplayDataChanged;
            ld.PPDData.CommandChanged += PPDData_CommandChanged;
            return ld;
        }

        void PPDData_CommandChanged(object sender, EventArgs e)
        {
            WindowUtility.TimeLineForm?.ContentChanged();
        }

        void Ld_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var ld = (LayerDisplay)sender;
            mousedown = true;
            dragging = false;
            mousedownpos = e.GetPosition(layersPanel);
            ld.CaptureMouse();
        }

        void Ld_MouseMove(object sender, MouseEventArgs e)
        {
            var ld = (LayerDisplay)sender;
            if (!mousedown || !ld.IsMouseCaptured)
            {
                return;
            }
            var p = e.GetPosition(layersPanel);
            if (!dragging)
            {
                if (Math.Abs(mousedownpos.X - p.X) < 5 && Math.Abs(mousedownpos.Y - p.Y) < 5)
                {
                    return;
                }
                dragging = true;
                movecount = 0;
                dragImage.Width = ld.ActualWidth;
                dragImage.Height = ld.ActualHeight;
                dragImage.Fill = new VisualBrush(ld) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top };
                dragImage.Visibility = Visibility.Visible;
                dropBar.Width = layersPanel.ActualWidth;
                dropBar.Visibility = Visibility.Visible;
            }
            var top = ld.TranslatePoint(new Point(), layersPanel).Y;
            System.Windows.Controls.Canvas.SetTop(dragImage, top + p.Y - mousedownpos.Y);
            System.Windows.Controls.Canvas.SetTop(dropBar, ld.ActualHeight * GetDropRow(p) - dropBar.Height / 2);
            if (movecount % 10 == 0)
            {
                // 見えているところの外までドラッグしたら、そちらへスクロールする
                var inViewer = e.GetPosition(scrollViewer);
                if (inViewer.Y < 0)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - ld.ActualHeight);
                }
                else if (inViewer.Y > scrollViewer.ViewportHeight)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + ld.ActualHeight);
                }
            }
            movecount++;
        }

        void Ld_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var ld = (LayerDisplay)sender;
            if (mousedown && dragging)
            {
                var index = layers.Count - GetDropRow(e.GetPosition(layersPanel));
                var selectedIndex = layers.IndexOf(ld);
                if (selectedIndex < index)
                {
                    index--;
                }
                if (selectedIndex != index)
                {
                    layers.RemoveAt(selectedIndex);
                    layers.Insert(index, ld);
                    Relocate();
                }
            }
            ld.ReleaseMouseCapture();
        }

        void Ld_LostMouseCapture(object sender, MouseEventArgs e)
        {
            mousedown = false;
            dragging = false;
            dragImage.Visibility = Visibility.Collapsed;
            dragImage.Fill = null;
            dropBar.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 並べている行の間のうち、落とす場所の番号を返す（一番上が 0）。
        /// </summary>
        private int GetDropRow(Point positionInPanel)
        {
            if (layers.Count == 0)
            {
                return 0;
            }
            var height = layers[0].ActualHeight;
            var row = (int)Math.Round(positionInPanel.Y / height);
            return Math.Clamp(row, 0, layers.Count);
        }

        void PPDData_DisplayDataChanged(object sender, EventArgs e)
        {
            ChangeDisplayData();
        }
        private void ChangeDisplayData()
        {
            LayerDisplay sld = SelectedLayer;
            if (sld != null && (sld != lastld || sld.PPDData.BPM != lastBPM || sld.PPDData.BPMStart != lastBPMOffset
                || sld.PPDData.DisplayWidth != lastDisplayWidth || sld.PPDData.BeatType != lastBeatType))
            {
                WindowUtility.Seekmain.SetSelectedSheetInfo(sld.PPDData.BPM, sld.PPDData.BPMStart, sld.PPDData.DisplayWidth,
                    sld.PPDData.DisplayMode, sld.PPDData.BeatType);
                lastld = sld;
                lastBPM = sld.PPDData.BPM;
                lastBPMOffset = sld.PPDData.BPMStart;
                lastDisplayWidth = sld.PPDData.DisplayWidth;
                lastDisplayMode = sld.PPDData.DisplayMode;
                lastBeatType = sld.PPDData.BeatType;
                WindowUtility.MainForm.BPM = lastBPM;
                WindowUtility.MainForm.BPMOffset = lastBPMOffset;
                WindowUtility.MainForm.DisplayWidth = lastDisplayWidth;
                WindowUtility.MainForm.DisplayMode = lastDisplayMode;
                WindowUtility.MainForm.BeatType = lastBeatType;
            }
        }
        void Ld_VisibleStateChanged(object sender, EventArgs e)
        {
            WindowUtility.Seekmain.DrawAndRefresh();
        }

        void Ld_Duplicated(object sender, EventArgs e)
        {
            if (sender is LayerDisplay senderld)
            {
                var index = layers.IndexOf(senderld);
                var ld = CreateLayerDisplay(senderld.DisplayName + copy);
                layers.Insert(index + 1, ld);
                Relocate();
                WindowUtility.Seekmain.DrawAndRefresh();
                SortedList<float, Mark>[] source = senderld.PPDData.Data;
                for (int i = 0; i < source.Length; i++)
                {
                    foreach (Mark mk in source[i].Values)
                    {
                        if (mk is ExMark exmk)
                        {
                            ld.PPDData.AddExMark(exmk.Time, exmk.EndTime, exmk.Position.X, exmk.Position.Y, exmk.Rotation, (int)exmk.Type, exmk.ID, [.. exmk.Parameters]);
                        }
                        else
                        {
                            ld.PPDData.AddMark(mk.Time, mk.Position.X, mk.Position.Y, mk.Rotation, (int)mk.Type, mk.ID, [.. mk.Parameters]);
                        }
                    }
                }
                ld.PPDData.BPM = senderld.PPDData.BPM;
                ld.PPDData.BPMStart = senderld.PPDData.BPMStart;
                ld.PPDData.DisplayWidth = senderld.PPDData.DisplayWidth;
                ld.PPDData.DisplayMode = senderld.PPDData.DisplayMode;
                ld.PPDData.BeatType = senderld.PPDData.BeatType;
                ld.BPM = senderld.BPM;
                ld.MarkColor = senderld.MarkColor;
                ContentChanged();
            }
        }

        void Ld_Deleted(object sender, EventArgs e)
        {
            var senderld = sender as LayerDisplay;
            var index = layers.IndexOf(senderld);
            layers.Remove(senderld);
            Relocate();
            index--;
            if (index >= layers.Count) index = layers.Count - 1;
            if (index < 0) index = 0;
            if (layers.Count != 0)
            {
                LayerDisplay ld = layers[index];
                ld?.SelectedWithEvent = true;
            }
            WindowUtility.Seekmain.DrawAndRefresh();
            ContentChanged();
        }

        void Ld_SelectStateChanged(object sender, EventArgs e)
        {
            var senderld = sender as LayerDisplay;
            foreach (LayerDisplay layer in layers)
            {
                if (senderld != layer)
                {
                    layer.Selected = false;
                }
            }
            ChangeDisplayData();
        }
        private LayerDisplay SelectedLayer
        {
            get
            {
                return layers.FirstOrDefault(l => l.Selected);
            }
        }
        public void Clear()
        {
            idProvider = new IDProvider(0);
            layers.Clear();
        }
        public MarkData[] GetAllData(bool any)
        {
            SortedList<float, MarkData>[] dest = new SortedList<float, MarkData>[10];
            for (int i = 0; i < dest.Length; i++)
            {
                dest[i] = [];
            }

            foreach (var layer in layers)
            {
                if (layer.DisplayVisible || any)
                {
                    SortedList<float, Mark>[] source = layer.PPDData.Data;
                    for (int j = 0; j < source.Length; j++)
                    {
                        foreach (Mark mk in source[j].Values)
                        {
                            if (!dest[j].ContainsKey(mk.Time))
                            {
                                if (mk is ExMark exmk)
                                {
                                    dest[j].Add(exmk.Time, exmk.Convert());
                                }
                                else
                                {
                                    dest[j].Add(mk.Time, mk.Convert());
                                }
                            }
                        }
                    }
                }
            }
            return Utility.GetSortedData<MarkData>(dest);
        }

        public void AddLayer(string name, float bpm, float bpmoffset, int displaywidth, bool visible, DisplayLineMode displayMode, DisplayBeatType beatType)
        {
            var ld = CreateLayerDisplay(name, bpm, bpmoffset, displaywidth, displayMode, beatType);
            ld.DisplayVisible = visible;
            AddLayerToPanel(ld);
        }

        public void SelectLastLayer()
        {
            if (layers.Count > 0)
            {
                layers[^1].SelectedWithEvent = true;
            }
        }

        public void SelectLayerByIndex(int index)
        {
            if (index < 0 || index >= layers.Count)
            {
                return;
            }
            layers[index].SelectedWithEvent = true;
        }

        public void CalculateMaxID()
        {
            uint max = 0;
            foreach (MarkData markData in GetAllData(true))
            {
                if (max < markData.ID)
                {
                    max = markData.ID;
                }
            }
            idProvider = new IDProvider(max);
        }
        public IDProvider IDProvider
        {
            get
            {
                return idProvider;
            }
        }
        public PPDSheet SelectedPpdSheet
        {
            get
            {
                LayerDisplay sld = SelectedLayer;
                if (sld != null)
                {
                    return sld.PPDData;
                }
                return null;
            }
        }
        public int SelectedLayerIndex
        {
            get
            {
                LayerDisplay sld = SelectedLayer;
                if (sld == null)
                {
                    return -1;
                }
                return layers.IndexOf(sld);
            }
        }
        public PPDSheet[] Sheets
        {
            get
            {
                return [.. layers.Where(l => l.DisplayVisible).Select(l => l.PPDData)];
            }
        }
        public LayerDisplay[] AllLayerDisplay
        {
            get
            {
                return [.. layers];
            }
        }
        public float BPM
        {
            set
            {
                LayerDisplay sld = SelectedLayer;
                if (sld != null)
                {
                    sld.BPM = BPMtext + " " + value;
                    sld.PPDData.BPM = value;
                }
            }
        }
        public float BPMOffset
        {
            set
            {
                LayerDisplay sld = SelectedLayer;
                sld?.PPDData.BPMStart = value;
            }
        }
        public int DisplayWidth
        {
            set
            {
                LayerDisplay sld = SelectedLayer;
                sld?.PPDData.DisplayWidth = value;
            }
        }
        public DisplayLineMode DisplayMode
        {
            set
            {
                LayerDisplay sld = SelectedLayer;
                sld?.PPDData.DisplayMode = value;
            }
        }
        public DisplayBeatType BeatType
        {
            set
            {
                LayerDisplay sld = SelectedLayer;
                sld?.PPDData.BeatType = value;
            }
        }
    }
}
