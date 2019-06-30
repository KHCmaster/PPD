using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using SlimDX.DirectInput;
using System.Windows.Forms;
using PPDFramework;

namespace testgame
{
    class Menu : CMenu
    {
        enum State
        {
            moving = 0,
            still = 1
        }
        State topstate = State.still;
        State bottomstate = State.still;
        State backstate = State.still;
        int count = 0;
        float speedscale = 1f;
        bool auto;
        bool moviechanged = true;
        Dictionary<string, ImageResource> resource;
        ArrayList pictureobjects;
        PictureObject background;
        MovieBox mb;
        MenuSelectSong mss;
        SongInfoControl sic;
        ConfirmControl cc;
        Moviethumb mt;
        StringObject modetext;
        StringObject autotext;
        StringObject speedtext;
        StringObject randomtext;
        EffectObject wave;
        SelectSongManager ssm;
        bool random;
        bool allowcommand = true;
        string[] soundfilenames = new string[] { "sounds\\cursor1.wav", "sounds\\cursor2.wav", "sounds\\cursor3.wav", "sounds\\cursor4.wav" };
        public Menu()
        {
        }
        public override void Load()
        {
            //init
            int selectnum = 0;
            string initialdirectory = "songs";
            object temp;
            if (PreviousParam.TryGetValue("PPDGameUtility", out temp))
            {
                PPDGameUtility gameutility = temp as PPDGameUtility;
                initialdirectory = gameutility.SongInformation.ParentDirectory;// Directory.GetParent(gameutility.SongInformation.DirectoryPath).Name;
            }
            ssm = new SelectSongManager();
            ssm.SongChanged += new EventHandler(ssm_SongChanged);
            ssm.DirectoryChanged += new EventHandler(ssm_DirectoryChanged);
            resource = new Dictionary<string, ImageResource>();
            ImageResource ir = new ImageResource("img\\default\\num.png", Device);
            resource.Add("img\\default\\num.png", ir);
            pictureobjects = new ArrayList();
            mb = new MovieBox(Device);
            mss = new MenuSelectSong(Device, Sprite, this, ssm.Directory);
            sic = new SongInfoControl(Device, Sprite);
            cc = new ConfirmControl(Device, Sprite);
            if (PPDSetting.Setting.LightMode)
            {
                mt = new Moviethumb(Device, Sprite);
            }
            mss.DisapperFinish += new EventHandler(cc.Focus);
            //factory

            factory(selectnum);
            if (temp != null)
            {
                PPDGameUtility gameutility = temp as PPDGameUtility;
                Auto = gameutility.Auto;
                Speedscale = gameutility.SpeedScale;
                Random = gameutility.Random;
                changeprofiledisplay();
            }
            ssm.Directory = initialdirectory;
            if (temp != null)
            {
                PPDGameUtility gameutility = temp as PPDGameUtility;
                SongInformation si = gameutility.SongInformation;
                selectnum = Array.FindIndex(ssm.SongInformations, (songinfo) => si.DirectoryName == songinfo.DirectoryName);
                if (selectnum < 0) selectnum = 0;
            }
            ssm.SelectedIndex = selectnum;
        }

        void ssm_DirectoryChanged(object sender, EventArgs e)
        {
            mss.SongInformations = ssm.SongInformations;
            if (mt != null)
            {
                mt.ChangeDirectory(ssm.SongInformations, ssm.SelectedIndex);
            }
        }

