using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D9;
using SlimDX.DirectInput;
using System.Runtime.InteropServices;
using PPDFramework;
using PPDPack;
using PPDFramework.Scene;

namespace PPD
{
    class Maingame : CSceneBase, IDisposable, ITweetManager
    {
        const float defaultbpm = 130;

        public event EventHandler fadeoutfinish;
        public event BoolEventHandler TweetFinished;

        enum State
        {
            playing = 0,
            stop = 1,
            result = 2
        }
        bool allowinput = true;
        bool godmode = false;
        bool waitingmoviestart = false;
        State state = State.playing;

        public bool initialized;
        IMovie m;
        KasiManager km;
        SoundManager sm;
        EventManager em;
        MarkManager mm;
        PPDEffectManager ppdem;
        CGameResult gr;
        CPauseMenu pd;
        GameResultManager grm;
        CGameInterface cgi;
        HoldManager hm;
        int[] evapoint = new int[5];
        PictureObject black;
        long lasttime;
        float templasttime;
        PPDGameUtility ppdgameutility;
        float startTime;
        float mmStartTime;
        float endTime;
        int[] keychange;
        bool mistake = false;
        bool exitonreturn = false;
        float PlatformGap = 0;

        bool Tweeted = false;
        bool TweetFinish = false;
        bool TweetSuccess = false;
        const string TweetHashTag = "#PPDXXX";
        float currentLatency = 0;
        int latencyCount = 0;
        public Maingame()
        {
        }
        public override void Load()
        {
            if (!Param.ContainsKey("PPDGameUtility"))
            {
                throw new PPDException(PPDExceptionType.SkinIsNotCorrectlyImplemented);
            }
            ppdgameutility = Param["PPDGameUtility"] as PPDGameUtility;
            currentLatency = ppdgameutility.SongInformation.Latency;
            if (Param.ContainsKey("ExitOnReturn")) exitonreturn = true;
            if (Param.ContainsKey("StartTime"))
            {
                ppdgameutility.IsDebug = true;
                startTime = (float)Param["StartTime"] - 3;
                mmStartTime = (float)Param["StartTime"];
            }
            else if (Param.ContainsKey("StartTimeEx"))
            {
                ppdgameutility.IsDebug = true;
                startTime = (float)Param["StartTimeEx"];
                mmStartTime = (float)Param["StartTimeEx"];
            }
            else
            {
                startTime = ppdgameutility.SongInformation.StartTime;
                mmStartTime = startTime;
            }

            black = new PictureObject("img\\default\\black.png", 0, 0, false, ResourceManager, Device);
            black.Alpha = 0;
            black.Hidden = true;

            string moviefile = ppdgameutility.SongInformation.MoviePath;

#if x64
            if (Path.GetExtension(moviefile) == ".mp4") PlatformGap = 0.03f;
#endif
            if (moviefile != "")
            {
                m = GameHost.GetMovie(moviefile);
                try
                {
                    if (m.Initialize() == -1)
                    {
                        throw new PPDException(PPDExceptionType.CannotOpenMovie);
                    }
                    readmoviesetting();
                    initialized = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    initialized = false;
                    ReturnToMenu();
                    return;
                }
            }
            else
            {
                MessageBox.Show("No moviefile");
                black.Hidden = false;
                if (exitonreturn) Application.Exit();
                else
                {
                    ReturnToMenu();
                    return;
                }
            }
            keychange = new int[(int)ButtonType.Start];
            for (int i = 0; i < keychange.Length; i++) keychange[i] = i;
            if (ppdgameutility.Random)
            {
                randomchange();
            }
            readprofile();
            hm = new HoldManager(Device, ResourceManager);
            hm.ScoreGained += new HoldManager.GainScoreEventHandler(hm_ScoreGained);
            ppdem = new PPDEffectManager(Device, ResourceManager);
            km = new KasiManager(ppdgameutility);
            km.KasiChanged += new KasiManager.KasiChangeEventHandler(km_KasiChanged);
            cgi = GameHost.GetCGameInterface();
            cgi.PPDGameUtility = ppdgameutility;
            cgi.ResourceManager = ResourceManager;
            cgi.Load();
            sm = new SoundManager(Sound as ExSound, ppdgameutility);
            if (ppdgameutility.SongInformation.IsOld)
            {
                em = new EventManager(ppdgameutility);
                mm = new MarkManager(Device, em, ppdgameutility, keychange, ppdem, GameHost.GetMarkImagePath(), ResourceManager);
            }
            else
            {
                string path = Path.Combine(ppdgameutility.SongInformation.DirectoryPath, DifficultyUtility.ConvertDifficulty(ppdgameutility.Difficulty) + ".ppd");
                PackReader reader = new PackReader(path);
                PPDPackStreamReader ppdpsr = reader.Read("evd");
                em = new EventManager(ppdgameutility, ppdpsr);
                ppdpsr.Close();
                ppdpsr = reader.Read("ppd");
                mm = new MarkManager(Device, em, ppdgameutility, keychange, ppdem, GameHost.GetMarkImagePath(), ppdpsr, ResourceManager);
                ppdpsr.Close();
                reader.Close();
            }
            grm = new GameResultManager();
            gr = GameHost.GetGameResult();
            gr.PPDGameUtility = ppdgameutility;
            gr.ResourceManager = ResourceManager;
            gr.TweetManager = this;
            gr.Load();
            pd = GameHost.GetPauseMenu();
            pd.PPDGameUtility = ppdgameutility;
            pd.ResourceManager = ResourceManager;
            pd.Load();
            pd.Resumed += new EventHandler(pd_Resumed);
            pd.Retryed += new EventHandler(pd_Retryed);
            pd.Returned += new EventHandler(pd_Returned);
            pd.LatencyChanged += new ChangeLatencyEventHandler(pd_LatencyChanged);
            gr.Retryed += new EventHandler(this.Retry);
            gr.Returned += new EventHandler(this.Return);
            mm.ChangeCombo += new MarkManager.ChangeComboHandler(mm_ChangeCombo);
            mm.PlaySound += new MarkManager.SpecialSoundHandler(this.SpecialPlaySound);
            mm.StopSound += new MarkManager.SpecialSoundHandler(this.SpecialStopSound);
            mm.EvaluateCount += new MarkManager.EvaluateCountHandler(this.EvaluateCount);
            mm.PressingButton += new MarkManager.PressingButtonHandler(mm_PressingButton);
            em.ChangeMovieVolume += new EventManager.VolumeHandler(ChangeMovieVolume);
            //seek
            mm.Seek(mmStartTime);
            em.Seek(startTime > 0 ? startTime : 0);
            sm.Seek(startTime);
            km.Seek(startTime);
            if (m != null && m.Initialized)
            {
                m.Play();
                m.Pause();
                m.Seek(startTime);
            }
            if (startTime < 0)
            {
                waitingmoviestart = true;
                lasttime = Win32API.timeGetTime();
            }
            else
            {
                if (m != null && m.Initialized)
                {
                    m.Play();
                }
            }

            this.AddChild(black);
        }

