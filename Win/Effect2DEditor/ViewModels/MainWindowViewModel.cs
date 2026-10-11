using BezierCaliculator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Effect2D;
using Effect2DEditor.Models;
using Effect2DEditor.ViewModels.Dialogs;
using PPDConfiguration;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using CommandManager = Effect2DEditor.Command.CommandManager;
using PointF = System.Drawing.PointF;

namespace Effect2DEditor.ViewModels
{
    /// <summary>
    /// ドックのウィンドウの種類です。
    /// </summary>
    public enum DockKind
    {
        Canvas,
        Property,
        History,
        TimeLine,
    }

    /// <summary>
    /// メインのウィンドウの ViewModel から使う、画面の機能です。
    /// </summary>
    public interface IMainWindowView
    {
        void ShowMessage(string text);

        /// <summary>
        /// はいといいえで確かめます。
        /// </summary>
        bool Confirm(string text, string caption);

        /// <summary>
        /// 保存するかを聞きます。
        /// </summary>
        /// <returns>はいのときは true、いいえのときは false、キャンセルのときは null。</returns>
        bool? AskSave(string text, string caption);

        /// <summary>
        /// 開くファイルを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは空の配列。</returns>
        string[] OpenFiles(string filter, bool multiselect, string fileName = null);

        /// <summary>
        /// 保存するファイルを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string SaveFile(string filter, string defaultExt, string fileName);

        /// <summary>
        /// ViewModel に合うダイアログを出します。
        /// </summary>
        /// <returns>OK で閉じたかどうか。</returns>
        bool ShowDialog(DialogViewModel viewModel);

        void ToggleDock(DockKind kind);

        /// <summary>
        /// カンバスを指定の大きさで作り直します。
        /// </summary>
        void InitializeCanvas(int width, int height);

        void RefreshCanvas();

        void RefreshTimeLine();

        /// <summary>
        /// タイムラインの横のスクロールを、エフェクトの長さに合わせます。
        /// </summary>
        void AdjustTimeLineHScroll();

        /// <summary>
        /// タイムラインの縦のスクロールを、指定の行にします（-1 のときは範囲だけ合わせる）。
        /// </summary>
        void MoveTimeLineVScroll(int line);

        /// <summary>
        /// タイムラインで選んでいるフレームです。
        /// </summary>
        int CurrentFrame { get; }

        /// <summary>
        /// 新しいエフェクトを、タイムラインとカンバスで使うようにします。
        /// </summary>
        void AttachEffect();

        /// <summary>
        /// カンバスで編集するベジエの点です。
        /// </summary>
        BezierControlPoint[] CanvasBezier { get; }

        /// <summary>
        /// カンバスで編集するベジエの点を変えます。
        /// </summary>
        /// <param name="allowEdit">マウスで編集できるようにするかどうか。</param>
        void SetCanvasBezier(BezierControlPoint[] bcps, bool allowEdit);

        void Close();
    }

    /// <summary>
    /// メニューの、ファイルを指す項目です（最近使ったファイル、言語）。
    /// </summary>
    public partial class FileMenuItem(string header, string path) : ObservableObject
    {
        /// <summary>
        /// 表示する文字（「_」はアクセスキーにならないようにしてある）。
        /// </summary>
        public string Header { get; } = header.Replace("_", "__");

        public string Path { get; } = path;

        [ObservableProperty]
        public partial bool IsChecked { get; set; }
    }

    /// <summary>
    /// エフェクトのエディターのメインのウィンドウの ViewModel です。ファイル、設定、再生と、編集の履歴に残す操作を扱います。
    /// </summary>
    /// <remarks>
    /// 編集の操作は MainWindowViewModel.Edit.cs にある。カンバス（DirectX）とタイムラインの描画は、ビューが行う。
    /// </remarks>
    public sealed partial class MainWindowViewModel : ObservableObject
    {
        readonly IMainWindowView view;
        readonly DispatcherTimer playTimer = new() { Interval = TimeSpan.FromMilliseconds(1000.0 / 60) };
        string openFileName = "";
        bool saved = true;
        bool ignorePlayingChange;