        void ssm_SongChanged(object sender, EventArgs e)
        {
            mss.SelectedIndex = ssm.SelectedIndex;
            if (!ssm.SelectedSongInformation.IsPPDSong)
            {
                mb.FadeOut();
                mb.CheckLoopAvailable = false;
                sic.HideInfo();
            }
            else
            {
                mb.FadeOut();
                mb.CheckLoopAvailable = true;
                count = 0;
                moviechanged = true;
                //sic.ChangeSongInfo(ssm.SelectedSongInformation);
            }
            sic.ChangeSongInfo(ssm.SelectedSongInformation);
        }
        public bool Auto
        {
            get
            {
                return auto;
            }
            set
            {
                auto = value;
                changeautomodedisplay();
            }
        }
        public Difficulty Difficulty
        {
            get
            {
                return sic.Difficulty;
            }
        }
        public string DifficultString
        {
            get
            {
                return sic.DiifficultyString;
            }
        }
        public float Speedscale
        {
            get
            {
                return speedscale;
            }
            set
            {
                speedscale = value;
                changesppedscaledisplay();
            }
        }
        public bool Random
        {
            get
            {
                return random;
            }
            set
            {
                random = value;
                changerandomdisplay();
            }
        }
        private void factory(int selectnum)
        {
            ImageResource back = new ImageResource("img\\default\\back.png", Device);
            resource.Add("img\\default\\back.png", back);
            background = new PictureObject("img\\default\\back.png", 0, 0, this.resource, Device);
            ImageResource p = new ImageResource("img\\default\\top.png", Device);
            resource.Add("img\\default\\top.png", p);
            PictureObject po = new PictureObject("img\\default\\top.png", 0, 0, resource, Device);
            pictureobjects.Add(po);
            p = new ImageResource("img\\default\\bottom.png", Device);
            resource.Add("img\\default\\bottom.png", p);
            po = new PictureObject("img\\default\\bottom.png", 0, 450 - p.Height, resource, Device);
            pictureobjects.Add(po);
            wave = new EffectObject("img\\default\\wave.etd", 340, 400, resource, Device);
            wave.PlayType = Effect2D.EffectManager.PlayType.Loop;
            wave.Play();
            modetext = new StringObject("", 20, 425, 20, new Color4(1, 1, 1, 1), Device, Sprite);
            autotext = new StringObject("", 670, 5, 15, new Color4(1, 1, 1, 1), Device, Sprite);
            speedtext = new StringObject("", 200, 5, 30, new Color4(1, 1, 1, 1), Device, Sprite);
            randomtext = new StringObject("", 670, 20, 15, new Color4(1, 1, 1, 1), Device, Sprite);
        }
        private void ChangeMovie()
        {
            //mb.releaseCOM();
            if (ssm.SelectedSongInformation == null || !ssm.SelectedSongInformation.IsPPDSong) return;
            if (ssm.SelectedSongInformation.MoviePath != null && ssm.SelectedSongInformation.MoviePath != "")
            {
                float rot = mb.Rotation;
                //mb.releaseCOM();
                mb.Dispose();
                mb = null;
                mb = new MovieBox(Device);
                mb.Movie = GameHost.GetMovie(ssm.SelectedSongInformation.MoviePath);
                mb.Rotation = rot;
                mb.FileName = ssm.SelectedSongInformation.MoviePath;
                if (mb.Initialize() != 0)
                {
                    MessageBox.Show("動画を開けませんでした\nCould not open movie");
                }
                else
                {
                    mb.TrimmingData = ssm.SelectedSongInformation.TrimmingData;
                    mb.SetLoop(ssm.SelectedSongInformation.ThumbStartTime, ssm.SelectedSongInformation.ThumbEndTime);
                    mb.ChangeCut();
                    mb.Play();
                    mb.FadeIn();
                }
            }
            else
            {
                Exception e = new Exception("movie.*の動画ファイルがありません(@" + ssm.SelectedSongInformation.DirectoryPath + ")");
                throw e;
            }
        }
        private void setvisible()
        {
            topstate = State.moving;
            bottomstate = State.moving;
            backstate = State.moving;
            mb.Hidden = false;
            mss.Hidden = false;
            sic.Hidden = false;
            cc.Hidden = false;
            allowcommand = true;
        }


