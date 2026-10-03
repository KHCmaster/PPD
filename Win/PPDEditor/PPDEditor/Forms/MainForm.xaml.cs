using PPDEditor.ViewModels.Docks;
using PPDEditor.ViewModels.Dialogs;
using PPDConfiguration;
using PPDEditor.Controls;
using PPDEditor.FlowScript;
using PPDEditorCommon;
using PPDEditorCommon.Dialog;
using PPDEditorCommon.Dialog.ViewModel;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using PPDPack;
using System.Numerics;
using PPDMath;
using Rectangle = Vortice.Mathematics.RectI;
using Point = Vortice.Mathematics.Int2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Keys = System.Windows.Forms.Keys;

namespace PPDEditor.Forms
{
    public partial class EditorForm : Window, IMovieManager, ISongInfo, IDisposable
    {

        const string rescuedirectory = "rescueddata";
        const string dockxmlname = "dockforminfo.xml";
        const string scriptdockxmlname = "scriptdockforminfo.xml";
        const string projectcache = "projectcache.dat";
        public const string iniFileName = "PPDEditor.ini";
        const string shortcutFileName = "PPDEditor_keybind.ini";
        const int latestprojectversion = 2;

        static PosAndAngleLoaderSaver paals;
        static LayoutAnchorable paalsDock;
        static KasiEditor ke;
        static LayoutAnchorable keDock;
        static SoundManager sm;
        static LayoutAnchorable smDock;
        static IniFileWriter ifw;
        static LayoutAnchorable ifwDock;
        static BPMMeasure bm;
        static LayoutAnchorable bmDock;
        static TimeLineForm tlf;
        static LayoutAnchorable tlfDock;
        static LayoutAnchorable dxfDock;
        static InfoForm infof;
        static LayoutAnchorable infofDock;
        static LayerManager lm;
        static LayoutAnchorable lmDock;
        static EventManager em;
        static LayoutAnchorable emDock;
        static GeometryCreator gc;
        static LayoutAnchorable gcDock;
        static MemoWindow mw;
        static LayoutAnchorable mwDock;
        static HelpForm hf;
        static LayoutAnchorable hfDock;
        static WriteDoneWindow wdf;
        static ResourceManager rm;
        static LayoutAnchorable rmDock;
        static ScriptManager scm;
        static LayoutAnchorable scmDock;
        static StatsManager stm;
        static LayoutAnchorable stmDock;

        long starttime;
        long movielastime;

        string projectfilename = "";
        string projectfiledir = "";
        string enabledareaselection = "エリア選択オン";
        string disabledareaselection = "エリア選択オフ";
        string openmoviefilter = "すべてのファイル(*.*)|*.*";
        string opensoundsetfilter = "TXTファイル(*.txt)|*.txt|すべてのファイル(*.*)|*.*";
        string openppdfilter = "PPDファイル(*.ppd)|*.ppd|すべてのファイル(*.*)|*.*";
        string openscdfilter = "SCDファイル(*.scd)|*.scd|すべてのファイル(*.*)|*.*";
        string openinifilter = "INIファイル(*.ini)|*.ini|すべてのファイル(*.*)|*.*";
        string openprojectfilter = "PPDProjectファイル(*.ppdproj)|*.ppdproj|すべてのファイル(*.*)|*.*";
        string openevdfilter = "EVDファイル(*.evd)|*.evd|すべてのファイル(*.*)|*.*";
        string openexefileter = "EXEファイル(*.exe)|*.exe";
        string invaliddata = "無効なデータです";
        string askkillprocess = "実行中のPPDプロセスがあります。終了しますか？";
        string askkillcommand = "実行中のコマンドがあります。終了しますか？";
        string asksave = "保存されていないデータが有ります。保存しますか？";
        string confirm = "確認";
        string existcopybuffer = "コピーバッファがあります";
        readonly string noproject = "プロジェクトがありません。プロジェクトとして保存してください";
        readonly string noppdexe = "適切なPPD.exeの参照がありません";
        string nosoundfile = "が存在しません。";
        string rebootnecessary = "再起動が必要です";
        string oldprojectdetected = "古いバージョンのプロジェクトです。変換しますか？";
        string alreadyexitdifficulty = "既にコピー先の難易度が存在しますが、上書きしますか？";
        string noselecteddifficulty = "選択された難易度が存在しません";
        string nomergedifficulty = "マージするプロジェクトが存在しません";

        bool moviefirstplayafterinitialized = true;

        private string langFileName = "";

        private AvailableDifficulty currentdifficulty = AvailableDifficulty.Base;
        private AvailableDifficulty availabledifficulty;
        public static readonly AvailableDifficulty[] DifficultyArray = Enum.GetValues<AvailableDifficulty>();

        private Process currentProcess;
        private Executor currentExecutor;

        public event Action ProcessStarted;
        public event Action ProcessExited;
        public event Action<string> CommandExecuting;
        public event Action<string> CommandExecuted;

        private Storage storage;

        readonly Microsoft.Win32.OpenFileDialog openFileDialog1 = new();
        readonly Microsoft.Win32.SaveFileDialog saveFileDialog1 = new();
        // ゲームの画面でマウスを押している間、マークを動かす（以前と同じく 100 ミリ秒ごと）
        readonly DispatcherTimer moveMarkTimer = new() { Interval = TimeSpan.FromMilliseconds(100) };
        // ゲームウィンドウのタブ（浮かせたときはタイトルバー）の右クリックのメニューに加える項目
        readonly MenuItem 画面比を169にMenuItem = new() { Header = "画面比を16:9に" };
        readonly MenuItem 標準解像度にするMenuItem = new() { Header = "標準解像度にする" };

        /// <summary>
        /// ゲームの画面です。
        /// </summary>
        public GameView GameView
        {
            get;
            private set;
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
            画面比を169にMenuItem.Click += 画面比を169にMenuItem_Click;
            標準解像度にするMenuItem.Click += 標準解像度にするMenuItem_Click;
            SizeChanged += (sender, e) => ChangeBackGround();
        }