        float pd_LatencyChanged(int sign)
        {
            latencyCount += Math.Sign(sign);
            currentLatency = ppdgameutility.SongInformation.Latency + 0.01f * latencyCount;
            return currentLatency;
        }

        void pd_Returned(object sender, EventArgs e)
        {
            this.ReturnToMenu();
            pd.Hidden = true;
        }

        void pd_Retryed(object sender, EventArgs e)
        {
            this.fadeoutfinish += new EventHandler(this.Retry);
            m.FadeOut();
            black.Hidden = false;
            pd.Hidden = true;
        }

        void pd_Resumed(object sender, EventArgs e)
        {
            state = State.playing;
            m.Play();
            hm.IsPaused = false;
            if (waitingmoviestart)
            {
                long temptime = Win32API.timeGetTime();
                lasttime = (long)((-templasttime + startTime) * 1000 + temptime);
            }
        }
        private void randomchange()
        {
            System.Random r = new Random();
            for (int i = 0; i < 100; i++)
            {
                int a = r.Next(0, 4), b = r.Next(0, 4);
                Swap(keychange, a, b);
                a = r.Next(4, 8);
                b = r.Next(4, 8);
                Swap(keychange, a, b);
            }
            if (r.Next(0, 2) % 2 == 0)
            {
                Swap(keychange, 8, 9);
            }
            if (r.Next(0, 2) % 2 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    Swap(keychange, i, i + 4);
                }
            }
        }
        private int[] Swap(int[] array, int a, int b)
        {
            int swap = array[a];
            array[a] = array[b];
            array[b] = swap;
            return array;
        }
        private void readprofile()
        {
            evapoint[0] = ppdgameutility.Profile.CoolPoint;
            evapoint[1] = ppdgameutility.Profile.GoodPoint;
            evapoint[2] = ppdgameutility.Profile.SafePoint;
            evapoint[3] = ppdgameutility.Profile.SadPoint;
            evapoint[4] = ppdgameutility.Profile.WorstPoint;
            godmode = ppdgameutility.Profile.GodMode;
        }
        private void readmoviesetting()
        {
            endTime = ppdgameutility.SongInformation.EndTime;
            if (endTime >= m.Length)
            {
                endTime = (float)m.Length;
            }
            m.Seek(startTime);
            m.TrimmingData = ppdgameutility.SongInformation.TrimmingData;
            m.Finished += new EventHandler(m_Finished);
        }