        public MainWindowViewModel(IMainWindowView view)
        {
            this.view = view;
            Property = new PropertyViewModel(this);
            Settings = EditorSettings.Load();
            foreach (var path in Settings.RecentFiles)
            {
                RecentFiles.Add(new FileMenuItem(path, path));
            }
            CommandManager.CommandChanged += CommandManager_CommandChanged;
            playTimer.Tick += PlayTimer_Tick;
            Texts.PropertyChanged += (sender, e) => OnPropertyChanged(nameof(PlayToolTip));
        }

        public LanguageTexts Texts
        {
            get { return LanguageTexts.Instance; }
        }

        public EditorSettings Settings { get; }

        public PropertyViewModel Property { get; }

        /// <summary>
        /// 編集の履歴です（ヒストリーウィンドウに表示する）。
        /// </summary>
        public CommandManager CommandManager { get; } = new();

        public EffectManager EffectManager { get; private set; }

        public SelectedManager SelectedManager { get; private set; }

        public ObservableCollection<FileMenuItem> RecentFiles { get; } = [];

        public ObservableCollection<FileMenuItem> Languages { get; } = [];

        /// <summary>
        /// 起動したときの処理です。言語を読み込み、新しいエフェクトにする（ビューを作り、カンバスを設定の大きさにした後に呼ぶ）。
        /// </summary>
        public void Start()
        {
            LoadLanguages();
            Texts.Load(LanguageFilePath(Settings.Language));
            ResetEffect(new EffectManager());
        }

        #region カンバスの背景

        public BackGroundMode BackGroundMode
        {
            get { return Settings.BackGroundMode; }
        }

        public System.Drawing.Color BackGroundColor
        {
            get { return Settings.BackGroundColor; }
        }

        public string BackGroundImage
        {
            get { return Settings.BackGroundImage; }
        }

        [RelayCommand]
        private void ChangeCanvasSize()
        {
            var viewModel = new CanvasSizeViewModel(Settings.CanvasWidth, Settings.CanvasHeight);
            if (view.ShowDialog(viewModel))
            {
                Settings.CanvasWidth = viewModel.CanvasWidth;
                Settings.CanvasHeight = viewModel.CanvasHeight;
                view.InitializeCanvas(Settings.CanvasWidth, Settings.CanvasHeight);
            }
        }

        [RelayCommand]
        private void ChangeBackGround()
        {
            var viewModel = new ChangeBackGroundViewModel(Settings.BackGroundMode, Settings.BackGroundColor, Settings.BackGroundImage);
            if (!view.ShowDialog(viewModel))
            {
                return;
            }
            Settings.BackGroundMode = viewModel.Mode;
            switch (viewModel.Mode)
            {
                case BackGroundMode.Color:
                    Settings.BackGroundColor = viewModel.Color;
                    break;
                case BackGroundMode.Image:
                    Settings.BackGroundImage = viewModel.FileName;
                    break;
            }
            view.RefreshCanvas();
        }

        #endregion

        #region 言語

        private static string ComponentName
        {
            // 起動したアセンブリでなく、このアセンブリの名前にする（テストから使ったときも、エディターの言語ファイルを読むように）
            get { return typeof(MainWindowViewModel).Assembly.GetName().Name; }
        }

        private static string LanguageFilePath(string iso)
        {
            return Path.Combine("Lang", String.Format("lang_{0}_{1}.ini", ComponentName, iso));
        }

        private void LoadLanguages()
        {
            if (!Directory.Exists("Lang"))
            {
                return;
            }
            foreach (string fileName in Directory.GetFiles("Lang", String.Format("lang_{0}_*.ini", ComponentName)))
            {
                var setting = new SettingReader(File.ReadAllText(fileName));
                Languages.Add(new FileMenuItem(setting["DisplayName"], fileName.ToLower())
                {
                    IsChecked = Path.GetFileName(fileName).Equals(Path.GetFileName(LanguageFilePath(Settings.Language)), StringComparison.CurrentCultureIgnoreCase)
                });
            }
        }

