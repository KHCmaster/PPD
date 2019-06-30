using PPDFramework;
using PPDFramework.Scene;
using PPDFramework.Sprites;
using PPDFrameworkCore;
using PPDInput;
using PPDMovie;
using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PPDTest
{
    class MyGame : Game, IGameHost
    {
        PPDFramework.Resource.ResourceManager resourceManager;
        Input input;
        SceneManager sceneManager;
        TestSceneManager testSceneManager;

        protected virtual void OnIMEStarted(EventArgs e)
        {
            IMEStarted?.Invoke(this, e);
        }

        public event EventHandler IMEStarted;

        protected virtual void OnTextBoxEnabledChanged(EventArgs e)
        {
            TextBoxEnabledChanged?.Invoke(this, e);
        }

        public event EventHandler TextBoxEnabledChanged;

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

        public MyGame(PPDExecuteArg args) : base(args)
        {
            System.Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            //ウィンドウの初期設定
            Form.MainForm.ClientSize = new Size(800, 450);
            Form.MainForm.Text = "PPD";
            Form.MainForm.MaximizeBox = false;
            Form.MainForm.MinimizeBox = false;
            Form.MainForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Form.MainForm.FormClosing += this.Exit;
        }

        protected override void Initialize()
        {
            input = new Input(Form.MainForm);
            input.Load();

            var dirs = new[] { @"img\PPD\home", @"img\PPD\main_game", @"img\PPD\single", @"img\PPD\multi", @"img\PPDEditor" };
            foreach (var dir in dirs)
            {
                var spriteManager = new DirSpriteManager(dir);
                spriteManager.Pack();
            }
            resourceManager = new PPDFramework.Resource.SpriteResourceManager(device, @"img\PPD\main_game");
            sceneManager = new SceneManager(device)
            {
                GameHost = this
            };
            testSceneManager = new TestSceneManager(sceneManager, device);
            testSceneManager.Add(typeof(TextScene));
            testSceneManager.Add(typeof(ScissorScene));
            testSceneManager.Add(typeof(ImageScene));
            testSceneManager.Add(typeof(FilterScene));
            testSceneManager.Add(typeof(NumberImageScene));
            testSceneManager.Add(typeof(ScreenFilterScene));
            testSceneManager.Add(typeof(MaskScene));
            testSceneManager.Add(typeof(MovieScene));
            var scene = testSceneManager.Initialize();
            SetPropertyToScene(scene);
            scene.Load();
            sceneManager.CurrentScene = scene;
            TextEditableControl.IMEStarted += control_IMEStarted;
        }

        private void SetPropertyToScene(ISceneBase scene)
        {
            if (scene == null) return;
            scene.ResourceManager = resourceManager;
            scene.SceneManager = sceneManager;
            scene.GameHost = this;
        }

        void control_IMEStarted(object sender, EventArgs e)
        {
        }

        private void FinishApplication(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Update()
        {
            InputInfoBase inputInfo = null;
            if (IsWindowActive)
            {
                input.GetInput(new Key[]
                {
                Key.H,
                Key.J,
                Key.K,
                Key.U,
                Key.A,
                Key.S,
                Key.D,
                Key.W,
                Key.Y,
                Key.E,
                Key.Escape,
                Key.Space
                }, new int[] {
                0,1,2,3,28024,19024,10024,1024,5,6,9,12
                }, out inputInfo);
            }
            if (inputInfo == null)
            {
                inputInfo = EmptyInputInfo.Instance;
            }
            sceneManager.Update(inputInfo, MouseInfo.Empty, null);
        }

        protected override void Draw()
        {
            sceneManager.Draw();
        }

        public void Exit(object sender, EventArgs e)
        {
        }

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
                CurrentPlayerBase = new SampleGrabberMovie(device, filename);
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
            throw new NotImplementedException();
        }

        public void CancelExit()
        {
            throw new NotImplementedException();
        }

        public void GoHome()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void SaveScreenShot(string filePath, Action<string> savedCallback)
        {
            throw new NotImplementedException();
        }

        public void SendToLoading(Dictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public int AddTimerCallBack(Action<int> callback, int milliSec, bool onceExecute, bool immediate)
        {
            throw new NotImplementedException();
        }

        public void RemoveTimerCallBack(int id)
        {
            throw new NotImplementedException();
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
                var p = new PointF(TextEditableControl.TextBoxLocation.X - device.Offset.X, TextEditableControl.TextBoxLocation.Y - device.Offset.Y);
                return new Vector2(p.X / device.Scale.X, p.Y / device.Scale.Y);
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
                    TextEditableControl.Font = new System.Drawing.Font(TextEditableControl.Font.FontFamily, value);
                }
            }
        }

        public bool TextBoxEnterClosed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsCloseRequired
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool CanGoHome
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsWindowActive
        {
            get
            {
                return System.Windows.Forms.Form.ActiveForm == Form;
            }
        }

        public IntPtr WindowHandle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public GameTimer GameTimer
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}