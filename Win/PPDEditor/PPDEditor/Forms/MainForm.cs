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
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor.Forms
{
    public partial class EditorForm : Form, IGameForm, IMovieManager, ISongInfo
    {

        const string rescuedirectory = "rescueddata";
        const string dockxmlname = "dockforminfo.xml";
        const string scriptdockxmlname = "scriptdockforminfo.xml";
        const string projectcache = "projectcache.dat";
        public const string iniFileName = "PPDEditor.ini";
        const string shortcutFileName = "PPDEditor_keybind.ini";
        const int latestprojectversion = 2;

        static PosAndAngleLoaderSaver paals;
        static WpfDockShell paalsDock;
        static KasiEditor ke;
        static WpfDockShell keDock;
        static SoundManager sm;
        static WpfDockShell smDock;
        static IniFileWriter ifw;
        static WpfDockShell ifwDock;
        static BPMMeasure bm;
        static WpfDockShell bmDock;
        static TimeLineForm tlf;
        static DXForm dxf;
        static InfoForm infof;
        static WpfDockShell infofDock;
        static LayerManager lm;
        static WpfDockShell lmDock;
        static EventManager em;
        static WpfDockShell emDock;
        static GeometryCreator gc;
        static WpfDockShell gcDock;
        static MemoWindow mw;
        static WpfDockShell mwDock;
        static HelpForm hf;
        static WpfDockShell hfDock;
        static WriteDoneWindow wdf;
        static ResourceManager rm;
        static WpfDockShell rmDock;
        static ScriptManager scm;
        static WpfDockShell scmDock;
        static StatsManager stm;
        static WpfDockShell stmDock;

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

        public EditorForm()
        {
            InitializeComponent();
            dockPanel1.Extender.FloatWindowFactory = new CustomFloatWindowFactory();
            this.SizeChanged += MainForm_SizeChanged;
        }

        void MainForm_SizeChanged(object sender, EventArgs e)
        {
            ChangeBackGround();
        }

        #region Initialize
        public void Initialize()
        {
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["PAALS"]));
            paals = new PosAndAngleLoaderSaver();
            paalsDock = new WpfDockShell(paals, "PPDEditor.PosAndAngleLoaderSaver");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["KE"]));
            ke = new KasiEditor();
            keDock = new WpfDockShell(ke, "PPDEditor.KasiEditor");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["SM"]));
            sm = new SoundManager();
            smDock = new WpfDockShell(sm, "PPDEditor.SoundManager");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["SFW"]));
            ifw = new IniFileWriter();
            ifwDock = new WpfDockShell(ifw, "PPDEditor.IniFileWriter");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["BPMMeasure"]));
            bm = new BPMMeasure();
            bmDock = new WpfDockShell(bm, "PPDEditor.BPMMeasure");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["TimeLine"]));
            tlf = new TimeLineForm();
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["GameWindow"]));
            dxf = new DXForm();
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["Property"]));
            infof = new InfoForm();
            infofDock = new WpfDockShell(infof, "PPDEditor.InfoForm");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["LayerManager"]));
            lm = new LayerManager();
            lmDock = new WpfDockShell(lm, "PPDEditor.LayerManager");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["EventManager"]));
            em = new EventManager();
            emDock = new WpfDockShell(em, "PPDEditor.EventManager");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["GeometryCreator"]));
            gc = new GeometryCreator();
            gcDock = new WpfDockShell(gc, "PPDEditor.GeometryCreator");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["Memo"]));
            mw = new MemoWindow();
            mwDock = new WpfDockShell(mw, "PPDEditor.MemoWindow");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["Help"]));
            hf = new HelpForm();
            hfDock = new WpfDockShell(hf, "PPDEditor.HelpForm");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["WriteDoneWindow"]));
            wdf = new WriteDoneWindow();
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["ResourceManager"]));
            rm = new ResourceManager();
            rmDock = new WpfDockShell(rm, "PPDEditor.ResourceManager");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["ScriptManager"]));
            scm = new ScriptManager();
            scmDock = new WpfDockShell(scm, "PPDEditor.ScriptManager");
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["StatsManager"]));
            stm = new StatsManager();
            stmDock = new WpfDockShell(stm, "PPDEditor.StatsManager");

            Grid = new SquareGrid();
            NativeMethods.timeBeginPeriod(1);
            WindowUtility.MainForm = this;
            WindowUtility.BPMMeasure = bm;
            WindowUtility.DXForm = dxf;
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

            paalsDock.VisibleChanged += Windowvisiblechanged;
            keDock.VisibleChanged += Windowvisiblechanged;
            smDock.VisibleChanged += Windowvisiblechanged;
            ifwDock.VisibleChanged += Windowvisiblechanged;
            bmDock.VisibleChanged += Windowvisiblechanged;
            tlf.VisibleChanged += Windowvisiblechanged;
            dxf.VisibleChanged += Windowvisiblechanged;
            infofDock.VisibleChanged += Windowvisiblechanged;
            lmDock.VisibleChanged += Windowvisiblechanged;
            emDock.VisibleChanged += Windowvisiblechanged;
            gcDock.VisibleChanged += Windowvisiblechanged;
            mwDock.VisibleChanged += Windowvisiblechanged;
            hfDock.VisibleChanged += Windowvisiblechanged;
            rmDock.VisibleChanged += Windowvisiblechanged;
            scmDock.VisibleChanged += Windowvisiblechanged;
            stmDock.VisibleChanged += Windowvisiblechanged;

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
            toolStrip1.Renderer = new CustomToolStripRenderer();
            if (File.Exists("skin.ini"))
            {
                dockPanel1.BackColor = PPDEditorSkin.Skin.BorderColor;
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

        private static void LoadMod(ToolStripMenuItem menu, string dir)
        {
            foreach (string dirName in Directory.GetDirectories(dir))
            {
                var newItem = new ToolStripMenuItem
                {
                    Text = Path.GetFileNameWithoutExtension(dirName)
                };
                menu.DropDownItems.Add(newItem);
                LoadMod(newItem, dirName);
            }
            foreach (string fileName in Directory.GetFiles(dir, "*.mod"))
            {
                menu.DropDownItems.Add(new ToolStripMenuItem
                {
                    Text = Path.GetFileNameWithoutExtension(fileName),
                    CheckOnClick = true,
                    Tag = true
                });
            }
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
                    var langtsmi = new ToolStripMenuItem
                    {
                        Text = name,
                        Name = fileName.ToLower(),
                        Checked = Path.GetFileName(fileName).Equals(String.Format("lang_{0}_{1}.ini", this.GetType().Assembly.GetName().Name, PPDStaticSetting.langFileISO), StringComparison.CurrentCultureIgnoreCase)
                    };
                    langtsmi.Click += Langtsmi_Click;
                    言語ToolStripMenuItem.DropDownItems.Add(langtsmi);
                }
            }
        }

        void Langtsmi_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                langFileName = tsmi.Name;
                var m = LangFileRegex().Match(Path.GetFileName(langFileName));
                if (m.Success)
                {
                    PPDStaticSetting.langFileISO = m.Groups["ISO"].Value;
                }
                SetLanguage(PPDStaticSetting.langFileISO);
                foreach (ToolStripMenuItem child in (tsmi.OwnerItem as ToolStripMenuItem).DropDownItems)
                {
                    child.Checked = false;
                }
                tsmi.Checked = true;
                ChangeHelpUrl();
            }
        }

        private void SetLanguage(string langIso)
        {
            Utility.ChangeLanguage(langIso);
            this.SuspendLayout();
            言語ToolStripMenuItem.Text = Utility.Language["Language"];
            this.ファイルToolStripMenuItem.Text = Utility.Language["File"];
            this.プロジェクトを開くToolStripMenuItem.Text = Utility.Language["OpenProject"];
            this.プロジェクトを上書き保存ToolStripMenuItem.Text = Utility.Language["OverWriteProject"];
            this.開くToolStripMenuItem.Text = Utility.Language["Open"];
            this.保存ToolStripMenuItem.Text = Utility.Language["Save"];
            this.最近使用したプロジェクトToolStripMenuItem.Text = Utility.Language["RecentUsedFile"];
            this.終了ToolStripMenuItem.Text = Utility.Language["Exit"];
            this.編集ToolStripMenuItem.Text = Utility.Language["Edit"];
            this.元に戻すToolStripMenuItem.Text = Utility.Language["Undo"];
            this.やり直すToolStripMenuItem.Text = Utility.Language["Redo"];
            this.ライン入れ替えToolStripMenuItem.Text = Utility.Language["SwapLine"];
            this.動画ToolStripMenuItem.Text = Utility.Language["Movie"];
            this.再生ToolStripMenuItem.Text = Utility.Language["Play"];
            this.一時停止ToolStripMenuItem.Text = Utility.Language["Pause"];
            this.停止ToolStripMenuItem.Text = Utility.Language["Stop"];
            this.打鍵記録ToolStripMenuItem.Text = Utility.Language["RecordKey"];
            this.切り取りToolStripMenuItem.Text = Utility.Language["Cut"];
            this.表示ToolStripMenuItem.Text = Utility.Language["Show"];
            this.強制的にラインに乗せるToolStripMenuItem.Text = Utility.Language["ForceFit"];
            this.レイヤー全体ToolStripMenuItem.Text = Utility.Language["EntireSelectedLayer"];
            this.選択範囲のみToolStripMenuItem.Text = Utility.Language["OnlyInSelection"];
            dxf.TabText = this.ゲームウィンドウToolStripMenuItem.Text = Utility.Language["GameWindow"];
            tlf.TabText = this.タイムラインウィンドウToolStripMenuItem.Text = Utility.Language["TimeLine"];
            infof.TabText = this.プロパティウィンドウToolStripMenuItem.Text = Utility.Language["Property"];
            paals.TabText = this.位置角度ローダーセーバーToolStripMenuItem.Text = Utility.Language["PAALS"];
            ke.TabText = this.歌詞エディターToolStripMenuItem.Text = Utility.Language["KE"];
            sm.TabText = this.サウンドマネージャーToolStripMenuItem.Text = Utility.Language["SM"];
            ifw.TabText = this.設定ファイルライターToolStripMenuItem.Text = Utility.Language["SFW"];
            bm.TabText = this.bPMToolStripMenuItem.Text = Utility.Language["BPMMeasure"];
            lm.TabText = this.レイヤーマネージャーToolStripMenuItem.Text = Utility.Language["LayerManager"];
            em.TabText = this.イベントマネージャーToolStripMenuItem.Text = Utility.Language["EventManager"];
            gc.TabText = this.幾何クリエーターToolStripMenuItem.Text = Utility.Language["GeometryCreator"];
            mw.TabText = this.メモToolStripMenuItem.Text = Utility.Language["Memo"];
            hf.TabText = this.ヘルプToolStripMenuItem.Text = Utility.Language["Help"];
            rm.TabText = this.リソースマネージャーToolStripMenuItem.Text = Utility.Language["ResourceManager"];
            scm.TabText = this.スクリプトマネージャーToolStripMenuItem.Text = Utility.Language["ScriptManager"];
            stm.TabText = this.統計ToolStripMenuItem.Text = Utility.Language["StatsManager"];
            this.新規プロジェクトを作成ToolStripMenuItem.Text = Utility.Language["CPF"];


            this.ゲームToolStripMenuItem.Text = Utility.Language["Game"];
            this.現在の時間から開始ToolStripMenuItem.Text = Utility.Language["GameStart1"];
            this.最初から開始ToolStripMenuItem.Text = Utility.Language["GameStart2"];
            this.現在の時間から開始オートToolStripMenuItem.Text = Utility.Language["GameStart3"];
            this.最初から開始オートToolStripMenuItem.Text = Utility.Language["GameStart4"];
            this.メニューから開始ToolStripMenuItem.Text = Utility.Language["StartFromMenu"];
            this.pPDプロセスを強制終了ToolStripMenuItem.Text = Utility.Language["ExitPPDProcess"];
            this.その他ToolStripMenuItem.Text = Utility.Language["Others"];
            this.設定ToolStripMenuItem.Text = Utility.Language["Setting"];
            this.プロジェクトをマージToolStripMenuItem.Text = Utility.Language["MergeProject"];
            this.発行ToolStripMenuItem.Text = Utility.Language["Publish"];
            this.ビューを読み込むToolStripMenuItem.Text = Utility.Language["ReadView"];
            this.現在の難易度をコピーToolStripMenuItem.Text = Utility.Language["CopyCurrentDifficulty"];
            this.難易度ToolStripMenuItem.Text = Utility.Language["Difficulty"];
            this.ツールバーを隠すToolStripMenuItem.Text = Utility.Language["HideToolBar"];
            this.ドックを固定ToolStripMenuItem.Text = Utility.Language["FixDockPanel"];
            this.mODを発行ToolStripMenuItem.Text = Utility.Language["PublishMod"];
            this.イベントToolStripMenuItem.Text = Utility.Language["Event"];
            this.インポートToolStripMenuItem.Text = Utility.Language["Import"];
            this.別のファイルToolStripMenuItem.Text = Utility.Language["Other Files"];
            this.disableExpansionToolStripMenuItem.Text = Utility.Language["DisableExpansion"];
            this.コマンドToolStripMenuItem.Text = Utility.Language["Command"];
            this.一覧を再読み込みToolStripMenuItem.Text = Utility.Language["ReloadList"];
            infof.SetLang();
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
            this.toolStripButton1.ToolTipText = Utility.Language["FixOp"];
            this.toolStripButton2.ToolTipText = Utility.Language["Drawtoggle"];
            this.toolStripButton3.ToolTipText = Utility.Language["ShowHoldExtent"];
            this.toolStripButton4.ToolTipText = Utility.Language["MarkFocus"];
            this.toolStripSplitButton1.Text = Utility.Language["LimitTimeLineRow"];
            this.exToolStripSplitButton1.ToolTipText = Utility.Language["DisplayGrid"];
            this.詳細設定ToolStripMenuItem.Text = Utility.Language["GridDetailSetting"];
            this.詳細設定ToolStripMenuItem1.Text = Utility.Language["GridDetailSetting"];
            this.詳細設定ToolStripMenuItem2.Text = Utility.Language["GridDetailSetting"];
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
            this.toolStripTextBox3.ToolTipText = Utility.Language["DisplayWidth"];
            //setlang
            ifw.SetLang();
            ke.SetLang();
            paals.SetLang();
            sm.SetLang();
            bm.SetLang();
            dxf.SetLang();
            em.SetLang();
            lm.SetLang();
            gc.SetLang();
            mw.SetLang();
            wdf.SetLang();
            rm.SetLang();
            scm.SetLang();
            stm.SetLang();
            tlf.SetLang();
            this.ResumeLayout();
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
                        var tsmi = new ToolStripMenuItem
                        {
                            Text = String.Format("{0}x{1}{2}", m.Groups["w"].Value, m.Groups["h"].Value, m.Groups["extra"].Value)
                        };
                        tsmi.Click += Viewtsmi_Click;
                        tsmi.Tag = file;
                        ビューを読み込むToolStripMenuItem.DropDownItems.Add(tsmi);
                    }
                }
            }
        }

        void Viewtsmi_Click(object sender, EventArgs e)
        {
            var tsmi = sender as ToolStripMenuItem;
            this.SuspendLayout();
            //this.Size = size;
            ReadDockXML(tsmi.Tag as string);
            this.ResumeLayout();
        }

        private void ReadDockXML(string path)
        {
            dockPanel1.SuspendLayout();
            paalsDock.DockPanel = null;
            keDock.DockPanel = null;
            smDock.DockPanel = null;
            ifwDock.DockPanel = null;
            bmDock.DockPanel = null;
            tlf.DockPanel = null;
            dxf.DockPanel = null;
            infofDock.DockPanel = null;
            lmDock.DockPanel = null;
            emDock.DockPanel = null;
            gcDock.DockPanel = null;
            mwDock.DockPanel = null;
            hfDock.DockPanel = null;
            rmDock.DockPanel = null;
            scmDock.DockPanel = null;
            stmDock.DockPanel = null;
            CloseAllDocuments();

            dockPanel1.LoadFromXml(path, this.DeserializeForm);
            dockPanel1.ResumeLayout();
        }

        private void CloseAllDocuments()
        {
            if (dockPanel1.DocumentStyle == DocumentStyle.SystemMdi)
            {
                foreach (Form form in MdiChildren)
                    form.Close();
            }
            else
            {
                for (int index = dockPanel1.Contents.Count - 1; index >= 0; index--)
                {
                    if (dockPanel1.Contents[index] is IDockContent content)
                    {
                        content.DockHandler.Close();
                    }
                }
            }
        }

        private static void ChangeAuthorName()
        {
            var anf = new AuthorNameWindow();
            anf.SetLang();
            if (PPDStaticSetting.AuthorName != null) anf.AuthorName = PPDStaticSetting.AuthorName;
            if (WpfDialog.ShowDialog(anf))
            {
                PPDStaticSetting.AuthorName = anf.AuthorName;
            }
        }

        private void InitializeDockWindow()
        {
            dockPanel1.SuspendLayout(true);
            var path = Path.Combine(Utility.AppDir, dockxmlname);
            if (File.Exists(path))
            {
                ReadDockXML(path);
            }
            else
            {
                dxf.Show(dockPanel1, DockState.Document);
                infofDock.Show(dockPanel1, DockState.DockRight);
                tlf.Show(dockPanel1, DockState.DockBottom);
            }
            scm.RestoreDock(Path.Combine(Utility.AppDir, scriptdockxmlname));
            dockPanel1.Skin = PPDEditorSkin.Skin.DockPanelSkin;
            this.toolStripStatusLabel1.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.toolStripStatusLabel2.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.toolStripStatusLabel3.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.ファイルToolStripMenuItem.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.編集ToolStripMenuItem.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.動画ToolStripMenuItem.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.表示ToolStripMenuItem.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            infof.SetSkin();
            ifw.SetSkin();
            ke.SetSkin();
            paals.SetSkin();
            sm.SetSkin();
            bm.SetSkin();
            tlf.SetSkin();
            gc.SetSkin();
            dockPanel1.ResumeLayout(true, true);
        }

        private void Windowvisiblechanged(object sender, EventArgs e)
        {
            this.bPMToolStripMenuItem.Checked = Utility.CheckVisible(bmDock);
            this.ゲームウィンドウToolStripMenuItem.Checked = Utility.CheckVisible(dxf);
            this.サウンドマネージャーToolStripMenuItem.Checked = Utility.CheckVisible(smDock);
            this.タイムラインウィンドウToolStripMenuItem.Checked = Utility.CheckVisible(tlf);
            this.プロパティウィンドウToolStripMenuItem.Checked = Utility.CheckVisible(infofDock);
            this.位置角度ローダーセーバーToolStripMenuItem.Checked = Utility.CheckVisible(paalsDock);
            this.歌詞エディターToolStripMenuItem.Checked = Utility.CheckVisible(keDock);
            this.設定ファイルライターToolStripMenuItem.Checked = Utility.CheckVisible(ifwDock);
            this.レイヤーマネージャーToolStripMenuItem.Checked = Utility.CheckVisible(lmDock);
            this.イベントマネージャーToolStripMenuItem.Checked = Utility.CheckVisible(emDock);
            this.幾何クリエーターToolStripMenuItem.Checked = Utility.CheckVisible(gcDock);
            this.メモToolStripMenuItem.Checked = Utility.CheckVisible(mwDock);
            this.ヘルプToolStripMenuItem.Checked = Utility.CheckVisible(hfDock);
            this.リソースマネージャーToolStripMenuItem.Checked = Utility.CheckVisible(rmDock);
            this.スクリプトマネージャーToolStripMenuItem.Checked = Utility.CheckVisible(scmDock);
            this.統計ToolStripMenuItem.Checked = Utility.CheckVisible(stmDock);
        }

        private IDockContent DeserializeForm(string persistentString)
        {
            return persistentString switch
            {
                "PPDEditor.BPMMeasure" => bmDock,
                "PPDEditor.DXForm" => dxf,
                "PPDEditor.InfoForm" => infofDock,
                "PPDEditor.KasiEditor" => keDock,
                "PPDEditor.PosAndAngleLoaderSaver" => paalsDock,
                "PPDEditor.SoundManager" => smDock,
                "PPDEditor.TimeLineForm" => tlf,
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
        private void ChangeBackGround()
        {
            if (PPDEditorSkin.Skin.BackGround == null || this.ClientSize.Width == 0 || this.ClientSize.Height == 0) return;
            try
            {
                this.statusStrip1.SuspendLayout();
                this.menuStrip1.SuspendLayout();
                this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
                this.toolStripContainer1.ContentPanel.SuspendLayout();
                this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
                this.toolStripContainer1.SuspendLayout();
                this.toolStrip1.SuspendLayout();
                this.toolStripContainer2.ContentPanel.SuspendLayout();
                this.toolStripContainer2.TopToolStripPanel.SuspendLayout();
                this.toolStripContainer2.SuspendLayout();
                this.SuspendLayout();
                ToolStripPanel top = this.toolStripContainer1.TopToolStripPanel;
                ToolStripContentPanel center = this.toolStripContainer1.ContentPanel;
                ToolStripPanel bottom = this.toolStripContainer1.BottomToolStripPanel;
                System.Drawing.Bitmap original = PPDEditorSkin.Skin.BackGround;
                var target = new System.Drawing.Bitmap(this.ClientSize.Width, this.ClientSize.Height);
                float ratio = (float)target.Height / original.Height;
                var g = System.Drawing.Graphics.FromImage(target);
                var sb = new System.Drawing.SolidBrush(PPDEditorSkin.Skin.FillColor);
                g.FillRectangle(sb, new System.Drawing.Rectangle(0, 0, target.Size.Width, target.Size.Height));
                sb.Dispose();
                sb = new System.Drawing.SolidBrush(PPDEditorSkin.Skin.OverLayColor);
                switch (PPDEditorSkin.Skin.DisplayMode)
                {
                    case PPDEditorSkin.BackGroundDisplayMode.right:
                        g.DrawImage(original, new System.Drawing.Rectangle(target.Width - (int)(original.Width * ratio), 0, (int)(original.Width * ratio), target.Height), new System.Drawing.Rectangle(0, 0, original.Width, original.Height), System.Drawing.GraphicsUnit.Pixel);
                        break;
                    case PPDEditorSkin.BackGroundDisplayMode.left:
                        g.DrawImage(original, new System.Drawing.Rectangle(0, 0, (int)(original.Width * ratio), target.Height), new System.Drawing.Rectangle(0, 0, original.Width, original.Height), System.Drawing.GraphicsUnit.Pixel);
                        break;
                    case PPDEditorSkin.BackGroundDisplayMode.center:
                        if (target.Width > (int)(original.Width * ratio))
                        {
                            g.DrawImage(original, new System.Drawing.Rectangle((target.Width - (int)(original.Width * ratio)) / 2, 0, (int)(original.Width * ratio), target.Height), new System.Drawing.Rectangle(0, 0, original.Width, original.Height), System.Drawing.GraphicsUnit.Pixel);
                        }
                        else
                        {
                            g.DrawImage(original, new System.Drawing.Rectangle(0, 0, target.Width, target.Height), new System.Drawing.Rectangle((original.Width - (int)(target.Width / ratio)) / 2, 0, (int)(target.Width / ratio), original.Height), System.Drawing.GraphicsUnit.Pixel);
                        }
                        break;
                    case PPDEditorSkin.BackGroundDisplayMode.fill:
                        g.DrawImage(original, new System.Drawing.Rectangle(0, 0, target.Width, target.Height), new System.Drawing.Rectangle(0, 0, original.Width, original.Height), System.Drawing.GraphicsUnit.Pixel);
                        break;
                }
                var topbit = new System.Drawing.Bitmap(top.Width, top.Height);
                g = System.Drawing.Graphics.FromImage(topbit);
                g.DrawImage(target, new System.Drawing.Rectangle(topbit.Width - target.Width, 0, target.Width, topbit.Height), new System.Drawing.Rectangle(0, 0, target.Width, topbit.Height), System.Drawing.GraphicsUnit.Pixel);
                g.FillRectangle(sb, new System.Drawing.Rectangle(0, 0, topbit.Width, topbit.Height));
                top.BackgroundImage = topbit;
                menuStrip1.BackgroundImage = topbit;

                var centbit = new System.Drawing.Bitmap(center.Width, center.Height);
                g = System.Drawing.Graphics.FromImage(centbit);
                g.DrawImage(target, new System.Drawing.Rectangle(centbit.Width - target.Width, 0, target.Width, centbit.Height), new System.Drawing.Rectangle(0, topbit.Height, target.Width, centbit.Height), System.Drawing.GraphicsUnit.Pixel);
                center.BackgroundImage = centbit;
                var bottombit = new System.Drawing.Bitmap(bottom.Width, bottom.Height);
                g = System.Drawing.Graphics.FromImage(bottombit);
                g.DrawImage(target, new System.Drawing.Rectangle(bottombit.Width - target.Width, 0, target.Width, bottombit.Height), new System.Drawing.Rectangle(0, topbit.Height + centbit.Height, target.Width, bottombit.Height), System.Drawing.GraphicsUnit.Pixel);
                g.FillRectangle(sb, new System.Drawing.Rectangle(0, 0, bottombit.Width, bottombit.Height));
                bottom.BackgroundImage = bottombit;
                statusStrip1.BackgroundImage = bottombit;

                top = this.toolStripContainer2.TopToolStripPanel;
                topbit = new System.Drawing.Bitmap(top.Width, top.Height);
                g = System.Drawing.Graphics.FromImage(topbit);
                g.DrawImage(center.BackgroundImage, new System.Drawing.Rectangle(0, 0, center.BackgroundImage.Width, topbit.Height), new System.Drawing.Rectangle(0, 0, center.BackgroundImage.Width, topbit.Height), System.Drawing.GraphicsUnit.Pixel);
                g.FillRectangle(sb, new System.Drawing.Rectangle(0, 0, topbit.Width, topbit.Height));
                top.BackgroundImage = topbit;

                var dockBit = new System.Drawing.Bitmap(dockPanel1.Width, dockPanel1.Height);
                g = System.Drawing.Graphics.FromImage(dockBit);
                g.DrawImage(center.BackgroundImage, new System.Drawing.Rectangle(0, 0, center.BackgroundImage.Width, center.BackgroundImage.Height), new System.Drawing.Rectangle(0, topbit.Height, dockBit.Width, dockBit.Height), System.Drawing.GraphicsUnit.Pixel);
                dockPanel1.BackgroundImage = dockBit;
                sb.Dispose();

                this.statusStrip1.ResumeLayout(false);
                this.statusStrip1.PerformLayout();
                this.menuStrip1.ResumeLayout(false);
                this.menuStrip1.PerformLayout();
                this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
                this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
                this.toolStripContainer1.ContentPanel.ResumeLayout(false);
                this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
                this.toolStripContainer1.TopToolStripPanel.PerformLayout();
                this.toolStripContainer1.ResumeLayout(false);
                this.toolStripContainer1.PerformLayout();
                this.toolStrip1.ResumeLayout(false);
                this.toolStrip1.PerformLayout();
                this.toolStripContainer2.ContentPanel.ResumeLayout(false);
                this.toolStripContainer2.TopToolStripPanel.ResumeLayout(false);
                this.toolStripContainer2.TopToolStripPanel.PerformLayout();
                this.toolStripContainer2.ResumeLayout(false);
                this.toolStripContainer2.PerformLayout();
                this.ResumeLayout(false);
            }
            catch { }
        }
        #endregion

        private void MovieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Filter = openmoviefilter;
            this.openFileDialog1.RestoreDirectory = true;
            this.openFileDialog1.FileName = "";
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
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

            this.プロジェクトを上書き保存ToolStripMenuItem.Enabled = this.難易度ToolStripMenuItem.Enabled =
                this.発行ToolStripMenuItem.Enabled = this.mODを発行ToolStripMenuItem.Enabled = IsProjectLoaded;

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
            if (InvokeRequired)
            {
                Invoke((Action)Play);
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
            if (InvokeRequired)
            {
                Invoke((Action)Pause);
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
            if (InvokeRequired)
            {
                Invoke((Action)Stop);
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
                tlf.Seekmain.HorizontalScroll.Value = 0;
            }
        }

        public void Seek(double time)
        {
            if (InvokeRequired)
            {
                Invoke((Action<double>)Seek, time);
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
            var fm2 = new MovieCutWindow(MovieTrimmingData);
            fm2.SetLang();
            if (WpfDialog.ShowDialog(fm2))
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
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
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
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
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
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
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
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
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
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
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
            this.saveFileDialog1.FileName = "NewFile.evd"; if (saveFileDialog1.ShowDialog() == DialogResult.OK)
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
            LayerDisplay[] lds = lm.AllLayerDisplay;
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

        private DialogResult CheckCanSaveAndSaveProject()
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
            DialogResult ret = saveFileDialog1.ShowDialog();
            if (ret == DialogResult.OK)
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
                this.プロジェクトを上書き保存ToolStripMenuItem.Enabled = this.難易度ToolStripMenuItem.Enabled
                    = this.発行ToolStripMenuItem.Enabled = this.mODを発行ToolStripMenuItem.Enabled = IsProjectLoaded;
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
            LayerDisplay[] lds = lm.AllLayerDisplay;
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
                        MessageBox.Show((datas[i]) + nosoundfile);
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
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
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
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
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
                MessageBox.Show(invaliddata);
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
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
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
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
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
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
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
                if (MessageBox.Show(oldprojectdetected, confirm, MessageBoxButtons.OKCancel) == DialogResult.OK)
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
            MessageBox.Show("Data was rescued at\n" + Path.GetFullPath(rescuedirectory));
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
            var fm5 = new SwapLineWindow();
            fm5.SetLang();
            if (WpfDialog.ShowDialog(fm5))
            {
                int first = fm5.First;
                int second = fm5.Second;
                tlf.Seekmain.Swap(first, second);
            }
        }
        private void サウンドマネージャーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, smDock, ModifierKeys);
        }

        private void 設定ファイルライターToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, ifwDock, ModifierKeys);
        }

        private void 位置角度ローダーセーバーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, paalsDock, ModifierKeys);
        }

        private void 歌詞エディターToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, keDock, ModifierKeys);
        }

        private void BPMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, bmDock, ModifierKeys);
        }

        private void ゲームウィンドウToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, dxf, ModifierKeys);
        }

        private void タイムラインウィンドウToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, tlf, ModifierKeys);
        }

        private void プロパティウィンドウToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, infofDock, ModifierKeys);
        }
        private void レイヤーマネージャーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, lmDock, ModifierKeys);
        }

        private void イベントマネージャーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, emDock, ModifierKeys);
        }
        private void 幾何クリエーターToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, gcDock, ModifierKeys);
        }

        private void メモToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, mwDock, ModifierKeys);
        }

        private void リソースマネージャーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, rmDock, ModifierKeys);
        }

        private void スクリプトマネージャーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, scmDock, ModifierKeys);
        }

        private void ヘルプToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, hfDock, ModifierKeys);
        }

        private void 統計ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, stmDock, ModifierKeys);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            scm.SaveDock(Path.Combine(Utility.AppDir, scriptdockxmlname));
            dockPanel1.SaveAsXml(Path.Combine(Utility.AppDir, dockxmlname));
            SaveShortcut();
        }

        public void ChangeMousePos(Point p)
        {
            MyGame.ChangeMousepos(p);
        }

        public void ChangeData(Vector2[] positions, float[] angles)
        {
            MyGame.ChangeData(positions, angles);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            const Int32 MF_BYPOSITION = 0x400;
            const Int32 MF_REMOVE = 0x1000;

            const UInt32 SC_CLOSE = 0x0000F060;
            const UInt32 MF_BYCOMMAND = 0x00000000;
            var menu = NativeMethods.GetSystemMenu(this.Handle, false);
            NativeMethods.RemoveMenu(menu, SC_CLOSE, MF_BYCOMMAND);
            var menuCount = NativeMethods.GetMenuItemCount(menu);
            if (menuCount > 1)
            {
                //メニューの「閉じる」を削除
                NativeMethods.RemoveMenu(menu, (uint)(menuCount - 1), MF_BYPOSITION | MF_REMOVE);
                NativeMethods.DrawMenuBar(this.Handle);
            }
        }

        private bool CheckProcess()
        {
            if (currentProcess != null && !currentProcess.HasExited)
            {
                var dr = MessageBox.Show(askkillprocess, confirm, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                switch (dr)
                {
                    case DialogResult.OK:
                        currentProcess.Kill();
                        break;
                    case DialogResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        private bool CheckExecutor()
        {
            if (currentExecutor != null && currentExecutor.IsWorking)
            {
                var dr = MessageBox.Show(askkillcommand, confirm, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                switch (dr)
                {
                    case DialogResult.OK:
                        currentExecutor.Kill();
                        currentExecutor = null;
                        CommandExecuted?.Invoke(null);
                        break;
                    case DialogResult.Cancel:
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
                var dr = MessageBox.Show(asksave, confirm, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                switch (dr)
                {
                    case DialogResult.Yes:
                        cancel |= CheckCanSaveAndSaveProject() == DialogResult.Cancel;

                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
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
            WindowState = PPDStaticSetting.LastWindowState;
            ClientSize = PPDStaticSetting.LastWindowSize;
            Location = PPDStaticSetting.LastWindowLocation;
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
            if (this.最近使用したプロジェクトToolStripMenuItem.DropDownItems.ContainsKey(path))
            {
                var tsmis = this.最近使用したプロジェクトToolStripMenuItem.DropDownItems.Find(path, false);
                if (tsmis.Length == 1)
                {
                    this.最近使用したプロジェクトToolStripMenuItem.DropDownItems.Remove(tsmis[0]);
                    this.最近使用したプロジェクトToolStripMenuItem.DropDownItems.Insert(0, tsmis[0]);
                }
            }
            else
            {
                var tsmi = new ToolStripMenuItem(path);
                tsmi.Name = tsmi.Text = path;
                tsmi.Click += Tsmi_Click;
                this.最近使用したプロジェクトToolStripMenuItem.DropDownItems.Insert(0, tsmi);
                if (this.最近使用したプロジェクトToolStripMenuItem.DropDownItems.Count > 5)
                {
                    this.最近使用したプロジェクトToolStripMenuItem.DropDownItems.RemoveAt(this.最近使用したプロジェクトToolStripMenuItem.DropDownItems.Count - 1);
                }
            }
        }

        void Tsmi_Click(object sender, EventArgs e)
        {
            CheckReadProject((sender as ToolStripMenuItem).Text);
        }

        private void SaveRecentUsedProject()
        {
            using StreamWriter sw = new(Path.Combine(Utility.AppDir, projectcache));
            foreach (ToolStripMenuItem tsmi in this.最近使用したプロジェクトToolStripMenuItem.DropDownItems)
            {
                sw.WriteLine(tsmi.Text);
            }
        }

        private void SaveConfig()
        {
            var path = Path.Combine(Utility.AppDir, iniFileName);
            var setting = new SettingReader(File.ReadAllText(path));
            setting.ReplaceOrAdd("Language", PPDStaticSetting.langFileISO);
            setting.ReplaceOrAdd("Maximised", WindowState == FormWindowState.Maximized);
            setting.ReplaceOrAdd("Size", String.Format("{0},{1}", this.ClientSize.Width, this.ClientSize.Height));
            setting.ReplaceOrAdd("Location", String.Format("{0},{1}", Location.X, Location.Y));
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
            現在の時間から開始ToolStripMenuItem.Enabled = 現在の時間から開始オートToolStripMenuItem.Enabled =
                最初から開始ToolStripMenuItem.Enabled = 最初から開始オートToolStripMenuItem.Enabled = メニューから開始ToolStripMenuItem.Enabled = false;
            pPDプロセスを強制終了ToolStripMenuItem.Enabled = true;
        }

        void CurrentProcess_Exited(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                現在の時間から開始ToolStripMenuItem.Enabled = 現在の時間から開始オートToolStripMenuItem.Enabled =
                    最初から開始ToolStripMenuItem.Enabled = 最初から開始オートToolStripMenuItem.Enabled = メニューから開始ToolStripMenuItem.Enabled = true;
                pPDプロセスを強制終了ToolStripMenuItem.Enabled = false;
                ProcessExited?.Invoke();
            }));
        }

        private void プロジェクトを上書き保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wdf.Hide();
            if (OverWriteProject())
            {
                wdf.Show();
                this.Activate();
            }
        }

        private void ToolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            WindowUtility.LayerManager.DisplayMode = (DisplayLineMode)this.toolStripComboBox1.SelectedIndex;
            WindowUtility.Seekmain.DisplayMode = (DisplayLineMode)this.toolStripComboBox1.SelectedIndex;
            WindowUtility.Seekmain.DrawAndRefresh();
        }

        private void ToolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
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
            WindowUtility.LayerManager.BeatType = (DisplayBeatType)this.toolStripComboBox3.SelectedIndex;
            WindowUtility.Seekmain.BeatType = (DisplayBeatType)this.toolStripComboBox3.SelectedIndex;
            WindowUtility.Seekmain.DrawAndRefresh();
        }

        private void ToolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
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
            if (float.TryParse(this.toolStripTextBox2.Text, out float result))
            {
                WindowUtility.LayerManager.BPMOffset = result;
            }
        }

        private void ToolStripTextBox3_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(this.toolStripTextBox3.Text, out int result))
            {
                WindowUtility.LayerManager.DisplayWidth = result;
            }
        }
        public string GetDisplayModeString(DisplayLineMode index)
        {
            if (index < 0 || (int)index >= toolStripComboBox1.Items.Count) return "";
            return toolStripComboBox1.Items[(int)index].ToString();
        }

        private void PxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.exToolStripSplitButton1.Checked = true;
            TurnOffExcept(sender as ToolStripMenuItem);
            GridPixel = 5;
        }

        private void PxToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.exToolStripSplitButton1.Checked = true;
            TurnOffExcept(sender as ToolStripMenuItem);
            GridPixel = 10;
        }

        private void PxToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.exToolStripSplitButton1.Checked = true;
            TurnOffExcept(sender as ToolStripMenuItem);
            GridPixel = 20;
        }

        private void PxToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            this.exToolStripSplitButton1.Checked = true;
            TurnOffExcept(sender as ToolStripMenuItem);
            GridPixel = 50;
        }

        private void TurnOffExcept(ToolStripMenuItem tsmi)
        {
            foreach (ToolStripMenuItem ttsmi in this.exToolStripSplitButton1.DropDownItems)
            {
                if (tsmi != ttsmi) ttsmi.Checked = false;
            }
        }
        private void 詳細設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gsf = new GridSettingWindow();
            gsf.SetLang();
            gsf.Grid = Grid;
            if (WpfDialog.ShowDialog(gsf))
            {
                Grid.CopyFrom(gsf.Grid);
                TurnOffExcept(sender as ToolStripMenuItem);
                this.exToolStripSplitButton1.Checked = true;
            }
        }

        private void 詳細設定ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var fsf = new FarnessSettingWindow
            {
                Farness = Farness
            };
            if (WpfDialog.ShowDialog(fsf))
            {
                Farness = fsf.Farness;
            }
        }

        private void 作者名を変更するToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeAuthorName();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            else e.Effect = DragDropEffects.None;

        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (File.Exists(fileName[0]) && Path.GetExtension(fileName[0]) == ".ppdproj") CheckReadProject(fileName[0]);
        }

        private void 設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sf = new SettingWindow();
            sf.SetLangAngShortcut(WindowUtility.Seekmain.ShortcutManager);
            sf.AuthorName = PPDStaticSetting.AuthorName;
            sf.MovieLatency = PPDStaticSetting.MovieLatency;
            sf.HideToggleArrow = PPDStaticSetting.HideToggleArrow;
            sf.HideToggleRectangle = PPDStaticSetting.HideToggleRectangle;
            sf.EnableToChangeMarkTypeAndTime = PPDStaticSetting.EnableToChangeMarkTypeAndTime;
            sf.Moves = PPDStaticSetting.Moves;
            sf.Angles = PPDStaticSetting.Angles;
            if (WpfDialog.ShowDialog(sf))
            {
                PPDStaticSetting.AuthorName = sf.AuthorName;
                PPDStaticSetting.MovieLatency = sf.MovieLatency;
                PPDStaticSetting.HideToggleArrow = sf.HideToggleArrow;
                PPDStaticSetting.HideToggleRectangle = sf.HideToggleRectangle;
                PPDStaticSetting.EnableToChangeMarkTypeAndTime = sf.EnableToChangeMarkTypeAndTime;
                PPDStaticSetting.Moves = sf.Moves;
                PPDStaticSetting.Angles = sf.Angles;
                SaveShortcut();
                ReadCommands();
                if (sf.ShouldRestart)
                {
                    MessageBox.Show(rebootnecessary);
                }
            }
        }

        private void EasyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeDifficulty(AvailableDifficulty.Easy);
            TurnOffExceptDifficulty(sender as ToolStripMenuItem);
            ChangeWindowText();
        }

        private void NormalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeDifficulty(AvailableDifficulty.Normal);
            TurnOffExceptDifficulty(sender as ToolStripMenuItem);
            ChangeWindowText();
        }

        private void HardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeDifficulty(AvailableDifficulty.Hard);
            TurnOffExceptDifficulty(sender as ToolStripMenuItem);
            ChangeWindowText();
        }

        private void ExtremeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeDifficulty(AvailableDifficulty.Extreme);
            TurnOffExceptDifficulty(sender as ToolStripMenuItem);
            ChangeWindowText();
        }

        private void BaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeDifficulty(AvailableDifficulty.Base);
            TurnOffExceptDifficulty(sender as ToolStripMenuItem);
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

        private void TurnOffExceptDifficulty(ToolStripMenuItem tsmi)
        {
            foreach (ToolStripMenuItem ttsmi in this.難易度ToolStripMenuItem.DropDownItems)
            {
                if (tsmi != ttsmi) ttsmi.Checked = false;
            }
        }

        private void 現在の難易度をコピーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckSaveStateAndSave(true))
            {
                return;
            }
            var cdf = new CopyDifficultyWindow();
            cdf.SetLang();
            cdf.CurrentDifficulty = CurrentDifficulty;
            if (WpfDialog.ShowDialog(cdf))
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
                    alreadyexist = !(MessageBox.Show(alreadyexitdifficulty, confirm, MessageBoxButtons.OKCancel) == DialogResult.OK);
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
            var pf = new PublishWindow();
            pf.SetLang();
            pf.AvailableDifficulty = AvailableDifficulty;
            pf.PublishFolderName = Path.GetFileNameWithoutExtension(projectfilename);
            if (WpfDialog.ShowDialog(pf))
            {
                if (pf.PublishDifficulty == AvailableDifficulty.None)
                {
                    MessageBox.Show(noselecteddifficulty);
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
            var mf = new MergeWindow();
            mf.SetLang();
            if (WpfDialog.ShowDialog(mf))
            {
                AvailableDifficulty mergeDifficulty = mf.MergeDifficulty;
                if (mergeDifficulty == AvailableDifficulty.None)
                {
                    MessageBox.Show(nomergedifficulty);
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
            ツールバーを隠すToolStripMenuItem.Checked = !ツールバーを隠すToolStripMenuItem.Checked;
            toolStripContainer1.TopToolStripPanelVisible = !ツールバーを隠すToolStripMenuItem.Checked;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                // WM_NCMOUSEMOVE
                case 0xA0:
                    base.WndProc(ref m);
                    if (!toolStripContainer1.TopToolStripPanelVisible)
                    {
                        toolStripContainer1.TopToolStripPanelVisible = true;
                        ツールバーを隠すToolStripMenuItem.Checked = false;
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void ChangeWindowText()
        {
            this.Text = string.Format("{0}({1}) - PPDEditor", Path.GetFileNameWithoutExtension(projectfilename), CurrentDifficulty);
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
                    MessageBox.Show(String.Format("{0}\n{1}", Utility.Language["SoundNotExist"], sound));
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
                    MessageBox.Show(String.Format("{0}\n{1}", Utility.Language["ScriptNotExist"], src));
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
                    var dr = MessageBox.Show(asksave, confirm, okCancel ? MessageBoxButtons.OKCancel : MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                    if (dr == DialogResult.Yes || dr == DialogResult.OK)
                    {
                        cancel |= CheckCanSaveAndSaveProject() == DialogResult.Cancel;
                    }
                    else if (dr == DialogResult.No)
                    {
                    }
                    else cancel |= dr == DialogResult.Cancel;
                }
                else
                {
                    var dr = MessageBox.Show(asksave, confirm, okCancel ? MessageBoxButtons.OKCancel : MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                    if (dr == DialogResult.Yes || dr == DialogResult.OK)
                    {
                        SaveProject();
                    }
                    cancel = dr == DialogResult.Cancel;
                }
            }

            return !cancel;
        }

        private void MODを発行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (scm.GetEnabledScriptList().Length < 1)
            {
                MessageBox.Show("MODの発行には少なくともひとつのスクリプトが必要です。");
                return;
            }

            var mpf = new ModPublishWindow();
            mpf.SetLang();
            if (WpfDialog.ShowDialog(mpf))
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

        private static void ImportEvent(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show(String.Format("Not Exist File:{0}", filePath));
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
            var form = new ImportEventWindow();
            form.SetCurrentEvent(em.ChangeData);
            form.SetReadOnlyEvent(data);
            form.SetLang();
            if (WpfDialog.ShowDialog(form))
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
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ImportEvent(this.openFileDialog1.FileName);
            }
        }

        private void イベントToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (!IsProjectLoaded)
            {
                easyToolStripMenuItem1.Enabled = normalToolStripMenuItem1.Enabled = hardToolStripMenuItem1.Enabled =
                    extremeToolStripMenuItem1.Enabled = baseToolStripMenuItem1.Enabled = false;
                return;
            }

            baseToolStripMenuItem1.Enabled = AvailableDifficulty.HasFlag(AvailableDifficulty.Base) && CurrentDifficulty != PPDEditor.AvailableDifficulty.Base;
            easyToolStripMenuItem1.Enabled = AvailableDifficulty.HasFlag(AvailableDifficulty.Easy) && CurrentDifficulty != PPDEditor.AvailableDifficulty.Easy;
            normalToolStripMenuItem1.Enabled = AvailableDifficulty.HasFlag(AvailableDifficulty.Normal) && CurrentDifficulty != PPDEditor.AvailableDifficulty.Normal;
            hardToolStripMenuItem1.Enabled = AvailableDifficulty.HasFlag(AvailableDifficulty.Hard) && CurrentDifficulty != PPDEditor.AvailableDifficulty.Hard;
            extremeToolStripMenuItem1.Enabled = AvailableDifficulty.HasFlag(AvailableDifficulty.Extreme) && CurrentDifficulty != PPDEditor.AvailableDifficulty.Extreme;
        }

        private void ToolStripSplitButton1_CheckStateChanged(object sender, EventArgs e)
        {
            tlf.RowManager.IsLimited = toolStripSplitButton1.Checked;
            tlf.InvalidateAll();
        }

        private void 詳細設定ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var form = new EditTimeLineRowWindow();
            form.SetRowManager(tlf.RowManager);
            form.SetLang();
            if (WpfDialog.ShowDialog(form))
            {
                form.UpdateRowManager(tlf.RowManager);
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
            while (コマンドToolStripMenuItem.DropDownItems.Count > 2)
            {
                コマンドToolStripMenuItem.DropDownItems.RemoveAt(0);
            }

            if (!Directory.Exists("Commands"))
            {
                Directory.CreateDirectory("Commands");
            }

            ReadCommands(コマンドToolStripMenuItem, "Commands");
        }

        private void ReadCommands(ToolStripMenuItem menuItem, string dir)
        {
            int insertIndex = 0;
            foreach (string childDir in Directory.GetDirectories(dir))
            {
                var name = Path.GetFileNameWithoutExtension(childDir);
                var childMenu = new ToolStripMenuItem
                {
                    Text = name
                };
                ReadCommands(childMenu, childDir);
                menuItem.DropDownItems.Insert(insertIndex++, childMenu);
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
                var childMenu = new ToolStripMenuItem
                {
                    Text = name,
                    Tag = childFile
                };
                if (shortcut != null)
                {
                    childMenu.ShortcutKeyDisplayString = shortcut.GetCommandText();
                    childMenu.ShowShortcutKeys = true;
                }
                childMenu.Click += ChildMenu_Click;
                menuItem.DropDownItems.Insert(insertIndex++, childMenu);
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
            コマンドToolStripMenuItem.Enabled = false;
            ゲームToolStripMenuItem.Enabled = false;
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
                    MessageBox.Show(executor.ErrorText);
                }
                foreach (var layer in executor.Layers)
                {
                    layer.Execute();
                }
                if (storage.IsChanged)
                {
                    storage.Save();
                }
                コマンドToolStripMenuItem.Enabled = true;
                ゲームToolStripMenuItem.Enabled = true;
                tlf.Seekmain.DrawAndRefresh();
            });
        }

        private void SafeInvoke(Action action)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(action);
                return;
            }
            action();
        }

        void ChildMenu_Click(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            var filePath = (string)menuItem.Tag;
            ExecuteScript(filePath);
        }

        private void 一覧を再読み込みToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReadCommands();
        }

        public void SetTrimming(MovieTrimmingData trimmingData, bool applyToIniFileWriter)
        {
            if (InvokeRequired)
            {
                Invoke((Action<MovieTrimmingData, bool>)SetTrimming, trimmingData, applyToIniFileWriter);
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