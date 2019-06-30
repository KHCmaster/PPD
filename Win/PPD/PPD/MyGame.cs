using PPDFramework;
using PPDFramework.Scene;
using PPDFramework.ScreenFilters;
using PPDFramework.Sprites;
using PPDFrameworkCore;
using PPDInput;
using PPDMovie;
using PPDSound;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PPD
{
    class MyGame : Game, IGameHost
    {
        public event EventHandler IMEStarted;
        public event EventHandler TextBoxEnabledChanged;
        InputBase input;
        Sound sound;
        SceneManager sceneManager;
        CloseOverlay currentOverray;
        GameLoader currentGame;
        Logo logo;
        Loading loading;
        MovieVolumeSprite movieVolumeSprite;
        bool isHome = true;
        bool goHome;
        TimerManager timerManager;

        KeyConfigManager keyConfigManager;
        GaussianFilter gaussianFilter = new GaussianFilter();

        NotifyControl notifyControl;

        MouseManager mouseManager;
        ScreenShotManager screenShotManager;
        PPDFramework.Resource.ResourceManager homeResourceManager;

        int ignoreInputCount;
        bool debugMode;

        protected override bool IsLoading
        {
            get { return sceneManager.Loading; }
        }

        protected override bool ShowFPS
        {
            get { return true; }
        }

        public SongInformation CurrentSong
        {
            get;
            private set;
        }

        public MusicPlayerBase CurrentPlayerBase
        {
            get;
            private set;
        }

        public TextEditableControl TextEditableControl
        {
            get { return ((TextEditableControl)Control); }
        }

        public MyGame(PPDExecuteArg args)
            : base(args)
        {
            System.Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            if (Args == null || Args.Count == 0)
            {
                FullScreen = PPDSetting.Setting.FullScreen;
            }
            Form.MainForm.Text = "PPD";
            Form.MainForm.MaximizeBox = false;
            Form.MainForm.MinimizeBox = false;
            Form.MainForm.Icon = PPD.Properties.Resources.ppd;
            Form.MainForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Form.MainForm.FormClosing += this.Exit;
        }

        protected override void Initialize()
        {
            base.Initialize();
            var dirs = new[] { @"img\PPD\home", @"img\PPD\main_game", @"img\PPD\single", @"img\PPD\multi" };
            foreach (var dir in dirs)
            {
                var spriteManager = new DirSpriteManager(dir);
                spriteManager.Pack();
            }
            homeResourceManager = new PPDFramework.Resource.SpriteResourceManager(device, @"img\PPD\home");
            input = PPDSetting.Setting.EveryFramePollingDisabled ?
                new AccurateInput(Form.MainForm, PPDSetting.Setting.AccurateInputSleepTime) :
                new Input(Form.MainForm);
            input.Load();
            sound = new Sound(Form.MainForm);
            sound.Initialize();
            keyConfigManager = new KeyConfigManager();
            keyConfigManager.Load("keyconfig.ini");
            for (int i = 0; i < PPDSetting.DefaultSounds.Length; i++)
            {
                sound.AddSound(PPDSetting.DefaultSounds[i]);
            }
            Form.MainForm.ClientSize = new System.Drawing.Size(PPDSetting.Setting.Width, PPDSetting.Setting.Height);
            mouseManager = new MouseManager(Control, device.Offset, device.Scale);
            screenShotManager = new ScreenShotManager(device);
            sceneManager = new SceneManager(device)
            {
                GameHost = this
            };
            sceneManager.Update(EmptyInputInfo.Instance, MouseInfo.Empty, sound);
            movieVolumeSprite = new MovieVolumeSprite(device, this);

            timerManager = new TimerManager(gameTimer);

            if (CheckExecuteMode(out string gamePath))
            {
                LoadSpecial(gamePath);
            }
            else
            {
                InitOverray();
                LoadNormal();
            }
            LoadCommon();
            TextEditableControl.IMEStarted += control_IMEStarted;
            TextEditableControl._EnabledChanged += control_EnabledChanged;
        }

        private bool CheckExecuteMode(out string gamePath)
        {
            gamePath = null;
            if (!Args.ContainsKey("game"))
            {
                return false;
            }
            var temp = String.Format("skins\\{0}.dll", Args["game"]);
            if (File.Exists(temp))
            {
                gamePath = temp;
                return true;
            }
            return false;
        }

        private void InitOverray()
        {
            currentOverray = new CloseOverlay(device);
            SetPropertyToScene(currentOverray);
            currentOverray.ResourceManager = new PPDFramework.Resource.ResourceManager(new Tuple<PPDFramework.Resource.ResourceManager, bool>[] {
                new Tuple<PPDFramework.Resource.ResourceManager, bool>( homeResourceManager,false)
            });
            currentOverray.Load();
            currentOverray.PreScreenFilters.Add(gaussianFilter);
        }

        private void LoadNormal()
        {
            logo = new Logo(device)
            {
                ResourceManager = homeResourceManager
            };
            SetPropertyToScene(logo);
            logo.Load();
            sceneManager.CurrentScene = logo;
            loading = new Loading(device)
            {
                ResourceManager = homeResourceManager
            };
            SetPropertyToScene(loading);
            loading.Load();
            sceneManager.LoadingScene = loading;
        }

        private void LoadCommon()
        {
            notifyControl = new NotifyControl(device, homeResourceManager, gameTimer);
        }

        private void LoadSpecial(string gamePath)
        {
            var gl = new GameLoader(gamePath);
            gl.Load();
            var entrySceneManager = gl.GetEntrySceneManager();

            sceneManager.LoadingScene = gl.GetLoading(device);
            SetPropertyToScene(sceneManager.LoadingScene);
            var spriteResourceManager = new PPDFramework.Resource.SpriteResourceManager(device, entrySceneManager.SpriteDir);
            sceneManager.LoadingScene.ResourceManager = spriteResourceManager;
            sceneManager.LoadingScene.Load();

            var scene = entrySceneManager.GetSceneWithArgs(device, Args, out Dictionary<string, object> dic);
            if (!String.IsNullOrEmpty(scene.SpriteDir))
            {
                spriteResourceManager = new PPDFramework.Resource.SpriteResourceManager(device, scene.SpriteDir);
            }
            sceneManager.ChangeGame(scene, dic, spriteResourceManager);

            currentGame = gl;
            debugMode = true;
        }

        void control_EnabledChanged(object sender, EventArgs e)
        {
            if (TextBoxEnabledChanged != null)
            {
                TextBoxEnabledChanged.Invoke(this, EventArgs.Empty);
            }
        }

        void control_IMEStarted(object sender, EventArgs e)
        {
            if (IMEStarted != null)
            {
                IMEStarted.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetPropertyToScene(ISceneBase scene)
        {
            if (scene == null) return;
            scene.Sound = sound;
            scene.SceneManager = sceneManager;
            scene.GameHost = this;
        }

        protected override void Update()
        {
            if (input.AssignMode)
            {
                ignoreInputCount = 0;
            }
            else
            {
                ignoreInputCount++;
                if (ignoreInputCount >= 60)
                {
                    ignoreInputCount = 60;
                }
            }

            var mouseInfo = mouseManager.GetMouseEvents();
            InputInfoBase inputInfo = null;
            if (IsWindowActive)
            {
                input.GetInput(KeyConfigManager.CurrentConfig.Keys,
                    KeyConfigManager.CurrentConfig.Buttons, out inputInfo);
            }
            if (inputInfo == null)
            {
                inputInfo = EmptyInputInfo.Instance;
            }
            if (ignoreInputCount < 30)
            {
                inputInfo = EmptyInputInfo.Instance;
            }

            if (Form.IsCloseRequired && debugMode && !TextBoxEnabled)
            {
                Exit();
            }

            if (!Form.IsCloseRequired && inputInfo.IsPressed(ButtonType.Home) && !Input.AssignMode && !TextBoxEnabled)
            {
                Form.MainForm.Close();
            }

            if (!Form.IsFirstCloseRequierd && inputInfo.IsPressed(ButtonType.Home))
            {
                CancelExit();
            }

            if (sceneManager != null)
            {
                if (!Form.IsCloseRequired)
                {
                    gaussianFilter.Disperson = 0;
                    sceneManager.Update(inputInfo, mouseInfo, sound);
                }
                else
                {
                    sceneManager.Update(EmptyInputInfo.Instance, MouseInfo.Empty, sound);
                    if (currentOverray != null)
                    {
                        currentOverray.Update(Form.IsFirstCloseRequierd, inputInfo, mouseInfo);
                    }
                    gaussianFilter.Disperson += 1;
                    if (gaussianFilter.Disperson >= 100)
                    {
                        gaussianFilter.Disperson = 100;
                    }
                    Form.IsFirstCloseRequierd = false;
                }
            }

            if (goHome)
            {
                sceneManager.PopToHome();
                goHome = false;
                isHome = true;
                CancelExit();

                if (!debugMode)
                {
                    (sceneManager.CurrentScene as HomeScene).ComeHome();
                }
            }

            notifyControl.Update();
            movieVolumeSprite.Update(mouseInfo);
            timerManager.Update();
            ThreadManager.Instance.Update();
        }

        protected override void Draw()
        {
            RestoreClipping();
            if (sceneManager != null)
            {
                sceneManager.Draw();
                if (Form.IsCloseRequired)
                {
                    if (currentOverray != null)
                    {
                        currentOverray.Draw();
                    }
                }
            }
            notifyControl.Draw();
            movieVolumeSprite.Draw();
        }

        protected override void DrawEnd()
        {
            screenShotManager.Update();
        }

        public void Exit(object sender, EventArgs e)
        {
            DisposeScene(loading);
            DisposeScene(logo);
            DisposeScene(currentOverray);
            DisposeIDisposable(sceneManager);
            DisposeIDisposable(sound);
            DisposeIDisposable(input);
            DisposeIDisposable(homeResourceManager);
        }

        private void DisposeIDisposable(IDisposable dispo)
        {
            if (dispo != null)
            {
                dispo.Dispose();
            }
        }

        private void DisposeScene(ISceneBase scene)
        {
            if (scene != null)
            {
                scene.Dispose();
                if (scene.ResourceManager != null)
                {
                    scene.ResourceManager.Dispose();
                }
            }
        }

        public InputBase Input
        {
            get
            {
                return input;
            }
        }

        public KeyConfigManager KeyConfigManager
        {
            get
            {
                return keyConfigManager;
            }
        }

        public IntPtr WindowHandle
        {
            get
            {
                return Form.MainForm.Handle;
            }
        }

        public void StartGame(GameLoader gl)
        {
            currentGame = gl;
            var entrySceneManager = gl.GetEntrySceneManager();
            sceneManager.LoadingScene = gl.GetLoading(device);
            SetPropertyToScene(sceneManager.LoadingScene);
            var spriteResourceManager = new PPDFramework.Resource.SpriteResourceManager(device, entrySceneManager.SpriteDir);
            sceneManager.LoadingScene.ResourceManager = spriteResourceManager;
            sceneManager.LoadingScene.Load();
            var scene = entrySceneManager.GetSceneWithArgs(device, Args, out Dictionary<string, object> dict);
            sceneManager.ChangeGame(scene, dict, spriteResourceManager);
            isHome = false;
        }

        #region IGameHost メンバ

        public IMovie GetMovie(SongInformation songInformation)
        {
            if (songInformation == null)
            {
                return null;
            }
            CurrentSong = songInformation;
            var filename = songInformation.MoviePath;
            if (PPDSetting.Setting.IsMovie(filename))
            {
                switch (PPDSetting.Setting.MoviePlayType)
                {
                    /*case MoviePlayType.VMR9:
                        CurrentPlayerBase = new VMR9Movie(device, ((PPDFramework.DX9.PPDDevice)device).D3D, filename);
                        break;*/
                    default:
                        CurrentPlayerBase = new SampleGrabberMovie(device, filename);
                        break;
                }
            }
            else
            {
                CurrentPlayerBase = new MusicPlayer(device, filename);
            }
            CurrentPlayerBase.UserVolume = songInformation.UserVolume;
            return CurrentPlayerBase;
        }

        public void Exit()
        {
            this.ShouldBeExit = true;
            Form.CloseAdmitted = true;
        }

        public void CancelExit()
        {
            Form.IsCloseRequired = false;
        }

        public void GoHome()
        {
            goHome = true;
        }

        public bool CanGoHome
        {
            get
            {
                return !isHome && !sceneManager.Loading;
            }
        }

        public bool TextBoxEnabled
        {
            get
            {
                return TextEditableControl.IsTextEditMode;
            }
            set
            {
                TextEditableControl.IsTextEditMode = value;
            }
        }

        public string TextBoxText
        {
            get
            {
                return TextEditableControl.TextBoxText;
            }
            set
            {
                TextEditableControl.TextBoxText = value;
            }
        }

        public Vector2 TextBoxLocation
        {
            get
            {
                return new Vector2((TextEditableControl.TextBoxLocation.X - device.Offset.X) / device.Scale.X,

                    (TextEditableControl.TextBoxLocation.Y - device.Offset.Y) / device.Scale.Y);
            }
            set
            {
                TextEditableControl.TextBoxLocation = new System.Drawing.Point((int)(device.Offset.X + device.Scale.X * value.X), (int)(device.Offset.Y + device.Scale.Y * value.Y));
            }
        }


        public TextBoxSelection TextBoxSelection
        {
            get
            {
                return TextEditableControl.Selection;
            }
            set
            {
                TextEditableControl.Selection = value;
            }
        }

        public int TextBoxCaretIndex
        {
            get
            {
                return TextEditableControl.CaretIndex;
            }
            set
            {
                TextEditableControl.CaretIndex = value;
            }
        }

        public int TextBoxFontSize
        {
            get
            {
                return (int)TextEditableControl.Font.Size;
            }
            set
            {
                if (TextEditableControl.Font.Size != value)
                {
                    TextEditableControl.Font = new Font(TextEditableControl.Font.FontFamily, value * device.Scale.Y);
                }
            }
        }

        public bool TextBoxEnterClosed
        {
            get
            {
                return TextEditableControl.EnterClosed;
            }
        }

        public bool IsCloseRequired
        {
            get
            {
                return Form.IsCloseRequired;
            }
        }

        public bool IsWindowActive
        {
            get
            {
                return SharpDX.Windows.RenderForm.ActiveForm == Form;
            }
        }

        public GameTimer GameTimer
        {
            get
            {
                return gameTimer;
            }
        }

        public int AddTimerCallBack(Action<int> callback, int milliSec, bool onceExecute, bool immediate)
        {
            return timerManager.AddTimerCallBack(callback, milliSec, onceExecute, immediate);
        }

        public void RemoveTimerCallBack(int id)
        {
            timerManager.RemoveTimerCallBack(id);
        }

        public void SetClipping(int x, int y, int width, int height)
        {
            var ratio = new Vector2((float)device.Width / width, (float)device.Height / height);
            var pos = new Vector2(x, y);
            device.SetScissorRect(new SharpDX.Rectangle((int)(x * device.Scale.X + device.Offset.X), (int)(y * device.Scale.Y + device.Offset.Y),
                (int)(width * device.Scale.X), (int)(height * device.Scale.Y)), true);
        }

        public void RestoreClipping()
        {
            device.SetScissorRect(new SharpDX.Rectangle(0, 0, device.Width, device.Height), false);
        }

        public void AddNotify(string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                notifyControl.AddNotify(text);
            }
        }

        public void SaveScreenShot(string filePath, Action<string> savedCallback)
        {
            screenShotManager.Add(filePath, savedCallback);
        }

        public void SendToLoading(Dictionary<string, object> parameters)
        {
            sceneManager.LoadingScene.SendToLoading(parameters);
        }

        #endregion
    }
}