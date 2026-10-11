using AvalonDock.Layout;
using AvalonDock.Serializer.Xml;
using PPDEditor.Controls;
using PPDEditor.DockForm;
using PPDEditor.ViewModels;
using PPDEditor.ViewModels.Dialogs;
using PPDEditorCommon.Dialog;
using PPDEditorCommon.Dialog.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PPDEditor.Forms
{
    /// <summary>
    /// エディターのメインのウィンドウです。処理は <see cref="MainWindowViewModel"/> にあります。
    /// </summary>
    /// <remarks>
    /// ここでは、ドックのウィンドウ（AvalonDock）の作成と配置、スキンの背景、ゲームの画面の操作、ダイアログの表示を行う。
    /// </remarks>
    public sealed partial class EditorForm : Window, IMainWindowView, IDisposable
    {
        const string dockxmlname = "dockforminfo.xml";
        const string scriptdockxmlname = "scriptdockforminfo.xml";

        /// <summary>
        /// 配置のファイルでのドックのウィンドウの名前です（以前のクラスの名前。配置のファイルとの互換のため）。
        /// </summary>
        static readonly Dictionary<DockKind, string> contentIds = new()
        {
            [DockKind.GameWindow] = "PPDEditor.DXForm",
            [DockKind.TimeLine] = "PPDEditor.TimeLineForm",
            [DockKind.Property] = "PPDEditor.InfoForm",
            [DockKind.PosAndAngle] = "PPDEditor.PosAndAngleLoaderSaver",
            [DockKind.Kasi] = "PPDEditor.KasiEditor",
            [DockKind.SoundManager] = "PPDEditor.SoundManager",
            [DockKind.IniFileWriter] = "PPDEditor.IniFileWriter",
            [DockKind.BPMMeasure] = "PPDEditor.BPMMeasure",
            [DockKind.LayerManager] = "PPDEditor.LayerManager",
            [DockKind.EventManager] = "PPDEditor.EventManager",
            [DockKind.GeometryCreator] = "PPDEditor.GeometryCreator",
            [DockKind.Memo] = "PPDEditor.MemoWindow",
            [DockKind.ResourceManager] = "PPDEditor.ResourceManager",
            [DockKind.ScriptManager] = "PPDEditor.ScriptManager",
            [DockKind.StatsManager] = "PPDEditor.StatsManager",
            [DockKind.Help] = "PPDEditor.HelpForm",
        };

        // 配置を読み込むと作り直されるので、そのたびに付け替える
        readonly Dictionary<DockKind, LayoutAnchorable> docks = [];

        readonly Microsoft.Win32.OpenFileDialog openFileDialog = new() { RestoreDirectory = true };
        readonly Microsoft.Win32.SaveFileDialog saveFileDialog = new() { RestoreDirectory = true };
        // ゲームの画面でマウスを押している間、マークを動かす（以前と同じく 100 ミリ秒ごと）
        readonly DispatcherTimer moveMarkTimer = new() { Interval = TimeSpan.FromMilliseconds(100) };
        // エラーで止まって閉じるときは、保存していない変更などを確かめない
        bool forceClosing;
        readonly WriteDoneWindow saveDoneWindow = new();

        /// <summary>
        /// ゲームの画面です。
        /// </summary>
        public GameView GameView
        {
            get;
            private set;
        }

        public MainWindowViewModel ViewModel
        {
            get;
        }

        public EditorForm()
        {
            InitializeComponent();
            // ゲームの画面は、DirectX の初期化（Game のコンストラクタ）より先に作る
            GameView = new GameView();
            GameView.GameMouseDown += GameView_GameMouseDown;
            GameView.GameMouseUp += GameView_GameMouseUp;
            GameView.GameMouseMove += GameView_GameMouseMove;
            GameView.GameMouseLeave += GameView_GameMouseLeave;
            GameView.GameKeyDown += GameView_GameKeyDown;
            moveMarkTimer.Tick += Movemark;
            SizeChanged += (sender, e) => ChangeBackGround();
            ViewModel = new MainWindowViewModel(this);
            DataContext = ViewModel;
        }

        #region Initialize
        public void Initialize()
        {
            var pal = CreateDock(DockKind.PosAndAngle, "PAALS", () => new PosAndAngleLoaderSaver());
            var ke = CreateDock(DockKind.Kasi, "KE", () => new KasiEditor());
            var sm = CreateDock(DockKind.SoundManager, "SM", () => new SoundManager());
            var ifw = CreateDock(DockKind.IniFileWriter, "SFW", () => new IniFileWriter());
            var bm = CreateDock(DockKind.BPMMeasure, "BPMMeasure", () => new BPMMeasure());
            var tlf = CreateDock(DockKind.TimeLine, "TimeLine", () => new TimeLineForm());
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["GameWindow"]));
            docks[DockKind.GameWindow] = new LayoutAnchorable
            {
                ContentId = contentIds[DockKind.GameWindow],
                Content = GameView,
                Title = "ゲームウィンドウ",
                CanClose = false
            };
            var infof = CreateDock(DockKind.Property, "Property", () => new InfoForm());
            var lm = CreateDock(DockKind.LayerManager, "LayerManager", () => new LayerManager());
            var em = CreateDock(DockKind.EventManager, "EventManager", () => new EventManager());
            var gc = CreateDock(DockKind.GeometryCreator, "GeometryCreator", () => new GeometryCreator());
            var mw = CreateDock(DockKind.Memo, "Memo", () => new MemoWindow());
            var hf = CreateDock(DockKind.Help, "Help", () => new HelpForm());
            var rm = CreateDock(DockKind.ResourceManager, "ResourceManager", () => new ResourceManager());
            var scm = CreateDock(DockKind.ScriptManager, "ScriptManager", () => new ScriptManager());
            var stm = CreateDock(DockKind.StatsManager, "StatsManager", () => new StatsManager());

            WindowUtility.MainForm = this;
            WindowUtility.Editor = ViewModel;
            WindowUtility.BPMMeasure = bm;
            WindowUtility.EventManager = em;
            WindowUtility.InfoForm = infof;
            WindowUtility.IniFileWriter = ifw;
            WindowUtility.KasiEditor = ke;
            WindowUtility.LayerManager = lm;
            WindowUtility.PosAndAngleLoaderSaver = pal;
            WindowUtility.Seekmain = tlf.Seekmain;
            WindowUtility.SoundManager = sm;
            WindowUtility.TimeLineForm = tlf;
            WindowUtility.GeometryCreator = gc;
            WindowUtility.DockPanel = dockPanel1;
            WindowUtility.MemoWindow = mw;
            WindowUtility.HelpForm = hf;
            WindowUtility.ResourceManager = rm;
            WindowUtility.ScriptManager = scm;
            WindowUtility.StatsManager = stm;

            em.SetEvent();
            sm.SetEvent();
            stm.SetEvent();

            // ドックのウィンドウの名前は、言語を変えたら付け直す
            LanguageTexts.Instance.PropertyChanged += (sender, e) => UpdateDockTitles();
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["Setting"]));
            LoadWindowPlacement();
            ViewModel.LoadSettings();

            if (File.Exists("skin.ini"))
            {
                PPDEditorSkin.Skin.Initialize("skin.ini");
            }

            lm.Initialize();
            ViewModel.InitializeToolbar();

            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["Skin"]));
            if (File.Exists("skin.ini"))
            {
                ChangeBackGround();
            }

            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["DockWindow"]));
            InitializeDockWindow(scm);
            ViewModel.Start();
        }

        /// <summary>
        /// ドックのウィンドウを作ります。
        /// </summary>
        private T CreateDock<T>(DockKind kind, string nameKey, Func<T> create) where T : DockView
        {
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language[nameKey]));
            var view = create();
            docks[kind] = new LayoutAnchorable
            {
                ContentId = contentIds[kind],
                Content = view,
                Title = view.TabText,
                CanClose = false
            };
            view.TabTextChanged += (sender, e) => docks[kind].Title = view.TabText;
            view.ShowRequested += (sender, e) =>
            {
                docks[kind].Show();
                docks[kind].IsActive = true;
            };
            return view;
        }

        private void UpdateDockTitles()
        {
            foreach (var dock in ViewModel.Docks)
            {
                var anchorable = docks[dock.Kind];
                if (anchorable.Content is DockView view)
                {
                    view.TabText = dock.Text;
                }
                else
                {
                    anchorable.Title = dock.Text;
                }
            }
        }

        private void LoadWindowPlacement()
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Width = PPDStaticSetting.LastWindowSize.Width;
            Height = PPDStaticSetting.LastWindowSize.Height;
            Left = PPDStaticSetting.LastWindowLocation.X;
            Top = PPDStaticSetting.LastWindowLocation.Y;
            WindowState = PPDStaticSetting.LastWindowState;
        }

        private void InitializeDockWindow(ScriptManager scm)
        {
            var path = Path.Combine(Utility.AppDir, dockxmlname);
            if (!File.Exists(path) || !ReadDockXML(path))
            {
                CreateDefaultLayout();
            }
            SetDocksFixed(ViewModel.FixDockPanel);
            scm.RestoreDock(Path.Combine(Utility.AppDir, scriptdockxmlname));
            var textBrush = WpfSkin.TransparentTextBrush;
            timeLabel.Foreground = textBrush;
            areaSelectionLabel.Foreground = textBrush;
            copyBufferLabel.Foreground = textBrush;
            fileMenu.Foreground = textBrush;
            editMenu.Foreground = textBrush;
            movieMenu.Foreground = textBrush;
            showMenu.Foreground = textBrush;
            WindowUtility.InfoForm.SetSkin();
            WindowUtility.IniFileWriter.SetSkin();
            WindowUtility.KasiEditor.SetSkin();
            WindowUtility.PosAndAngleLoaderSaver.SetSkin();
            WindowUtility.SoundManager.SetSkin();
            WindowUtility.BPMMeasure.SetSkin();
            WindowUtility.TimeLineForm.SetSkin();
            WindowUtility.GeometryCreator.SetSkin();
        }

        /// <summary>
        /// ドックの配置を読み込みます。AvalonDock の形式と、以前の WinForms 版（DockPanel Suite）の形式を読めます。
        /// </summary>
        /// <returns>読み込めたかどうか。</returns>
        private bool ReadDockXML(string path)
        {
            if (DockLayoutImporter.IsDockPanelSuiteLayout(path))
            {
                var imported = DockLayoutImporter.Import(path, dockPanel1, GetAnchorable, CreateDefaultLayout);
                RebindAnchorables();
                return imported;
            }
            try
            {
                var serializer = new XmlLayoutSerializer(dockPanel1);
                serializer.LayoutSerializationCallback += (sender, e) =>
                {
                    e.Content = GetAnchorable(e.Model.ContentId)?.Content;
                    e.Cancel = e.Content == null;
                };
                serializer.Deserialize(path);
                RebindAnchorables();
                return true;
            }
            catch (Exception)
            {
                CreateDefaultLayout();
                return false;
            }
        }

        /// <summary>
        /// 既定の配置にします（ゲームの画面を真ん中、プロパティを右、タイムラインを下）。
        /// </summary>
        private void CreateDefaultLayout()
        {
            foreach (var anchorable in docks.Values)
            {
                anchorable.Parent?.RemoveChild(anchorable);
            }
            var documentPane = new LayoutDocumentPane(docks[DockKind.GameWindow]);
            var timeLinePane = new LayoutAnchorablePane(docks[DockKind.TimeLine]) { DockHeight = new GridLength(300) };
            var propertyPane = new LayoutAnchorablePane(docks[DockKind.Property]) { DockWidth = new GridLength(330) };
            var center = new LayoutPanel { Orientation = Orientation.Vertical };
            center.Children.Add(new LayoutDocumentPaneGroup(documentPane));
            center.Children.Add(timeLinePane);
            var root = new LayoutPanel { Orientation = Orientation.Horizontal };
            root.Children.Add(center);
            root.Children.Add(propertyPane);
            dockPanel1.Layout = new LayoutRoot { RootPanel = root };
            // ほかのウィンドウは右に置いて隠しておく（表示のメニューで出す）
            foreach (var anchorable in docks.Values.Where(a => a.Root == null))
            {
                anchorable.AddToLayout(dockPanel1, AnchorableShowStrategy.Right);
                anchorable.Hide();
            }
        }

        /// <summary>
        /// 配置を読み込むと、ウィンドウ（LayoutAnchorable）は新しく作られるので、持っているものを付け替えます。
        /// 読み込んだ配置になかったウィンドウは、配置に加えて隠しておきます。
        /// </summary>
        private void RebindAnchorables()
        {
            var anchorables = dockPanel1.Layout.Descendents().OfType<LayoutAnchorable>()
                .Concat(dockPanel1.Layout.Hidden)
                .Distinct()
                .ToList();
            foreach (var kind in docks.Keys.ToArray())
            {
                var original = docks[kind];
                var found = anchorables.FirstOrDefault(a => a.ContentId == original.ContentId);
                if (found != null)
                {
                    found.Title = original.Title;
                    found.CanClose = false;
                    docks[kind] = found;
                }
                else if (original.Root == null)
                {
                    original.AddToLayout(dockPanel1, AnchorableShowStrategy.Right);
                    original.Hide();
                }
            }
            SetDocksFixed(ViewModel.FixDockPanel);
        }

        /// <summary>
        /// 配置のファイルの名前（以前のクラスの名前）から、ドックのウィンドウを返す。
        /// </summary>
        private LayoutAnchorable GetAnchorable(string persistentString)
        {
            foreach (var pair in contentIds)
            {
                if (pair.Value == persistentString)
                {
                    return docks[pair.Key];
                }
            }
            return null;
        }

        private void ShowMenu_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            ViewModel.RefreshDockStates();
        }
        #endregion

        #region BackGround
        /// <summary>
        /// スキンの背景を表示します。メニューと状態の表示のところには、スキンの重ねる色を塗る。
        /// </summary>
        private void ChangeBackGround()
        {
            var original = PPDEditorSkin.Skin.BackGround;
            if (original == null)
            {
                return;
            }
            if (backgroundImage.Source == null)
            {
                backgroundImage.Source = ToBitmapSource(original);
                backgroundFill.Fill = WpfSkin.ToBrush(PPDEditorSkin.Skin.FillColor);
                topOverlay.Fill = bottomOverlay.Fill = WpfSkin.ToBrush(PPDEditorSkin.Skin.OverLayColor);
            }
            // 画像は高さに合わせて、スキンの指定の位置に置く
            switch (PPDEditorSkin.Skin.DisplayMode)
            {
                case PPDEditorSkin.BackGroundDisplayMode.right:
                    backgroundImage.Stretch = Stretch.Uniform;
                    backgroundImage.HorizontalAlignment = HorizontalAlignment.Right;
                    break;
                case PPDEditorSkin.BackGroundDisplayMode.left:
                    backgroundImage.Stretch = Stretch.Uniform;
                    backgroundImage.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case PPDEditorSkin.BackGroundDisplayMode.center:
                    backgroundImage.Stretch = Stretch.UniformToFill;
                    backgroundImage.HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case PPDEditorSkin.BackGroundDisplayMode.fill:
                    backgroundImage.Stretch = Stretch.Fill;
                    backgroundImage.HorizontalAlignment = HorizontalAlignment.Stretch;
                    break;
            }
        }

        private static BitmapSource ToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }
        #endregion

        #region Window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddGameWindowMenu();
            ViewModel.OnLoaded();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // 終了のメニューでも閉じるボタンでも、保存していない変更などを確かめる（エラーで止まったときは確かめない）
            if (!forceClosing && !ViewModel.CanClose())
            {
                e.Cancel = true;
                return;
            }
            moveMarkTimer.Stop();
            try
            {
                WindowUtility.ScriptManager.SaveDock(Path.Combine(Utility.AppDir, scriptdockxmlname));
                new XmlLayoutSerializer(dockPanel1).Serialize(Path.Combine(Utility.AppDir, dockxmlname));
                ViewModel.OnClosing();
            }
            catch (IOException)
            {
            }
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            ViewModel.OpenDroppedFile(fileNames[0]);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle).AddHook(WndProc);
            // 以前は DirectX 9 のデバイスのリセットの問題のために閉じるボタンを無くしていたが、DirectX 11 では起きないので、閉じるボタンは残す
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // WM_NCMOUSEMOVE: タイトルバーにマウスが来たら、隠したメニューを出す
            if (msg == 0xA0 && ViewModel.IsMenuHidden)
            {
                ViewModel.IsMenuHidden = false;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// ツールバーのボタンの横の ▼ で、メニューを出す。
        /// </summary>
        private void DropDownButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }

        /// <summary>
        /// 位置角度ローダーセーバーを開いているときは、ゲームの画面にマークの位置を描く。
        /// </summary>
        public bool DrawPoint
        {
            get { return docks.TryGetValue(DockKind.PosAndAngle, out var dock) && dock.IsActive; }
        }

        /// <summary>
        /// 確認しないで閉じます（エラーで止まったとき）。
        /// </summary>
        public void ForceClose()
        {
            forceClosing = true;
            try
            {
                Close();
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void Dispose()
        {
            ViewModel.Dispose();
            GC.SuppressFinalize(this);
        }
        #endregion

        #region IMainWindowView
        void IMainWindowView.ShowMessage(string text)
        {
            MessageBox.Show(this, text);
        }

        bool IMainWindowView.Confirm(string text, string caption, bool warning)
        {
            return MessageBox.Show(this, text, caption, MessageBoxButton.OKCancel, warning ? MessageBoxImage.Exclamation : MessageBoxImage.None) == MessageBoxResult.OK;
        }

        bool? IMainWindowView.AskSave(string text, string caption, bool okCancel)
        {
            var result = MessageBox.Show(this, text, caption, okCancel ? MessageBoxButton.OKCancel : MessageBoxButton.YesNoCancel,
                MessageBoxImage.Exclamation, okCancel ? MessageBoxResult.Cancel : MessageBoxResult.No);
            return result switch
            {
                MessageBoxResult.Yes or MessageBoxResult.OK => true,
                MessageBoxResult.No => false,
                _ => null,
            };
        }

        string IMainWindowView.OpenFile(string filter, string fileName)
        {
            openFileDialog.Filter = filter;
            openFileDialog.FileName = fileName;
            return openFileDialog.ShowDialog(this) == true ? openFileDialog.FileName : null;
        }

        string IMainWindowView.SaveFile(string filter, string fileName, string initialDirectory)
        {
            saveFileDialog.Filter = filter;
            saveFileDialog.FileName = fileName;
            if (initialDirectory != null)
            {
                saveFileDialog.InitialDirectory = initialDirectory;
            }
            return saveFileDialog.ShowDialog(this) == true ? saveFileDialog.FileName : null;
        }

        bool IMainWindowView.ShowDialog(object viewModel)
        {
            Window window = viewModel switch
            {
                AuthorNameViewModel vm => new AuthorNameWindow(vm),
                MovieCutViewModel vm => new MovieCutWindow(vm),
                SwapLineViewModel vm => new SwapLineWindow(vm),
                GridSettingViewModel vm => new GridSettingWindow(vm),
                FarnessSettingViewModel vm => new FarnessSettingWindow(vm),
                SettingViewModel vm => new SettingWindow(vm),
                CopyDifficultyViewModel vm => new CopyDifficultyWindow(vm),
                PublishViewModel vm => new PublishWindow(vm),
                MergeViewModel vm => new MergeWindow(vm),
                ModPublishViewModel vm => new ModPublishWindow(vm),
                ImportEventViewModel vm => new ImportEventWindow(vm),
                EditTimeLineRowViewModel vm => new EditTimeLineRowWindow(vm),
                NewProjectWindowViewModel vm => new NewProjectWindow { DataContext = vm },
                _ => throw new ArgumentException("Unknown dialog view model: " + viewModel?.GetType(), nameof(viewModel)),
            };
            return WpfDialog.ShowDialog(window);
        }

        void IMainWindowView.ShowInExplorer(string path)
        {
            try
            {
                Process.Start("EXPLORER.EXE", @"/select," + path);
            }
            catch
            {
            }
        }

        void IMainWindowView.ToggleDock(DockKind kind)
        {
            DockUtility.ShowOrHideWindow(docks[kind]);
        }

        bool IMainWindowView.IsDockShown(DockKind kind)
        {
            return DockUtility.IsShown(docks[kind]);
        }

        /// <summary>
        /// 固定するときは、ウィンドウを動かしたり浮かせたりできなくする。
        /// </summary>
        public void SetDocksFixed(bool value)
        {
            foreach (var anchorable in docks.Values)
            {
                anchorable.CanMove = anchorable.CanFloat = anchorable.CanDockAsTabbedDocument = !value;
            }
        }

        void IMainWindowView.LoadLayout(string path)
        {
            ReadDockXML(path);
        }

        void IMainWindowView.ShowSaveDone()
        {
            saveDoneWindow.Hide();
            saveDoneWindow.Owner ??= this;
            saveDoneWindow.Show();
            Activate();
        }

        (Rect Bounds, bool Maximized) IMainWindowView.Placement
        {
            // 最大化しているときは、元の大きさと位置
            get { return (WindowState == WindowState.Normal ? new Rect(Left, Top, Width, Height) : RestoreBounds, WindowState == WindowState.Maximized); }
        }
        #endregion
    }
}