        void m_Finished(object sender, EventArgs e)
        {
            if (black.Hidden)
            {
                this.fadeoutfinish += new EventHandler(this.setresult);
                m.FadeOut();
                black.Hidden = false;
            }
        }
        private string readdata(string s, string meta)
        {
            int a = s.IndexOf("[" + meta + "]");
            if (a == -1)
            {
                return "";
            }
            else
            {
                int b = s.IndexOf('\n', a + 1);
                if (b == -1)
                {
                    return s.Substring(a + meta.Length + 2);
                }
                else
                {
                    return s.Substring(a + meta.Length + 2, b - a - meta.Length - 2);
                }
            }
        }
        private void ReturnToMenu()
        {
            if (exitonreturn)
            {
                GameHost.Exit();
            }
            else
            {
                if (!ppdgameutility.IsDebug && latencyCount != 0)
                {
                    ppdgameutility.SongInformation.UpdateLatency(currentLatency);
                }
                if (IsInSceneStack)
                {
                    SceneManager.PopCurrentScene();
                }
                else
                {
                    SceneManager.PrepareNextScene(this, GameHost.GetMenu(), null, null);
                }
            }
        }
        public void setresult(object sender, EventArgs e)
        {
            m.Stop();
            TweetFinish = false;
            TweetSuccess = false;
            gr.SetResult(grm.CurrentScore, grm.MaxCombo, grm.Evacount, mistake ? ResultEvaluateType.Mistake : ResultEvaluateType.Great, (float)m.MoviePosition, ppdgameutility);
            state = State.result;
            if (CanTweet)
            {
                TweetText = CreateTweetText(grm.CurrentScore, gr.Result, grm.Evacount, grm.MaxCombo);
            }
            else
            {
                TweetText = "";
            }
            allowinput = true;
            this.fadeoutfinish -= new EventHandler(this.setresult);
        }

        public bool CanTweet
        {
            get
            {
                return TwitterManager.Manager.IsAvailable && ppdgameutility.IsRegular && !mistake && !Tweeted;
            }
        }

        public string TweetText
        {
            get;
            private set;
        }

        private string CreateTweetText(int score, ResultEvaluateType result, int[] markevals, int maxcombo)
        {
            string text = string.Format("譜面:{0} 難易度:{1}で、スコア:{2} 評価:{3} C{4} G{5} SF{6} SD{7} W{8} MC{9} を記録しました。{10} ",
                ppdgameutility.SongInformation.DirectoryName,
                ppdgameutility.Difficulty,
                score,
                result,
                markevals[0],
                markevals[1],
                markevals[2],
                markevals[3],
                markevals[4],
                maxcombo,
                DateTime.Now);

            if (text.Length >= 140 - TweetHashTag.Length)
            {
                text = text.Substring(0, 140 - TweetHashTag.Length) + TweetHashTag;
            }
            else
            {
                text += TweetHashTag;
            }
            return text;
        }

        public void Tweet()
        {
            if (CanTweet)
            {
                TwitterManager.Manager.PostStatus(TweetText, EndTweetCallback);
            }
        }