        #region Initialize
        public void Initialize()
        {
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["PAALS"]));
            paals = new PosAndAngleLoaderSaver();
            paalsDock = CreateAnchorable(paals, "PPDEditor.PosAndAngleLoaderSaver");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["KE"]));
            ke = new KasiEditor();
            keDock = CreateAnchorable(ke, "PPDEditor.KasiEditor");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["SM"]));
            sm = new SoundManager();
            smDock = CreateAnchorable(sm, "PPDEditor.SoundManager");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["SFW"]));
            ifw = new IniFileWriter();
            ifwDock = CreateAnchorable(ifw, "PPDEditor.IniFileWriter");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["BPMMeasure"]));
            bm = new BPMMeasure();
            bmDock = CreateAnchorable(bm, "PPDEditor.BPMMeasure");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["TimeLine"]));
            tlf = new TimeLineForm();
            tlfDock = CreateAnchorable(tlf, "PPDEditor.TimeLineForm");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["GameWindow"]));
            dxfDock = new LayoutAnchorable
            {
                ContentId = "PPDEditor.DXForm",
                Content = GameView,
                Title = "ゲームウィンドウ",
                CanClose = false
            };
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["Property"]));
            infof = new InfoForm();
            infofDock = CreateAnchorable(infof, "PPDEditor.InfoForm");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["LayerManager"]));
            lm = new LayerManager();
            lmDock = CreateAnchorable(lm, "PPDEditor.LayerManager");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["EventManager"]));
            em = new EventManager();
            emDock = CreateAnchorable(em, "PPDEditor.EventManager");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["GeometryCreator"]));
            gc = new GeometryCreator();
            gcDock = CreateAnchorable(gc, "PPDEditor.GeometryCreator");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["Memo"]));
            mw = new MemoWindow();
            mwDock = CreateAnchorable(mw, "PPDEditor.MemoWindow");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["Help"]));
            hf = new HelpForm();
            hfDock = CreateAnchorable(hf, "PPDEditor.HelpForm");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["WriteDoneWindow"]));
            wdf = new WriteDoneWindow();
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["ResourceManager"]));
            rm = new ResourceManager();
            rmDock = CreateAnchorable(rm, "PPDEditor.ResourceManager");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["ScriptManager"]));
            scm = new ScriptManager();
            scmDock = CreateAnchorable(scm, "PPDEditor.ScriptManager");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["StatsManager"]));
            stm = new StatsManager();
            stmDock = CreateAnchorable(stm, "PPDEditor.StatsManager");

            Grid = new SquareGrid();
            NativeMethods.timeBeginPeriod(1);
            WindowUtility.MainForm = this;
            WindowUtility.BPMMeasure = bm;
            WindowUtility.EventManager = em;
            WindowUtility.InfoForm = infof;
            WindowUtility.IniFileWriter = ifw;
            WindowUtility.KasiEditor = ke;
            WindowUtility.LayerManager = lm;
            WindowUtility.PosAndAngleLoaderSaver = paals;
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

            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["Setting"]));
            LoadConfig();
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["LanguageFile"]));
            CheckLangFiles();
            SetLanguage(PPDStaticSetting.langFileISO);
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["View"]));
            ReadViews();

            if (File.Exists("skin.ini"))
            {
                PPDEditorSkin.Skin.Initialize("skin.ini");
            }

            lm.Initialize();

            this.toolStripStatusLabel2.Text = disabledareaselection;
            this.toolStripComboBox1.SelectedIndex = 0;
            this.toolStripComboBox2.SelectedIndex = 2;
            Farness = 0.5f;

            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["Skin"]));
            if (File.Exists("skin.ini"))
            {
                ChangeBackGround();
            }

            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["DockWindow"]));
            InitializeDockWindow();

            AvailableDifficulty = AvailableDifficulty.Base;
            CurrentDifficulty = AvailableDifficulty.Base;
            ChangeHelpUrl();

            LoadShortcut();
            LoadMod();
            ReadCommands();
            storage = new Storage();
        }

        private void LoadMod()
        {
            if (!Directory.Exists("mods"))
            {
                return;
            }

            LoadMod(mODToolStripMenuItem, "mods");
        }

        private static void LoadMod(MenuItem menu, string dir)
        {
            foreach (string dirName in Directory.GetDirectories(dir))
            {
                var newItem = new MenuItem
                {
                    Header = ToMenuText(Path.GetFileNameWithoutExtension(dirName)),
                    Tag = Path.GetFileNameWithoutExtension(dirName)
                };
                menu.Items.Add(newItem);
                LoadMod(newItem, dirName);
            }
            foreach (string fileName in Directory.GetFiles(dir, "*.mod"))
            {
                menu.Items.Add(new MenuItem
                {
                    Header = ToMenuText(Path.GetFileNameWithoutExtension(fileName)),
                    IsCheckable = true,
                    Tag = new ModMenuInfo(Path.GetFileNameWithoutExtension(fileName))
                });
            }
        }

        /// <summary>
        /// Mod のファイルのメニューに入れる情報です（名前）。
        /// </summary>
        sealed record ModMenuInfo(string Name);

        /// <summary>
        /// メニューでは「_」がアクセスキーの印になるので、そのまま出るようにする。
        /// </summary>
        private static string ToMenuText(string text)
        {
            return text.Replace("_", "__");
        }

        private static void LoadShortcut()
        {
            WindowUtility.Seekmain.ShortcutManager.ClearShortcut();
            try
            {
                var path = Path.Combine(Utility.AppDir, shortcutFileName);
                if (File.Exists(path))
                {
                    foreach (string line in File.ReadAllLines(path))
                    {
                        var info = ShortcutInfo.Deserialize(line);
                        if (info != null)
                        {
                            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(info);
                        }

                    }
                    return;
                }
            }
            catch
            {

            }

            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Left, false, false, false, ShortcutType.Left0));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Right, false, false, false, ShortcutType.Right0));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Down, false, false, false, ShortcutType.Down0));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Up, false, false, false, ShortcutType.Up0));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Left, true, false, false, ShortcutType.Left1));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Right, true, false, false, ShortcutType.Right1));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Down, true, false, false, ShortcutType.Down1));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Up, true, false, false, ShortcutType.Up1));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Left, false, true, false, ShortcutType.Left2));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Right, false, true, false, ShortcutType.Right2));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Down, false, true, false, ShortcutType.Down2));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Up, false, true, false, ShortcutType.Up2));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Delete, false, false, false, ShortcutType.DeleteMark));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Tab, false, false, false, ShortcutType.Next));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Tab, true, false, false, ShortcutType.Previous));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.A, false, true, false, ShortcutType.PreviousAll));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.D, false, true, false, ShortcutType.NextAll));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.R, false, true, false, ShortcutType.CopyAngle));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Q, false, true, false, ShortcutType.CopyPosition));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.F, false, true, false, ShortcutType.Fusion));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.F, true, true, false, ShortcutType.Defusion));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Z, false, true, false, ShortcutType.Undo));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.Y, false, true, false, ShortcutType.Redo));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.C, false, true, false, ShortcutType.CopyMark));
            WindowUtility.Seekmain.ShortcutManager.RegisterShortcut(new ShortcutInfo(Keys.C, true, true, false, ShortcutType.ClearCopyBuffer));
        }

        private static void SaveShortcut()
        {
            using StreamWriter writer = new(Path.Combine(Utility.AppDir, shortcutFileName));
            foreach (ShortcutInfo info in WindowUtility.Seekmain.ShortcutManager.Shortcuts)
            {
                writer.WriteLine(info.Serialize());
            }
        }

        private static void ChangeHelpUrl()
        {
            hf.ChangeUrl(String.Format(@"{0}\help\{1}\index.html", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), PPDStaticSetting.langFileISO));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            AddGameWindowMenu();
            //check author name
            if (string.IsNullOrEmpty(PPDStaticSetting.AuthorName))
            {
                ChangeAuthorName();
            }
        }

        private void CheckLangFiles()
        {
            if (Directory.Exists("Lang"))
            {
                var componentName = this.GetType().Assembly.GetName().Name;
                foreach (string fileName in Directory.GetFiles("Lang", String.Format("lang_{0}_*.ini", componentName)))
                {
                    if (!Regex.IsMatch(Path.GetFileName(fileName), String.Format("lang_{0}_[0-9a-zA-Z]+\\.ini", componentName)))
                    {
                        continue;
                    }
                    var sr = new StreamReader(fileName);
                    var lang = new SettingReader(sr.ReadToEnd());
                    sr.ReadToEnd();
                    var name = lang.ReadString("DisplayName");
                    var langtsmi = new MenuItem
                    {
                        Header = name,
                        Tag = fileName.ToLower(),
                        IsChecked = Path.GetFileName(fileName).Equals(String.Format("lang_{0}_{1}.ini", this.GetType().Assembly.GetName().Name, PPDStaticSetting.langFileISO), StringComparison.CurrentCultureIgnoreCase)
                    };
                    langtsmi.Click += Langtsmi_Click;
                    言語ToolStripMenuItem.Items.Add(langtsmi);
                }
            }
        }

        void Langtsmi_Click(object sender, EventArgs e)
        {
            if (sender is MenuItem tsmi)
            {
                langFileName = (string)tsmi.Tag;
                var m = LangFileRegex().Match(Path.GetFileName(langFileName));
                if (m.Success)
                {
                    PPDStaticSetting.langFileISO = m.Groups["ISO"].Value;
                }
                SetLanguage(PPDStaticSetting.langFileISO);
                foreach (MenuItem child in 言語ToolStripMenuItem.Items)
                {
                    child.IsChecked = false;
                }
                tsmi.IsChecked = true;
                ChangeHelpUrl();
            }
        }

        private void SetLanguage(string langIso)
        {
            Utility.ChangeLanguage(langIso);
            言語ToolStripMenuItem.Header = Utility.Language["Language"];
            this.ファイルToolStripMenuItem.Header = Utility.Language["File"];
            this.プロジェクトを開くToolStripMenuItem.Header = Utility.Language["OpenProject"];
            this.プロジェクトを上書き保存ToolStripMenuItem.Header = Utility.Language["OverWriteProject"];
            this.開くToolStripMenuItem.Header = Utility.Language["Open"];
            this.保存ToolStripMenuItem.Header = Utility.Language["Save"];
            this.最近使用したプロジェクトToolStripMenuItem.Header = Utility.Language["RecentUsedFile"];
            this.終了ToolStripMenuItem.Header = Utility.Language["Exit"];
            this.編集ToolStripMenuItem.Header = Utility.Language["Edit"];
            this.元に戻すToolStripMenuItem.Header = Utility.Language["Undo"];
            this.やり直すToolStripMenuItem.Header = Utility.Language["Redo"];
            this.ライン入れ替えToolStripMenuItem.Header = Utility.Language["SwapLine"];
            this.動画ToolStripMenuItem.Header = Utility.Language["Movie"];
            this.再生ToolStripMenuItem.Header = Utility.Language["Play"];
            this.一時停止ToolStripMenuItem.Header = Utility.Language["Pause"];
            this.停止ToolStripMenuItem.Header = Utility.Language["Stop"];
            this.打鍵記録ToolStripMenuItem.Header = Utility.Language["RecordKey"];
            this.切り取りToolStripMenuItem.Header = Utility.Language["Cut"];
            this.表示ToolStripMenuItem.Header = Utility.Language["Show"];
            this.強制的にラインに乗せるToolStripMenuItem.Header = Utility.Language["ForceFit"];
            this.レイヤー全体ToolStripMenuItem.Header = Utility.Language["EntireSelectedLayer"];
            this.選択範囲のみToolStripMenuItem.Header = Utility.Language["OnlyInSelection"];
            dxfDock.Title = (string)(this.ゲームウィンドウToolStripMenuItem.Header = Utility.Language["GameWindow"]);
            tlf.TabText = (string)(this.タイムラインウィンドウToolStripMenuItem.Header = Utility.Language["TimeLine"]);
            infof.TabText = (string)(this.プロパティウィンドウToolStripMenuItem.Header = Utility.Language["Property"]);
            paals.TabText = (string)(this.位置角度ローダーセーバーToolStripMenuItem.Header = Utility.Language["PAALS"]);
            ke.TabText = (string)(this.歌詞エディターToolStripMenuItem.Header = Utility.Language["KE"]);
            sm.TabText = (string)(this.サウンドマネージャーToolStripMenuItem.Header = Utility.Language["SM"]);
            ifw.TabText = (string)(this.設定ファイルライターToolStripMenuItem.Header = Utility.Language["SFW"]);
            bm.TabText = (string)(this.bPMToolStripMenuItem.Header = Utility.Language["BPMMeasure"]);
            lm.TabText = (string)(this.レイヤーマネージャーToolStripMenuItem.Header = Utility.Language["LayerManager"]);
            em.TabText = (string)(this.イベントマネージャーToolStripMenuItem.Header = Utility.Language["EventManager"]);
            gc.TabText = (string)(this.幾何クリエーターToolStripMenuItem.Header = Utility.Language["GeometryCreator"]);
            mw.TabText = (string)(this.メモToolStripMenuItem.Header = Utility.Language["Memo"]);
            hf.TabText = (string)(this.ヘルプToolStripMenuItem.Header = Utility.Language["Help"]);
            rm.TabText = (string)(this.リソースマネージャーToolStripMenuItem.Header = Utility.Language["ResourceManager"]);
            scm.TabText = (string)(this.スクリプトマネージャーToolStripMenuItem.Header = Utility.Language["ScriptManager"]);
            stm.TabText = (string)(this.統計ToolStripMenuItem.Header = Utility.Language["StatsManager"]);
            this.新規プロジェクトを作成ToolStripMenuItem.Header = Utility.Language["CPF"];


            this.ゲームToolStripMenuItem.Header = Utility.Language["Game"];
            this.現在の時間から開始ToolStripMenuItem.Header = Utility.Language["GameStart1"];
            this.最初から開始ToolStripMenuItem.Header = Utility.Language["GameStart2"];
            this.現在の時間から開始オートToolStripMenuItem.Header = Utility.Language["GameStart3"];
            this.最初から開始オートToolStripMenuItem.Header = Utility.Language["GameStart4"];
            this.メニューから開始ToolStripMenuItem.Header = Utility.Language["StartFromMenu"];
            this.pPDプロセスを強制終了ToolStripMenuItem.Header = Utility.Language["ExitPPDProcess"];
            this.その他ToolStripMenuItem.Header = Utility.Language["Others"];
            this.設定ToolStripMenuItem.Header = Utility.Language["Setting"];
            this.プロジェクトをマージToolStripMenuItem.Header = Utility.Language["MergeProject"];
            this.発行ToolStripMenuItem.Header = Utility.Language["Publish"];
            this.ビューを読み込むToolStripMenuItem.Header = Utility.Language["ReadView"];
            this.現在の難易度をコピーToolStripMenuItem.Header = Utility.Language["CopyCurrentDifficulty"];
            this.難易度ToolStripMenuItem.Header = Utility.Language["Difficulty"];
            this.ツールバーを隠すToolStripMenuItem.Header = Utility.Language["HideToolBar"];
            this.ドックを固定ToolStripMenuItem.Header = Utility.Language["FixDockPanel"];
            this.mODを発行ToolStripMenuItem.Header = Utility.Language["PublishMod"];
            this.イベントToolStripMenuItem.Header = Utility.Language["Event"];
            this.インポートToolStripMenuItem.Header = Utility.Language["Import"];
            this.別のファイルToolStripMenuItem.Header = Utility.Language["Other Files"];
            this.disableExpansionToolStripMenuItem.Header = Utility.Language["DisableExpansion"];
            this.コマンドToolStripMenuItem.Header = Utility.Language["Command"];
            this.一覧を再読み込みToolStripMenuItem.Header = Utility.Language["ReloadList"];
            openinifilter = Utility.Language["INIFilter"];
            openmoviefilter = Utility.Language["MovieFilter"];
            openppdfilter = Utility.Language["PPDFilter"];
            openprojectfilter = Utility.Language["ProjectFilter"];
            openscdfilter = Utility.Language["SCDFilter"];
            opensoundsetfilter = Utility.Language["SoundSetFilter"];
            openexefileter = Utility.Language["EXEFilter"];
            openevdfilter = Utility.Language["EVDFilter"];
            enabledareaselection = Utility.Language["AreaSelectionEnabled"];
            disabledareaselection = Utility.Language["AreaSelectionDiabled"];
            invaliddata = Utility.Language["InvalidData"];
            asksave = Utility.Language["AskSave"];
            askkillprocess = Utility.Language["AskKill"];
            askkillcommand = Utility.Language["AskKillCommand"];
            confirm = Utility.Language["Confirm"];
            existcopybuffer = Utility.Language["ExistCopyBuffer"];
            nosoundfile = Utility.Language["NoSoundFile"];
            rebootnecessary = Utility.Language["RebootNecessary"];
            oldprojectdetected = Utility.Language["OldProjectDetected"];
            alreadyexitdifficulty = Utility.Language["AlreadyExistDifficulty"];
            noselecteddifficulty = Utility.Language["NoSelectedDifficulty"];
            nomergedifficulty = Utility.Language["NoMergeDifficulty"];
            //toolbar
            this.toolStripButton1.ToolTip = Utility.Language["FixOp"];
            this.toolStripButton2.ToolTip = Utility.Language["Drawtoggle"];
            this.toolStripButton3.ToolTip = Utility.Language["ShowHoldExtent"];
            this.toolStripButton4.ToolTip = Utility.Language["MarkFocus"];
            this.toolStripSplitButton1.ToolTip = Utility.Language["LimitTimeLineRow"];
            this.exToolStripSplitButton1.ToolTip = Utility.Language["DisplayGrid"];
            this.詳細設定ToolStripMenuItem.Header = Utility.Language["GridDetailSetting"];
            this.詳細設定ToolStripMenuItem1.Header = Utility.Language["GridDetailSetting"];
            this.詳細設定ToolStripMenuItem2.Header = Utility.Language["GridDetailSetting"];
            int prevSelectedIndex = toolStripComboBox1.SelectedIndex;
            this.toolStripComboBox1.Items.Clear();
            this.toolStripComboBox1.Items.Add(Utility.Language["Displaylevel1"]);
            this.toolStripComboBox1.Items.Add(Utility.Language["Displaylevel2"]);
            this.toolStripComboBox1.Items.Add(Utility.Language["Displaylevel3"]);
            this.toolStripComboBox1.Items.Add(Utility.Language["Displaylevel4"]);
            this.toolStripComboBox1.Items.Add(Utility.Language["Displaylevel5"]);
            this.toolStripComboBox1.Items.Add(Utility.Language["Displaylevel6"]);
            this.toolStripComboBox1.Items.Add(Utility.Language["Displaylevel7"]);
            this.toolStripComboBox1.Items.Add(Utility.Language["Displaylevel8"]);
            toolStripComboBox1.SelectedIndex = prevSelectedIndex;
            prevSelectedIndex = toolStripComboBox3.SelectedIndex;
            this.toolStripComboBox3.Items.Clear();
            this.toolStripComboBox3.Items.Add(Utility.Language["BeatSecond"]);
            this.toolStripComboBox3.Items.Add(Utility.Language["BeatThird"]);
            this.toolStripComboBox3.Items.Add(Utility.Language["BeatFourth"]);
            this.toolStripComboBox3.Items.Add(Utility.Language["BeatFifth"]);
            toolStripComboBox3.SelectedIndex = prevSelectedIndex;
            this.toolStripTextBox3.ToolTip = Utility.Language["DisplayWidth"];
            //setlang
            sm.SetLang();
            画面比を169にMenuItem.Header = Utility.Language["ChangeAspect"];
            標準解像度にするMenuItem.Header = Utility.Language["GWDefaultResolution"];
            if (extraGameWindowMenuItems.Count == 2)
            {
                extraGameWindowMenuItems[0].Header = 画面比を169にMenuItem.Header;
                extraGameWindowMenuItems[1].Header = 標準解像度にするMenuItem.Header;
            }
            gc.SetLang();
            rm.SetLang();
            scm.SetLang();
        }

        private void ReadViews()
        {
            if (Directory.Exists("view"))
            {
                var regex = ViewLayoutFileRegex();
                foreach (string file in Directory.GetFiles("view"))
                {
                    var m = regex.Match(Path.GetFileName(file));
                    if (m.Success)
                    {
                        var tsmi = new MenuItem
                        {
                            Header = ToMenuText(String.Format("{0}x{1}{2}", m.Groups["w"].Value, m.Groups["h"].Value, m.Groups["extra"].Value))
                        };
                        tsmi.Click += Viewtsmi_Click;
                        tsmi.Tag = file;
                        ビューを読み込むToolStripMenuItem.Items.Add(tsmi);
                    }
                }
            }
        }

        void Viewtsmi_Click(object sender, EventArgs e)
        {
            var tsmi = sender as MenuItem;
            ReadDockXML(tsmi.Tag as string);
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
            foreach (var anchorable in AllAnchorables)
            {
                anchorable.Parent?.RemoveChild(anchorable);
            }
            var documentPane = new LayoutDocumentPane(dxfDock);
            var timeLinePane = new LayoutAnchorablePane(tlfDock) { DockHeight = new GridLength(300) };
            var propertyPane = new LayoutAnchorablePane(infofDock) { DockWidth = new GridLength(330) };
            var center = new LayoutPanel { Orientation = Orientation.Vertical };
            center.Children.Add(new LayoutDocumentPaneGroup(documentPane));
            center.Children.Add(timeLinePane);
            var root = new LayoutPanel { Orientation = Orientation.Horizontal };
            root.Children.Add(center);
            root.Children.Add(propertyPane);
            dockPanel1.Layout = new LayoutRoot { RootPanel = root };
            // ほかのウィンドウは右に置いて隠しておく（表示のメニューで出す）
            foreach (var anchorable in AllAnchorables.Where(a => a.Root == null))
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
            LayoutAnchorable Rebind(LayoutAnchorable original)
            {
                var found = anchorables.FirstOrDefault(a => a.ContentId == original.ContentId);
                if (found != null)
                {
                    found.Title = original.Title;
                    found.CanClose = false;
                    return found;
                }
                if (original.Root == null)
                {
                    original.AddToLayout(dockPanel1, AnchorableShowStrategy.Right);
                    original.Hide();
                }
                return original;
            }
            paalsDock = Rebind(paalsDock);
            keDock = Rebind(keDock);
            smDock = Rebind(smDock);
            ifwDock = Rebind(ifwDock);
            bmDock = Rebind(bmDock);
            tlfDock = Rebind(tlfDock);
            dxfDock = Rebind(dxfDock);
            infofDock = Rebind(infofDock);
            lmDock = Rebind(lmDock);
            emDock = Rebind(emDock);
            gcDock = Rebind(gcDock);
            mwDock = Rebind(mwDock);
            hfDock = Rebind(hfDock);
            rmDock = Rebind(rmDock);
            scmDock = Rebind(scmDock);
            stmDock = Rebind(stmDock);
            FixDockPanel = FixDockPanel;
        }

        private static IEnumerable<LayoutAnchorable> AllAnchorables
        {
            get
            {
                return [paalsDock, keDock, smDock, ifwDock, bmDock, tlfDock, dxfDock, infofDock, lmDock, emDock, gcDock, mwDock, hfDock, rmDock, scmDock, stmDock];
            }
        }

        /// <summary>
        /// ドックのウィンドウを作ります。ContentId は以前のクラスの名前（配置のファイルとの互換のため）。
        /// </summary>
        private static LayoutAnchorable CreateAnchorable(DockView view, string contentId)
        {
            var anchorable = new LayoutAnchorable
            {
                ContentId = contentId,
                Content = view,
                Title = view.TabText,
                CanClose = false
            };
            view.TabTextChanged += (sender, e) => GetAnchorableOf(view)?.Title = view.TabText;
            view.ShowRequested += (sender, e) =>
            {
                var target = GetAnchorableOf(view);
                if (target != null)
                {
                    target.Show();
                    target.IsActive = true;
                }
            };
            return anchorable;
        }

        /// <summary>
        /// 中身のドックのウィンドウを返す（配置を読み込むと作り直されるので、そのたびに探す）。
        /// </summary>
        private static LayoutAnchorable GetAnchorableOf(object content)
        {
            return AllAnchorables.FirstOrDefault(a => a?.Content == content);
        }

        private static void ChangeAuthorName()
        {
            var viewModel = new AuthorNameViewModel { AuthorName = PPDStaticSetting.AuthorName ?? "" };
            if (WpfDialog.ShowDialog(new AuthorNameWindow(viewModel)))
            {
                PPDStaticSetting.AuthorName = viewModel.AuthorName;
            }
        }

        private void InitializeDockWindow()
        {
            var path = Path.Combine(Utility.AppDir, dockxmlname);
            if (!File.Exists(path) || !ReadDockXML(path))
            {
                CreateDefaultLayout();
            }
            scm.RestoreDock(Path.Combine(Utility.AppDir, scriptdockxmlname));
            var textBrush = WpfSkin.TransparentTextBrush;
            this.toolStripStatusLabel1.Foreground = textBrush;
            this.toolStripStatusLabel2.Foreground = textBrush;
            this.toolStripStatusLabel3.Foreground = textBrush;
            this.ファイルToolStripMenuItem.Foreground = textBrush;
            this.編集ToolStripMenuItem.Foreground = textBrush;
            this.動画ToolStripMenuItem.Foreground = textBrush;
            this.表示ToolStripMenuItem.Foreground = textBrush;
            infof.SetSkin();
            ifw.SetSkin();
            ke.SetSkin();
            paals.SetSkin();
            sm.SetSkin();
            bm.SetSkin();
            tlf.SetSkin();
            gc.SetSkin();
        }

        private void 表示ToolStripMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            this.bPMToolStripMenuItem.IsChecked = DockUtility.IsShown(bmDock);
            this.ゲームウィンドウToolStripMenuItem.IsChecked = DockUtility.IsShown(dxfDock);
            this.サウンドマネージャーToolStripMenuItem.IsChecked = DockUtility.IsShown(smDock);
            this.タイムラインウィンドウToolStripMenuItem.IsChecked = DockUtility.IsShown(tlfDock);
            this.プロパティウィンドウToolStripMenuItem.IsChecked = DockUtility.IsShown(infofDock);
            this.位置角度ローダーセーバーToolStripMenuItem.IsChecked = DockUtility.IsShown(paalsDock);
            this.歌詞エディターToolStripMenuItem.IsChecked = DockUtility.IsShown(keDock);
            this.設定ファイルライターToolStripMenuItem.IsChecked = DockUtility.IsShown(ifwDock);
            this.レイヤーマネージャーToolStripMenuItem.IsChecked = DockUtility.IsShown(lmDock);
            this.イベントマネージャーToolStripMenuItem.IsChecked = DockUtility.IsShown(emDock);
            this.幾何クリエーターToolStripMenuItem.IsChecked = DockUtility.IsShown(gcDock);
            this.メモToolStripMenuItem.IsChecked = DockUtility.IsShown(mwDock);
            this.ヘルプToolStripMenuItem.IsChecked = DockUtility.IsShown(hfDock);
            this.リソースマネージャーToolStripMenuItem.IsChecked = DockUtility.IsShown(rmDock);
            this.スクリプトマネージャーToolStripMenuItem.IsChecked = DockUtility.IsShown(scmDock);
            this.統計ToolStripMenuItem.IsChecked = DockUtility.IsShown(stmDock);
        }

        /// <summary>
        /// 配置のファイルの名前（以前のクラスの名前）から、ドックのウィンドウを返す。
        /// </summary>
        private static LayoutAnchorable GetAnchorable(string persistentString)
        {
            return persistentString switch
            {
                "PPDEditor.BPMMeasure" => bmDock,
                "PPDEditor.DXForm" => dxfDock,
                "PPDEditor.InfoForm" => infofDock,
                "PPDEditor.KasiEditor" => keDock,
                "PPDEditor.PosAndAngleLoaderSaver" => paalsDock,
                "PPDEditor.SoundManager" => smDock,
                "PPDEditor.TimeLineForm" => tlfDock,
                "PPDEditor.IniFileWriter" => ifwDock,
                "PPDEditor.LayerManager" => lmDock,
                "PPDEditor.EventManager" => emDock,
                "PPDEditor.GeometryCreator" => gcDock,
                "PPDEditor.MemoWindow" => mwDock,
                "PPDEditor.HelpForm" => hfDock,
                "PPDEditor.ResourceManager" => rmDock,
                "PPDEditor.ScriptManager" => scmDock,
                "PPDEditor.StatsManager" => stmDock,
                _ => null,
            };
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

        private void MovieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Filter = openmoviefilter;
            this.openFileDialog1.RestoreDirectory = true;
            this.openFileDialog1.FileName = "";
            if (this.openFileDialog1.ShowDialog(this) == true)
            {
                ReadMovie(this.openFileDialog1.FileName);
            }
        }
        private void ReadMovie(string filename)
        {
            if (!MyGame.InitializeMovie(filename))
            {
                return;
            }
            UpdateMovieStatus();
            tlf.Seekmain.Length = MovieLength;
            infof.SetMovieInfo(MovieWidth.ToString(), MovieHeight.ToString(), GetTimeDisplay(MovieLength));

            this.プロジェクトを上書き保存ToolStripMenuItem.IsEnabled = this.難易度ToolStripMenuItem.IsEnabled =
                this.発行ToolStripMenuItem.IsEnabled = this.mODを発行ToolStripMenuItem.IsEnabled = IsProjectLoaded;

            moviefirstplayafterinitialized = true;
        }

        private static string GetMilliSecond(float ms)
        {
            var s = ms.ToString();
            string ret;
            if (s.Length == 1)
            {
                ret = "000";
            }
            else if (s.Length < 5)
            {
                ret = s[2..];
                while (ret.Length < 3)
                {
                    ret += "0";
                }
            }
            else
            {
                ret = s.Substring(2, 3);
            }
            return ret;
        }

        public void ChangeAreaSelect(bool enable)
        {
            if (enable)
            {
                this.toolStripStatusLabel2.Text = enabledareaselection;
            }
            else
            {
                this.toolStripStatusLabel2.Text = disabledareaselection;
            }
        }

        public void ChangeCopyBuffer(bool exist)
        {
            if (exist)
            {
                this.toolStripStatusLabel3.Text = existcopybuffer;
            }
            else
            {
                this.toolStripStatusLabel3.Text = "";
            }
        }

        public void Play()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(Play);
                return;
            }

            if (MovieInitialized)
            {
                if (moviefirstplayafterinitialized)
                {
                    moviefirstplayafterinitialized = false;
                    ChangeVolume(-1000);
                }
                this.MyGame.StartPlay();
                PlayMovie();
            }
        }

        public void Pause()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(Pause);
                return;
            }

            if (MovieInitialized)
            {
                PauseMovie();
                this.MyGame.StopPlay();
                this.MyGame.StopRecord();
            }
        }

        public void Stop()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(Stop);
                return;
            }

            if (MovieInitialized)
            {
                StopMovie();
                SeekMovie(0);
                PlayMovie();
                PauseMovie();
                this.MyGame.StopRecord();
                this.MyGame.StopPlay();
                tlf.Seekmain.Currenttime = 0;
                tlf.Seekmain.ResetScroll();
            }
        }

        public void Seek(double time)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => Seek(time));
                return;
            }

            SeekMovie(time);
        }

        private void 再生ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Play();
        }

        private void 一時停止ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Pause();
        }

        private void 停止ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void 打鍵記録ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MovieInitialized)
            {
                if (MyGame.Recording)
                {
                    MyGame.StopRecord();
                    PauseMovie();
                }
                else
                {
                    this.MyGame.StartRecord();
                    this.MyGame.StartPlay();
                    PlayMovie();
                    starttime = NativeMethods.timeGetTime();
                }
            }
        }


        public void ChangeVolume(int vol)
        {
            if (MovieInitialized)
            {
                MyGame.Movie.Volume = vol;
            }
        }

        public void UpdateStatus()
        {
            UpdateMovieStatus();
            if (NativeMethods.timeGetTime() - movielastime > 50)
            {
                tlf.Seekmain.Currenttime = MovieInitialized ? MoviePosition : 0;
                movielastime = NativeMethods.timeGetTime();
            }
        }

        private void UpdateMovieStatus()
        {
            if (MovieInitialized)
            {
                this.toolStripStatusLabel1.Text = GetTimeDisplay(MoviePosition);
            }
        }

        private static string GetTimeDisplay(double d)
        {
            int h = (int)d / 3600;
            int m = (int)(d - 3600 * h) / 60;
            var s = (int)(d - 3600 * h - 60 * m);
            var ms = (float)(d - 3600 * h - 60 * m - s);
            return h + ":" + (m >= 10 ? m.ToString() : "0" + m) + ":" + (s >= 10 ? s.ToString() : "0" + s) + ":" + GetMilliSecond(ms);
        }

        public void Refleshdata()
        {
            tlf.Seekmain.DrawAndRefresh();
        }

        private void 切り取りToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fm2 = new MovieCutViewModel(MovieTrimmingData);
            if (WpfDialog.ShowDialog(new MovieCutWindow(fm2)))
            {
                MovieTrimmingData = fm2.TrimmingData;
                if (fm2.ApplyToIni)
                {
                    ifw.SetTrimmingData(fm2.TrimmingData);
                }
                PPDStaticSetting.ApplyToIniMovieTrimming = fm2.ApplyToIni;
            }
        }

        public void Record(double time, int num)
        {
            tlf.Seekmain.AddData(time, num);
        }

        public bool[] Updatemark(double time)
        {
            return tlf.Seekmain.UpdateMark(time + PPDStaticSetting.MovieLatency);
        }

        public void DrawMark()
        {
            tlf.Seekmain.DrawMark();
        }

        public void SeekMovie(double time)
        {
            if (MovieInitialized)
            {
                MyGame.Movie.Seek(time);
            }
            UpdateMovieStatus();
        }

        public void SetMarkInfo(Vector2 vec, float rotation, uint id)
        {
            infof.SetMarkInfo(vec.ToString(), Math.Round((rotation / Math.PI * 180), 3).ToString(), id.ToString());
        }

        public void SetMarkInfo(Mark mark)
        {
            SetMarkInfo(mark.Position, mark.Rotation, mark.ID);
            infof.UpdateParameters([mark]);
        }

        public void SetMarksInfo(Mark[] marks)
        {
            infof.UpdateParameters(marks);
            if (marks.Length == 0)
            {
                infof.SetMarkInfo("", "", "");
            }
            else
            {
                SetMarkInfo(marks[0].Position, marks[0].Rotation, marks[0].ID);
            }
        }

        private void SoundsettxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.saveFileDialog1.Filter = opensoundsetfilter;
            this.saveFileDialog1.RestoreDirectory = true;
            this.saveFileDialog1.FileName = "soundset.txt";
            if (saveFileDialog1.ShowDialog(this) == true)
            {
                if (saveFileDialog1.FileName[^4..] != ".txt")
                {
                    string fn = saveFileDialog1.FileName;
                    var a = fn.IndexOf('.');
                    if (a != -1)
                    {
                        fn = fn[..a];
                    }
                    fn += ".txt";
                    saveFileDialog1.FileName = fn;
                }
                if (File.Exists(saveFileDialog1.FileName))
                {
                    File.Delete(saveFileDialog1.FileName);
                }
                SaveSoundSet(this.saveFileDialog1.FileName);
            }
        }

        private static void SaveSoundSet(string filename)
        {
            using StreamWriter sw = new(filename);
            sw.Write(sm.Savesoundset());
            sw.Close();
            sm.ContentSaved();
        }

        private void ScdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.saveFileDialog1.Filter = openscdfilter;
            this.saveFileDialog1.RestoreDirectory = true;
            this.saveFileDialog1.FileName = "new.scd";
            if (saveFileDialog1.ShowDialog(this) == true)
            {
                if (saveFileDialog1.FileName[^4..] != ".txt")
                {
                    string fn = saveFileDialog1.FileName;
                    var a = fn.IndexOf('.');
                    if (a != -1)
                    {
                        fn = fn[..a];
                    }
                    fn += ".scd";
                    saveFileDialog1.FileName = fn;
                }
                if (File.Exists(saveFileDialog1.FileName))
                {
                    File.Delete(saveFileDialog1.FileName);
                }
                SaveScd(this.saveFileDialog1.FileName);
            }
        }

        private static void SaveScd(string filename)
        {
            using (FileStream fs = File.Create(filename))
            {
                SCDWriter.Write(fs, sm.GetSCDData());
            }
            sm.ContentSaved();
        }

        private void PpdToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.saveFileDialog1.Filter = openppdfilter;
            this.saveFileDialog1.RestoreDirectory = true;
            this.saveFileDialog1.FileName = "NewFile.ppd";
            if (saveFileDialog1.ShowDialog(this) == true)
            {
                if (saveFileDialog1.FileName[^4..] != ".ppd")
                {
                    string fn = saveFileDialog1.FileName;
                    var a = fn.IndexOf('.');
                    if (a != -1)
                    {
                        fn = fn[..a];
                    }
                    fn += ".ppd";
                    saveFileDialog1.FileName = fn;
                }
                if (File.Exists(saveFileDialog1.FileName))
                {
                    File.Delete(saveFileDialog1.FileName);
                }
                Saveppdfile(this.saveFileDialog1.FileName, tlf.Seekmain.GetSorteData());
            }
        }

        private static void SavePpd(Stream stream, MarkData[] sorted)
        {
            PPDWriter.Write(stream, sorted);
            tlf.ContentSaved();
        }

        private static void Saveppdfile(string filename, MarkData[] sorted)
        {
            using FileStream fs = File.Create(filename);
            SavePpd(fs, sorted);
        }

        private void KasitxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.saveFileDialog1.Filter = opensoundsetfilter;
            this.saveFileDialog1.RestoreDirectory = true;
            this.saveFileDialog1.FileName = "kasi.txt";
            if (saveFileDialog1.ShowDialog(this) == true)
            {
                if (saveFileDialog1.FileName[^4..] != ".txt")
                {
                    string fn = saveFileDialog1.FileName;
                    var a = fn.IndexOf('.');
                    if (a != -1)
                    {
                        fn = fn[..a];
                    }
                    fn += ".txt";
                    saveFileDialog1.FileName = fn;
                }
                if (File.Exists(saveFileDialog1.FileName))
                {
                    File.Delete(saveFileDialog1.FileName);
                }
                SaveKasi(this.saveFileDialog1.FileName);
            }
        }

        private static void SaveKasi(string filename)
        {
            using StreamWriter sw = new(filename);
            sw.Write(ke.Savedata());
            sw.Close();
            ke.ContentSaved();
        }

        private void DatainiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.saveFileDialog1.Filter = openinifilter;
            this.saveFileDialog1.RestoreDirectory = true;
            this.saveFileDialog1.FileName = "data.ini";
            if (saveFileDialog1.ShowDialog(this) == true)
            {
                if (saveFileDialog1.FileName[^4..] != ".ini")
                {
                    string fn = saveFileDialog1.FileName;
                    var a = fn.IndexOf('.');
                    if (a != -1)
                    {
                        fn = fn[..a];
                    }
                    fn += ".ini";
                    saveFileDialog1.FileName = fn;
                }
                if (File.Exists(saveFileDialog1.FileName))
                {
                    File.Delete(saveFileDialog1.FileName);
                }
                SaveIni(this.saveFileDialog1.FileName);
            }
        }

        private static void SaveIni(string filename)
        {
            var sw = new SettingWriter(filename, true);
            ifw.SaveIni(sw);
            sw.Close();
            ifw.ContentSaved();
        }

        private void EvdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.saveFileDialog1.Filter = openevdfilter;
            this.saveFileDialog1.RestoreDirectory = true;
            this.saveFileDialog1.FileName = "NewFile.evd"; if (saveFileDialog1.ShowDialog(this) == true)
            {
                if (saveFileDialog1.FileName[^4..] != ".evd")
                {
                    string fn = saveFileDialog1.FileName;
                    var a = fn.IndexOf('.');
                    if (a != -1)
                    {
                        fn = fn[..a];
                    }
                    fn += ".evd";
                    saveFileDialog1.FileName = fn;
                }
                if (File.Exists(saveFileDialog1.FileName))
                {
                    File.Delete(saveFileDialog1.FileName);
                }
                SaveEvd(this.saveFileDialog1.FileName);
                em.ContentSaved();
            }
        }

        private static void SaveEvd(Stream stream)
        {
            EVDWriter.Write(stream, em.GetEvdData());
            em.ContentSaved();
        }

        private static void SaveEvd(string filename)
        {
            using FileStream fs = File.Create(filename);
            SaveEvd(fs);
        }

        private static void SaveLayers(string insideDirectory)
        {
            LayerViewModel[] lds = lm.AllLayerDisplay;
            if (!Directory.Exists(insideDirectory + "\\layers"))
            {
                Directory.CreateDirectory(insideDirectory + "\\layers");
            }
            for (int i = 0; i < lds.Length; i++)
            {
                if (File.Exists(insideDirectory + "\\layers" + "\\layer" + i + ".ppd"))
                {
                    File.Delete(insideDirectory + "\\layers" + "\\layer" + i + ".ppd");
                }
                Saveppdfile(insideDirectory + "\\layers" + "\\layer" + i + ".ppd", lds[i].PPDData.GetSortedDataAsMarkData());
            }
        }

        /// <summary>
        /// 保存する場所を選んで保存します。
        /// </summary>
        /// <returns>保存したかどうか（やめたときは false）。</returns>
        private bool CheckCanSaveAndSaveProject()
        {
            this.saveFileDialog1.Filter = openprojectfilter;
            this.saveFileDialog1.RestoreDirectory = true;
            if (this.projectfiledir != "")
            {
                this.saveFileDialog1.InitialDirectory = this.projectfiledir;
            }
            if (this.projectfilename != "")
            {
                this.saveFileDialog1.FileName = projectfilename;
            }
            else
            {
                this.saveFileDialog1.FileName = "New.ppdproj";
            }
            var ret = saveFileDialog1.ShowDialog(this) == true;
            if (ret)
            {
                if (saveFileDialog1.FileName[^8..] != ".ppdproj")
                {
                    string fn = saveFileDialog1.FileName;
                    var a = fn.IndexOf('.');
                    if (a != -1)
                    {
                        fn = fn[..a];
                    }
                    fn += ".ppdproj";
                    saveFileDialog1.FileName = fn;
                }
                this.projectfilename = Path.GetFileName(this.saveFileDialog1.FileName);
                this.projectfiledir = Path.GetDirectoryName(this.saveFileDialog1.FileName);
                SaveProject();
                this.プロジェクトを上書き保存ToolStripMenuItem.IsEnabled = this.難易度ToolStripMenuItem.IsEnabled
                    = this.発行ToolStripMenuItem.IsEnabled = this.mODを発行ToolStripMenuItem.IsEnabled = IsProjectLoaded;
                ChangeWindowText();
            }
            return ret;
        }

        private void SaveProject()
        {
            SaveProject(CurrentDifficulty, true);
        }

        private void SaveProject(AvailableDifficulty Difficulty, bool changeDifficulty)
        {
            var projectdir = Path.Combine(projectfiledir, Path.GetFileNameWithoutExtension(projectfilename));
            string projectData = "";
            if (File.Exists(Path.Combine(projectfiledir, projectfilename)))
            {
                var sr = new StreamReader(Path.Combine(projectfiledir, projectfilename));
                projectData = sr.ReadToEnd();
                sr.Close();
            }
            var setting = new SettingReader(projectData);
            var head = Difficulty.ToString();
            string headdot = head + ".";
            LayerViewModel[] lds = lm.AllLayerDisplay;
            if (Directory.Exists(Path.Combine(projectdir, head + "_" + "layers")))
            {
                Directory.Delete(Path.Combine(projectdir, head + "_" + "layers"), true);
            }
            Directory.CreateDirectory(Path.Combine(projectdir, head + "_" + "layers"));
            for (int i = 0; i < lds.Length; i++)
            {
                setting.ReplaceOrAdd(headdot + "layer" + i, lds[i].PPDData.DisplayName);
                setting.ReplaceOrAdd(headdot + "layer" + i + "BPM", lds[i].PPDData.BPM);
                setting.ReplaceOrAdd(headdot + "layer" + i + "BPMOffset", lds[i].PPDData.BPMStart);
                setting.ReplaceOrAdd(headdot + "layer" + i + "DisplayWidth", lds[i].PPDData.DisplayWidth);
                setting.ReplaceOrAdd(headdot + "layer" + i + "Visible", lds[i].DisplayVisible);
                setting.ReplaceOrAdd(headdot + "layer" + i + "DisplayMode", (int)lds[i].PPDData.DisplayMode);
                setting.ReplaceOrAdd(headdot + "layer" + i + "BeatType", (int)lds[i].PPDData.BeatType);
                setting.ReplaceOrAdd(headdot + "layer" + i + "MarkColor", lds[i].MarkColor.ToArgb());
                var writepath = Path.Combine(Path.Combine(projectdir, head + "_" + "layers"), "layer" + i + ".ppd");
                if (File.Exists(writepath))
                {
                    File.Delete(writepath);
                }
                Saveppdfile(writepath, lds[i].PPDData.GetSortedDataAsMarkData());
            }
            lm.ContentSaved();

            setting.ReplaceOrAdd(headdot + "selectedlayerindex", lm.SelectedLayerIndex);
            setting.ReplaceOrAdd(headdot + "bpm", tlf.Seekmain.BPM);
            setting.ReplaceOrAdd(headdot + "bpmstart", tlf.Seekmain.BPMSTART);
            double d = 0;
            if (MovieInitialized)
            {
                d = MoviePosition;
            }
            setting.ReplaceOrAdd(headdot + "lastseektime", d);
            setting.ReplaceOrAdd(headdot + "displaymode", (int)DisplayMode);
            setting.ReplaceOrAdd(headdot + "beattype", (int)BeatType);
            setting.ReplaceOrAdd(headdot + "bpmfixed", Bpmfixed);
            setting.ReplaceOrAdd(headdot + "drawtoggle", DrawToggle);
            setting.ReplaceOrAdd(headdot + "showholdextent", ShowHoldExtent);
            setting.ReplaceOrAdd(headdot + "markfocus", MarkFocus);
            setting.ReplaceOrAdd(headdot + "pandadisplayangle", paals.DisplayAngle);
            setting.ReplaceOrAdd(headdot + "displaywidth", DisplayWidth);
            setting.ReplaceOrAdd(headdot + "speedscale", SpeedScale);
            setting.ReplaceOrAdd(headdot + "fixeddistance", gc.FixedDistance);
            setting.ReplaceOrAdd(headdot + "antialias", gc.Antialias);
            setting.ReplaceOrAdd(headdot + "drawpos", gc.DrawPos);
            setting.ReplaceOrAdd(headdot + "drawangle", gc.DrawAngle);
            setting.ReplaceOrAdd(headdot + "applymoved", gc.ApplyMoved);
            setting.ReplaceOrAdd(headdot + "gridwidth", Grid.Width);
            setting.ReplaceOrAdd(headdot + "gridheight", Grid.Height);
            setting.ReplaceOrAdd(headdot + "gridoffsetx", Grid.OffsetX);
            setting.ReplaceOrAdd(headdot + "gridoffsety", Grid.OffsetY);
            setting.ReplaceOrAdd(headdot + "gridcolor", System.Drawing.ColorTranslator.ToHtml(Grid.GridColor));
            setting.ReplaceOrAdd(headdot + "displaygrid", DisplayGrid);
            setting.ReplaceOrAdd(headdot + "gcgridwidth", gc.SquareGrid.Width);
            setting.ReplaceOrAdd(headdot + "gcgridheight", gc.SquareGrid.Height);
            setting.ReplaceOrAdd(headdot + "gcgridoffsetx", gc.SquareGrid.OffsetX);
            setting.ReplaceOrAdd(headdot + "gcgridoffsety", gc.SquareGrid.OffsetY);
            setting.ReplaceOrAdd(headdot + "gcgridcolor", System.Drawing.ColorTranslator.ToHtml(gc.SquareGrid.GridColor));
            setting.ReplaceOrAdd(headdot + "gcdisplaygrid", gc.SquareGridEnabled);
            setting.ReplaceOrAdd(headdot + "farness", Farness);
            setting.ReplaceOrAdd(headdot + "anglerestriction", gc.AngleRestriction);
            setting.ReplaceOrAdd(headdot + "disabledscript", scm.GetDisableScriptList());
            setting.ReplaceOrAdd(headdot + "connect", Connect);
            setting.ReplaceOrAdd(headdot + "modlist", ModList);
            setting.ReplaceOrAdd(headdot + "disableexpansion", DisableExpansion);

            setting.ReplaceOrAdd("memo", mw.MemoText.Replace("[", "\\[").Replace("]", "\\]"));
            mw.ContentSaved();
            setting.ReplaceOrAdd("version", latestprojectversion);
            if (changeDifficulty)
            {
                setting.ReplaceOrAdd("selecteddifficulty", CurrentDifficulty.ToString());
            }
            setting.ReplaceOrAdd("disabledresources", rm.GetDisableList());
            setting.ReplaceOrAdd("timelineroworders", String.Join(",", tlf.RowManager.RowOrders));
            setting.ReplaceOrAdd("timelinerowvisibilities", String.Join(",", tlf.RowManager.RowVisibilities.Select(r => r ? 1 : 0)));
            setting.ReplaceOrAdd("timelinerowlimited", IsTimeLineRowLimited);

            var sw = new SettingWriter(Path.Combine(projectfiledir, projectfilename), false);
            foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
            {
                sw.Write(kvp.Key, kvp.Value);
            }
            sw.Close();

            NonExistCreateDirectory(projectdir);
            ExistDeleteFile(Path.Combine(projectdir, "kasi.txt"));
            SaveKasi(Path.Combine(projectdir, "kasi.txt"));
            ExistDeleteFile(Path.Combine(projectdir, "data.ini"));
            SaveIni(Path.Combine(projectdir, "data.ini"));
            ExistDeleteFile(Path.Combine(projectdir, "soundset.txt"));
            SaveSoundSet(Path.Combine(projectdir, "soundset.txt"));
            ExistDeleteFile(Path.Combine(projectdir, headdot + "scd"));
            SaveScd(Path.Combine(projectdir, headdot + "scd"));

            ExistDeleteFile(Path.Combine(projectdir, headdot + "ppd"));
            scm.SaveAll();
            var pw = new PackWriter(Path.Combine(projectdir, headdot + "ppd"));
            var packFileList = new List<string>(scm.GetEnabledScriptList());
            packFileList.AddRange(["ppd", "evd"]);
            foreach (PPDPackStreamWriter ppdpsw in pw.Write([.. packFileList]))
            {
                switch (ppdpsw.Filename)
                {
                    case "ppd":
                        SavePpd(ppdpsw, lm.GetAllData(false));
                        break;
                    case "evd":
                        SaveEvd(ppdpsw);
                        break;
                    default:
                        scm.WriteScript(ppdpsw, ppdpsw.Filename);
                        break;
                }
            }
            pw.Close();
            scm.ContentSaved();

            pw = new PackWriter(Path.Combine(projectdir, "resource.pak"));
            foreach (PPDPackStreamWriter ppdpsw in pw.Write(rm.GetEnabledList()))
            {
                rm.WriteResource(ppdpsw, ppdpsw.Filename);
            }
            pw.Close();
            rm.ContentSaved();

            var fns = Directory.GetFiles(projectdir);
            NonExistCreateDirectory(Path.Combine(projectdir, "sound"));
            List<string> datas = sm.Sounddatapaths;
            for (int i = 1; i < datas.Count; i++)
            {
                if (!ExDirectory.Contains(Path.Combine(projectdir, "sound"), Path.GetFileName(datas[i])))
                {
                    if (File.Exists(datas[i]))
                    {
                        File.Copy(datas[i], Path.Combine(Path.Combine(projectdir, "sound"), Path.GetFileName(datas[i])), true);
                    }
                    else
                    {
                        MessageBox.Show(this, (datas[i]) + nosoundfile);
                    }
                }
            }
            var filenames = Directory.GetFiles(Path.Combine(projectdir, "sound"));
            for (int i = 0; i < filenames.Length; i++)
            {
                bool ok = false;
                for (int j = 1; j < datas.Count; j++)
                {
                    if (Path.GetFileName(filenames[i]) == Path.GetFileName(datas[j]))
                    {
                        ok = true;
                        break;
                    }
                }
                if (!ok)
                {
                    File.Delete(filenames[i]);
                }
            }
            bool found = false;
            for (int i = 0; i < fns.Length; i++)
            {
                if (Path.GetFileNameWithoutExtension(fns[i]) == "movie")
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                File.Copy(MovieFileName, Path.Combine(projectdir, "movie" + Path.GetExtension(MovieFileName)), true);
            }
        }

        private static void ExistDeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static void NonExistCreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void KasitxtToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Filter = opensoundsetfilter;
            this.openFileDialog1.RestoreDirectory = true;
            this.openFileDialog1.FileName = "kasi.txt";
            if (this.openFileDialog1.ShowDialog(this) == true)
            {
                ReadKasi(this.openFileDialog1.FileName);
            }
        }

        private static void ReadKasi(string filename)
        {
            using StreamReader sr = new(filename);
            ke.Setdata(sr.ReadToEnd());
            sr.Close();
            ke.ContentSaved();
        }
        private void PpdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Filter = openppdfilter;
            this.openFileDialog1.RestoreDirectory = true;
            this.openFileDialog1.FileName = "";
            if (this.openFileDialog1.ShowDialog(this) == true)
            {
                ReadPpdAsPack(this.openFileDialog1.FileName);
            }
        }

        private void ReadPpdAsPack(string filename)
        {
            try
            {
                using PackReader reader = new(filename);
                var ppdpsr = reader.Read("ppd");
                ReadPpd(ppdpsr);
            }
            catch
            {
                ReadPpd(filename);
            }
        }
        private void ReadPpd(string filename)
        {
            using FileStream fs = File.Open(filename, FileMode.Open);
            ReadPpd(fs);
        }

        private void ReadPpd(Stream stream)
        {
            bool signError = false;
            var markData = PPDReader.Read(stream, ref signError);
            if (signError)
            {
                MessageBox.Show(this, invaliddata);
                return;
            }

            foreach (MarkDataBase data in markData)
            {
                if (data is ExMarkData)
                {
                    tlf.Seekmain.ExAddData(data.Time, (data as ExMarkData).EndTime, data.X, data.Y, data.Angle, (int)data.ButtonType, data.ID, data.Parameters, false);
                }
                else if (data is MarkData)
                {
                    tlf.Seekmain.AddData(data.Time, data.X, data.Y, data.Angle, (int)data.ButtonType, data.ID, data.Parameters, false);
                }
            }
            tlf.Seekmain.ClearHistory();
            tlf.Seekmain.DrawAndRefresh();
            tlf.ContentSaved();
        }

        private void DatainiToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Filter = openinifilter;
            this.openFileDialog1.RestoreDirectory = true;
            this.openFileDialog1.FileName = "data.ini";
            if (this.openFileDialog1.ShowDialog(this) == true)
            {
                ReadIni(this.openFileDialog1.FileName);
            }
        }

        private void ReadIni(string filename)
        {
            using StreamReader sr = new(filename);
            var s = sr.ReadToEnd();
            sr.Close();
            var setting = new SettingReader(s);
            float[] times = new float[4];
            string[] diffs = new string[4];
            string guid;
            string bpmstring;
            float.TryParse(setting.ReadString("thumbtimestart"), NumberStyles.Float, CultureInfo.InvariantCulture, out times[0]);
            float.TryParse(setting.ReadString("thumbtimeend"), NumberStyles.Float, CultureInfo.InvariantCulture, out times[1]);
            float.TryParse(setting.ReadString("start"), NumberStyles.Float, CultureInfo.InvariantCulture, out times[2]);
            float.TryParse(setting.ReadString("end"), NumberStyles.Float, CultureInfo.InvariantCulture, out times[3]);
            float.TryParse(setting.ReadString("bpm"), NumberStyles.Float, CultureInfo.InvariantCulture, out float bpm);
            diffs[0] = setting.ReadString("difficulty easy");
            diffs[1] = setting.ReadString("difficulty normal");
            diffs[2] = setting.ReadString("difficulty hard");
            diffs[3] = setting.ReadString("difficulty extreme");
            float.TryParse(setting.ReadString("moviecutleft"), NumberStyles.Float, CultureInfo.InvariantCulture, out float left);
            float.TryParse(setting.ReadString("moviecutright"), NumberStyles.Float, CultureInfo.InvariantCulture, out float right);
            float.TryParse(setting.ReadString("moviecuttop"), NumberStyles.Float, CultureInfo.InvariantCulture, out float top);
            float.TryParse(setting.ReadString("moviecutbottom"), NumberStyles.Float, CultureInfo.InvariantCulture, out float bottom);
            guid = setting.ReadString("guid");
            bpmstring = setting.ReadString("bpmstring");
            MovieTrimmingData = new MovieTrimmingData(top, left, right, bottom);
            ifw.SetIni(times, diffs, bpm, bpmstring, MovieTrimmingData);
            if (guid != "")
            {
                ifw.SongGuid = new Guid(guid);
            }
            ifw.ContentSaved();
        }

        private static void ReadSoundSet(string filename)
        {
            using StreamReader sr = new(filename);
            var insidedirectory = Path.GetDirectoryName(filename);
            var s = sr.ReadToEnd().Replace("\r\n", "\r").Replace("\r", "\n");
            sr.Close();
            if (s.Length == 0) return;
            if (s[^1] == '\n')
            {
                s = s[..^1];
            }
            var pathes = s.Split('\n');
            for (int i = 0; i < pathes.Length; i++)
            {
                pathes[i] = insidedirectory + "\\sound\\" + pathes[i];
            }
            sm.Readsounds(pathes);
            sm.ContentSaved();
        }

        private static void ReadScd(string filename)
        {
            var scdData = SCDReader.Read(filename);
            if (scdData.Length == 0)
            {
                return;
            }
            float lastTime = scdData[0].Time;
            ushort[] changes = new ushort[10];
            var times = new List<float>();
            var soundChanges = new List<ushort[]>();
            if (lastTime > 0)
            {
                times.Add(0);
                ushort[] trueChange = new ushort[10];
                Array.Copy(changes, trueChange, 10);
                soundChanges.Add(trueChange);
            }
            foreach (SCDData scd in scdData)
            {
                if (lastTime != scd.Time)
                {
                    times.Add(lastTime);
                    ushort[] trueChange = new ushort[10];
                    Array.Copy(changes, trueChange, 10);
                    soundChanges.Add(trueChange);
                }
                lastTime = scd.Time;
                changes[(int)scd.ButtonType] = scd.SoundIndex;
            }
            times.Add(lastTime);
            ushort[] truec = new ushort[10];
            Array.Copy(changes, truec, 10);
            soundChanges.Add(truec);
            sm.Readchangedata([.. times], [.. soundChanges]);
            sm.ContentSaved();
        }

        private void EvdToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Filter = openevdfilter;
            this.openFileDialog1.RestoreDirectory = true;
            this.openFileDialog1.FileName = "";
            if (this.openFileDialog1.ShowDialog(this) == true)
            {
                ReadEvdAsPack(this.openFileDialog1.FileName);
            }
        }

        private static SortedList<float, EventData> ReadEvdAsEventData(Stream stream)
        {
            var ret = new SortedList<float, EventData>();
            var evdData = EVDReader.Read(stream);
            if (evdData.Length == 0)
            {
                return ret;
            }
            float lasttime = evdData[0].Time;
            var evdata = new EventData
            {
                BPM = ifw.BPM
            };
            foreach (IEVDData eventData in evdData)
            {
                if (lasttime != eventData.Time)
                {
                    ret.Add(lasttime, evdata);
                    evdata = evdata.Clone();
                }
                lasttime = eventData.Time;
                switch (eventData.EventType)
                {
                    case PPDFramework.PPDStructure.EVDData.EventType.ChangeVolume:
                        evdata.SetVolume((eventData as ChangeVolumeEvent).Channel, (eventData as ChangeVolumeEvent).Volume);
                        break;
                    case PPDFramework.PPDStructure.EVDData.EventType.ChangeBPM:
                        evdata.BPM = (eventData as ChangeBPMEvent).BPM;
                        evdata.BPMRapidChange = false;
                        break;
                    case PPDFramework.PPDStructure.EVDData.EventType.RapidChangeBPM:
                        evdata.BPM = (eventData as RapidChangeBPMEvent).BPM;
                        evdata.BPMRapidChange = (eventData as RapidChangeBPMEvent).Rapid;
                        break;
                    case PPDFramework.PPDStructure.EVDData.EventType.ChangeSoundPlayMode:
                        evdata.SetKeepPlaying((eventData as ChangeSoundPlayModeEvent).Channel, (eventData as ChangeSoundPlayModeEvent).KeepPlaying);
                        break;
                    case PPDFramework.PPDStructure.EVDData.EventType.ChangeDisplayState:
                        evdata.DisplayState = (eventData as ChangeDisplayStateEvent).DisplayState;
                        break;
                    case PPDFramework.PPDStructure.EVDData.EventType.ChangeMoveState:
                        evdata.MoveState = (eventData as ChangeMoveStateEvent).MoveState;
                        break;
                    case PPDFramework.PPDStructure.EVDData.EventType.ChangeReleaseSound:
                        evdata.SetReleaseSound((eventData as ChangeReleaseSoundEvent).Channel, (eventData as ChangeReleaseSoundEvent).ReleaseSound);
                        break;
                    case PPDFramework.PPDStructure.EVDData.EventType.ChangeNoteType:
                        evdata.NoteType = (eventData as ChangeNoteTypeEvent).NoteType;
                        break;
                    case PPDFramework.PPDStructure.EVDData.EventType.ChangeSlideScale:
                        evdata.SlideScale = (eventData as ChangeSlideScaleEvent).SlideScale;
                        break;
                    case PPDFramework.PPDStructure.EVDData.EventType.ChangeInitializeOrder:
                        evdata.InitializeOrder = new ButtonType[10];
                        for (int i = 0; i < evdata.InitializeOrder.Length; i++)
                        {
                            evdata.InitializeOrder[i] = (eventData as ChangeInitializeOrderEvent).InitializeOrder[i];
                        }
                        break;
                }
            }
            ret.Add(lasttime, evdata);
            return ret;
        }

        private static void ReadEvd(Stream stream)
        {
            em.Clear();
            var data = ReadEvdAsEventData(stream);
            foreach (KeyValuePair<float, EventData> kvp in data)
            {
                em.AddChange(kvp.Key, kvp.Value);
            }
            em.ContentSaved();
        }

        private static void ReadEvdAsPack(string filename)
        {
            try
            {
                using PackReader reader = new(filename);
                var ppdpsr = reader.Read("evd");
                ReadEvd(ppdpsr);
            }
            catch
            {
                ReadEvd(filename);
            }
        }

        private static void ReadEvd(string filename)
        {
            using FileStream fs = File.Open(filename, FileMode.Open);
            ReadEvd(fs);
        }

        private void プロジェクトを開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Filter = openprojectfilter;
            this.openFileDialog1.RestoreDirectory = true;
            this.openFileDialog1.FileName = "";
            if (this.openFileDialog1.ShowDialog(this) == true)
            {
                CheckReadProject(this.openFileDialog1.FileName);
            }
        }

        private void CheckReadProject(string path)
        {
            if (!CheckSaveStateAndSave(false))
            {
                return;
            }

            if (CheckProjectVersion(path))
            {
                ReadProject(path);
            }
        }

        private bool CheckProjectVersion(string path)
        {
            var version = GetProjectVersion(path);
            bool convert = version != latestprojectversion;
            if (!convert)
            {
                return true;
            }
            else
            {
                if (MessageBox.Show(this, oldprojectdetected, confirm, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    BackUpProject(path);
                    ConvertProject(path, version);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static int GetProjectVersion(string path)
        {
            var sr = new StreamReader(path);
            var setting = new SettingReader(sr.ReadToEnd());
            sr.Close();
            var version = setting.ReadString("version");
            return version switch
            {
                "" => 1,
                _ => int.Parse(version),
            };
        }

        private static void BackUpProject(string path)
        {
            var filename = Path.GetFileNameWithoutExtension(path);
            var parentdirectory = Path.GetDirectoryName(path);
            string directory = Path.Combine(parentdirectory, filename) + "_backup";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            // copy projectfile
            File.Copy(path, Path.Combine(directory, Path.GetFileName(path)), true);

            var copydir = Path.Combine(directory, filename);

            if (!Directory.Exists(copydir))
            {
                Directory.CreateDirectory(copydir);
            }
            Utility.CopyDirectory(Path.Combine(parentdirectory, filename), copydir, [MovieFileRegex()]);
        }

        private static void ConvertProject(string path, int version)
        {
            if (version <= 1) ConvertProject1TO2(path);
        }

        private static void ConvertProject1TO2(string path)
        {
            var sr = new StreamReader(path);
            var setting = new SettingReader(sr.ReadToEnd());
            sr.Close();
            var sw = new SettingWriter(path, false);
            var head = AvailableDifficulty.Base.ToString();
            foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
            {
                sw.Write(head + "." + kvp.Key, kvp.Value);
            }
            // rename folder(layers->Base_layers)
            var projectdir = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
            var dir = Path.Combine(projectdir, "layers");
            var newdir = Path.Combine(projectdir, head + "_" + "layers");
            if (Directory.Exists(dir))
            {
                Directory.Move(dir, newdir);
            }
            else
            {
                var filePath = Path.Combine(projectdir, head + ".ppd");
                NonExistCreateDirectory(newdir);
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, Path.Combine(newdir, "layer0.ppd"), true);
                }
                sw.Write("Base.layer0", "Layer0");
                sw.Write("Base.layer0BPM", setting.ReadString("bpm"));
                sw.Write("Base.layer0BPMOffset", setting.ReadString("bpmstart"));
                sw.Write("Base.layer0DisplayWidth", 100);
                sw.Write("Base.layer0Visible", 1);
            }
            sw.Write("version", latestprojectversion.ToString());
            sw.Write("selecteddifficulty", head);
            sw.Close();

            // merge ppd and evd
            var tempfilename = Path.Combine(projectdir, Path.GetRandomFileName());
            var pw = new PackWriter(tempfilename);
            foreach (PPDPackStreamWriter ppdpsw in pw.Write(["ppd", "evd"]))
            {
                string filepath = string.Empty;
                switch (ppdpsw.Filename)
                {
                    case "ppd":
                        filepath = Path.Combine(projectdir, head + ".ppd");
                        break;
                    case "evd":
                        filepath = Path.Combine(projectdir, head + ".evd");
                        break;
                }
                if (File.Exists(filepath))
                {
                    var fs = File.Open(filepath, FileMode.Open);
                    byte[] data = new byte[fs.Length];
                    fs.ReadAtLeast(data, data.Length, throwOnEndOfStream: false);
                    fs.Close();
                    ppdpsw.Write(data, 0, data.Length);
                    File.Delete(filepath);
                }
            }
            pw.Close();
            File.Move(tempfilename, Path.Combine(projectdir, head + ".ppd"));
        }


        public static AvailableDifficulty GetDifficultyFromString(string s)
        {
            s = s.ToLower();
            return s switch
            {
                "base" => AvailableDifficulty.Base,
                "easy" => AvailableDifficulty.Easy,
                "normal" => AvailableDifficulty.Normal,
                "hard" => AvailableDifficulty.Hard,
                "extreme" => AvailableDifficulty.Extreme,
                _ => AvailableDifficulty.None,
            };
        }

        private void ClearData()
        {
            IsTimeLineRowLimited = false;
            tlf.RowManager.Restore();
            tlf.Seekmain.DeleteData(-1);
            ke.Clear();
            sm.Clear();
            lm.Clear();
            em.Clear();
            ifw.Clear();
            rm.Clear();
            scm.Clear();
            mw.Clear();

            BPM = 100;
            BPMOffset = 0;
            DisplayMode = 0;
            Bpmfixed = true;
            ShowHoldExtent = true;
            SpeedScale = 2;
            DisplayWidth = 240;
            CurrentDifficulty = AvailableDifficulty.Base;
            AvailableDifficulty = AvailableDifficulty.Base;
        }

        private void ReadMovieFromProject()
        {
            var file = Directory.GetFiles(CurrentProjectDir).FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == "movie");
            if (!String.IsNullOrEmpty(file))
            {
                ReadMovie(file);
            }
        }

        private void ReadProject(string path)
        {
            ClearData();
            this.projectfilename = Path.GetFileName(path);
            this.projectfiledir = Path.GetDirectoryName(path);
            ReadMovieFromProject();
            if (File.Exists(Path.Combine(CurrentProjectDir, "kasi.txt")))
            {
                ReadKasi(Path.Combine(CurrentProjectDir, "kasi.txt"));
            }
            if (File.Exists(Path.Combine(CurrentProjectDir, "data.ini")))
            {
                ReadIni(Path.Combine(CurrentProjectDir, "data.ini"));
            }
            if (File.Exists(Path.Combine(CurrentProjectDir, "soundset.txt")))
            {
                ReadSoundSet(Path.Combine(CurrentProjectDir, "soundset.txt"));
            }
            var sr = new StreamReader(path);
            var setting = new SettingReader(sr.ReadToEnd());
            sr.Close();
            //check project version
            var difficulty = GetDifficultyFromString(setting.ReadString("selecteddifficulty"));
            CurrentDifficulty = difficulty;
            CheckDifficultyAvailable(CurrentProjectDir);
            mw.MemoText = setting.ReadString("memo").Replace("\n", System.Environment.NewLine);
            mw.ContentSaved();
            rm.ReadResource(CurrentProjectDir, setting.ReadString("disabledresources"));
            var temp = setting.ReadString("timelineroworders");
            if (!String.IsNullOrEmpty(temp))
            {
                var array = temp.Split(',').Select(r => int.Parse(r)).ToArray();
                if (array.Length == 10 && array.All(r => r >= 0 && r < 10) && array.Distinct().Count() == 10)
                {
                    tlf.RowManager.RowOrders = array;
                }
            }
            temp = setting.ReadString("timelinerowvisibilities");
            if (!String.IsNullOrEmpty(temp))
            {
                var array = temp.Split(',').Select(r => int.Parse(r) == 1).ToArray();
                if (array.Length == 10)
                {
                    tlf.RowManager.RowVisibilities = array;
                }
            }
            IsTimeLineRowLimited = setting.ReadBoolean("timelinerowlimited");
            ReadSpecificDifficulty(setting, difficulty, CurrentProjectDir);
            rm.ContentSaved();

            AddRecentUsedProject(path);
            SaveRecentUsedProject();
            ChangeWindowText();
        }

        private void ReadSpecificDifficulty()
        {
            var sr = new StreamReader(Path.Combine(projectfiledir, projectfilename));
            var setting = new SettingReader(sr.ReadToEnd());
            sr.Close();
            ReadSpecificDifficulty(setting, CurrentDifficulty, Path.Combine(projectfiledir, Path.GetFileNameWithoutExtension(projectfilename)));
        }

        private void ReadSpecificDifficulty(SettingReader setting, AvailableDifficulty difficulty, string projectdir)
        {
            var head = difficulty.ToString();
            string headdot = head + ".";

            sm.ClearSetting();
            if (File.Exists(Path.Combine(projectdir, headdot + "scd")))
            {
                ReadScd(Path.Combine(projectdir, headdot + "scd"));
            }

            if (File.Exists(Path.Combine(projectdir, headdot + "ppd")))
            {
                var pr = new PackReader(Path.Combine(projectdir, headdot + "ppd"));
                using (Stream stream = pr.Read("evd"))
                {
                    ReadEvd(stream);
                }
                pr.Close();
            }

            scm.Clear();
            scm.ReadScript(projectdir, difficulty, setting.ReadString(headdot + "disabledscript"));
            scm.ContentSaved();

            // layer data
            int iter = 0;
            lm.Clear();
            string headlayer = headdot + "layer";
            int iTemp;
            DisplayLineMode displayMode;
            DisplayBeatType beatType;
            while (true)
            {
                var layername = setting.ReadString(headlayer + iter);
                var ppdfilePath = Path.Combine(Path.Combine(projectdir, head + "_" + "layers"), "layer" + iter + ".ppd");
                if (layername == "" || !File.Exists(ppdfilePath)) break;
                float.TryParse(setting.ReadString(headlayer + iter + "BPM"), NumberStyles.Float, CultureInfo.InvariantCulture, out float layerbpm);
                float.TryParse(setting.ReadString(headlayer + iter + "BPMOffset"), NumberStyles.Float, CultureInfo.InvariantCulture, out float layerbpmoffset);
                int.TryParse(setting.ReadString(headlayer + iter + "DisplayWidth"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int layerdisplaywidth);
                bool visible = setting.ReadString(headlayer + iter + "Visible") == "1";
                int.TryParse(setting.ReadString(headlayer + iter + "DisplayMode"), NumberStyles.Integer, CultureInfo.InvariantCulture, out iTemp);
                displayMode = (DisplayLineMode)iTemp;
                if (!int.TryParse(setting.ReadString(headlayer + iter + "BeatType"), NumberStyles.Integer, CultureInfo.InvariantCulture, out iTemp))
                {
                    iTemp = (int)DisplayBeatType.Fourth;
                }
                beatType = (DisplayBeatType)iTemp;
                lm.AddLayer(layername, layerbpm, layerbpmoffset, layerdisplaywidth, visible, displayMode, beatType);
                lm.SelectLastLayer();
                ReadPpd(ppdfilePath);
                if (int.TryParse(setting.ReadString(headlayer + iter + "MarkColor"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int markColor))
                {
                    lm.AllLayerDisplay[lm.SelectedLayerIndex].MarkColor = System.Drawing.Color.FromArgb(markColor);
                }
                iter++;
            }

            if (!int.TryParse(setting.ReadString(headdot + "selectedlayerindex"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int selectedlayerindex))
            {
                selectedlayerindex = -1;
            }
            lm.SelectLayerByIndex(selectedlayerindex);
            lm.CalculateMaxID();

            if (!float.TryParse(setting.ReadString(headdot + "bpm"), NumberStyles.Float, CultureInfo.InvariantCulture, out float bpm))
            {
                bpm = 100;
            }
            if (!float.TryParse(setting.ReadString(headdot + "bpmstart"), NumberStyles.Float, CultureInfo.InvariantCulture, out float bpmstart))
            {
                bpmstart = 0;
            }
            if (!int.TryParse(setting.ReadString(headdot + "displaymode"), NumberStyles.Integer, CultureInfo.InvariantCulture, out iTemp))
            {
                iTemp = (int)DisplayLineMode.Fourth;
            }
            displayMode = (DisplayLineMode)iTemp;
            if (!int.TryParse(setting.ReadString(headdot + "beattype"), NumberStyles.Integer, CultureInfo.InvariantCulture, out iTemp))
            {
                iTemp = (int)DisplayBeatType.Fourth;
            }
            beatType = (DisplayBeatType)iTemp;
            bool bpmfixed = setting.ReadString(headdot + "bpmfixed") == "1";
            bool drawtoggle = setting.ReadString(headdot + "drawtoggle") == "1";
            bool markfocus = setting.ReadString(headdot + "markfocus") == "1";
            bool connect = setting.ReadString(headdot + "connect") == "1";
            bool showHoldExtent = setting.ReadString(headdot + "showholdextent") == "1";
            bool disableExpansion = setting.ReadString(headdot + "disableexpansion") == "1";
            var modList = setting.ReadString(headdot + "modlist");
            if (!int.TryParse(setting.ReadString(headdot + "speedscale"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int speedscale))
            {
                speedscale = 0;
            }
            var dw = setting.ReadString(headdot + "displaywidth");
            int displaywidth = IsNumber(dw) ? int.Parse(dw, CultureInfo.InvariantCulture) : 240;
            if (setting.ReadString(headdot + "layer0") == "")
            {
                this.BPM = bpm;
                this.BPMOffset = bpmstart;
                this.DisplayMode = displayMode;
                this.Bpmfixed = bpmfixed;
                this.SpeedScale = speedscale;
                this.DisplayWidth = displaywidth;
                this.BeatType = beatType;
                this.ShowHoldExtent = showHoldExtent;
            }
            else
            {
                this.Bpmfixed = bpmfixed;
                this.SpeedScale = speedscale;
                this.DisplayMode = displayMode;
                this.BeatType = beatType;
                this.ShowHoldExtent = showHoldExtent;
            }
            this.DrawToggle = drawtoggle;
            this.MarkFocus = markfocus;
            this.Connect = connect;
            this.ModList = modList;
            this.DisableExpansion = disableExpansion;
            double.TryParse(setting.ReadString(headdot + "lastseektime"), NumberStyles.Float, CultureInfo.InvariantCulture, out double seektime);
            if (MovieInitialized)
            {
                this.SeekMovie(seektime);
                tlf.Seekmain.Currenttime = seektime;
            }
            int.TryParse(setting.ReadString(headdot + "fixeddistance"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int fixeddistance);
            gc.FixedDistance = fixeddistance;
            gc.Antialias = setting.ReadString(headdot + "antialias") == "1";
            gc.DrawPos = setting.ReadString(headdot + "drawpos") == "1";
            gc.DrawAngle = setting.ReadString(headdot + "drawangle") == "1";
            gc.ApplyMoved = setting.ReadString(headdot + "applymoved") == "1";
            if (!int.TryParse(setting.ReadString(headdot + "gridwidth"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int gridwidth)) gridwidth = 5;
            if (!int.TryParse(setting.ReadString(headdot + "gridheight"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int gridheight)) gridheight = 5;
            if (!int.TryParse(setting.ReadString(headdot + "gridoffsetx"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int gridoffsetx)) gridoffsetx = 0;
            if (!int.TryParse(setting.ReadString(headdot + "gridoffsety"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int gridoffsety)) gridoffsety = 0;
            if (!float.TryParse(setting.ReadString(headdot + "farness"), NumberStyles.Integer, CultureInfo.InvariantCulture, out float farness)) farness = 0.5f;
            Farness = farness;
            try
            {
                Grid.GridColor = System.Drawing.ColorTranslator.FromHtml(setting.ReadString(headdot + "gridcolor"));
            }
            catch
            {
            }
            Grid.Width = gridwidth;
            Grid.Height = gridheight;
            Grid.OffsetX = gridoffsetx;
            Grid.OffsetY = gridoffsety;
            DisplayGrid = setting.ReadString(headdot + "displaygrid") == "1";

            if (!int.TryParse(setting.ReadString(headdot + "gcgridwidth"), NumberStyles.Integer, CultureInfo.InvariantCulture, out gridwidth)) gridwidth = 25;
            if (!int.TryParse(setting.ReadString(headdot + "gcgridheight"), NumberStyles.Integer, CultureInfo.InvariantCulture, out gridheight)) gridheight = 25;
            if (!int.TryParse(setting.ReadString(headdot + "gcgridoffsetx"), NumberStyles.Integer, CultureInfo.InvariantCulture, out gridoffsetx)) gridoffsetx = 0;
            if (!int.TryParse(setting.ReadString(headdot + "gcgridoffsety"), NumberStyles.Integer, CultureInfo.InvariantCulture, out gridoffsety)) gridoffsety = 0;
            try
            {
                gc.SquareGrid.GridColor = System.Drawing.ColorTranslator.FromHtml(setting.ReadString(headdot + "gcgridcolor"));
            }
            catch
            {
            }
            gc.SquareGrid.Width = gridwidth;
            gc.SquareGrid.Height = gridheight;
            gc.SquareGrid.OffsetX = gridoffsetx;
            gc.SquareGrid.OffsetY = gridoffsety;
            gc.SquareGridEnabled = setting.ReadString(headdot + "gcdisplaygrid") == "1";

            int.TryParse(setting.ReadString(headdot + "anglerestriction"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int anglerestriction);
            gc.AngleRestriction = anglerestriction;
        }

        private static bool IsNumber(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            for (int i = 0; i < s.Length; i++)
            {
                if (!char.IsNumber(s[i])) return false;
            }
            return true;
        }

        public void PlayOrPause()
        {
            if (!MyGame.Playing)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        public void RescueData()
        {
            if (!Directory.Exists(rescuedirectory))
            {
                Directory.CreateDirectory(rescuedirectory);
            }
            Saveppdfile(rescuedirectory + "\\base.ppd", lm.GetAllData(true));
            SaveScd(rescuedirectory + "\\base.scd");
            SaveSoundSet(rescuedirectory + "\\soundset.txt");
            SaveKasi(rescuedirectory + "\\kasi.txt");
            SaveIni(rescuedirectory + "\\data.ini");
            SaveEvd(rescuedirectory + "\\base.evd");
            SaveLayers(rescuedirectory);
            MessageBox.Show(this, "Data was rescued at\n" + Path.GetFullPath(rescuedirectory));
        }

        private void 元に戻すToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tlf.Seekmain.Undo();
        }

        private void やり直すToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tlf.Seekmain.Redo();
        }

        private void ライン入れ替えToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var viewModel = new SwapLineViewModel();
            if (WpfDialog.ShowDialog(new SwapLineWindow(viewModel)))
            {
                int first = viewModel.First;
                int second = viewModel.Second;
                tlf.Seekmain.Swap(first, second);
            }
        }
        private void サウンドマネージャーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(smDock);
        }

        private void 設定ファイルライターToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(ifwDock);
        }

        private void 位置角度ローダーセーバーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(paalsDock);
        }

        private void 歌詞エディターToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(keDock);
        }

        private void BPMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(bmDock);
        }

        private void ゲームウィンドウToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(dxfDock);
        }

        private void タイムラインウィンドウToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(tlfDock);
        }

        private void プロパティウィンドウToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(infofDock);
        }
        private void レイヤーマネージャーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(lmDock);
        }

        private void イベントマネージャーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(emDock);
        }
        private void 幾何クリエーターToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(gcDock);
        }

        private void メモToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(mwDock);
        }

        private void リソースマネージャーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(rmDock);
        }

        private void スクリプトマネージャーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(scmDock);
        }

        private void ヘルプToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(hfDock);
        }

        private void 統計ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockUtility.ShowOrHideWindow(stmDock);
        }

        private void Form1_FormClosing(object sender, CancelEventArgs e)
        {
            moveMarkTimer.Stop();
            try
            {
                scm.SaveDock(Path.Combine(Utility.AppDir, scriptdockxmlname));
                new XmlLayoutSerializer(dockPanel1).Serialize(Path.Combine(Utility.AppDir, dockxmlname));
                SaveShortcut();
            }
            catch (IOException)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                currentProcess?.Dispose();
                currentProcess = null;
            }
        }

        /// <summary>
        /// 確認しないで閉じます（エラーで止まったとき）。
        /// </summary>
        public void ForceClose()
        {
            try
            {
                Close();
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void ChangeMousePos(Point p)
        {
            MyGame.ChangeMousepos(p);
        }

        public void ChangeData(Vector2[] positions, float[] angles)
        {
            MyGame.ChangeData(positions, angles);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle).AddHook(WndProc);
            const Int32 MF_BYPOSITION = 0x400;
            const Int32 MF_REMOVE = 0x1000;

            const UInt32 SC_CLOSE = 0x0000F060;
            const UInt32 MF_BYCOMMAND = 0x00000000;
            var menu = NativeMethods.GetSystemMenu(handle, false);
            NativeMethods.RemoveMenu(menu, SC_CLOSE, MF_BYCOMMAND);
            var menuCount = NativeMethods.GetMenuItemCount(menu);
            if (menuCount > 1)
            {
                //メニューの「閉じる」を削除
                NativeMethods.RemoveMenu(menu, (uint)(menuCount - 1), MF_BYPOSITION | MF_REMOVE);
                NativeMethods.DrawMenuBar(handle);
            }
        }

        private bool CheckProcess()
        {
            if (currentProcess != null && !currentProcess.HasExited)
            {
                var dr = MessageBox.Show(this, askkillprocess, confirm, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                switch (dr)
                {
                    case MessageBoxResult.OK:
                        currentProcess.Kill();
                        break;
                    case MessageBoxResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        private bool CheckExecutor()
        {
            if (currentExecutor != null && currentExecutor.IsWorking)
            {
                var dr = MessageBox.Show(this, askkillcommand, confirm, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                switch (dr)
                {
                    case MessageBoxResult.OK:
                        currentExecutor.Kill();
                        currentExecutor = null;
                        CommandExecuted?.Invoke(null);
                        break;
                    case MessageBoxResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        private void 終了ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckProcess())
            {
                return;
            }

            if (!CheckExecutor())
            {
                return;
            }

            bool cancel = false;
            if (IsNotSaved)
            {
                var dr = MessageBox.Show(this, asksave, confirm, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.No);
                switch (dr)
                {
                    case MessageBoxResult.Yes:
                        cancel |= !CheckCanSaveAndSaveProject();

                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        cancel = true;
                        break;
                }
            }
            if (cancel)
            {
                return;
            }
            SaveConfig();
            this.Close();
        }

        private void LoadConfig()
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Width = PPDStaticSetting.LastWindowSize.Width;
            Height = PPDStaticSetting.LastWindowSize.Height;
            Left = PPDStaticSetting.LastWindowLocation.X;
            Top = PPDStaticSetting.LastWindowLocation.Y;
            WindowState = PPDStaticSetting.LastWindowState;
            FixDockPanel = PPDStaticSetting.FixDockPanel;
            mw.MemoFontSize = PPDStaticSetting.MemoFontSize;
            LoadRecentUserProject();
            if (File.Exists(Path.Combine(Utility.AppDir, iniFileName)))
            {
                langFileName = Path.Combine("Lang", String.Format("lang_{0}_{1}.ini", this.GetType().Assembly.GetName().Name, PPDStaticSetting.langFileISO));
            }
        }

        private void LoadRecentUserProject()
        {
            var path = Path.Combine(Utility.AppDir, projectcache);
            if (File.Exists(path))
            {
                foreach (string line in File.ReadAllLines(path).Reverse())
                {
                    if (!File.Exists(line))
                    {
                        continue;
                    }
                    AddRecentUsedProject(line);
                }
            }
        }

        private void AddRecentUsedProject(string path)
        {
            var existing = this.最近使用したプロジェクトToolStripMenuItem.Items.OfType<MenuItem>().FirstOrDefault(m => (string)m.Tag == path);
            if (existing != null)
            {
                this.最近使用したプロジェクトToolStripMenuItem.Items.Remove(existing);
                this.最近使用したプロジェクトToolStripMenuItem.Items.Insert(0, existing);
            }
            else
            {
                var tsmi = new MenuItem
                {
                    Header = ToMenuText(path),
                    Tag = path
                };
                tsmi.Click += Tsmi_Click;
                this.最近使用したプロジェクトToolStripMenuItem.Items.Insert(0, tsmi);
                if (this.最近使用したプロジェクトToolStripMenuItem.Items.Count > 5)
                {
                    this.最近使用したプロジェクトToolStripMenuItem.Items.RemoveAt(this.最近使用したプロジェクトToolStripMenuItem.Items.Count - 1);
                }
            }
        }

        void Tsmi_Click(object sender, EventArgs e)
        {
            CheckReadProject((string)((MenuItem)sender).Tag);
        }

        private void SaveRecentUsedProject()
        {
            using StreamWriter sw = new(Path.Combine(Utility.AppDir, projectcache));
            foreach (MenuItem tsmi in this.最近使用したプロジェクトToolStripMenuItem.Items)
            {
                sw.WriteLine((string)tsmi.Tag);
            }
        }

        private void SaveConfig()
        {
            var path = Path.Combine(Utility.AppDir, iniFileName);
            var setting = new SettingReader(File.ReadAllText(path));
            setting.ReplaceOrAdd("Language", PPDStaticSetting.langFileISO);
            // 最大化しているときは、元の大きさと位置を保存する
            var bounds = WindowState == WindowState.Normal ? new Rect(Left, Top, Width, Height) : RestoreBounds;
            setting.ReplaceOrAdd("Maximised", WindowState == WindowState.Maximized);
            setting.ReplaceOrAdd("Size", String.Format("{0},{1}", (int)bounds.Width, (int)bounds.Height));
            setting.ReplaceOrAdd("Location", String.Format("{0},{1}", (int)bounds.X, (int)bounds.Y));
            setting.ReplaceOrAdd("AuthorName", PPDStaticSetting.AuthorName);
            setting.ReplaceOrAdd("MovieLatency", PPDStaticSetting.MovieLatency);
            setting.ReplaceOrAdd("HideToggleRectangle", PPDStaticSetting.HideToggleRectangle);
            setting.ReplaceOrAdd("EnableToChangeMarkTypeAndTime", PPDStaticSetting.EnableToChangeMarkTypeAndTime);
            setting.ReplaceOrAdd("HideToggleArrow", PPDStaticSetting.HideToggleArrow);
            setting.ReplaceOrAdd("FixDockPanel", PPDStaticSetting.FixDockPanel);
            setting.ReplaceOrAdd("MemoFontSize", PPDStaticSetting.MemoFontSize);
            setting.ReplaceOrAdd("ApplyToIniMovieTrimming", PPDStaticSetting.ApplyToIniMovieTrimming);
            setting.ReplaceOrAdd("CustomCanvasBackColor", String.Format("{0},{1},{2}",
                PPDStaticSetting.CustomCanvasBackColor.R,
                PPDStaticSetting.CustomCanvasBackColor.G,
                PPDStaticSetting.CustomCanvasBackColor.B
                ));
            setting.ReplaceOrAdd("CanvasSizeIndex", PPDStaticSetting.CanvasSizeIndex);
            setting.ReplaceOrAdd("CanvasColorIndex", PPDStaticSetting.CanvasColorIndex);
            for (int i = 0; i < 8; i++)
            {
                setting.ReplaceOrAdd(String.Format("Move{0}", i), PPDStaticSetting.Moves[i]);
                setting.ReplaceOrAdd(String.Format("Angle{0}", i), PPDStaticSetting.Angles[i]);
            }
            using (SettingWriter sw = new(path, false))
            {
                foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
                {
                    sw.Write(kvp.Key, kvp.Value);
                }
            }
            SaveRecentUsedProject();
        }

        private void シート全体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowUtility.Seekmain.ForceToNearBar(false);
        }

        private void 選択範囲のみToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowUtility.Seekmain.ForceToNearBar(true);
        }

        private bool OverWriteProject()
        {
            if (Directory.Exists(projectfiledir) && File.Exists(projectfiledir + "\\" + projectfilename))
            {
                SaveProject();
                return true;
            }
            return false;
        }

        private void 現在の時間から開始ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecutePPD(false, WindowUtility.Seekmain.Currenttime);
        }

        private void 最初から開始ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecutePPD(false, WindowUtility.IniFileWriter.StartTime);
        }

        private void 現在の時間から開始オートToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecutePPD(true, WindowUtility.Seekmain.Currenttime);
        }

        private void 最初から開始オートToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecutePPD(true, WindowUtility.IniFileWriter.StartTime);
        }

        private void メニューから開始ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecutePPDFromMenu();
        }

        private void ExecutePPD(bool auto, double starttime)
        {
            if (!CanExecutePpd) return;
            if (!OverWriteProject()) return;
            var psi = new ProcessStartInfo
            {
                Arguments = String.Format("-game PPDSingle -d \"{0}\" -auto {1} -bpm {2} -st {3} -dif {4} -connect {5} -mod \"{6}\" {7}",
                Path.Combine(projectfiledir, Path.GetFileNameWithoutExtension(projectfilename)),
                auto ? "1" : "0",
                WindowUtility.IniFileWriter.BPM,
                starttime,
                CurrentDifficulty.ToString(),
                Connect ? "1" : "0",
                ModList,
                DisableExpansion ? "-disableexpansion" : "")
            };
            ExecutePPD(psi);
        }

        private void ExecutePPDFromMenu()
        {
            if (!CanExecutePpd) return;
            if (!OverWriteProject()) return;
            var psi = new ProcessStartInfo
            {
                Arguments = String.Format("-game PPDSingle -songdir \"{0}\"", projectfiledir)
            };
            ExecutePPD(psi);
        }

        private void ExecutePPD(ProcessStartInfo processStartInfo)
        {
            currentProcess = new System.Diagnostics.Process
            {
                EnableRaisingEvents = true
            };
            processStartInfo.FileName = Program.PPDExePath;
            processStartInfo.WorkingDirectory = Path.GetDirectoryName(Program.PPDExePath);
            currentProcess.StartInfo = processStartInfo;
            ProcessStarted?.Invoke();
            currentProcess.Exited += CurrentProcess_Exited;
            currentProcess.Start();
            現在の時間から開始ToolStripMenuItem.IsEnabled = 現在の時間から開始オートToolStripMenuItem.IsEnabled =
                最初から開始ToolStripMenuItem.IsEnabled = 最初から開始オートToolStripMenuItem.IsEnabled = メニューから開始ToolStripMenuItem.IsEnabled = false;
            pPDプロセスを強制終了ToolStripMenuItem.IsEnabled = true;
        }

        void CurrentProcess_Exited(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                現在の時間から開始ToolStripMenuItem.IsEnabled = 現在の時間から開始オートToolStripMenuItem.IsEnabled =
                    最初から開始ToolStripMenuItem.IsEnabled = 最初から開始オートToolStripMenuItem.IsEnabled = メニューから開始ToolStripMenuItem.IsEnabled = true;
                pPDプロセスを強制終了ToolStripMenuItem.IsEnabled = false;
                ProcessExited?.Invoke();
            });
        }

        private void プロジェクトを上書き保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wdf.Hide();
            if (OverWriteProject())
            {
                if (wdf.Owner == null)
                {
                    wdf.Owner = this;
                }
                wdf.Show();
                this.Activate();
            }
        }

        private void ToolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // WPF では XAML を読み込む間にも呼ばれるので、ドックを作るまでは何もしない
            if (WindowUtility.LayerManager == null)
            {
                return;
            }
            WindowUtility.LayerManager.DisplayMode = (DisplayLineMode)this.toolStripComboBox1.SelectedIndex;
            WindowUtility.Seekmain.DisplayMode = (DisplayLineMode)this.toolStripComboBox1.SelectedIndex;
            WindowUtility.Seekmain.DrawAndRefresh();
        }

        private void ToolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // WPF では XAML を読み込む間にも呼ばれるので、ドックを作るまでは何もしない
            if (WindowUtility.LayerManager == null)
            {
                return;
            }
            float result = 0;
            switch (this.toolStripComboBox2.SelectedIndex)
            {
                case 0:
                    result = 0.5f;
                    break;
                case 1:
                    result = 0.75f;
                    break;
                case 2:
                    result = 1.0f;
                    break;
                case 3:
                    result = 1.25f;
                    break;
                case 4:
                    result = 1.5f;
                    break;
                case 5:
                    result = 1.75f;
                    break;
                case 6:
                    result = 2.0f;
                    break;
            }
            WindowUtility.Seekmain.SpeedScale = result;
        }


        private void ToolStripComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            // WPF では XAML を読み込む間にも呼ばれるので、ドックを作るまでは何もしない
            if (WindowUtility.LayerManager == null)
            {
                return;
            }
            WindowUtility.LayerManager.BeatType = (DisplayBeatType)this.toolStripComboBox3.SelectedIndex;
            WindowUtility.Seekmain.BeatType = (DisplayBeatType)this.toolStripComboBox3.SelectedIndex;
            WindowUtility.Seekmain.DrawAndRefresh();
        }

        private void ToolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            // WPF では XAML を読み込む間にも呼ばれるので、ドックを作るまでは何もしない
            if (WindowUtility.LayerManager == null)
            {
                return;
            }
            if (float.TryParse(this.toolStripTextBox1.Text, out float result))
            {
                if (result <= 500)
                {
                    WindowUtility.LayerManager.BPM = result;
                }
            }
        }

        private void ToolStripTextBox2_TextChanged(object sender, EventArgs e)
        {
            // WPF では XAML を読み込む間にも呼ばれるので、ドックを作るまでは何もしない
            if (WindowUtility.LayerManager == null)
            {
                return;
            }
            if (float.TryParse(this.toolStripTextBox2.Text, out float result))
            {
                WindowUtility.LayerManager.BPMOffset = result;
            }
        }

        private void ToolStripTextBox3_TextChanged(object sender, EventArgs e)
        {
            // WPF では XAML を読み込む間にも呼ばれるので、ドックを作るまでは何もしない
            if (WindowUtility.LayerManager == null)
            {
                return;
            }
            if (int.TryParse(this.toolStripTextBox3.Text, out int result))
            {
                WindowUtility.LayerManager.DisplayWidth = result;
            }
        }
        public string GetDisplayModeString(DisplayLineMode index)
        {
            if (index < 0 || (int)index >= toolStripComboBox1.Items.Count) return "";
            var item = toolStripComboBox1.Items[(int)index];
            return (item as ComboBoxItem)?.Content?.ToString() ?? item.ToString();
        }

        private void PxToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.exToolStripSplitButton1.IsChecked = true;
            TurnOffExcept(sender as MenuItem);
            GridPixel = 10;
        }

        private void PxToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.exToolStripSplitButton1.IsChecked = true;
            TurnOffExcept(sender as MenuItem);
            GridPixel = 20;
        }

        private void PxToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            this.exToolStripSplitButton1.IsChecked = true;
            TurnOffExcept(sender as MenuItem);
            GridPixel = 50;
        }

        private void TurnOffExcept(MenuItem tsmi)
        {
            foreach (MenuItem ttsmi in this.gridMenu.Items)
            {
                if (tsmi != ttsmi) ttsmi.IsChecked = false;
            }
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
        private void 詳細設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gsf = new GridSettingViewModel(Grid);
            if (WpfDialog.ShowDialog(new GridSettingWindow(gsf)))
            {
                Grid.CopyFrom(gsf.Grid);
                TurnOffExcept(sender as MenuItem);
                this.exToolStripSplitButton1.IsChecked = true;
            }
        }

        private void 詳細設定ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var viewModel = new FarnessSettingViewModel(Farness);
            if (WpfDialog.ShowDialog(new FarnessSettingWindow(viewModel)))
            {
                Farness = viewModel.Farness;
            }
        }

        private void 作者名を変更するToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeAuthorName();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
            else e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (File.Exists(fileName[0]) && Path.GetExtension(fileName[0]) == ".ppdproj") CheckReadProject(fileName[0]);
        }

        private void 設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sf = new SettingViewModel(WindowUtility.Seekmain.ShortcutManager);
            if (WpfDialog.ShowDialog(new SettingWindow(sf)))
            {
                sf.Apply();
                SaveShortcut();
                ReadCommands();
                if (sf.ShouldRestart)
                {
                    MessageBox.Show(this, rebootnecessary);
                }
            }
        }

        private void EasyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeDifficulty(AvailableDifficulty.Easy);
            TurnOffExceptDifficulty(sender as MenuItem);
            ChangeWindowText();
        }

        private void NormalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeDifficulty(AvailableDifficulty.Normal);
            TurnOffExceptDifficulty(sender as MenuItem);
            ChangeWindowText();
        }

        private void HardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeDifficulty(AvailableDifficulty.Hard);
            TurnOffExceptDifficulty(sender as MenuItem);
            ChangeWindowText();
        }

        private void ExtremeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeDifficulty(AvailableDifficulty.Extreme);
            TurnOffExceptDifficulty(sender as MenuItem);
            ChangeWindowText();
        }

        private void BaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeDifficulty(AvailableDifficulty.Base);
            TurnOffExceptDifficulty(sender as MenuItem);
            ChangeWindowText();
        }

        private void ChangeDifficulty(AvailableDifficulty difficulty)
        {
            if (CurrentDifficulty != difficulty)
            {
                //save current
                if (!CheckSaveStateAndSave(true))
                {
                    return;
                }
                CurrentDifficulty = difficulty;
                ReadSpecificDifficulty();
            }
        }

        private void TurnOffExceptDifficulty(MenuItem tsmi)
        {
            foreach (MenuItem ttsmi in this.難易度ToolStripMenuItem.Items.OfType<MenuItem>())
            {
                if (tsmi != ttsmi) ttsmi.IsChecked = false;
            }
        }

        private void 現在の難易度をコピーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckSaveStateAndSave(true))
            {
                return;
            }
            var cdf = new CopyDifficultyViewModel(CurrentDifficulty);
            if (WpfDialog.ShowDialog(new CopyDifficultyWindow(cdf)))
            {
                bool alreadyexist = false;
                switch (cdf.DestDifficulty)
                {
                    case AvailableDifficulty.Easy:
                        alreadyexist = (AvailableDifficulty & AvailableDifficulty.Easy) == AvailableDifficulty.Easy;
                        break;
                    case AvailableDifficulty.Normal:
                        alreadyexist = (AvailableDifficulty & AvailableDifficulty.Normal) == AvailableDifficulty.Normal;
                        break;
                    case AvailableDifficulty.Hard:
                        alreadyexist = (AvailableDifficulty & AvailableDifficulty.Hard) == AvailableDifficulty.Hard;
                        break;
                    case AvailableDifficulty.Extreme:
                        alreadyexist = (AvailableDifficulty & AvailableDifficulty.Extreme) == AvailableDifficulty.Extreme;
                        break;
                    case AvailableDifficulty.Base:
                        alreadyexist = (AvailableDifficulty & AvailableDifficulty.Base) == AvailableDifficulty.Base;
                        break;
                }
                if (alreadyexist)
                {
                    alreadyexist = !(MessageBox.Show(this, alreadyexitdifficulty, confirm, MessageBoxButton.OKCancel) == MessageBoxResult.OK);
                }
                if (!alreadyexist)
                {
                    // Copy
                    AvailableDifficulty |= cdf.DestDifficulty;

                    SaveProject(cdf.DestDifficulty, false);

                    // CopyScripts
                    string srcFolder = Path.Combine(CurrentProjectDir, CurrentDifficulty + "_" + "Scripts"),
                        destFolder = Path.Combine(CurrentProjectDir, cdf.DestDifficulty + "_" + "Scripts");
                    if (Directory.Exists(srcFolder))
                    {
                        Utility.CopyDirectory(srcFolder, destFolder, []);
                    }
                }
            }
        }

        private void CheckDifficultyAvailable(string projectdir)
        {
            availabledifficulty = AvailableDifficulty.None;
            foreach (AvailableDifficulty difficulty in DifficultyArray)
            {
                availabledifficulty |= Directory.Exists(Path.Combine(projectdir, difficulty + "_" + "layers")) ? difficulty : AvailableDifficulty.None;
            }
            AvailableDifficulty = availabledifficulty;
        }

        public static AvailableDifficulty GetDifficultyAvailable(string projectdir)
        {
            AvailableDifficulty ret = AvailableDifficulty.None;
            foreach (AvailableDifficulty difficulty in DifficultyArray)
            {
                ret |= Directory.Exists(Path.Combine(projectdir, difficulty + "_" + "layers")) ? difficulty : AvailableDifficulty.None;
            }
            return ret;
        }

        private void 発行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckSaveStateAndSave(true))
            {
                return;
            }
            var pf = new PublishViewModel(AvailableDifficulty, Path.GetFileNameWithoutExtension(projectfilename), CurrentProjectDir);
            if (WpfDialog.ShowDialog(new PublishWindow(pf)))
            {
                if (pf.PublishDifficulty == AvailableDifficulty.None)
                {
                    MessageBox.Show(this, noselecteddifficulty);
                    return;
                }
                NonExistCreateDirectory(pf.PublishFolderPath);
                var destdir = Path.Combine(pf.PublishFolderPath, pf.PublishFolderName);
                if (Directory.Exists(destdir))
                {
                    Directory.Delete(destdir, true);
                }
                NonExistCreateDirectory(destdir);
                var projectdir = Path.Combine(projectfiledir, Path.GetFileNameWithoutExtension(projectfilename));

                //copy
                CopySpecificFile(projectdir, destdir, "data.ini");
                CopySpecificFile(projectdir, destdir, "kasi.txt");
                CopySpecificFile(projectdir, destdir, "soundset.txt");
                CopySpecificFile(projectdir, destdir, "resource.pak");
                foreach (AvailableDifficulty difficulty in DifficultyArray)
                {
                    if (difficulty == AvailableDifficulty.None) continue;
                    if ((pf.PublishDifficulty & difficulty) == difficulty)
                    {
                        CopySpecificFile(projectdir, destdir, difficulty + ".ppd");
                        CopySpecificFile(projectdir, destdir, difficulty + ".scd");
                    }
                }
                Utility.CopyDirectory(Path.Combine(projectdir, "sound"), Path.Combine(destdir, "sound"), []);
                ShowExplorerWithDir(destdir);
            }
        }

        private static void ShowExplorerWithDir(string path)
        {
            try
            {
                Process.Start("EXPLORER.EXE", @"/select," + path);
            }
            catch
            {
            }
        }

        private static void CopySpecificFile(string src, string dest, string filename)
        {
            File.Copy(Path.Combine(src, filename), Path.Combine(dest, filename), true);
        }

        private void プロジェクトをマージToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var mf = new MergeViewModel();
            if (WpfDialog.ShowDialog(new MergeWindow(mf)))
            {
                AvailableDifficulty mergeDifficulty = mf.MergeDifficulty;
                if (mergeDifficulty == AvailableDifficulty.None)
                {
                    MessageBox.Show(this, nomergedifficulty);
                    return;
                }
                foreach (AvailableDifficulty difficulty in DifficultyArray)
                {
                    if (difficulty == AvailableDifficulty.None) continue;
                    if ((mergeDifficulty & difficulty) == difficulty)
                    {
                        var mi = mf.GetMergeInfo(difficulty);
                        if (mi.ProjectVersion != latestprojectversion)
                        {
                            BackUpProject(mi.ProjectPath);
                            ConvertProject(mi.ProjectPath, mi.ProjectVersion);
                        }
                    }
                }
                var tempdir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
                var projectdir = Path.Combine(tempdir, Path.GetFileNameWithoutExtension(mf.DestProjectFilePath));
                NonExistCreateDirectory(projectdir);
                var setting = new SettingReader("");
                setting.ReplaceOrAdd("version", latestprojectversion.ToString());
                string moviePath = "";
                foreach (AvailableDifficulty difficulty in DifficultyArray)
                {
                    if (difficulty == AvailableDifficulty.None) continue;
                    if ((mergeDifficulty & difficulty) == difficulty)
                    {
                        var mi = mf.GetMergeInfo(difficulty);
                        Utility.CopyDirectory(Path.Combine(GetProjectDirFromPath(mi.ProjectPath), mi.Difficulty + "_" + "layers"),
                            Path.Combine(projectdir, difficulty + "_" + "layers"), []);
                        File.Copy(Path.Combine(GetProjectDirFromPath(mi.ProjectPath), mi.Difficulty + ".ppd"),
                            Path.Combine(projectdir, difficulty + ".ppd"), true);
                        File.Copy(Path.Combine(GetProjectDirFromPath(mi.ProjectPath), mi.Difficulty + ".scd"),
                            Path.Combine(projectdir, difficulty + ".scd"), true);

                        var scriptDir = Path.Combine(GetProjectDirFromPath(mi.ProjectPath), mi.Difficulty + "_" + "Scripts");
                        if (Directory.Exists(scriptDir))
                        {
                            Utility.CopyDirectory(scriptDir, Path.Combine(projectdir, difficulty + "_" + "Scripts"), []);
                        }

                        if (difficulty == mf.BaseInfoProjectDifficulty)
                        {
                            var resourceDir = Path.Combine(GetProjectDirFromPath(mi.ProjectPath), "Resource");
                            if (Directory.Exists(resourceDir))
                            {
                                Utility.CopyDirectory(resourceDir, Path.Combine(projectdir, "Resource"), []);
                            }

                            Utility.CopyDirectory(Path.Combine(GetProjectDirFromPath(mi.ProjectPath), "sound"), Path.Combine(projectdir, "sound"), []);

                            File.Copy(Path.Combine(GetProjectDirFromPath(mi.ProjectPath), "data.ini"),
                                Path.Combine(projectdir, "data.ini"), true);
                            File.Copy(Path.Combine(GetProjectDirFromPath(mi.ProjectPath), "kasi.txt"),
                                Path.Combine(projectdir, "kasi.txt"), true);
                            File.Copy(Path.Combine(GetProjectDirFromPath(mi.ProjectPath), "soundset.txt"),
                                Path.Combine(projectdir, "soundset.txt"), true);
                            var fns = Directory.GetFiles(GetProjectDirFromPath(mi.ProjectPath), "movie.*");
                            if (fns.Length > 0) moviePath = fns[0];
                        }
                        var sr = new StreamReader(mi.ProjectPath);
                        var tempSetting = new SettingReader(sr.ReadToEnd());
                        sr.Close();
                        foreach (KeyValuePair<string, string> kvp in tempSetting.Dictionary)
                        {
                            if (kvp.Key.StartsWith(mi.Difficulty.ToString()))
                            {
                                setting.ReplaceOrAdd(kvp.Key.Replace(mi.Difficulty.ToString(), difficulty.ToString()), kvp.Value);
                            }
                        }
                        setting.ReplaceOrAdd("selecteddifficulty", difficulty.ToString());
                    }
                }
                var sw = new SettingWriter(Path.Combine(tempdir, Path.GetFileName(mf.DestProjectFilePath)), false);
                foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
                {
                    sw.Write(kvp.Key, kvp.Value);
                }
                sw.Close();

                //Copy To Local
                Utility.CopyDirectory(tempdir, Path.GetDirectoryName(mf.DestProjectFilePath), []);
                if (File.Exists(moviePath))
                {
                    File.Copy(moviePath, Path.Combine(GetProjectDirFromPath(mf.DestProjectFilePath), Path.GetFileName(moviePath)), true);
                }
            }
        }

        private static string GetProjectDirFromPath(string projectPath)
        {
            return Path.Combine(Path.GetDirectoryName(projectPath), Path.GetFileNameWithoutExtension(projectPath));
        }

        private void ツールバーを隠すToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ツールバーを隠すToolStripMenuItem.IsChecked = !ツールバーを隠すToolStripMenuItem.IsChecked;
            menuStrip1.Visibility = ツールバーを隠すToolStripMenuItem.IsChecked ? Visibility.Collapsed : Visibility.Visible;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // WM_NCMOUSEMOVE: タイトルバーにマウスが来たら、隠したメニューを出す
            if (msg == 0xA0 && menuStrip1.Visibility != Visibility.Visible)
            {
                menuStrip1.Visibility = Visibility.Visible;
                ツールバーを隠すToolStripMenuItem.IsChecked = false;
            }
            return IntPtr.Zero;
        }

        private void ChangeWindowText()
        {
            this.Title = string.Format("{0}({1}) - PPDEditor", Path.GetFileNameWithoutExtension(projectfilename), CurrentDifficulty);
        }

        private void ドックを固定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FixDockPanel = !FixDockPanel;
        }

        private void 新規プロジェクトを作成ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //save current
            if (!CheckSaveStateAndSave(false))
            {
                return;
            }

            var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (!Directory.Exists(Path.Combine(dir, "Projects")))
            {
                Directory.CreateDirectory(Path.Combine(dir, "Projects"));
            }
            var viewModel = new NewProjectWindowViewModel(Utility.Language)
            {
                FolderPath = Path.Combine(dir, "Projects")
            };
            var npw = new NewProjectWindow
            {
                DataContext = viewModel
            };
            if (npw.ShowDialog() != true)
            {
                return;
            }

            // restore settings
            ClearData();
            lm.Initialize();

            projectfiledir = viewModel.FolderPath;
            projectfilename = Path.ChangeExtension(viewModel.Name, ".ppdproj");
            var template = viewModel.SelectedItem.Value;
            foreach (var p in template.EventManagerInfo.Events)
            {
                var eventData = new EventData
                {
                    BPM = p.Value.BPM
                };
                for (int i = 0; i < Math.Min(eventData.InitializeOrder.Length, p.Value.InitializeOrders.Length); i++)
                {
                    eventData.InitializeOrder[i] = (ButtonType)p.Value.InitializeOrders[i];
                }
                eventData.NoteType = p.Value.NoteType;
                em.AddChange(p.Key, eventData);
            }
            Array.Copy(template.TimeLineInfo.RowOrders, tlf.RowManager.RowOrders, Math.Min(tlf.RowManager.RowOrders.Length, template.TimeLineInfo.RowOrders.Length));
            Array.Copy(template.TimeLineInfo.RowVisibilities, tlf.RowManager.RowVisibilities, Math.Min(tlf.RowManager.RowVisibilities.Length, template.TimeLineInfo.RowVisibilities.Length));
            IsTimeLineRowLimited = template.TimeLineInfo.RowLimited;
            var sounds = new List<string>();
            foreach (var sound in template.SoundManagerInfo.Sounds.Select(s => Path.Combine(Utility.AppDir, s)))
            {
                if (!File.Exists(sound))
                {
                    MessageBox.Show(this, String.Format("{0}\n{1}", Utility.Language["SoundNotExist"], sound));
                    continue;
                }
                sounds.Add(sound);
            }
            if (sounds.Count > 0)
            {
                sm.Readsounds([.. sounds]);
            }
            foreach (var change in template.SoundManagerInfo.Changes)
            {
                for (int i = 0; i < change.Value.Length; i++)
                {
                    if (change.Value[i] > sounds.Count)
                    {
                        change.Value[i] = 0;
                    }
                }
            }
            sm.Readchangedata([.. template.SoundManagerInfo.Changes.Select(p => p.Key)],
                [.. template.SoundManagerInfo.Changes.Select(p => p.Value)]);

            // create folders
            NonExistCreateDirectory(Path.Combine(CurrentProjectDir, "Base_Scripts"));
            NonExistCreateDirectory(Path.Combine(CurrentProjectDir, "Base_layers"));
            rm.CreateResourceFolders();
            foreach (var script in template.ScriptManagerInfo.Scripts)
            {
                var src = Path.Combine(Utility.AppDir, script.Src);
                if (!File.Exists(src))
                {
                    MessageBox.Show(this, String.Format("{0}\n{1}", Utility.Language["ScriptNotExist"], src));
                    continue;
                }
                var dest = Path.Combine(CurrentProjectDir, "Base_Scripts", script.Dest);
                Utility.SafeCopyFile(script.Src, dest);
            }
            scm.ReadScript(CurrentProjectDir, PPDEditor.AvailableDifficulty.Base, "");

            ReadMovie(viewModel.MoviePath);
            SaveProject();
            ChangeWindowText();
            AddRecentUsedProject(CurrentProjectFilePath);
            SaveRecentUsedProject();
            tlf.InvalidateAll();
            ReadMovieFromProject();
        }

        private bool CheckSaveStateAndSave(bool okCancel)
        {
            bool cancel = false;
            if (IsNotSaved)
            {
                if (String.IsNullOrEmpty(projectfiledir))
                {
                    var dr = MessageBox.Show(this, asksave, confirm, okCancel ? MessageBoxButton.OKCancel : MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, okCancel ? MessageBoxResult.Cancel : MessageBoxResult.No);
                    if (dr == MessageBoxResult.Yes || dr == MessageBoxResult.OK)
                    {
                        cancel |= !CheckCanSaveAndSaveProject();
                    }
                    else if (dr == MessageBoxResult.No)
                    {
                    }
                    else cancel |= dr == MessageBoxResult.Cancel;
                }
                else
                {
                    var dr = MessageBox.Show(this, asksave, confirm, okCancel ? MessageBoxButton.OKCancel : MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, okCancel ? MessageBoxResult.Cancel : MessageBoxResult.No);
                    if (dr == MessageBoxResult.Yes || dr == MessageBoxResult.OK)
                    {
                        SaveProject();
                    }
                    cancel = dr == MessageBoxResult.Cancel;
                }
            }

            return !cancel;
        }

        private void MODを発行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (scm.GetEnabledScriptList().Length < 1)
            {
                MessageBox.Show(this, "MODの発行には少なくともひとつのスクリプトが必要です。");
                return;
            }

            var mpf = new ModPublishViewModel();
            if (WpfDialog.ShowDialog(new ModPublishWindow(mpf)))
            {
                var path = Path.Combine(mpf.PublishFolder, mpf.ModFileName);
                path = Path.ChangeExtension(path, "mod");
                using (PackWriter writer = new(path))
                {
                    var dict = new Dictionary<string, string>
                    {
                        {"Mod\\DisplayName",mpf.ModDisplayName},                        {"Mod\\AuthorName",mpf.ModAuthorName},                        {"Mod\\Version",mpf.ModVersion},                        {"Mod\\FlowScriptVersion",GetVersion("FlowScriptEngine.dll")},                        {"Mod\\FlowScriptBasicVersion",GetVersion("dlls\\FlowScriptEngineBasic.dll")},                        {"Mod\\FlowScriptPPDVersion",GetVersion("dlls\\FlowScriptEnginePPD.dll")},                        {"Mod\\FlowScriptSharpDXVersion",GetVersion("dlls\\FlowScriptEngineSlimDX.dll")},                        {"Mod\\FlowScriptBasicExtensionVersion",GetVersion("dlls\\FlowScriptEngineBasicExtension.dll")},                        {"Mod\\FlowScriptDataVersion",GetVersion("dlls\\FlowScriptEngineData.dll")}                    };
                    var list = new List<string>();
                    list.AddRange(dict.Keys);
                    list.AddRange(scm.GetEnabledScriptList());
                    list.AddRange(rm.GetEnabledList());
                    foreach (PPDPackStreamWriter packWriter in writer.Write([.. list]))
                    {
                        if (packWriter.Filename.StartsWith("Mod"))
                        {
                            string value = dict[packWriter.Filename];
                            var bytes = Encoding.UTF8.GetBytes(value);
                            packWriter.Write(bytes, 0, bytes.Length);
                        }
                        else if (packWriter.Filename.StartsWith("Scripts"))
                        {
                            scm.WriteScript(packWriter, packWriter.Filename);
                        }
                        else
                        {
                            rm.WriteResource(packWriter, packWriter.Filename);
                        }
                    }
                }
                ShowExplorerWithDir(path);
            }
        }

        private static string GetVersion(string path)
        {
            var fvi = FileVersionInfo.GetVersionInfo(path);
            return fvi.FileVersion;
        }

        private void ConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Connect = !Connect;
        }

        private void 角度補完ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowUtility.Seekmain.AngleInterpolation(true);
        }

        private void ImportEvent(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show(this, String.Format("Not Exist File:{0}", filePath));
                return;
            }

            SortedList<float, EventData> data = null;
            try
            {
                using PackReader reader = new(filePath);
                var ppdpsr = reader.Read("evd");
                data = ReadEvdAsEventData(ppdpsr);
            }
            catch
            {
                using FileStream stream = File.Open(filePath, FileMode.Open);
                data = ReadEvdAsEventData(stream);
            }
            var form = new ImportEventViewModel(em.ChangeData, data);
            if (WpfDialog.ShowDialog(new ImportEventWindow(form)))
            {
                em.ChangeData = form.ChangeData;
            }
        }

        private void EasyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ImportEvent(Path.Combine(CurrentProjectDir, "easy.ppd"));
        }

        private void NormalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ImportEvent(Path.Combine(CurrentProjectDir, "normal.ppd"));
        }

        private void HardToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ImportEvent(Path.Combine(CurrentProjectDir, "hard.ppd"));
        }

        private void ExtremeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ImportEvent(Path.Combine(CurrentProjectDir, "extreme.ppd"));
        }

        private void BaseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ImportEvent(Path.Combine(CurrentProjectDir, "base.ppd"));
        }

        private void 別のファイルToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Filter = openevdfilter;
            this.openFileDialog1.RestoreDirectory = true;
            this.openFileDialog1.FileName = "";
            if (this.openFileDialog1.ShowDialog(this) == true)
            {
                ImportEvent(this.openFileDialog1.FileName);
            }
        }

        private void イベントToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (!IsProjectLoaded)
            {
                easyToolStripMenuItem1.IsEnabled = normalToolStripMenuItem1.IsEnabled = hardToolStripMenuItem1.IsEnabled =
                    extremeToolStripMenuItem1.IsEnabled = baseToolStripMenuItem1.IsEnabled = false;
                return;
            }

            baseToolStripMenuItem1.IsEnabled = AvailableDifficulty.HasFlag(AvailableDifficulty.Base) && CurrentDifficulty != PPDEditor.AvailableDifficulty.Base;
            easyToolStripMenuItem1.IsEnabled = AvailableDifficulty.HasFlag(AvailableDifficulty.Easy) && CurrentDifficulty != PPDEditor.AvailableDifficulty.Easy;
            normalToolStripMenuItem1.IsEnabled = AvailableDifficulty.HasFlag(AvailableDifficulty.Normal) && CurrentDifficulty != PPDEditor.AvailableDifficulty.Normal;
            hardToolStripMenuItem1.IsEnabled = AvailableDifficulty.HasFlag(AvailableDifficulty.Hard) && CurrentDifficulty != PPDEditor.AvailableDifficulty.Hard;
            extremeToolStripMenuItem1.IsEnabled = AvailableDifficulty.HasFlag(AvailableDifficulty.Extreme) && CurrentDifficulty != PPDEditor.AvailableDifficulty.Extreme;
        }

        private void ToolStripSplitButton1_CheckStateChanged(object sender, EventArgs e)
        {
            tlf.RowManager.IsLimited = toolStripSplitButton1.IsChecked == true;
            tlf.InvalidateAll();
        }

        private void 詳細設定ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var form = new EditTimeLineRowViewModel(tlf.RowManager);
            if (WpfDialog.ShowDialog(new EditTimeLineRowWindow(form)))
            {
                form.Apply(tlf.RowManager);
            }
            tlf.InvalidateAll();
        }

        private void DisableExpansionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisableExpansion = !DisableExpansion;
        }


        private void PPDプロセスを強制終了ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckProcess();
        }

        public void PlayMovie()
        {
            MyGame.Movie?.Play();
        }
        public void StopMovie()
        {
            MyGame.Movie?.Stop();
        }
        public void PauseMovie()
        {
            MyGame.Movie?.Pause();
        }

        private void ReadCommands()
        {
            while (コマンドToolStripMenuItem.Items.Count > 2)
            {
                コマンドToolStripMenuItem.Items.RemoveAt(0);
            }

            if (!Directory.Exists("Commands"))
            {
                Directory.CreateDirectory("Commands");
            }

            ReadCommands(コマンドToolStripMenuItem, "Commands");
        }

        private void ReadCommands(MenuItem menuItem, string dir)
        {
            int insertIndex = 0;
            foreach (string childDir in Directory.GetDirectories(dir))
            {
                var name = Path.GetFileNameWithoutExtension(childDir);
                var childMenu = new MenuItem
                {
                    Header = ToMenuText(name)
                };
                ReadCommands(childMenu, childDir);
                menuItem.Items.Insert(insertIndex++, childMenu);
            }

            foreach (string childFile in Directory.GetFiles(dir))
            {
                if (!Path.GetExtension(childFile).Equals(".fsml", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                var targetPath = Path.Combine(Path.GetDirectoryName(childFile), Path.GetFileNameWithoutExtension(childFile)).ToLower();
                var shortcut = WindowUtility.Seekmain.ShortcutManager.Shortcuts.FirstOrDefault(s =>
                    s.ShortcutType == ShortcutType.Custom && s.ScriptPath.Equals(targetPath, StringComparison.CurrentCultureIgnoreCase));
                var name = Path.GetFileNameWithoutExtension(childFile);
                var childMenu = new MenuItem
                {
                    Header = ToMenuText(name),
                    Tag = childFile
                };
                if (shortcut != null)
                {
                    childMenu.InputGestureText = shortcut.GetCommandText();
                }
                childMenu.Click += ChildMenu_Click;
                menuItem.Items.Insert(insertIndex++, childMenu);
            }
        }

        public void ExecuteScript(string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                return;
            }

            var sheets = WindowUtility.LayerManager.Sheets;
            var selectedSheet = WindowUtility.LayerManager.SelectedPpdSheet;
            var layers = new List<Layer>();
            foreach (var sheet in sheets)
            {
                layers.Add(new Layer(sheet, lm.IDProvider.Clone(), sheet == selectedSheet));
            }
            var fullPath = Path.GetFullPath(scriptPath).ToLower();
            CommandExecuting?.Invoke(fullPath);
            コマンドToolStripMenuItem.IsEnabled = false;
            ゲームToolStripMenuItem.IsEnabled = false;
            currentExecutor = new Executor(fullPath, [.. layers]);
            currentExecutor.AddItem("Storage", storage);
            currentExecutor.AddItem("PosAndAngleLoader", PosAndAngleLoader.Instance);
            currentExecutor.AddItem("WindowManager", this);
            currentExecutor.AddItem("Events", em.ChangeData.ToArray());
            currentExecutor.AddItem("Movie", MyGame.Movie);
            currentExecutor.AddItem("MovieManager", this);
            currentExecutor.AddItem("SongInfo", this);
            currentExecutor.AddItem("ParameterPresets", infof.Presets);
            currentExecutor.Finished += Executor_Finished;
            currentExecutor.Execute();
        }

        void Executor_Finished(object sender, EventArgs e)
        {
            currentExecutor = null;
            SafeInvoke(() =>
            {
                var executor = (Executor)sender;
                CommandExecuted?.Invoke(executor.ScriptFilePath);
                if (executor.HasError)
                {
                    MessageBox.Show(this, executor.ErrorText);
                }
                foreach (var layer in executor.Layers)
                {
                    layer.Execute();
                }
                if (storage.IsChanged)
                {
                    storage.Save();
                }
                コマンドToolStripMenuItem.IsEnabled = true;
                ゲームToolStripMenuItem.IsEnabled = true;
                tlf.Seekmain.DrawAndRefresh();
            });
        }

        private void SafeInvoke(Action action)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(action);
                return;
            }
            action();
        }

        void ChildMenu_Click(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var filePath = (string)menuItem.Tag;
            ExecuteScript(filePath);
        }

        private void 一覧を再読み込みToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReadCommands();
        }

        public void SetTrimming(MovieTrimmingData trimmingData, bool applyToIniFileWriter)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SetTrimming(trimmingData, applyToIniFileWriter));
                return;
            }

            MovieTrimmingData = trimmingData;
            if (applyToIniFileWriter)
            {
                ifw.SetTrimmingData(MovieTrimmingData);
            }
        }

        [GeneratedRegex("^lang_\\w+_(?<ISO>\\w+).ini$")]
        private static partial Regex LangFileRegex();
        [GeneratedRegex("^view(?<w>\\d+)x(?<h>\\d+)(?<extra>.*)\\.xml$")]
        private static partial Regex ViewLayoutFileRegex();
        [GeneratedRegex("^movie\\.\\w+$")]
        private static partial Regex MovieFileRegex();
    }
}