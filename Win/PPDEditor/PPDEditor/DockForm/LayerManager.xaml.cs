using PPDEditor.Command.PPDSheet;
using PPDEditor.Controls;
using PPDEditor.Forms;
using PPDEditor.ViewModels.Dialogs;
using PPDEditor.ViewModels.Docks;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PPDEditor
{
    /// <summary>
    /// レイヤーの一覧です。処理は <see cref="LayerManagerViewModel"/> にあります。
    /// </summary>
    /// <remarks>
    /// ドラッグでの並べ替え（マウスの扱いと、つかんでいるレイヤーの絵）はここで行う。
    /// </remarks>
    public sealed partial class LayerManager : DockView, ILayerHost
    {
        // ドラッグで並べ替えるときの状態
        LayerDisplay dragTarget;
        bool dragging;
        Point mousedownpos;
        int movecount;

        public LayerManager()
        {
            InitializeComponent();
            AttachViewModel(new LayerManagerViewModel(this));
        }

        public LayerManagerViewModel ViewModel
        {
            get { return (LayerManagerViewModel)DataContext; }
        }

        public void Initialize()
        {
            ViewModel.Initialize();
        }

        public void Clear()
        {
            ViewModel.Clear();
        }

        public MarkData[] GetAllData(bool any)
        {
            return ViewModel.GetAllData(any);
        }

        public void AddLayer(string name, float bpm, float bpmoffset, int displaywidth, bool visible, DisplayLineMode displayMode, DisplayBeatType beatType)
        {
            ViewModel.AddLayer(name, bpm, bpmoffset, displaywidth, visible, displayMode, beatType);
        }

        public void SelectLastLayer()
        {
            ViewModel.SelectLastLayer();
        }

        public void SelectLayerByIndex(int index)
        {
            ViewModel.SelectLayerByIndex(index);
        }

        public void CalculateMaxID()
        {
            ViewModel.CalculateMaxID();
        }

        public IDProvider IDProvider
        {
            get { return ViewModel.IDProvider; }
        }

        public PPDSheet SelectedPpdSheet
        {
            get { return ViewModel.SelectedPpdSheet; }
        }

        public int SelectedLayerIndex
        {
            get { return ViewModel.SelectedLayerIndex; }
        }

        public PPDSheet[] Sheets
        {
            get { return ViewModel.Sheets; }
        }

        /// <summary>
        /// すべてのレイヤー（保存する順）です。
        /// </summary>
        public LayerViewModel[] AllLayerDisplay
        {
            get { return ViewModel.Layers; }
        }

        public float BPM
        {
            set { ViewModel.SetSelectedBpm(value); }
        }

        public float BPMOffset
        {
            set { ViewModel.SetSelectedBpmOffset(value); }
        }

        public int DisplayWidth
        {
            set { ViewModel.SetSelectedDisplayWidth(value); }
        }

        public DisplayLineMode DisplayMode
        {
            set { ViewModel.SetSelectedDisplayMode(value); }
        }

        public DisplayBeatType BeatType
        {
            set { ViewModel.SetSelectedBeatType(value); }
        }

        private static LayerDisplay FindLayerDisplay(object source)
        {
            for (var element = source as DependencyObject; element != null; element = VisualTreeHelper.GetParent(element))
            {
                if (element is LayerDisplay layerDisplay)
                {
                    return layerDisplay;
                }
            }
            return null;
        }

        private void Layers_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var layerDisplay = FindLayerDisplay(e.OriginalSource);
            if (layerDisplay == null)
            {
                return;
            }
            dragTarget = layerDisplay;
            dragging = false;
            mousedownpos = e.GetPosition(layersPanel);
            layerDisplay.CaptureMouse();
        }

        private void Layers_MouseMove(object sender, MouseEventArgs e)
        {
            var ld = dragTarget;
            if (ld == null || !ld.IsMouseCaptured)
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
            Canvas.SetTop(dragImage, top + p.Y - mousedownpos.Y);
            Canvas.SetTop(dropBar, ld.ActualHeight * GetDropRow(p, ld.ActualHeight) - dropBar.Height / 2);
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

        private void Layers_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var ld = dragTarget;
            if (ld == null)
            {
                return;
            }
            if (dragging)
            {
                ViewModel.MoveLayer((LayerViewModel)ld.DataContext, GetDropRow(e.GetPosition(layersPanel), ld.ActualHeight));
            }
            ld.ReleaseMouseCapture();
        }

        private void Layers_LostMouseCapture(object sender, MouseEventArgs e)
        {
            dragTarget = null;
            dragging = false;
            dragImage.Visibility = Visibility.Collapsed;
            dragImage.Fill = null;
            dropBar.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 並べている行の間のうち、落とす場所の番号を返す（一番上が 0）。
        /// </summary>
        private int GetDropRow(Point positionInPanel, double rowHeight)
        {
            var count = ViewModel.DisplayLayers.Count;
            if (count == 0 || rowHeight <= 0)
            {
                return 0;
            }
            return Math.Clamp((int)Math.Round(positionInPanel.Y / rowHeight), 0, count);
        }

        bool ILayerHost.Confirm(string text, string caption)
        {
            return MessageBox.Show(Window.GetWindow(this), text, caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }

        string ILayerHost.AskNewLayerName(string defaultName)
        {
            var viewModel = new NewLayerViewModel(defaultName);
            return WpfDialog.ShowDialog(new NewLayerWindow(viewModel)) ? viewModel.LayerName : null;
        }

        System.Drawing.Color? ILayerHost.PickColor(System.Drawing.Color current)
        {
            var dialog = new FlowScriptDrawControl.Dialog.ColorDialog
            {
                CurrentColor = Color.FromRgb(current.R, current.G, current.B)
            };
            if (!WpfDialog.ShowDialog(dialog))
            {
                return null;
            }
            return System.Drawing.Color.FromArgb(dialog.CurrentColor.R, dialog.CurrentColor.G, dialog.CurrentColor.B);
        }

        (float Bpm, float BpmOffset, int DisplayWidth) ILayerHost.CurrentSettings
        {
            get { return (WindowUtility.MainForm.BPM, WindowUtility.MainForm.BPMOffset, WindowUtility.MainForm.DisplayWidth); }
        }

        void ILayerHost.ApplySheetSettings(PPDSheet sheet)
        {
            WindowUtility.Seekmain.SetSelectedSheetInfo(sheet.BPM, sheet.BPMStart, sheet.DisplayWidth, sheet.DisplayMode, sheet.BeatType);
            WindowUtility.MainForm.BPM = sheet.BPM;
            WindowUtility.MainForm.BPMOffset = sheet.BPMStart;
            WindowUtility.MainForm.DisplayWidth = sheet.DisplayWidth;
            WindowUtility.MainForm.DisplayMode = sheet.DisplayMode;
            WindowUtility.MainForm.BeatType = sheet.BeatType;
        }

        void ILayerHost.RefreshTimeline()
        {
            WindowUtility.Seekmain?.DrawAndRefresh();
        }

        void ILayerHost.OnSheetChanged()
        {
            WindowUtility.TimeLineForm?.ContentChanged();
        }
    }
}