        private void EndTweetCallback(bool success)
        {
            Tweeted = true;
            TweetFinish = true;
            TweetSuccess = success;
        }

        public void Return(object sender, EventArgs e)
        {
            this.ReturnToMenu();
        }

        public void Retry(object sender, EventArgs e)
        {
            hm.IsPaused = false;
            mistake = Tweeted = false;
            black.Hidden = true;
            black.Alpha = 0;
            if (startTime < 0)
            {
                m.Seek(0);
            }
            else
            {
                m.Seek(startTime);
            }
            m.Play();
            m.Pause();
            if (startTime < 0)
            {
                m.Seek(0);
            }
            else
            {
                m.Seek(startTime);
            }
            m.SetDefaultVisible();
            //seek
            mm.Seek(mmStartTime);
            em.Seek(startTime > 0 ? startTime : 0);
            sm.Seek(startTime);
            km.Seek(startTime);
            System.Threading.Thread.Sleep(1000);
            state = State.playing;
            cgi.Retry();
            gr.Retry();
            pd.Retry();
            grm.Retry();
            hm.Retry();
            em.Update(startTime);
            if (startTime < 0)
            {
                waitingmoviestart = true;
                lasttime = Win32API.timeGetTime();
            }
            else
            {
                m.Play();
            }
            this.fadeoutfinish = null;
        }
        public override void Update(int[] presscount, bool[] released)
        {
            if (!black.Hidden)
            {
                if (black.Alpha >= 1)
                {
                    black.Alpha = 1;
                    if (this.fadeoutfinish != null)
                    {
                        this.fadeoutfinish(this, new EventArgs());
                        return;
                    }
                }
                else
                {
                    black.Alpha += 0.05f;
                    if (black.Alpha >= 1)
                    {
                        black.Alpha = 1;
                    }
                }
            }
            float time = (float)m.MoviePosition;
            time += PlatformGap + PPDSetting.Setting.MovieLatency + currentLatency;
            if (waitingmoviestart && state == State.playing)
            {
                long deftime = Win32API.timeGetTime();
                /*if ((deftime - lasttime) / (double)1000 >= Math.Abs(ppdgameutil.StartTime + util.startgap) && !m.Playing)
                {
                    m.Play();
                }*/
                if ((deftime - lasttime) / (double)1000 >= Math.Abs(startTime))
                {
                    waitingmoviestart = false;
                    m.Play();
                }
                else
                {
                    time = (float)((deftime - lasttime) / (double)1000 + startTime);
                    time += PlatformGap + PPDSetting.Setting.MovieLatency + currentLatency;
                }
            }
            if (endTime <= time && black.Hidden)
            {
                black.Hidden = false;
                this.fadeoutfinish += new EventHandler(this.setresult);
                m.FadeOut();
                mistake = false;
            }
            bool[] b = new bool[presscount.Length];
            for (int k = 0; k < presscount.Length; k++)
            {
                if ((presscount[k] == 1))
                {
                    b[k] = true;
                }
                if (!allowinput) b[k] = false;
            }
            if (b[10])
            {
                if (state < State.result)
                {
                    if (state == State.playing)
                    {
                        state = State.stop;
                        if (waitingmoviestart)
                        {
                            templasttime = time;
                        }
                        m.Pause();
                        hm.IsPaused = true;
                        pd.Hidden = false;
                        return;
                    }
                    else
                    {
                        state = State.playing;
                        hm.IsPaused = false;
                        m.Play();
                        return;
                    }
                }
            }
            if (state == State.playing)
            {
                if (GameHost.IsCloseRequired)
                {
                    state = State.stop;
                    if (waitingmoviestart)
                    {
                        templasttime = time;
                    }
                    m.Pause();
                    hm.IsPaused = true;
                    pd.Hidden = false;
                }
                else
                {
                    bool[] tempb = new bool[b.Length];
                    for (int i = 0; i < b.Length; i++)
                    {
                        if (b[i])
                        {
                            int index = Array.IndexOf(keychange, i);
                            tempb[index] = true;
                        }
                    }
                    em.Update(time);
                    km.Update(time);
                    if (!ppdgameutility.Profile.MuteSE && !ppdgameutility.MuteSE)
                    {
                        sm.Update(time, tempb, em.VolumePercents, em.KeepPlaying, released);
                    }
                    mm.Update(time, b, released, !black.Hidden);
                    hm.Update();
                    ppdem.Update();
                    cgi.ChangeMoviePosition((float)((time - startTime) / (endTime - startTime) * CGameInterface.MaxMoviePosition));
                    cgi.Update();
                    if (grm.IfDeath && !godmode && !mistake)
                    {
                        allowinput = false;
                        this.fadeoutfinish += new EventHandler(this.setresult);
                        m.FadeOut();
                        black.Hidden = false;
                        mistake = true;
                    }
                }
            }
            else if (state == State.result)
            {
                if (TweetFinish)
                {
                    TweetFinish = false;
                    OnTweetFinish();
                }
                gr.Update(b, released);
            }
            else if (state == State.stop)
            {
                pd.Update(b);
            }
            base.Update();
        }

