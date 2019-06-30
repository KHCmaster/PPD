using PPDFramework;
using PPDFramework.Sprites;
using PPDInput;
using PPDSound;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PPDEditor
{
    public class MyGame : Game
    {
        Input input;
        Sound sound;
        MainGame mg;
        bool recording;
        bool playing;
        PPDFramework.Resource.ResourceManager resourceManager;
        KeyConfigManager keyConfigManager;
        SoundManager sm;
        Vector2 mousepos = new Vector2(-1, -1);
        float[] eval = { 0.05f, 0.1f, 0.15f, 0.2f };
        Vector2[] circlepoints;

        public IMovie Movie
        {
            get { return mg?.Movie; }
        }

        public bool Recording
        {
            get { return recording; }
        }

        public bool Playing
        {
            get { return playing; }
        }

        public MyGame(PPDExecuteArg args) : base(args)
        {
            this.Window.FormClosing += Window_FormClosing;
        }

        private void Window_FormClosing(object sender, FormClosingEventArgs e)
        {
            ShouldBeExit = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Window.MyGame = this;
            //Inputクラスの生成
            input = new Input(this.Window);
            input.Load();
            //Soundクラスの生成
            sound = new Sound(this.Window);
            sound.Initialize();
            keyConfigManager = new KeyConfigManager();
            keyConfigManager.Load("keyconfig.ini");

            var dirs = new[] { @"img\PPDEditor" };
            foreach (var dir in dirs)
            {
                var spriteManager = new DirSpriteManager(dir);
                spriteManager.Pack();
            }
            resourceManager = new PPDFramework.Resource.SpriteResourceManager(device, @"img\PPDEditor");
            CreateResource();
            sm = this.Window.SoundManager;
            sm.setSound(this.sound);
            this.Window.ResourceManager.SetSound(sound);
            mg = new MainGame(device, this, resourceManager, Window.Grid);
            mg.Dummy.Drawing += Dummy_Drawing;
            Utility.Device = device;
            Utility.ResourceManager = resourceManager;
            Utility.Eval = eval;
            Utility.CirclePoints = circlepoints;
        }

        private void Dummy_Drawing()
        {
            this.Window.DrawMark();
        }

        private void CreateResource()
        {
            var pathes = new List<string>(30);
            var checktracelist = new List<string>(10);
            for (ButtonType type = ButtonType.Square; type < ButtonType.Start; type++)
            {
                pathes.Add(PPDEditorSkin.Skin.GetMarkImagePath(type));
                pathes.Add(PPDEditorSkin.Skin.GetMarkColorImagePath(type));
                string tracepath = PPDEditorSkin.Skin.GetTraceImagePath(type);
                pathes.Add(tracepath);
                if (!checktracelist.Contains(tracepath))
                {
                    checktracelist.Add(tracepath);
                }
            }
            pathes.Add(PPDEditorSkin.Skin.GetCircleAxisImagePath());
            pathes.Add(PPDEditorSkin.Skin.GetClockAxisImagePath());
            PPDEditorSkin.Skin.GetLongNoteCircleInfo(out PathObject fn, out float inner, out float outer);
            pathes.Add(fn);
            CreateCirclePoints(inner, outer);
            PPDEditorSkin.Skin.GetHoldInfo(out fn, out inner, out outer);
            pathes.Add(fn);
        }
        private void CreateCirclePoints(float smallrad, float bigrad)
        {
            int num = 361;
            circlepoints = new Vector2[num * 2];
            for (int i = 0; i < num; i++)
            {
                circlepoints[i * 2].X = (float)(bigrad * (Math.Cos(Math.PI * i / (num - 1) * 2 - Math.PI / 2)));
                circlepoints[i * 2].Y = (float)(bigrad * (Math.Sin(Math.PI * i / (num - 1) * 2 - Math.PI / 2)));
                circlepoints[i * 2 + 1].X = (float)(smallrad * (Math.Cos(Math.PI * i / (num - 1) * 2 - Math.PI / 2)));
                circlepoints[i * 2 + 1].Y = (float)(smallrad * (Math.Sin(Math.PI * i / (num - 1) * 2 - Math.PI / 2)));
            }
        }
        public void ChangeMousepos(Point p)
        {
            mousepos = new Vector2(p.X, p.Y);
        }
        public void StartRecord()
        {
            recording = true;
        }
        public void StopRecord()
        {
            if (recording)
            {
                this.Window.Refleshdata();
            }
            recording = false;
        }
        public void StartPlay()
        {
            playing = true;
        }
        public void StopPlay()
        {
            playing = false;
        }

        void m_Finished(object sender, EventArgs e)
        {
            if (playing)
            {
                playing = false;
            }
            if (recording)
            {
                recording = false;
            }
        }

        public void ChangeData(Vector2[] positions, float[] angles)
        {
            mg.MarkPointDrawer.ChangeData(positions, angles);
        }

        protected override void Update()
        {
            if (recording || playing)
            {
                this.Window.UpdateStatus();
            }
            if (recording)
            {
                var shouldbecheck = input.GetInput(keyConfigManager.CurrentConfig.Keys,
                    keyConfigManager.CurrentConfig.Buttons, out InputInfoBase inputInfo);
                if (inputInfo == null)
                {
                    inputInfo = EmptyInputInfo.Instance;
                }
                if (shouldbecheck)
                {
                    double movietime = Window.CurrentTime;
                    for (int i = 0; i < keyConfigManager.CurrentConfig.Buttons.Length; i++)
                    {
                        if (inputInfo.IsPressed((ButtonType)i))
                        {
                            this.Window.Record(movietime, i);
                        }
                    }
                }
            }
            if (!recording)
            {
                var ret = this.Window.Updatemark(this.Window.CurrentTime);
                if (playing)
                {
                    sm.playsound(ret);
                }
            }
            mg.UpdateAssist(this.Window.MarkSelectMode, this.Window.OnMouseMarkSelectMode);
            mg.Grid.Hidden = !this.Window.DisplayGrid;
            mg.Kasi.Text = this.Window.CurrentKasi;
            mg.MarkPointDrawer.DrawAngle = this.Window.DrawAngle;
            mg.MarkPointDrawer.Hidden = !this.Window.DrawPoint;
            if (mg != null && !mg.Disposed)
            {
                mg.Update();
            }
        }

        protected override void Draw()
        {
            mg.Draw();
        }

        public bool InitializeMovie(string fileName)
        {
            try
            {
                mg.InitializeMovie(fileName);
                mg.Movie.Finished += m_Finished;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (mg != null && !mg.Disposed)
            {
                mg.Dispose();
                mg = null;
            }
            if (sound != null)
            {
                sound.Dispose();
                sound = null;
            }
            if (input != null)
            {
                input.Dispose();
                input = null;
            }
        }
    }
}