        [RelayCommand]
        private void SelectLanguage(FileMenuItem item)
        {
            var m = LangFileRegex().Match(Path.GetFileName(item.Path));
            if (m.Success)
            {
                Settings.Language = m.Groups["ISO"].Value;
            }
            Texts.Load(item.Path);
            foreach (var language in Languages)
            {
                language.IsChecked = language == item;
            }
        }

        #endregion

        #region ファイル

        [RelayCommand]
        private void NewEffect()
        {
            if (CheckSave())
            {
                ResetEffect(new EffectManager());
            }
        }

        [RelayCommand]
        private void Open()
        {
            if (!CheckSave())
            {
                return;
            }
            var paths = view.OpenFiles(EffectFile.Filter, false, openFileName == "" ? null : Path.GetFullPath(openFileName));
            if (paths.Length > 0)
            {
                OpenEffect(paths[0]);
            }
        }

        [RelayCommand]
        private void OpenRecentFile(FileMenuItem item)
        {
            if (CheckSave())
            {
                OpenEffect(item.Path);
            }
        }

        /// <summary>
        /// ウィンドウに落としたファイルがエフェクトなら開きます。
        /// </summary>
        public void OpenDroppedFile(string path)
        {
            if (File.Exists(path) && Path.GetExtension(path) == ".etd" && CheckSave())
            {
                OpenEffect(path);
            }
        }

        private void OpenEffect(string path)
        {
            ResetEffect(EffectFile.Load(path, () => view.ShowMessage("Failed to load image")));
            openFileName = path;

            var existing = RecentFiles.FirstOrDefault(item => item.Path == path);
            if (existing != null)
            {
                RecentFiles.Move(RecentFiles.IndexOf(existing), 0);
                return;
            }
            RecentFiles.Insert(0, new FileMenuItem(path, path));
            if (RecentFiles.Count > 5)
            {
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            }
        }

        [RelayCommand]
        private void SaveAs()
        {
            TrySaveAs();
        }

        /// <summary>
        /// 名前を付けて保存します。
        /// </summary>
        /// <returns>保存したかどうか（キャンセルしたときは false）。</returns>
        private bool TrySaveAs()
        {
            var path = view.SaveFile(EffectFile.Filter, "etd", openFileName == "" ? "NewEffect.etd" : Path.GetFullPath(openFileName));
            if (path == null)
            {
                return false;
            }
            Save(path);
            return true;
        }

        [RelayCommand]
        private void Overwrite()
        {
            if (File.Exists(openFileName))
            {
                Save(openFileName);
            }
            else
            {
                TrySaveAs();
            }
        }

        private void Save(string path)
        {
            EffectFile.Save(EffectManager, path);
            openFileName = path;
            saved = true;
        }

        /// <summary>
        /// 保存していない変更があれば、保存するかを聞きます。
        /// </summary>
        /// <returns>続けてよいかどうか（キャンセルしたときは false）。</returns>
        private bool CheckSave()
        {
            if (saved)
            {
                return true;
            }
            var answer = view.AskSave(Texts["AskSave"], Texts["Confirm"]);
            if (answer == true)
            {
                return TrySaveAs();
            }
            return answer == false;
        }

        [RelayCommand]
        private void Exit()
        {
            view.Close();
        }

        /// <summary>
        /// ウィンドウを閉じるときに、保存するかを聞き、設定を保存します。
        /// </summary>
        /// <param name="bounds">ウィンドウの位置と大きさ（最大化しているときは元の大きさ）。</param>
        /// <returns>閉じてよいかどうか。</returns>
        public bool OnClosing(Rect bounds, bool maximized)
        {
            if (!CheckSave())
            {
                return false;
            }
            Settings.WindowWidth = bounds.Width;
            Settings.WindowHeight = bounds.Height;
            Settings.Maximized = maximized;
            Settings.RecentFiles.Clear();
            Settings.RecentFiles.AddRange(RecentFiles.Select(item => item.Path));
            Settings.Save();
            return true;
        }