        protected void OnTweetFinish()
        {
            if (TweetFinished != null)
            {
                TweetFinished.Invoke(TweetSuccess);
            }
        }

        public override void Draw()
        {
            if (state < State.result)
            {
                if (!waitingmoviestart)
                {
                    m.Draw();
                }
                hm.Draw();
                ppdem.Draw();
                mm.Draw();
                cgi.Draw();
                base.Draw();
            }
            else
            {
                base.Draw();
                gr.Draw();
            }
            if (state == State.stop)
            {
                pd.Draw();
            }
        }
        public void SpecialPlaySound(int index, bool keep)
        {
            if (ppdgameutility.Profile.MuteSE || ppdgameutility.MuteSE) return;
            if (keep) sm.spKeepPlaysound(index, em.GetVolPercent(index), em.KeepPlaying);
            else sm.spPlaysound(index, em.GetVolPercent(index));
        }
        public void SpecialStopSound(int index, bool keep)
        {
            if (ppdgameutility.Profile.MuteSE) return;
            sm.spStopsound(index);
        }

        void km_KasiChanged(string kasi)
        {
            cgi.ChangeKasi(kasi);
        }

        void mm_ChangeCombo(bool gain, Vector2 pos)
        {
            if (gain)
            {
                grm.CurrentCombo++;
            }
            else
            {
                grm.CurrentCombo = 0;
            }
            cgi.ChangeCombo(pos, grm.CurrentCombo);
        }
        public void EvaluateCount(int index, bool isMissPress)
        {
            grm.CurrentLife += isMissPress ? evapoint[4] * (5 - index) / 8 : evapoint[4 - index];
            grm.GainScore((MarkEvaluateType)(4 - index), isMissPress);
            grm.Evacount[4 - index]++;
            float sum = 0;
            foreach (int count in grm.Evacount) sum += count;
            float ratio = ((float)(grm.Evacount[0] + grm.Evacount[1]) / sum - 0.7f) / 0.3f;
            if (ratio <= 0) ratio = 0;
            if (ratio >= 1) ratio = 1;
            cgi.ChangeEvaluate((MarkEvaluateType)(4 - index), isMissPress, grm.CurrentLifeAsFloat, ratio, ResultEvaluator.Evaluate(grm.Evacount));
            cgi.ChangeScore(grm.CurrentScore);
        }
        bool mm_PressingButton(ButtonType buttonType, bool pressing)
        {
            return hm.SetPressing(buttonType, pressing);
        }
        void hm_ScoreGained(int gain)
        {
            grm.GainScore(gain);
            cgi.ChangeScore(grm.CurrentScore);
        }
        public void ChangeMovieVolume(int volume)
        {
            if (m != null && m.Initialized)
            {
                m.SetVolume(volume);
            }
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (m != null)
            {
                m.Dispose();
            }
            if (sm != null)
            {
                sm.Dispose();
            }
            if (cgi != null)
            {
                cgi.Dispose();
            }
            if (em != null)
            {
                em.Dispose();
            }
            if (mm != null)
            {
                mm.Dispose();
            }
            if (pd != null)
            {
                pd.Dispose();
            }
            if (ppdem != null)
            {
                ppdem.Dispose();
            }
            if (gr != null)
            {
                gr.Dispose();
            }
            if (hm != null)
            {
                hm.Dispose();
            }
        }
    }
}
