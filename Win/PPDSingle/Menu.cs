using PPDCore;
using PPDFramework;
using PPDFramework.Scene;
using PPDFramework.Web;
using PPDFrameworkCore;
using PPDShareComponent;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PPDSingle
{
    public class Menu : SceneBase
    {
        enum LastPlayState
        {
            None = 0,
            Play,
            Preview,
            Replay,
        }

        LastPlayState lastPlayState = LastPlayState.None;
        int count;
        bool moviechanged = true;
        MenuMovie menuMovie;
        MenuSelectSong mss;
        SongInfoControl sic;
        ConfirmControl cc;
        OptionControl oc;
        SelectSongManager ssm;
        LeftMenu lm;
        ScoreSearcher ss;
        FocusManager fm;
        DxTextBox textBox;
        BackGroundDisplay bgd;
        OpenBrowserDialog obd;
        RectangleComponent black;

        bool allowcommand = true;
        bool goToPlay;

        public Menu(PPDDevice device) : base(device)
        {
        }

        public override bool Load()
        {
            WebManager.Instance.UpdateAccountInfo();
            //init
            var list = SongInformation.All;
            int selectnum = 0;
            string initialdirectory = PPDSetting.Setting.SongDir;
            ContestInfo contestInfo = null;
            WebSongInformation[] activeScores = null;
            ListInfo[] lists = null;
            Action[] actions = {
                () => { contestInfo = WebManager.Instance.GetContestInfo(); },
                () => { activeScores = WebManager.Instance.GetScores(true); },
                () => { lists = WebManager.Instance.GetListInfos(); },
                PerfectTrialCache.Instance.Update
            };
            Parallel.ForEach(actions, (action) => action());
            // 前のメニューからのパラメーターを調べる
            if (PreviousParam.TryGetValue("PPDGameUtility", out object temp))
            {
                var gameutility = temp as PPDGameUtility;
                initialdirectory = gameutility.SongInformation.ParentDirectory;
            }
            ssm = new SelectSongManager(contestInfo, activeScores, lists);
            ssm.SongChanged += ssm_SongChanged;
            ssm.DirectoryChanged += ssm_DirectoryChanged;
            ssm.ModeChanged += ssm_ModeChanged;
            ssm.Filter.Desc = SkinSetting.Setting.Desc;
            ssm.Filter.Difficulty = SkinSetting.Setting.Difficulty;
            ssm.Filter.Field = SkinSetting.Setting.SortField;
            ssm.Filter.Type = SkinSetting.Setting.ScoreType;
            textBox = new DxTextBox(device, GameHost, ResourceManager);
            mss = new MenuSelectSong(device, ResourceManager, this);
            mss.Inputed += mss_Inputed;
            mss.GotFocused += mss_GotFocused;
            sic = new SongInfoControl(device, ResourceManager);
            sic.Inputed += sic_Inputed;
            cc = new ConfirmControl(device, ResourceManager);
            cc.Inputed += cc_Inputed;
            oc = new OptionControl(device, ResourceManager, Sound);
            lm = new LeftMenu(device, GameHost, ResourceManager, textBox, ssm, Sound);
            lm.RandomSelected += lm_RandomSelected;
            lm.FilterChanged += lm_FilterChanged;
            ss = new ScoreSearcher(device, ResourceManager, textBox, ssm.Filter);
            obd = new OpenBrowserDialog(device, ResourceManager, Sound);
            fm = new FocusManager(this);
            fm.Focus(mss);

            mss.DisapperFinish += cc.Show;
            bgd = new BackGroundDisplay(device, ResourceManager, "skins\\PPDSingle_BackGround.xml", "Menu");
            black = new RectangleComponent(device, ResourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0
            };

            menuMovie = new MenuMovie(device);
            this.AddChild(black);
            this.AddChild(textBox);
            this.AddChild(obd);
            this.AddChild(cc);
            this.AddChild(oc);
            this.AddChild(ss);
            this.AddChild(lm);
            this.AddChild(sic);
            this.AddChild(mss);
            var po = new PictureObject(device, ResourceManager, Utility.Path.Combine("bottom.png"));
            po.Position = new Vector2(0, 450 - po.Height + 1);
            po.AddChild(new TextureString(device, Utility.Language["SearchScore"], 14, PPDColors.Gray)
            {
                Position = new Vector2(85, 7)
            });
            po.AddChild(new TextureString(device, Utility.Language["Move"], 14, PPDColors.Gray)
            {
                Position = new Vector2(205, 7)
            });
            po.AddChild(new TextureString(device, Utility.Language["Menu"], 14, PPDColors.Gray)
            {
                Position = new Vector2(325, 7)
            });
            po.AddChild(new TextureString(device, Utility.Language["Option"], 14, PPDColors.Gray)
            {
                Position = new Vector2(445, 7)
            });
            po.AddChild(new TextureString(device, Utility.Language["Decide"], 14, PPDColors.Gray)
            {
                Position = new Vector2(565, 7)
            });
            po.AddChild(new TextureString(device, Utility.Language["Back"], 14, PPDColors.Gray)
            {
                Position = new Vector2(685, 7)
            });
            this.AddChild(po);
            this.AddChild(menuMovie);
            this.AddChild(bgd);

            oc.Connect = SkinSetting.Setting.Connect;
            oc.RivalGhost = SkinSetting.Setting.RivalGhost;

            ssm.CurrentRoot = SongInformation.Root;
            ssm.CurrentLogicRoot = LogicFolderInfomation.Root;

            if (PreviousParam.TryGetValue("CurrentRoot", out temp))
            {
                var root = temp as SongInformation;
                ssm.CurrentRoot = root;
            }
            if (PreviousParam.TryGetValue("CurrentLogicRoot", out temp))
            {
                var root = temp as LogicFolderInfomation;
                ssm.CurrentLogicRoot = root;
            }
            if (PreviousParam.TryGetValue("CurrentMode", out temp))
            {
                var mode = (SelectSongManager.Mode)temp;
                ssm.ChangeMode(mode);
            }
            if (PreviousParam.TryGetValue("SelectedIndex", out temp))
            {
                selectnum = (int)temp;
            }
            ssm.SelectedIndex = selectnum;

            return true;
        }

        void lm_FilterChanged()
        {
            ssm.Regenerate();
            SkinSetting.Setting.ScoreType = ssm.Filter.Type;
            SkinSetting.Setting.Difficulty = ssm.Filter.Difficulty;
            SkinSetting.Setting.Desc = ssm.Filter.Desc;
            SkinSetting.Setting.SortField = ssm.Filter.Field;
        }

        void lm_RandomSelected(RandomSelectType randomSelectType)
        {
            ssm.RandomSelect(randomSelectType);
        }

        void mss_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (args.FocusObject != null)
            {
                if (args.FocusObject.GetType() == typeof(ScoreSearcher))
                {
                    if (ss.SelectedSongInformation != null && ss.Selected)
                    {
                        ssm.ChangeMode(SelectSongManager.Mode.SongInfo);
                        ssm.CurrentRoot = ss.SelectedSongInformation.Parent;
                        ssm.SelectedIndex = ssm.SongInformations.FindIndex(link => link.SongInfo == ss.SelectedSongInformation);
                    }
                }
                else if (args.FocusObject.GetType() == typeof(LeftMenu))
                {
                    if (lm.ShouldFinish)
                    {
                        GameHost.GoHome();
                    }
                }
            }
            sic.Show();
            mss.Start();
        }

        void cc_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (!cc.Appeared)
                {
                    cc.ForceShow();
                }
                else if (cc.Focused)
                {
                    allowcommand = false;
                    cc.Next();
                    mss.DisapperFinish -= cc.Show;
                    if (PPDSetting.Setting.MenuMoviePreviewDisabled)
                    {
                        PlayGame();
                    }
                    else
                    {
                        goToPlay = true;
                        menuMovie.FadeOut();
                        menuMovie.CheckLoopAvailable = false;
                    }
                    Sound.Play(PPDSetting.DefaultSounds[1], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                fm.RemoveFocus();
                cc.Vanish();
                Sound.Play(PPDSetting.DefaultSounds[2], -1000);
            }
        }

        void sic_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (sic.CanGoNext())
                {
                    fm.Focus(cc);
                    sic.FadeOut();
                    mss.Disappear();
                    cc.SetInfo(ssm.SelectedSongInformation.SongInfo.DirectoryName, sic.Difficult);
                    Sound.Play(PPDSetting.DefaultSounds[1], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                fm.RemoveFocus();
                Sound.Play(PPDSetting.DefaultSounds[2], -1000);
            }
            else
            {
                if (args.InputInfo.IsPressed(ButtonType.Left))
                {
                    if (sic.ChangeDifficulty(-1))
                        Sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
                if (args.InputInfo.IsPressed(ButtonType.Right))
                {
                    if (sic.ChangeDifficulty(1))
                        Sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
            }
        }

        void mss_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (ssm.CanSelect)
                {
                    if (ssm.CanGoDown)
                    {
                        ssm.GoDownDirectory();
                    }
                    else
                    {
                        fm.Focus(sic);
                    }
                    Sound.Play(PPDSetting.DefaultSounds[1], -1000);
                }
                else
                {
                    if (ssm.SelectedSongInformation == null)
                    {
                        return;
                    }
                    if (ssm.SelectedSongInformation is ContestSelectedSongInfo)
                    {
                        obd.ScoreLibraryId = (ssm.SelectedSongInformation as ContestSelectedSongInfo).ContestInfo.ScoreLibraryId;
                    }
                    else if (ssm.SelectedSongInformation is ActiveScoreSelectedSongInfo)
                    {
                        obd.ScoreLibraryId = (ssm.SelectedSongInformation as ActiveScoreSelectedSongInfo).ActiveScore.Hash;
                    }
                    else if (ssm.SelectedSongInformation is ListInfoSelectedSongInfo)
                    {
                        obd.ScoreLibraryId = (ssm.SelectedSongInformation as ListInfoSelectedSongInfo).ListScoreInfo.ScoreId;
                    }
                    fm.Focus(obd);
                    Sound.Play(PPDSetting.DefaultSounds[1], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                if (ssm.CanGoUp) ssm.GoUpDirectory();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Triangle))
            {
                fm.Focus(oc);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Square))
            {
                fm.Focus(lm);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Start))
            {
                if (!PPDSetting.Setting.TextBoxDisabled)
                {
                    fm.Focus(ss);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.R))
            {
                ssm.ChangeMode(true);
            }
            else if (args.InputInfo.IsPressed(ButtonType.L))
            {
                ssm.ChangeMode(false);
            }
            else
            {
                if (args.InputInfo.IsPressed(ButtonType.Left))
                {
                    ssm.SeekSong(-4);
                    Sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
                if (args.InputInfo.IsPressed(ButtonType.Right))
                {
                    ssm.SeekSong(4);
                    Sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
                if (args.InputInfo.IsPressed(ButtonType.Up))
                {
                    ssm.PreviousSong();
                    Sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
                if (args.InputInfo.IsPressed(ButtonType.Down))
                {
                    ssm.NextSong();
                    Sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
            }
        }

        void ssm_DirectoryChanged(object sender, EventArgs e)
        {
            mss.SongInformations = ssm.SongInformations;
        }

        void ssm_SongChanged(SelectSongManager.SongChangeMode obj)
        {
            mss.ChangeIndex(ssm.SelectedIndex, obj);
            if (ssm.SelectedSongInformation == null)
            {
                sic.HideInfo();
                return;
            }
            if (ssm.SelectedSongInformation.IsFolder)
            {
                menuMovie.FadeOut();
                menuMovie.CheckLoopAvailable = false;
                sic.HideInfo();
            }
            else
            {
                menuMovie.FadeOut();
                menuMovie.CheckLoopAvailable = true;
                count = 0;
                moviechanged = true;
                SongInformation current = ssm.SelectedSongInformation.SongInfo;
                sic.ChangeSongInfo(ssm.SelectedSongInformation);
            }
        }
        void ssm_ModeChanged(object sender, EventArgs e)
        {
            mss.ChangeMode(ssm.CurrentMode);
        }
        public Difficulty Difficulty
        {
            get
            {
                return sic.Difficulty;
            }
        }
        /// <summary>
        /// 動画を変更する
        /// </summary>
        private void ChangeMovie()
        {
            if (!allowcommand)
            {
                PlayGame();
                return;
            }
            if (ssm.SelectedSongInformation == null || ssm.SelectedSongInformation.IsFolder || ssm.SelectedSongInformation.SongInfo == null) return;
            string moviePath = ssm.SelectedSongInformation.SongInfo.MoviePath;
            SongInformation songinfo = ssm.SelectedSongInformation.SongInfo;
            if (!string.IsNullOrEmpty(moviePath))
            {
                if (PPDSetting.Setting.MenuMoviePreviewDisabled)
                {
                    return;
                }

                var index = this.IndexOf(menuMovie);
                this.RemoveChild(menuMovie);
                menuMovie.Dispose();
                menuMovie = new MenuMovie(device);
                this.InsertChild(menuMovie, index);
                // 動画取得
                menuMovie.Movie = GameHost.GetMovie(songinfo);
                // 動画の最大ボリューム設定
                menuMovie.MaximumVolume = songinfo.MovieVolume;
                menuMovie.FileName = moviePath;
                // 初期化
                try
                {
                    menuMovie.Initialize();
                    // 動画のカット設定
                    menuMovie.TrimmingData = songinfo.TrimmingData;
                    // 動画のループ設定
                    menuMovie.SetLoop(songinfo.ThumbStartTime, songinfo.ThumbEndTime);
                    // 再生、フェードイン
                    menuMovie.Play();
                    menuMovie.FadeIn();
                }
                catch
                {
                    GameHost.AddNotify(PPDExceptionContentProvider.Provider.GetContent(PPDExceptionType.CannotOpenMovie));
                }
            }
            else
            {
                MessageBox.Show(PPDExceptionContentProvider.Provider.GetContent(PPDExceptionType.NoMovieFileInDirectory) + "\n@" + songinfo.DirectoryPath);
            }
        }

        public override void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            inputInfo = new MenuInputInfo(inputInfo);
            count++;
            if (count > 60 && moviechanged)
            {
                // いきなり動画を変えると重いので１秒(60フレーム)待つ
                count = 0;
                if (moviechanged)
                {
                    ChangeMovie();
                    moviechanged = false;
                }
            }
            if (allowcommand)
            {
                fm.ProcessInput(inputInfo);
            }
            ssm.Update();
            if (goToPlay)
            {
                if (black.Alpha >= 1)
                {
                    PlayGame();
                    goToPlay = false;
                }
                else
                {
                    black.Alpha += 0.1f;
                    if (black.Alpha >= 1)
                    {
                        black.Alpha = 1;
                    }
                }
            }
            Update();
        }

        public void PlayGame()
        {
            ChangeSceneToMainGame(Difficulty, ssm.SelectedSongInformation.SongInfo.StartTime, true, true);
            lastPlayState = LastPlayState.Play;
        }

        /// <summary>
        /// 今選択している譜面をプレイします
        /// </summary>
        public void PreviewPlay(Difficulty difficulty, float startTime)
        {
            ChangeSceneToMainGame(difficulty, startTime, true, false);
            lastPlayState = LastPlayState.Preview;
        }

        /// <summary>
        /// リプレイします
        /// </summary>
        /// <param name="songInfo"></param>
        /// <param name="replayInfo"></param>
        public void Replay(SongInformation songInfo, Difficulty difficulty, ReplayInfo replayInfo)
        {
            ChangeSceneToMainGame(songInfo, difficulty, songInfo.StartTime, true, false, replayInfo.ResultId);
        }

        private void ChangeSceneToMainGame(Difficulty difficulty, float startTime, bool useStack, bool useItem)
        {
            ChangeSceneToMainGame(ssm.SelectedSongInformation.SongInfo, difficulty, startTime, useStack, useItem, -1);
        }

        private void ChangeSceneToMainGame(SongInformation songInfo, Difficulty difficulty, float startTime, bool useStack, bool useItem, int replayResultId)
        {
            // メインゲーム用のパラメータの準備
            var gameutility = new PPDGameUtility
            {
                SongInformation = songInfo,
                Difficulty = difficulty,
                DifficultString = songInfo.GetDifficultyString(difficulty),
                Profile = ProfileManager.Instance.Current,
                AutoMode = oc.AutoMode,
                SpeedScale = oc.SpeedScale,
                Random = oc.Random,
                MuteSE = oc.MuteSE,
                Connect = oc.Connect,
                PerfectTrial = oc.PerfectTrial,
                RivalGhost = oc.RivalGhost,
                RivalGhostCount = oc.RivalGhostCount,
                ReplayResultId = replayResultId
            };
            if (useItem && lm.UseItem != null && !lm.UseItem.IsUsed)
            {
                gameutility.AddItem(lm.UseItem);
                gameutility.AutoMode = AutoMode.None;
            }
            if (ssm.SelectedSongInformation is ContestSelectedSongInfo)
            {
                var contest = ssm.SelectedSongInformation as ContestSelectedSongInfo;
                gameutility.RankingUpdateFunc = contest.GetRanking;
            }
            SkinSetting.Setting.Connect = oc.Connect;
            SkinSetting.Setting.RivalGhost = oc.RivalGhost;
            SkinSetting.Setting.RivalGhostCount = oc.RivalGhostCount;
            var dic = new Dictionary<string, object>
            {
                { "PPDGameUtility", gameutility },
                { "GameInterface", new GameInterface(device) },
                { "GameResult", new GameResult(device) },
                { "PauseMenu", new PauseMenu(device, Utility.Path) },
                { "MarkImagePath", new MarkImagePaths() }
            };
            if (!useStack)
            {
                dic.Add("NextScene", typeof(Menu));
            }
            if (gameutility.SongInformation.StartTime != startTime)
            {
                dic.Add("StartTime", startTime);
            }
            // 最後の曲選択を残しておく
            dic.Add("CurrentRoot", ssm.CurrentRoot);
            dic.Add("CurrentLogicRoot", ssm.CurrentLogicRoot);
            dic.Add("CurrentMode", ssm.CurrentMode);
            dic.Add("SelectedIndex", ssm.SelectedIndex);
            var mainGame = new MainGame(device);
            mainGame.CannotStartPerfectTrial += mainGame_CannotStartPerfectTrial;
            mainGame.PerfectTrialError += mainGame_PerfectTrialError;
            mainGame.PerfectTrialStart += mainGame_PerfectTrialStart;
            SceneManager.PrepareNextScene(this, mainGame, dic, dic, useStack);

            if (useStack)
            {
                menuMovie.Stop();
            }
        }

        void mainGame_PerfectTrialStart(int arg1, int arg2)
        {
            GameHost.AddNotify(String.Format(Utility.Language["PerfectTrialStart"]));
            GameHost.AddNotify(String.Format(Utility.Language["MoneyChange"], arg1, arg2));
        }

        void mainGame_PerfectTrialError(ErrorReason obj)
        {
            GameHost.AddNotify(Utility.Language["PerfectTrialStartFailed"]);
            switch (obj)
            {
                case ErrorReason.NetworkError:
                case ErrorReason.ArgumentError:
                case ErrorReason.AuthFailed:
                case ErrorReason.ValidateFailed:
                    GameHost.AddNotify(String.Format(Utility.Language["PerfectTrialStartFailedReason"],
                        obj));
                    break;
                case ErrorReason.ScoreNotFound:
                    GameHost.AddNotify(Utility.Language["PerfectTrialStartScoreNotFound"]);
                    break;
                case ErrorReason.LackOfMoney:
                    GameHost.AddNotify(Utility.Language["PerfectTrialStartLackOfMoney"]);
                    break;
                case ErrorReason.NotAvailableScore:
                    GameHost.AddNotify(Utility.Language["PerfectTrialStartNotAvailableScore"]);
                    break;
                case ErrorReason.AlreadyCleared:
                    GameHost.AddNotify(Utility.Language["PerfectTrialStartAlreadyCleared"]);
                    break;
            }
        }

        void mainGame_CannotStartPerfectTrial()
        {
            GameHost.AddNotify(String.Format(Utility.Language["CannotStartPerfectTrial"]));
        }

        public override void SceneStackPoped(Dictionary<string, object> param)
        {
            switch (lastPlayState)
            {
                case LastPlayState.Play:
                    if (param.ContainsKey("PerfectTrialSucceess") && (bool)param["PerfectTrialSucceess"])
                    {
                        var gameUtility = (PPDGameUtility)param["GameUtility"];
                        PerfectTrialCache.Instance.AddPerfectTrial(CryptographyUtility.Getx2Encoding(
                            gameUtility.SongInformation.GetScoreHash(gameUtility.Difficulty)));
                        ssm.SelectedSongInformation.UpdatePerfectTrialInfo();
                    }
                    allowcommand = true;
                    cc.Next();
                    mss.DisapperFinish += cc.Show;
                    fm.RemoveFocus();
                    cc.Vanish();
                    menuMovie.Seek(ssm.SelectedSongInformation.SongInfo.ThumbStartTime);
                    menuMovie.FadeIn();
                    sic.UpdateResult();
                    sic.UpdatePerfectTrials();
                    goto default;
                default:
                    menuMovie.Play();
                    break;
            }
            menuMovie.CheckLoopAvailable = true;
            goToPlay = false;
            black.Alpha = 0;
        }

        protected override void DisposeResource()
        {
            menuMovie.Stop();
            base.DisposeResource();
            textBox.Dispose();
            ssm.Dispose();
        }
    }
}