        /// <summary>
        /// ウィンドウを閉じた後の処理です。
        /// </summary>
        public void OnClosed()
        {
            playTimer.Stop();
        }

        private void ResetEffect(EffectManager effectManager)
        {
            openFileName = "";
            EffectManager = effectManager;
            SelectedManager = new SelectedManager(EffectManager);
            EffectManager.Finish += EffectManager_Finish;
            SelectedManager.StateChanged += SelectedManager_StateChanged;
            FpsText = EffectManager.FPS.ToString();
            view.AttachEffect();
            CommandManager.ClearAll();
            view.RefreshTimeLine();
            saved = true;
        }

        [RelayCommand]
        private void ToggleDock(DockKind kind)
        {
            view.ToggleDock(kind);
        }

        #endregion

        #region 再生

        /// <summary>
        /// 再生のしかたです。
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PlayToolTip))]
        public partial EffectManager.PlayType PlayType { get; set; } = EffectManager.PlayType.Once;

        /// <summary>
        /// 再生しているかどうか（再生のボタンを押すと切り替わる）。
        /// </summary>
        [ObservableProperty]
        public partial bool IsPlaying { get; set; }

        [ObservableProperty]
        public partial string FpsText { get; set; } = "60";

        /// <summary>
        /// 再生のボタンの説明（再生のしかた）です。
        /// </summary>
        public string PlayToolTip
        {
            get
            {
                return Texts[PlayType switch
                {
                    EffectManager.PlayType.ReverseOnce => "PlayOnceReverse",
                    EffectManager.PlayType.Loop => "PlayLoop",
                    EffectManager.PlayType.ReverseLoop => "PlayReverseLoop",
                    _ => "PlayOnce",
                }];
            }
        }

        partial void OnFpsTextChanged(string value)
        {
            if (EffectManager != null && float.TryParse(value, out float val))
            {
                EffectManager.FPS = val;
            }
        }

        partial void OnIsPlayingChanged(bool value)
        {
            if (ignorePlayingChange)
            {
                return;
            }
            if (value)
            {
                Play();
            }
            else
            {
                playTimer.Stop();
                EffectManager.Pause();
            }
        }

        /// <summary>
        /// 再生のしかたを選んで、再生します。
        /// </summary>
        /// <param name="type">再生のしかたの名前（<see cref="EffectManager.PlayType"/> の値の名前）。</param>
        [RelayCommand]
        private void SelectPlayType(string type)
        {
            PlayType = Enum.Parse<EffectManager.PlayType>(type);
            SetPlaying(true);
            Play();
        }

        [RelayCommand]
        private void Stop()
        {
            playTimer.Stop();
            EffectManager.Update(0, null);
            EffectManager.Stop();
            view.RefreshCanvas();
            SetPlaying(false);
        }

        private void Play()
        {
            playTimer.Stop();
            EffectManager.CheckFrameLength();
            EffectManager.Play(PlayType);
            playTimer.Start();
        }

        /// <summary>
        /// 再生しているかの表示だけを変えます（再生や一時停止はしない）。
        /// </summary>
        private void SetPlaying(bool value)
        {
            ignorePlayingChange = true;
            IsPlaying = value;
            ignorePlayingChange = false;
        }

        void PlayTimer_Tick(object sender, EventArgs e)
        {
            if (EffectManager.State == EffectManager.PlayState.Stop)
            {
                playTimer.Stop();
            }
            EffectManager.Update();
            view.RefreshCanvas();
        }

        void EffectManager_Finish(object sender, EventArgs e)
        {
            playTimer.Stop();
            SetPlaying(false);
        }

        #endregion

        [GeneratedRegex("^lang_\\w+_(?<ISO>\\w+).ini$")]
        private static partial Regex LangFileRegex();
    }
}