        public override void Update(int[] presscount, bool[] released)
        {
            count++;
            if (count > 60 && moviechanged)
            {
                count = 0;
                if (moviechanged)
                {
                    ChangeMovie();
                    moviechanged = false;
                }
            }
            foreach (PictureObject po in pictureobjects)
            {
                po.Update();
            }
            if (topstate == State.moving)
            {
                PictureObject po = pictureobjects[0] as PictureObject;
                float y = po.Position.Y + 1;
                if (y >= 0)
                {
                    po.Position = new Vector2(0, 0);
                    topstate = State.still;
                }
                else
                {
                    po.Position = new Vector2(0, y);
                }
            }
            if (bottomstate == State.moving)
            {
                PictureObject po = pictureobjects[1] as PictureObject;
                float y = po.Position.Y - 1;
                if (y <= 441)
                {
                    po.Position = new Vector2(0, 411);
                    bottomstate = State.still;
                }
                else
                {
                    po.Position = new Vector2(0, y);
                }
            }
            if (backstate == State.moving)
            {
                background.Alpha += 0.05f;
                if (background.Alpha >= 1)
                {
                    background.Alpha = 1f;
                    backstate = State.still;
                }

            }
            bool[] b = new bool[presscount.Length];
            for (int k = 0; k < b.Length; k++)
            {
                if ((presscount[k] == 1))
                {
                    b[k] = true;
                }
                if (k >= 4 && k <= 7)
                {
                    if (presscount[k] % 10 == 9 && presscount[k] > 20)
                    {
                        b[k] = true;
                    }
                }
            }
            if (allowcommand)
            {
                if (b[(int)ButtonType.Circle])
                {
                    if (mss.Focused)
                    {
                        if (ssm.CanGoDown)
                        {
                            ssm.GoDownDirectory();
                        }
                        else
                        {
                            mss.Focused = false;
                            sic.Focus();
                        }
                        Sound.Play(soundfilenames[1], -1000);
                    }
                    else if (sic.Focused && sic.CanGoNext())
                    {
                        sic.Focused = false;
                        sic.FadeOut();
                        mss.Disappear();
                        cc.SetInfo(ssm.SelectedSongInformation.DirectoryName, sic.Difficult);
                        Sound.Play(soundfilenames[1], -1000);
                    }
                    else if (!cc.Appeared)
                    {
                        cc.Show();
                    }
                    else if (cc.Focused)
                    {
                        allowcommand = false;
                        cc.Next();
                        mb.FadeOutFinished += new EventHandler(this.End);
                        mb.FadeOut();
                        Sound.Play(soundfilenames[1], -1000);
                    }
                }
                if (b[(int)ButtonType.Cross])
                {
                    //cross
                    if (mss.Focused)
                    {
                        if (ssm.CanGoUp) ssm.GoUpDirectory();
                    }
                    if (sic.Focused)
                    {
                        sic.UnFocus();
                        mss.Focused = true;
                        Sound.Play(soundfilenames[2], -1000);
                    }
                    if (cc.Focused)
                    {
                        mss.Focused = true;
                        mss.Start();
                        cc.Vanish();
                        sic.UnFocus();
                        Sound.Play(soundfilenames[2], -1000);
                    }
                }
                if (b[(int)ButtonType.Left])
                {//left
                    if (mss.Focused)
                    {
                        int gain = ssm.SelectedIndex - 3 < 0 ? -ssm.SelectedIndex : -3;
                        if (gain != 0)
                        {
                            ssm.SeekSong(gain);
                            if (mt != null)
                            {
                                mt.ChangeSelectIndex(gain);
                            }
                            Sound.Play(soundfilenames[0], -1000);
                        }
                    }
                    if (sic.Focused)
                    {
                        sic.ChangeDifficulty(-1);
                        Sound.Play(soundfilenames[0], -1000);
                    }
                }
                if (b[(int)ButtonType.Right])
                {//right
                    if (mss.Focused)
                    {
                        int gain = ssm.SelectedIndex + 3 < ssm.Count ? 3 : ssm.Count - ssm.SelectedIndex - 1;
                        if (gain != 0)
                        {
                            ssm.SeekSong(gain);
                            if (mt != null)
                            {
                                mt.ChangeSelectIndex(gain);
                            }
                            Sound.Play(soundfilenames[0], -1000);
                        }
                    }
                    if (sic.Focused)
                    {
                        sic.ChangeDifficulty(1);
                        Sound.Play(soundfilenames[0], -1000);
                    }
                }
                if (b[(int)ButtonType.Triangle])
                {
                    if (mss.Focused)
                    {
                        ProfileManager.Instance.Next();
                        Sound.Play(soundfilenames[3], -1000);
                        changeprofiledisplay();
                    }
                }
                if (b[(int)ButtonType.Square])
                {
                    if (mss.Focused)
                    {
                        if (!auto && !random)
                        {
                            auto = true;
                        }
                        else if (auto && !random)
                        {
                            auto = false;
                            random = true;
                        }
                        else if (!auto && random)
                        {
                            auto = true;
                        }
                        else
                        {
                            auto = false;
                            random = false;
                        }
                        changeautomodedisplay();
                        changerandomdisplay();
                        Sound.Play(soundfilenames[3], -1000);
                    }
                }

                if (b[(int)ButtonType.Up])
                {//up
                    if (mss.Focused)
                    {
                        ssm.PreviousSong();
                        if (mt != null)
                        {
                            mt.ChangeSelectIndex(-1);
                        }
                        Sound.Play(soundfilenames[0], -1000);
                    }
                }
                if (b[(int)ButtonType.Down])
                {//down
                    if (mss.Focused)
                    {
                        ssm.NextSong();
                        if (mt != null)
                        {
                            mt.ChangeSelectIndex(1);
                        }
                        Sound.Play(soundfilenames[0], -1000);
                    }
                }
                if (b[(int)ButtonType.R])
                {
                    //R
                    if (mss.Focused)
                    {
                        speedscale += 0.25f;
                        if (speedscale > 2.0f)
                        {
                            speedscale = 0.5f;
                        }
                        Sound.Play(soundfilenames[3], -1000);
                        changesppedscaledisplay();
                    }
                }
                if (b[(int)ButtonType.L])
                {
                    //L
                    if (mss.Focused)
                    {
                        speedscale -= 0.25f;
                        if (speedscale < 0.5f)
                        {
                            speedscale = 2.0f;
                        }
                        Sound.Play(soundfilenames[3], -1000);
                        changesppedscaledisplay();
                    }
                }
            }
            if (wave != null) wave.Update();
            if (mb.Initialized) mb.Update();
            if (sic != null) sic.Update();
            if (cc != null) cc.Update();
            if (mss != null) mss.Update();
            if (mt != null) mt.Update();
        }
        private void changeprofiledisplay()
        {
            modetext.Text = ProfileManager.Instance.Current.DisplayText;
        }
        private void changeautomodedisplay()
        {
            if (auto)
            {
                autotext.Text = "オートモード ON";
            }
            else
            {
                autotext.Text = "";
            }
        }
        private void changesppedscaledisplay()
        {
            if (speedscale != 1)
            {
                speedtext.Text = "スピード X" + speedscale.ToString();
            }
            else
            {
                speedtext.Text = "";
            }
        }
        private void changerandomdisplay()
        {
            if (random)
            {
                randomtext.Text = "ランダム　オン";
            }
            else
            {
                randomtext.Text = "";
            }
        }
        public override void Draw()
        {
            background.Draw();
            if (!PPDSetting.Setting.LightMode)
            {
                wave.Draw();
            }
            if (mb != null) mb.Draw();
            if (mt != null) mt.Draw();
            if (sic != null) sic.Draw();
            if (cc != null) cc.Draw();
            if (mss != null) mss.Draw();
            foreach (PictureObject po in pictureobjects)
            {
                po.Draw();
            }
            if (!cc.Focused)
            {
                modetext.Draw();
                autotext.Draw();
                speedtext.Draw();
                randomtext.Draw();
            }
        }
        public void End(object sender, EventArgs e)
        {
            PPDGameUtility gameutility = new PPDGameUtility();
            gameutility.SongInformation = ssm.SelectedSongInformation;
            gameutility.Difficulty = Difficulty;
            gameutility.DifficultString = DifficultString;
            gameutility.Profile = ProfileManager.Instance.Current;
            gameutility.Auto = Auto;
            gameutility.SpeedScale = Speedscale;
            gameutility.Random = random;
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("PPDGameUtility", gameutility);
            SceneManager.PrepareNextScene(this, GameHost.GetMainGame(), dic, dic);
        }
        protected override void DisposeResource()
        {
            mb.Stop();
            mb.Dispose();
            mss.Dispose();
            sic.Dispose();
            cc.Dispose();
            if (mt != null)
            {
                mt.Dispose();
            }
            background.Dispose();
            foreach (ImageResource p in resource.Values)
            {
                p.Dispose();
            }
        }
    }
}
