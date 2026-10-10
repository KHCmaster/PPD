using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDConfiguration;
using PPDEditor.Models;
using PPDEditor.ViewModels.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace PPDEditor.ViewModels
{
    /// <summary>
    /// メインのウィンドウの ViewModel から使う、画面の機能です。
    /// </summary>
    public interface IMainWindowView
    {
        void ShowMessage(string text);

        /// <summary>
        /// OK とキャンセルで確かめます。
        /// </summary>
        /// <param name="warning">警告の印を出すかどうか。</param>
        bool Confirm(string text, string caption, bool warning = false);

        /// <summary>
        /// 保存するかを聞きます。
        /// </summary>
        /// <param name="okCancel">OK とキャンセルで聞くか（false のときは、はい、いいえ、キャンセル）。</param>
        /// <returns>はいか OK のときは true、いいえのときは false、キャンセルのときは null。</returns>
        bool? AskSave(string text, string caption, bool okCancel);

        /// <summary>
        /// 開くファイルを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string OpenFile(string filter, string fileName);

        /// <summary>
        /// 保存するファイルを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string SaveFile(string filter, string fileName, string initialDirectory = null);

        /// <summary>
        /// ViewModel に合うダイアログを出します。
        /// </summary>
        /// <returns>OK で閉じたかどうか。</returns>
        bool ShowDialog(object viewModel);

        /// <summary>
        /// エクスプローラーで、ファイルかフォルダを選んだ状態で開きます。
        /// </summary>
        void ShowInExplorer(string path);

        /// <summary>
        /// ドックのウィンドウを出すか隠します。
        /// </summary>
        void ToggleDock(DockKind kind);

        bool IsDockShown(DockKind kind);

        /// <summary>
        /// ドックのウィンドウを動かせなくするかどうかを変えます。
        /// </summary>
        void SetDocksFixed(bool value);

        /// <summary>
        /// ドックの配置を読み込みます。
        /// </summary>
        void LoadLayout(string path);

        /// <summary>
        /// 保存したことを知らせる小さなウィンドウを出します。
        /// </summary>
        void ShowSaveDone();

        /// <summary>
        /// ウィンドウの位置と大きさ（最大化しているときは元の大きさ）と、最大化しているかどうかです。
        /// </summary>
        (Rect Bounds, bool Maximized) Placement { get; }

        void Close();
    }

    /// <summary>
    /// エディターのメインのウィンドウの ViewModel です。メニュー、ツールバー、状態の表示と、プロジェクトの読み書きを扱います。
    /// </summary>
    /// <remarks>
    /// エディターの中心なので、ドックのウィンドウ（<see cref="WindowUtility"/>）を直接使う。
    /// ダイアログやファイルの選択、ドックの配置などの画面の処理は <see cref="IMainWindowView"/> で行う。
    /// プロジェクトの処理は MainWindowViewModel.Project.cs、動画とゲームとコマンドの処理は MainWindowViewModel.Game.cs にある。
    /// </remarks>
    public sealed partial class MainWindowViewModel : ObservableObject
    {
        readonly IMainWindowView view;
        readonly SynchronizationContext uiContext;
        readonly int uiThreadId;

        public MainWindowViewModel(IMainWindowView view)
        {
            this.view = view;
            uiContext = SynchronizationContext.Current ?? new DispatcherSynchronizationContext();
            uiThreadId = Environment.CurrentManagedThreadId;
            DisplayModes = [.. Enumerable.Range(1, 8).Select(i => new LocalizedText("Displaylevel" + i))];
            BeatTypes = [new("BeatSecond"), new("BeatThird"), new("BeatFourth"), new("BeatFifth")];
            Docks =
            [
                new(DockKind.GameWindow, "GameWindow"),
                new(DockKind.TimeLine, "TimeLine"),
                new(DockKind.Property, "Property"),
                new(DockKind.PosAndAngle, "PAALS"),
                new(DockKind.Kasi, "KE"),
                new(DockKind.SoundManager, "SM"),
                new(DockKind.IniFileWriter, "SFW"),
                new(DockKind.BPMMeasure, "BPMMeasure"),
                new(DockKind.LayerManager, "LayerManager"),
                new(DockKind.EventManager, "EventManager"),
                new(DockKind.GeometryCreator, "GeometryCreator"),
                new(DockKind.Memo, "Memo"),
                new(DockKind.ResourceManager, "ResourceManager"),
                new(DockKind.ScriptManager, "ScriptManager"),
                new(DockKind.StatsManager, "StatsManager"),
                new(DockKind.Help, "Help"),
            ];
            Texts.PropertyChanged += (sender, e) => OnLanguageChanged();
        }

        public LanguageTexts Texts
        {
            get { return LanguageTexts.Instance; }
        }

        #region 起動

        /// <summary>
        /// 設定と、メニューの言語、ビューの配置を読み込みます（ドックのウィンドウを作った後、配置する前に呼ぶ）。
        /// </summary>
        public void LoadSettings()
        {
            NativeMethods.timeBeginPeriod(1);
            FixDockPanel = PPDStaticSetting.FixDockPanel;
            WindowUtility.MemoWindow.MemoFontSize = PPDStaticSetting.MemoFontSize;
            foreach (var path in EditorSettingsStore.LoadRecentProjects().Reverse())
            {
                AddRecentProject(path);
            }
            LoadLanguages();
            Utility.ChangeLanguage(PPDStaticSetting.langFileISO);
            LoadViewLayouts();
        }

        /// <summary>
        /// ツールバーを既定の状態にします（レイヤーを初期化した後に呼ぶ）。
        /// </summary>
        public void InitializeToolbar()
        {
            IsAreaSelectionEnabled = false;
            DisplayModeIndex = 0;
            SpeedScale = 2;
            Farness = 0.5f;
        }

        /// <summary>
        /// 難易度、ショートカット、MOD、コマンドを読み込みます（ドックを配置した後に呼ぶ）。
        /// </summary>
        public void Start()
        {
            AvailableDifficulty = AvailableDifficulty.Base;
            CurrentDifficulty = AvailableDifficulty.Base;
            ChangeHelpUrl();
            EditorSettingsStore.LoadShortcuts(WindowUtility.Seekmain.ShortcutManager);
            LoadMods();
            ReadCommands();
            storage = new FlowScript.Storage();
        }

        /// <summary>
        /// ウィンドウを表示したときの処理です。作者名がなければ入力させる。
        /// </summary>
        public void OnLoaded()
        {
            if (string.IsNullOrEmpty(PPDStaticSetting.AuthorName))
            {
                ChangeAuthorName();
            }
        }

        private void ChangeAuthorName()
        {
            var viewModel = new AuthorNameViewModel { AuthorName = PPDStaticSetting.AuthorName ?? "" };
            if (view.ShowDialog(viewModel))
            {
                PPDStaticSetting.AuthorName = viewModel.AuthorName;
            }
        }

        /// <summary>
        /// ウィンドウを閉じるときに、ショートカットを保存します。
        /// </summary>
        public void OnClosing()
        {
            EditorSettingsStore.SaveShortcuts(WindowUtility.Seekmain.ShortcutManager);
        }

        #endregion

        #region 言語

        /// <summary>
        /// 言語のメニューです（Lang のフォルダの言語ファイル）。
        /// </summary>
        public ObservableCollection<FileMenuItem> Languages { get; } = [];

        private void LoadLanguages()
        {
            if (!Directory.Exists("Lang"))
            {
                return;
            }
            var componentName = Assembly.GetEntryAssembly().GetName().Name;
            foreach (string fileName in Directory.GetFiles("Lang", String.Format("lang_{0}_*.ini", componentName)))
            {
                if (!Regex.IsMatch(Path.GetFileName(fileName), String.Format("lang_{0}_[0-9a-zA-Z]+\\.ini", componentName)))
                {
                    continue;
                }
                var lang = new SettingReader(File.ReadAllText(fileName));
                Languages.Add(new FileMenuItem(lang.ReadString("DisplayName"), fileName.ToLower())
                {
                    IsChecked = Path.GetFileName(fileName).Equals(String.Format("lang_{0}_{1}.ini", componentName, PPDStaticSetting.langFileISO), StringComparison.CurrentCultureIgnoreCase)
                });
            }
        }

        [RelayCommand]
        private void SelectLanguage(FileMenuItem item)
        {
            var m = LangFileRegex().Match(Path.GetFileName(item.Path));
            if (m.Success)
            {
                PPDStaticSetting.langFileISO = m.Groups["ISO"].Value;
            }
            Utility.ChangeLanguage(PPDStaticSetting.langFileISO);
            foreach (var language in Languages)
            {
                language.IsChecked = language == item;
            }
            ChangeHelpUrl();
        }

        private void OnLanguageChanged()
        {
            foreach (var text in DisplayModes.Concat(BeatTypes).Concat(Docks))
            {
                text.Refresh();
            }
            OnPropertyChanged(nameof(AreaSelectionText));
            OnPropertyChanged(nameof(CopyBufferText));
        }

        private static void ChangeHelpUrl()
        {
            WindowUtility.HelpForm.ChangeUrl(String.Format(@"{0}\help\{1}\index.html", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), PPDStaticSetting.langFileISO));
        }

        #endregion

        #region ドック

        /// <summary>
        /// 表示のメニューの、ドックのウィンドウの項目です。
        /// </summary>
        public DockMenuItem[] Docks { get; }

        /// <summary>
        /// ビューを読み込むのメニューです（view のフォルダの配置のファイル）。
        /// </summary>
        public ObservableCollection<FileMenuItem> ViewLayouts { get; } = [];

        [ObservableProperty]
        private bool fixDockPanel;

        partial void OnFixDockPanelChanged(bool value)
        {
            PPDStaticSetting.FixDockPanel = value;
            view.SetDocksFixed(value);
        }

        /// <summary>
        /// メニューを隠しているかどうか（タイトルバーにマウスが来たら出す）。
        /// </summary>
        [ObservableProperty]
        private bool isMenuHidden;

        private void LoadViewLayouts()
        {
            if (!Directory.Exists("view"))
            {
                return;
            }
            var regex = ViewLayoutFileRegex();
            foreach (string file in Directory.GetFiles("view"))
            {
                var m = regex.Match(Path.GetFileName(file));
                if (m.Success)
                {
                    ViewLayouts.Add(new FileMenuItem(String.Format("{0}x{1}{2}", m.Groups["w"].Value, m.Groups["h"].Value, m.Groups["extra"].Value), file));
                }
            }
        }

        [RelayCommand]
        private void LoadViewLayout(FileMenuItem item)
        {
            view.LoadLayout(item.Path);
        }

        [RelayCommand]
        private void ToggleDock(DockMenuItem item)
        {
            view.ToggleDock(item.Kind);
        }

        /// <summary>
        /// 表示のメニューを開いたときに、出ているウィンドウにチェックを付けます。
        /// </summary>
        public void RefreshDockStates()
        {
            foreach (var dock in Docks)
            {
                dock.IsShown = view.IsDockShown(dock.Kind);
            }
        }

        #endregion

        #region ツールバー

        /// <summary>
        /// 表示する拍の細かさの項目です。
        /// </summary>
        public LocalizedText[] DisplayModes { get; }

        /// <summary>
        /// 拍子の項目です。
        /// </summary>
        public LocalizedText[] BeatTypes { get; }

        [ObservableProperty]
        private string bpmText = "100";

        [ObservableProperty]
        private string bpmOffsetText = "0";

        [ObservableProperty]
        private string displayWidthText = "240";

        [ObservableProperty]
        private int displayModeIndex = -1;

        [ObservableProperty]
        private int beatTypeIndex = -1;

        /// <summary>
        /// 速さの倍率の番号です（0 から 6 で、0.5 倍から 2 倍）。
        /// </summary>
        [ObservableProperty]
        private int speedScale = -1;

        /// <summary>
        /// 打鍵の操作を BPM に合わせるかどうか。
        /// </summary>
        [ObservableProperty]
        private bool bpmfixed = true;

        [ObservableProperty]
        private bool drawToggle = true;

        [ObservableProperty]
        private bool markFocus;

        [ObservableProperty]
        private bool showHoldExtent = true;

        [ObservableProperty]
        private bool displayGrid;

        /// <summary>
        /// グリッドの大きさの、選んでいるメニューの項目です。
        /// </summary>
        [ObservableProperty]
        private GridSizeChoice gridSize;

        [ObservableProperty]
        private bool isTimeLineRowLimited;

        /// <summary>
        /// マークフォーカスで、マークを画面のどこに出すか（0 から 1）。
        /// </summary>
        public float Farness { get; set; }

        public SquareGrid Grid { get; } = new();

        // XAML を読み込む間やドックを作る前にも値が変わるので、ドックを作るまでは何もしない
        private static bool DocksReady
        {
            get { return WindowUtility.LayerManager != null; }
        }

        partial void OnBpmTextChanged(string value)
        {
            if (DocksReady && float.TryParse(value, out float result) && result <= 500)
            {
                WindowUtility.LayerManager.BPM = result;
            }
        }

        partial void OnBpmOffsetTextChanged(string value)
        {
            if (DocksReady && float.TryParse(value, out float result))
            {
                WindowUtility.LayerManager.BPMOffset = result;
            }
        }

        partial void OnDisplayWidthTextChanged(string value)
        {
            if (DocksReady && int.TryParse(value, out int result))
            {
                WindowUtility.LayerManager.DisplayWidth = result;
            }
        }

        partial void OnDisplayModeIndexChanged(int value)
        {
            ApplyDisplayMode();
        }

        private void ApplyDisplayMode()
        {
            if (!DocksReady)
            {
                return;
            }
            WindowUtility.LayerManager.DisplayMode = (DisplayLineMode)DisplayModeIndex;
            WindowUtility.Seekmain.DisplayMode = (DisplayLineMode)DisplayModeIndex;
            WindowUtility.Seekmain.DrawAndRefresh();
        }

        partial void OnBeatTypeIndexChanged(int value)
        {
            if (!DocksReady)
            {
                return;
            }
            WindowUtility.LayerManager.BeatType = (DisplayBeatType)value;
            WindowUtility.Seekmain.BeatType = (DisplayBeatType)value;
            WindowUtility.Seekmain.DrawAndRefresh();
        }

        partial void OnSpeedScaleChanged(int value)
        {
            ApplySpeedScale();
        }

        private void ApplySpeedScale()
        {
            if (DocksReady)
            {
                WindowUtility.Seekmain.SpeedScale = SpeedScaleAsFloat;
            }
        }

        partial void OnIsTimeLineRowLimitedChanged(bool value)
        {
            var rowManager = WindowUtility.TimeLineForm.RowManager;
            if (rowManager.IsLimited != value)
            {
                rowManager.IsLimited = value;
                WindowUtility.TimeLineForm.InvalidateAll();
            }
        }

        public float BPM
        {
            get { return float.TryParse(BpmText, out float ret) ? ret : 100; }
            set
            {
                if (value > 0 && value < 500)
                {
                    BpmText = value.ToString();
                }
            }
        }

        public float BPMOffset
        {
            get { return float.TryParse(BpmOffsetText, out float ret) ? ret : 0; }
            set { BpmOffsetText = value.ToString(); }
        }

        public int DisplayWidth
        {
            get { return int.TryParse(DisplayWidthText, out int ret) ? ret : 240; }
            set { DisplayWidthText = value.ToString(); }
        }

        public DisplayLineMode DisplayMode
        {
            get { return (DisplayLineMode)DisplayModeIndex; }
            set
            {
                if (value >= 0 && (int)value < DisplayModes.Length)
                {
                    DisplayModeIndex = (int)value;
                }
            }
        }

        public DisplayBeatType BeatType
        {
            get { return (DisplayBeatType)BeatTypeIndex; }
            set
            {
                if (value >= 0 && (int)value < BeatTypes.Length)
                {
                    BeatTypeIndex = (int)value;
                }
            }
        }

        public float SpeedScaleAsFloat
        {
            get
            {
                return SpeedScale switch
                {
                    0 => 0.5f,
                    1 => 0.75f,
                    2 => 1,
                    3 => 1.25f,
                    4 => 1.5f,
                    5 => 1.75f,
                    6 => 2,
                    _ => 1,
                };
            }
        }

        [RelayCommand]
        private void SetGridSize(GridSizeChoice choice)
        {
            var pixel = choice switch
            {
                GridSizeChoice.Px10 => 10,
                GridSizeChoice.Px20 => 20,
                _ => 50,
            };
            Grid.Width = pixel;
            Grid.Height = pixel;
            GridSize = choice;
            DisplayGrid = true;
        }

        [RelayCommand]
        private void EditGrid()
        {
            var viewModel = new GridSettingViewModel(Grid);
            if (view.ShowDialog(viewModel))
            {
                Grid.CopyFrom(viewModel.Grid);
                GridSize = GridSizeChoice.Custom;
                DisplayGrid = true;
            }
        }

        [RelayCommand]
        private void EditFarness()
        {
            var viewModel = new FarnessSettingViewModel(Farness);
            if (view.ShowDialog(viewModel))
            {
                Farness = viewModel.Farness;
            }
        }

        [RelayCommand]
        private void EditTimeLineRows()
        {
            var rowManager = WindowUtility.TimeLineForm.RowManager;
            var viewModel = new EditTimeLineRowViewModel(rowManager);
            if (view.ShowDialog(viewModel))
            {
                viewModel.Apply(rowManager);
            }
            WindowUtility.TimeLineForm.InvalidateAll();
        }

        #endregion

        #region 状態の表示

        [ObservableProperty]
        private string timeText = "0:00:00:000";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreaSelectionText))]
        private bool isAreaSelectionEnabled;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CopyBufferText))]
        private bool hasCopyBuffer;

        public string AreaSelectionText
        {
            get { return Utility.Language[IsAreaSelectionEnabled ? "AreaSelectionEnabled" : "AreaSelectionDiabled"]; }
        }

        public string CopyBufferText
        {
            get { return HasCopyBuffer ? Utility.Language["ExistCopyBuffer"] : ""; }
        }

        public void ChangeAreaSelect(bool enable)
        {
            IsAreaSelectionEnabled = enable;
        }

        public void ChangeCopyBuffer(bool exist)
        {
            HasCopyBuffer = exist;
        }

        #endregion

        #region 編集

        [RelayCommand]
        private static void Undo()
        {
            WindowUtility.Seekmain.Undo();
        }

        [RelayCommand]
        private static void Redo()
        {
            WindowUtility.Seekmain.Redo();
        }

        [RelayCommand]
        private void SwapLine()
        {
            var viewModel = new SwapLineViewModel();
            if (view.ShowDialog(viewModel))
            {
                WindowUtility.Seekmain.Swap(viewModel.First, viewModel.Second);
            }
        }

        /// <summary>
        /// レイヤー全体のマークを、近くの拍の線に乗せます。
        /// </summary>
        [RelayCommand]
        private static void ForceFitLayer()
        {
            WindowUtility.Seekmain.ForceToNearBar(false);
        }

        /// <summary>
        /// 選択範囲のマークを、近くの拍の線に乗せます。
        /// </summary>
        [RelayCommand]
        private static void ForceFitSelection()
        {
            WindowUtility.Seekmain.ForceToNearBar(true);
        }

        [RelayCommand]
        private void OpenSetting()
        {
            var viewModel = new SettingViewModel(WindowUtility.Seekmain.ShortcutManager);
            if (view.ShowDialog(viewModel))
            {
                viewModel.Apply();
                EditorSettingsStore.SaveShortcuts(WindowUtility.Seekmain.ShortcutManager);
                ReadCommands();
                if (viewModel.ShouldRestart)
                {
                    view.ShowMessage(Utility.Language["RebootNecessary"]);
                }
            }
        }

        #endregion

        /// <summary>
        /// UI のスレッドで実行します（コマンドやゲームのプロセスのスレッドから呼ばれるとき）。
        /// </summary>
        private void OnUIThread(Action action)
        {
            if (Environment.CurrentManagedThreadId == uiThreadId)
            {
                action();
            }
            else
            {
                uiContext.Send(_ => action(), null);
            }
        }

        [GeneratedRegex("^lang_\\w+_(?<ISO>\\w+).ini$")]
        private static partial Regex LangFileRegex();
        [GeneratedRegex("^view(?<w>\\d+)x(?<h>\\d+)(?<extra>.*)\\.xml$")]
        private static partial Regex ViewLayoutFileRegex();
    }
}
