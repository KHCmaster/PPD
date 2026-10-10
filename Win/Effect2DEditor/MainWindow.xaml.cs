using AvalonDock.Layout;
using AvalonDock.Serializer.Xml;
using BezierCaliculator;
using Effect2DEditor.ViewModels;
using Effect2DEditor.ViewModels.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Effect2DEditor
{
    /// <summary>
    /// エフェクトのエディターのメインのウィンドウです。処理は <see cref="MainWindowViewModel"/> にあります。
    /// </summary>
    /// <remarks>
    /// ここでは、ドックのウィンドウ（AvalonDock）の配置と、ダイアログやファイルの選択、カンバスとタイムラインの描画の指示を行う。
    /// </remarks>
    public sealed partial class MainWindow : Window, IMainWindowView
    {
        const string dockFileName = "dockinfo.xml";

        public MainWindowViewModel ViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel(this);
            DataContext = ViewModel;
            // ドックの中身は AvalonDock の中に置かれ、ウィンドウの DataContext を受け継がないので、それぞれに設定する
            canvasDock.DataContext = ViewModel;
            timeLineDock.DataContext = ViewModel;
            propertyDock.DataContext = ViewModel.Property;
            historyDock.CommandManager = ViewModel.CommandManager;
            canvasDock.BezierEdited += (sender, e) => ViewModel.OnBezierEdited();

            var settings = ViewModel.Settings;
            if (settings.WindowWidth.HasValue) Width = settings.WindowWidth.Value;
            if (settings.WindowHeight.HasValue) Height = settings.WindowHeight.Value;
            if (settings.Maximized) WindowState = WindowState.Maximized;
            // カンバスは、ドックの配置を読み込む前に作る（以前と同じ順番）
            canvasDock.Initialize(settings.CanvasWidth, settings.CanvasHeight);

            // 配置を読み込むとウィンドウのタイトルも読み込まれるので、言語はその後に設定する
            RestoreDock();
            LanguageTexts.Instance.PropertyChanged += (sender, e) => UpdateDockTitles();
            ViewModel.Start();
            UpdateDockTitles();
        }

        #region ドック

        /// <summary>
        /// ウィンドウの配置を読み込みます。以前の WinForms 版の形式など、読み込めないときは既定の配置のままにします。
        /// </summary>
        private void RestoreDock()
        {
            if (!File.Exists(dockFileName))
            {
                return;
            }
            try
            {
                var serializer = new XmlLayoutSerializer(dockingManager);
                serializer.LayoutSerializationCallback += (sender, e) =>
                {
                    e.Content = e.Model.ContentId switch
                    {
                        "Effect2DEditor.DockForm.CanvasDock" => canvasDock,
                        "Effect2DEditor.DockForm.PropertyDock" => propertyDock,
                        "Effect2DEditor.DockForm.HistoryDock" => historyDock,
                        "Effect2DEditor.DockForm.TimeLineDock" => timeLineDock,
                        _ => null,
                    };
                    e.Cancel = e.Content == null;
                };
                serializer.Deserialize(dockFileName);
            }
            catch (Exception)
            {
            }
            RebindAnchorables();
        }

        /// <summary>
        /// 配置を読み込むと、ウィンドウ（LayoutAnchorable）は新しく作られるので、「表示」メニューで扱うものを付け替えます。
        /// 読み込んだ配置になかったウィンドウは、配置に加え直します。
        /// </summary>
        private void RebindAnchorables()
        {
            var anchorables = dockingManager.Layout.Descendents().OfType<LayoutAnchorable>()
                .Concat(dockingManager.Layout.Hidden)
                .Distinct()
                .ToList();
            canvasAnchorable = Rebind(anchorables, canvasAnchorable);
            timeLineAnchorable = Rebind(anchorables, timeLineAnchorable);
            historyAnchorable = Rebind(anchorables, historyAnchorable);
            propertyAnchorable = Rebind(anchorables, propertyAnchorable);
        }

        private LayoutAnchorable Rebind(List<LayoutAnchorable> anchorables, LayoutAnchorable original)
        {
            var found = anchorables.FirstOrDefault(a => a.ContentId == original.ContentId);
            if (found != null)
            {
                return found;
            }
            if (original.Root == null)
            {
                original.AddToLayout(dockingManager, AnchorableShowStrategy.Most);
            }
            return original;
        }

        private void SaveDock()
        {
            try
            {
                new XmlLayoutSerializer(dockingManager).Serialize(dockFileName);
            }
            catch (IOException)
            {
            }
        }

        private void UpdateDockTitles()
        {
            var texts = LanguageTexts.Instance;
            canvasAnchorable.Title = texts["CanvasWindow"];
            propertyAnchorable.Title = texts["PropertyWindow"];
            historyAnchorable.Title = texts["HistoryWindow"];
            timeLineAnchorable.Title = texts["TimeLineWindow"];
        }

        private LayoutAnchorable GetAnchorable(DockKind kind)
        {
            return kind switch
            {
                DockKind.Canvas => canvasAnchorable,
                DockKind.Property => propertyAnchorable,
                DockKind.History => historyAnchorable,
                _ => timeLineAnchorable,
            };
        }

        #endregion

        #region ウィンドウ

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var bounds = WindowState == WindowState.Normal ? new Rect(Left, Top, Width, Height) : RestoreBounds;
            if (!ViewModel.OnClosing(bounds, WindowState == WindowState.Maximized))
            {
                e.Cancel = true;
                return;
            }
            SaveDock();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ViewModel.OnClosed();
            canvasDock.Close();
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop, false) is string[] fileNames)
            {
                ViewModel.OpenDroppedFile(fileNames[0]);
            }
        }

        #endregion

        #region IMainWindowView

        void IMainWindowView.ShowMessage(string text)
        {
            MessageBox.Show(this, text);
        }

        bool IMainWindowView.Confirm(string text, string caption)
        {
            return MessageBox.Show(this, text, caption, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }

        bool? IMainWindowView.AskSave(string text, string caption)
        {
            return MessageBox.Show(this, text, caption, MessageBoxButton.YesNoCancel) switch
            {
                MessageBoxResult.Yes => true,
                MessageBoxResult.No => false,
                _ => null,
            };
        }

        string[] IMainWindowView.OpenFiles(string filter, bool multiselect, string fileName)
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter,
                Multiselect = multiselect
            };
            SetFileName(dialog, fileName);
            return dialog.ShowDialog(this) == true ? dialog.FileNames : [];
        }

        string IMainWindowView.SaveFile(string filter, string defaultExt, string fileName)
        {
            var dialog = new SaveFileDialog
            {
                Filter = filter,
                DefaultExt = defaultExt
            };
            SetFileName(dialog, fileName);
            return dialog.ShowDialog(this) == true ? dialog.FileName : null;
        }

        /// <summary>
        /// 最初に選んでおくファイルを設定します。フォルダも含むパスのときは、そのフォルダを開く。
        /// </summary>
        private static void SetFileName(FileDialog dialog, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }
            dialog.FileName = Path.GetFileName(fileName);
            if (Path.IsPathRooted(fileName))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(fileName);
            }
        }

        bool IMainWindowView.ShowDialog(DialogViewModel viewModel)
        {
            Window window = viewModel switch
            {
                AskFrameCountViewModel => new AskFrameCountWindow(),
                CanvasSizeViewModel => new CanvasSizeWindow(),
                ChangeBackGroundViewModel => new ChangeBackGroundWindow(),
                RatioChangeViewModel => new RatioChangeWindow(),
                _ => throw new ArgumentException("Unknown dialog view model: " + viewModel.GetType(), nameof(viewModel)),
            };
            window.Owner = this;
            window.DataContext = viewModel;
            viewModel.CloseRequested += () => window.DialogResult = true;
            return window.ShowDialog() == true;
        }

        void IMainWindowView.ToggleDock(DockKind kind)
        {
            Utility.ShowOrHideWindow(GetAnchorable(kind));
        }

        void IMainWindowView.InitializeCanvas(int width, int height)
        {
            canvasDock.Initialize(width, height);
        }

        void IMainWindowView.RefreshCanvas()
        {
            canvasDock.RefreshCanvas();
        }

        void IMainWindowView.RefreshTimeLine()
        {
            timeLineDock.DrawAndRefresh();
        }

        void IMainWindowView.AdjustTimeLineHScroll()
        {
            timeLineDock.AdjustHScroll();
        }

        void IMainWindowView.MoveTimeLineVScroll(int line)
        {
            timeLineDock.MoveVScroll(line);
        }

        int IMainWindowView.CurrentFrame
        {
            get { return timeLineDock.CurrentFrame; }
        }

        void IMainWindowView.AttachEffect()
        {
            timeLineDock.Attach(ViewModel);
        }

        BezierControlPoint[] IMainWindowView.CanvasBezier
        {
            get { return canvasDock.BezierControl.Controller.BCPS; }
        }

        void IMainWindowView.SetCanvasBezier(BezierControlPoint[] bcps, bool allowEdit)
        {
            canvasDock.BezierControl.Controller.BCPS = bcps;
            canvasDock.BezierControl.Controller.AllowMouseOperation = allowEdit;
        }

        #endregion
    }
}
