using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.Mod;
using PPDFramework.PPDStructure.PPDData;
using PPDFramework.Scene;
using PPDFramework.Shaders;
using PPDFramework.Web;
using PPDPack;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PPDCore
{
    public delegate void DrawEventHandler(LayerType layerType);
    public class MainGameComponent : GameComponent
    {
        public event DrawEventHandler Drawed;
        public event EventHandler Paused;
        public event EventHandler Finished;
        public event Action<int> ScoreChanged;
        public event Action<MarkEvaluateType, bool> EvaluateChanged;
        public event Action<int> LifeChanged;
        public event Action<int, Vector2> ComboChanged;
        public event Action CannotStartPerfectTrial;
        public event Action<ErrorReason> PerfectTrialError;
        public event Action<int, int> PerfectTrialStart;

        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;
        ISceneBase scene;

        const float defaultbpm = 130;
        const int maxMovieTimesCount = 180;
        const double AprilFoolPlayRate1 = 1.25;
        const double AprilFoolPlayRate2 = 0.8;

        double playRatio = 1;
        float aprilFoolMovieLatency;

        enum State
        {
            Playing = 0,
            Stop = 1,
            Result = 2,
            Waiting = 3,
        }

        bool allowInput = true;
        bool waitingMovieStart;
        State state = State.Playing;

        public bool initialized;
        IMovie movie;
        KasiManager kasiManager;
        SoundManager soundManager;
        EventManager eventManager;
        MarkManager markManager;
        PPDEffectManager effectManager;
        HoldManager holdManager;
        SlideManager slideManager;
        FlowScriptManager scriptManager;
        ScoreStorage scoreStorage;
        Combo combo;
        int[] evaPoint = new int[5];
        long lastTime;
        float tempLastTime;
        PPDGameUtility gameUtility;
        PPDPlayRecorder playRecorder;
        PPDPlayDecorder playDecorder;
        bool recording = true;
        float endTime;
        RandomChangeManager randomChangeManager = new RandomChangeManager();
        bool mistake;

        LatencyUpdaterBase latencyUpdater;
        LatencyFixer latencyFixer;

        float startTime;
        float markStartTime;

        IGameHost gameHost;
        GameInterfaceBase gameInterface;
        MarkImagePathsBase markImagePath;
        GameResultBase gameResult;
        PauseMenuBase pauseMenu;
        MainGameConfigBase config;
        List<float> movieTimes;

        bool movieFinishCalled;
        bool finishCalled;
        float currentMovieTime;
        long currentTime;
        float currentUpdateTime;
        bool superAuto;

        string perfectTrialToken;
        bool perfectTrialing;

        bool dontUpdateExtraProcess;

        SpriteObject hiddenSprite;

        public int Score
        {
            get
            {
                return GameResultManager.CurrentScore;
            }
        }

        public int CoolCount
        {
            get
            {
                return GameResultManager.CoolCount;
            }
        }

        public int GoodCount
        {
            get
            {
                return GameResultManager.GoodCount;
            }
        }

        public int SafeCount
        {
            get
            {
                return GameResultManager.SafeCount;
            }
        }

        public int SadCount
        {
            get
            {
                return GameResultManager.SadCount;
            }
        }

        public int WorstCount
        {
            get
            {
                return GameResultManager.WorstCount;
            }
        }

        public int MaxCombo
        {
            get
            {
                return GameResultManager.MaxCombo;
            }
        }

        public float MoviePosition
        {
            get
            {
                return (float)movie.MoviePosition;
            }
        }

        public float CurrentUpdateTime
        {
            get
            {
                return currentUpdateTime;
            }
        }

        public bool PauseMovieWhenPause
        {
            get;
            set;
        }

        public bool Replaying
        {
            get
            {
                return !recording;
            }
        }

        public MainGameComponent(PPDDevice device, IGameHost gameHost, PPDFramework.Resource.ResourceManager resourceManager,
            ISound sound, ISceneBase scene, PPDGameUtility gameUtility, GameInterfaceBase gameInterface, MarkImagePathsBase markImagePath,
            GameResultBase gameResult, PauseMenuBase pauseMenu, MainGameConfigBase config, float startTime, float markStartTime) : base(device)
        {
            this.gameHost = gameHost;
            this.resourceManager = resourceManager;
            this.sound = sound;
            this.scene = scene;
            this.gameUtility = gameUtility;
            this.gameInterface = gameInterface;
            this.gameResult = gameResult;
            this.pauseMenu = pauseMenu;
            this.markImagePath = markImagePath;
            this.startTime = startTime;
            this.markStartTime = markStartTime;
            this.config = config;

            PauseMovieWhenPause = true;
        }

        public void Initialize(bool play = true, bool isGhost = false, Dictionary<string, object> scriptItems = null)
        {
            StartRecord();
            movieTimes = new List<float>();
            switch (gameUtility.LatencyType)
            {
                case LatencyType.Db:
                    latencyUpdater = new DbLatencyUpdater(gameUtility);
                    break;
                case LatencyType.File:
                    latencyUpdater = new FileLatencyUpdater(gameUtility);
                    break;
            }

            if (!isGhost)
            {
                string moviefile = gameUtility.SongInformation.MoviePath;
                if (moviefile != "")
                {
                    movie = gameHost.GetMovie(gameUtility.SongInformation);
                    try
                    {
                        movie.Initialize();
                    }
                    catch (Exception e)
                    {
                        throw new PPDException(PPDExceptionType.CannotOpenMovie, "Failed to initialize movie", e);
                    }
                    movie.PlayRate = playRatio;
                    if (movie.IsAudioOnly)
                    {
                        aprilFoolMovieLatency = 0;
                    }
                    ReadMovieSetting();
                    initialized = true;

                }
                else
                {
                    throw new PPDException(PPDExceptionType.CannotOpenMovie, "movie file is empty", null);
                }
            }

            if (!isGhost)
            {
                PerfectTrial();
            }
            ReadProfile();
            var defaultResource = new ScriptResourceManager(device, sound,
                Path.Combine(gameUtility.SongInformation.DirectoryPath, "resource.pak"),
                Path.Combine("scores", gameUtility.SongInformation.ID.ToString()));
            scriptManager = new FlowScriptManager(device, sound);
            holdManager = new HoldManager(device, resourceManager);
            holdManager.ScoreGained += hm_ScoreGained;
            slideManager = new SlideManager(device, resourceManager);
            effectManager = new PPDEffectManager(device, resourceManager);
            kasiManager = new KasiManager(gameUtility);
            kasiManager.KasiChanged += km_KasiChanged;
            soundManager = new SoundManager(new ExSound(sound), gameUtility);
            combo = new Combo(device, resourceManager);
            scoreStorage = ScoreStorage.GetStorageFromSongInformation(gameUtility.SongInformation);

            if (gameUtility.SongInformation.IsOld)
            {
                eventManager = new EventManager(gameUtility, config, scriptManager);
                markManager = new MarkManager(device, resourceManager, gameUtility, randomChangeManager, effectManager, markImagePath, config, scriptManager);
            }
            else
            {
                var path = Path.Combine(gameUtility.SongInformation.DirectoryPath, DifficultyUtility.ConvertDifficulty(gameUtility.Difficulty) + ".ppd");
                using (PackReader reader = new PackReader(path))
                {
                    foreach (string content in reader.FileList)
                    {
                        using (PPDPackStreamReader ppdpsr = reader.Read(content))
                        {
                            switch (content)
                            {
                                case "evd":
                                    eventManager = new EventManager(gameUtility, ppdpsr, config, scriptManager);
                                    break;
                                case "ppd":
                                    markManager = new MarkManager(device, resourceManager, gameUtility, randomChangeManager, effectManager, markImagePath, config, scriptManager, ppdpsr);
                                    break;
                                default:
                                    if (content.StartsWith("Scripts"))
                                    {
                                        scriptManager.LoadScript(ppdpsr, defaultResource, content);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            soundManager.EventManager = eventManager;

            scriptManager.GameResultManager.ExpectedTotalSlideBonus = CalculateExpectedTotalSlideBonus();
            scriptManager.AddItem("GameInterface", gameInterface ?? (GameComponent)new SpriteObject(device));
            scriptManager.AddItem("Movie", movie);
            scriptManager.AddItem("PPDGameUtility", gameUtility);
            scriptManager.AddItem("Lylics", kasiManager);
            scriptManager.AddItem("MarkManager", markManager);
            scriptManager.AddItem("HoldLayer", holdManager);
            scriptManager.AddItem("MarkEffectLayer", effectManager);
            scriptManager.AddItem("ConnectLayer", markManager.ConnectLayer);
            scriptManager.AddItem("MarkLayer", markManager.MarkLayer);
            scriptManager.AddItem("ComboLayer", combo);
            scriptManager.AddItem("SlideLayer", slideManager);
            scriptManager.AddItem("SoundManager", soundManager);
            scriptManager.AddItem("EventManager", eventManager);
            scriptManager.AddItem("GameHost", gameHost);
            scriptManager.AddItem("ScoreStorage", scoreStorage);
            scriptManager.AddItem("PlayRatio", playRatio);
            if (scriptItems != null)
            {
                foreach (KeyValuePair<string, object> item in scriptItems)
                {
                    scriptManager.AddItem(item.Key, item.Value);
                }
            }

            if (!isGhost)
            {
                LoadMod();
            }

            markManager.EventManager = eventManager;
            if (pauseMenu != null)
            {
                pauseMenu.Resumed += pd_Resumed;
                pauseMenu.Retryed += pd_Retryed;
                pauseMenu.Returned += pd_Returned;
                pauseMenu.Replayed += pauseMenu_Replayed;
                pauseMenu.LatencyChanged += pd_LatencyChanged;
            }
            if (gameResult != null)
            {
                gameResult.CanReplay = gameUtility.AutoMode == AutoMode.None;
                gameResult.Retryed += gameResult_Retryed;
                gameResult.Replayed += gameResult_Replayed;
            }
            markManager.ChangeCombo += mm_ChangeCombo;
            markManager.PlaySound += SpecialPlaySound;
            markManager.StopSound += SpecialStopSound;
            markManager.EvaluateCount += EvaluateCount;
            markManager.PressingButton += mm_PressingButton;
            markManager.Slide += markManager_Slide;
            markManager.MaxSlide += markManager_MaxSlide;
            markManager.PreEvaluate += markManager_PreEvaluate;
            eventManager.ChangeMovieVolume += ChangeMovieVolume;
            holdManager.HoldStart += h =>
            {
                scriptManager.Call(HoldInfo.HoldStart, h);
            };
            holdManager.HoldChange += h =>
            {
                scriptManager.Call(HoldInfo.HoldChange, h);
            };
            holdManager.HoldEnd += h =>
            {
                scriptManager.Call(HoldInfo.HoldEnd, h);
            };
            holdManager.HoldMaxHold += h =>
            {
                scriptManager.Call(HoldInfo.MaxHold, h);
            };
            //seek
            markManager.Seek(markStartTime);
            eventManager.Seek(startTime > 0 ? startTime : 0);
            soundManager.Seek(startTime);
            kasiManager.Seek(startTime);
            scriptManager.Initialize();
            scriptManager.UpdateManager.MovieTime = startTime;
            scriptManager.UpdateManager.Update();
            scriptManager.Update(dontUpdateExtraProcess);
            if (movie != null && movie.Initialized)
            {
                movie.Play();
                movie.Pause();
                movie.Seek(startTime);
            }

            hiddenSprite = new SpriteObject(device);
            hiddenSprite.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800
            });
            if (defaultResource != null && defaultResource.ImageResources != null)
            {
                foreach (var imageResource in defaultResource.ImageResources)
                {
                    hiddenSprite.AddChild(new PictureObject(device, imageResource, false)
                    {
                        Rectangle = new RectangleF(0, 0, 100, 100)
                    });
                }
            }
            foreach (var resource in scriptManager.ScriptResourceManagers)
            {
                if (resource.ImageResources != null)
                {
                    foreach (var imageResource in resource.ImageResources)
                    {
                        hiddenSprite.AddChild(new PictureObject(device, imageResource, false)
                        {
                            Rectangle = new RectangleF(0, 0, 100, 100)
                        });
                    }
                }
            }

            if (!isGhost)
            {
                if (!UseItem())
                {
                    ResultReplay();
                }
            }

            if (!isGhost)
            {
                if (!Replaying && gameUtility.AutoMode == AutoMode.None && !PPDSetting.Setting.AutoAdjustLatencyDisabled)
                {
                    latencyFixer = new LatencyFixer(latencyUpdater.BaseLantencyCount);
                }
            }

            if (startTime < 0)
            {
                waitingMovieStart = true;
                lastTime = Win32API.timeGetTime();
            }
            else
            {
                if (play)
                {
                    Play();
                }
                else
                {
                    state = State.Waiting;
                }
            }
        }

        public GhostFrame[] GetGhostFrames(byte[] replayData, Action<int> progressCallback)
        {
            scriptManager.SuppressErrorLog = true;
            effectManager.SuspendAdd = true;
            dontUpdateExtraProcess = true;
            Retry();
            var ret = new List<GhostFrame>();
            playDecorder = PPDPlayDecorder.FromBytes(replayData);
            Action<int> lifeChangeHandler = l =>
            {
                ret.Add(new GhostFrame(currentUpdateTime, GameResultManager.CurrentScore, GameResultManager.CurrentLife));
            };
            Action<int> scoreChangeHandler = s =>
            {
                ret.Add(new GhostFrame(currentUpdateTime, GameResultManager.CurrentScore, GameResultManager.CurrentLife));
            };
            Action<MarkEvaluateType, bool> evaluateChangeHandler = (e, m) =>
            {
                ret.Add(new GhostFrame(currentUpdateTime, e, m));
            };
            LifeChanged += lifeChangeHandler;
            ScoreChanged += scoreChangeHandler;
            EvaluateChanged += evaluateChangeHandler;
            PreUpdate(float.MaxValue, EmptyInputInfo.Instance, progressCallback);
            LifeChanged -= lifeChangeHandler;
            ScoreChanged -= scoreChangeHandler;
            EvaluateChanged -= evaluateChangeHandler;
            effectManager.SuspendAdd = false;
            scriptManager.SuppressErrorLog = false;
            return ret.ToArray();
        }

        private void PerfectTrial()
        {
            if (!gameUtility.PerfectTrial)
            {
                return;
            }
            if (!gameUtility.IsRegular || Replaying)
            {
                CannotStartPerfectTrial?.Invoke();
                return;
            }
            var result = WebManager.Instance.PerfectTrialPrepare(gameUtility.SongInformation.GetScoreHash(gameUtility.Difficulty), out Dictionary<string, string> data);
            if (result != ErrorReason.OK)
            {
                perfectTrialToken = null;
                PerfectTrialError?.Invoke(result);
                perfectTrialing = false;
            }
            else
            {
                perfectTrialToken = data["Token"];
                int fromMoney = int.Parse(data["MoneyFrom"]), toMoney = int.Parse(data["MoneyTo"]);
                PerfectTrialStart?.Invoke(fromMoney, toMoney);
                perfectTrialing = true;
            }
        }

        void creator_Progressed(int obj)
        {
            gameHost.SendToLoading(new Dictionary<string, object>
            {
                {"Progress",obj}            });
        }

        void gameResult_Retryed(object sender, EventArgs e)
        {
            Retry();
        }

        private int CalculateExpectedTotalSlideBonus()
        {
            var ppdData = markManager.Marks;
            var groupData = ppdData.GroupBy(p => p.Time).Select(
                g => new MarkGroupData(g.Key, g.ToArray())).ToArray();
            var noteTypes = new Dictionary<float, NoteType>();
            var slideScales = new Dictionary<float, float>();
            foreach (var data in ppdData)
            {
                if (!noteTypes.ContainsKey(data.Time))
                {
                    noteTypes.Add(data.Time, eventManager.GetNoteType(data.Time));
                }
            }
            int sum = 0;
            foreach (var g in groupData)
            {
                var type = noteTypes[g.Key];
                if (type != NoteType.ACFT)
                {
                    continue;
                }
                var slideScale = eventManager.GetSlideScale(g.First().Time);
                foreach (var mk in g)
                {
                    if (!(mk is ExMarkData))
                    {
                        continue;
                    }
                    var exmk = (ExMarkData)mk;
                    if (g.SameTimings.IsSlideButton(exmk.ButtonType))
                    {
                        var count = (int)((exmk.EndTime - exmk.Time) / SlideExMark.ExFrameSec * slideScale);
                        var score = (10 + 10 * count) * count / 2;
                        sum += score + 1000;
                    }
                }
            }
            return sum;
        }

        private bool UseItem()
        {
            if (gameUtility.Items.Length == 0)
            {
                return false;
            }

            var autoItem = gameUtility.Items.FirstOrDefault(i => i.ItemType >= ItemType.Auto1 && i.ItemType <= ItemType.AutoFreePass);
            if (autoItem == null)
            {
                return false;
            }
            var isFreePass = autoItem.ItemType == ItemType.AutoFreePass;
            var itemType = isFreePass ? (ItemType)autoItem["SubItemType"] : autoItem.ItemType;
            var allowAllButtons = autoItem.ContainsParameter("AllowAllButton") && (bool)autoItem["AllowAllButton"];
            var includeFine = autoItem.ContainsParameter("IncludeFine") && (bool)autoItem["IncludeFine"];
            var allowWarnScripts = autoItem.ContainsParameter("AllowWarnScript") && (bool)autoItem["AllowWarnScript"];

            var creator = new SuperAutoCreator(scene, markManager, eventManager, scriptManager, startTime,
                endTime, itemType, evaPoint, includeFine)
            {
                AllowAllButtons = allowAllButtons,
                AllowWarnScripts = allowWarnScripts
            };
            if (creator.CanCreate())
            {
                creator.Progressed += creator_Progressed;
                var decorder = creator.Create();
                if (isFreePass || WebManager.Instance.UseItem(autoItem))
                {
                    recording = false;
                    playDecorder = decorder;
                    playRecorder = null;
                    superAuto = true;
                }
                return true;
            }

            return false;
        }

        private void ResultReplay()
        {
            if (gameUtility.ReplayResultId <= 0)
            {
                return;
            }

            var data = WebManager.Instance.GetReplayData(gameUtility.ReplayResultId);
            if (data == null || data.Length < 1024)
            {
                throw new Exception("Failed to get replay data.\n" + Encoding.UTF8.GetString(data));
            }
            recording = false;
            playDecorder = PPDPlayDecorder.FromBytes(data);
            playRecorder = null;
        }

        private void LoadMod()
        {
            if (gameUtility.AppliedMods == null)
            {
                return;
            }

            foreach (ModInfo modInfo in gameUtility.AppliedMods)
            {
                if (!modInfo.CanApply() || !modInfo.NotModified() || !gameUtility.CanApplyMod(modInfo))
                {
                    continue;
                }
                var tempResourceManager = new ScriptResourceManager(device, sound, modInfo.ModPath, Path.Combine("mods", String.Join("", modInfo.UniqueName.Split(Path.GetInvalidPathChars()))));
                using (PackReader reader = new PackReader(modInfo.ModPath))
                {
                    foreach (string content in reader.FileList)
                    {
                        using (PPDPackStreamReader ppdpsr = reader.Read(content))
                        {
                            if (content.StartsWith("Scripts"))
                            {
                                scriptManager.LoadScript(ppdpsr, tempResourceManager, modInfo);
                            }
                        }
                    }
                }
            }
        }

        public void Play()
        {
            if (startTime < 0)
            {
                waitingMovieStart = true;
                lastTime = Win32API.timeGetTime();
            }
            else if (movie != null && movie.Initialized)
            {
                movie.Play();
            }
            state = State.Playing;
        }

        protected void OnPause()
        {
            if (Paused != null)
            {
                Paused.Invoke(this, EventArgs.Empty);
            }
        }

        protected void OnFinish()
        {
            finishCalled = true;
            if (Finished != null)
            {
                Finished.Invoke(this, EventArgs.Empty);
            }
        }

        public void SaveLatency()
        {
            latencyUpdater.SaveLatency();
        }

        float pd_LatencyChanged(int sign)
        {
            if (latencyFixer != null)
            {
                latencyFixer.Enabled = false;
            }
            int lastCount = latencyUpdater.LatencyCount;
            latencyUpdater.LatencyCount += sign;
            if (lastCount != latencyUpdater.LatencyCount)
            {
                latencyUpdater.SaveLatency();
            }
            return latencyUpdater.Latency;
        }

        public bool Mistake
        {
            get
            {
                return mistake;
            }
        }

        public GameResultManager GameResultManager
        {
            get
            {
                return scriptManager.GameResultManager;
            }
        }

        public void ShowResult()
        {
            ProcessRestRecord();
            movie.Stop();
            if (gameResult != null)
            {
                gameResult.SetResult(GameResultManager, mistake ? ResultEvaluateType.Mistake : ResultEvaluateType.Great,
                    (float)movie.MoviePosition, gameUtility, playRecorder?.GetBytes(), Replaying, perfectTrialToken);
                state = State.Result;
            }
            VirtualizeReleaseInput();
            if (latencyFixer != null)
            {
                latencyUpdater.SaveLatency();
            }
            if (!Replaying && playDecorder == null)
            {
                scoreStorage.Save();
            }

            allowInput = true;
        }

        private void VirtualizeReleaseInput()
        {
            // 仮想的に全てのキーがリリースされたとする
            if (playRecorder != null)
            {
                int[] presscount = new int[10];
                bool[] pressed = new bool[10];
                bool[] released = new bool[10];
                for (int i = 0; i < presscount.Length; i++)
                {
                    presscount[i] = 0;
                    pressed[i] = false;
                    released[i] = true;
                }
                playRecorder.Update(currentMovieTime - PPDSetting.Setting.AdjustGapTime, currentTime, presscount, pressed, released);
            }
        }

        void pd_Returned(object sender, EventArgs e)
        {
            pauseMenu.Hidden = true;
        }

        void pd_Retryed(object sender, EventArgs e)
        {
            pauseMenu.Hidden = true;
        }

        void pauseMenu_Replayed(object sender, EventArgs e)
        {
            pauseMenu.Hidden = true;
        }

        private void StartRecord()
        {
            recording = true;
            playRecorder = new PPDPlayRecorder();
            playDecorder = null;
        }

        private void StartReplay()
        {
            if (recording)
            {
                recording = false;
                playDecorder = PPDPlayDecorder.FromBytes(playRecorder.GetBytes());
                playRecorder = null;
            }
            else
            {
                if (playDecorder != null)
                {
                    playDecorder.Reset();
                }
            }
        }

        void pd_Resumed(object sender, EventArgs e)
        {
            state = State.Playing;
            if (PauseMovieWhenPause)
            {
                if (waitingMovieStart)
                {
                    var temptime = Win32API.timeGetTime();
                    lastTime = (long)((-tempLastTime + startTime) * 1000 + temptime);
                }
                else
                {
                    movie.Play();
                }
            }
            scriptManager.PauseManager.Resume();
        }

        private void ReadProfile()
        {
            evaPoint[0] = gameUtility.Profile.CoolPoint;
            evaPoint[1] = gameUtility.Profile.GoodPoint;
            evaPoint[2] = gameUtility.Profile.SafePoint;
            evaPoint[3] = gameUtility.Profile.SadPoint;
            evaPoint[4] = gameUtility.Profile.WorstPoint;
            gameUtility.GodMode |= gameUtility.Profile.GodMode;
        }
        private void ReadMovieSetting()
        {
            endTime = gameUtility.SongInformation.EndTime;
            if (endTime >= movie.Length)
            {
                endTime = (float)movie.Length;
            }
            movie.Seek(startTime);
            movie.TrimmingData = gameUtility.SongInformation.TrimmingData;
            movie.Finished += m_Finished;
        }

        void m_Finished(object sender, EventArgs e)
        {
            movieFinishCalled = true;
        }

        void gameResult_Replayed(object sender, EventArgs e)
        {
            Replay();
        }

        public void Retry()
        {
            if (!superAuto)
            {
                StartRecord();
            }
            else
            {
                StartReplay();
            }
            Reset();
        }

        public void Replay()
        {
            StartReplay();
            Reset();
        }

        private void Reset()
        {
            allowInput = true;
            movieFinishCalled = finishCalled = mistake = false;
            if (movie != null)
            {
                if (startTime < 0)
                {
                    movie.Seek(0);
                }
                else
                {
                    movie.Seek(startTime);
                }
                movie.Play();
                movie.Pause();
                if (startTime < 0)
                {
                    movie.Seek(0);
                }
                else
                {
                    movie.Seek(startTime);
                }
                movie.SetDefaultVisible();
            }
            movieTimes.Clear();
            //seek
            markManager.Seek(markStartTime);
            eventManager.Seek(startTime > 0 ? startTime : 0);
            soundManager.Seek(startTime);
            kasiManager.Seek(startTime);
            System.Threading.Thread.Sleep(1000);
            currentUpdateTime = startTime;
            state = State.Playing;
            combo.Retry();
            scoreStorage.Reload();
            if (gameInterface != null)
            {
                gameInterface.Retry();
            }
            effectManager.Retry();
            if (gameResult != null)
            {
                gameResult.Retry();
            }
            if (pauseMenu != null)
            {
                pauseMenu.Retry(Replaying);
            }
            GameResultManager.Retry(Replaying);
            holdManager.Retry();
            slideManager.Retry();
            scriptManager.Retry();
            eventManager.Update(startTime);
            scriptManager.UpdateManager.MovieTime = startTime;
            scriptManager.UpdateManager.Update();
            scriptManager.Update(dontUpdateExtraProcess);
            if (latencyFixer != null)
            {
                latencyFixer = new LatencyFixer(latencyUpdater.BaseLantencyCount + latencyUpdater.LatencyCount);
            }
            if (perfectTrialing)
            {
                PerfectTrial();
            }
            if (movie != null)
            {
                if (startTime < 0)
                {
                    waitingMovieStart = true;
                    lastTime = Win32API.timeGetTime();
                }
                else
                {
                    movie.Play();
                }
            }
        }

        public void MovieFadeOut()
        {
            if (movie != null)
            {
                movie.FadeOut();
            }
        }

        public void MovieStop()
        {
            if (movie != null)
            {
                movie.Stop();
            }
        }

        private bool CheckFinish(float movieTime)
        {
            if (endTime <= movieTime)
            {
                return true;
            }

            if (state == State.Playing)
            {
                movieTimes.Add(movieTime);
                while (movieTimes.Count > maxMovieTimesCount)
                {
                    movieTimes.RemoveAt(0);
                }
                if (movieTimes.Count == maxMovieTimesCount)
                {
                    float top = movieTimes[0];
                    if (movieTimes.TrueForAll(val => val == top) && top > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ProcessRestRecord()
        {
            if (playDecorder != null)
            {
                int[] presscount = new int[10];
                bool[] pressed = new bool[10];
                bool[] released = new bool[10];
                while (playDecorder.Update(10000000, presscount, pressed, released, out float newTime, out long newCurrentTime))
                {
                    currentTime = newCurrentTime;
                    InputInfoBase customInputInfo = new CustomInputInfo(presscount, pressed, released, EmptyInputInfo.Instance);
                    UpdateImpl(newTime + PPDSetting.Setting.AdjustGapTime, customInputInfo);
                }
            }
        }

        private void PreUpdate(float time, InputInfoBase inputInfo, Action<int> progressCallback = null)
        {
            currentMovieTime = time;
            currentTime = Win32API.timeGetTime();

            if (playDecorder != null)
            {
                float newTime;
                long newCurrentTime;
                int[] presscount = new int[10];
                bool[] pressed = new bool[10];
                bool[] released = new bool[10];
                if (state == State.Stop)
                {
                    UpdateImpl(time, inputInfo);
                }
                while (playDecorder.Update(time, presscount, pressed, released, out newTime, out newCurrentTime))
                {
                    var customInputInfo = new CustomInputInfo(presscount, pressed, released, inputInfo);
                    currentTime = newCurrentTime;
                    UpdateImpl(newTime + PPDSetting.Setting.AdjustGapTime, customInputInfo);
                    progressCallback?.Invoke(playDecorder.Position * 100 / playDecorder.Length);
                }
                if (playDecorder.IsEnd)
                {
                    UpdateImpl(time, inputInfo);
                }
                else if (CheckFinish((float)movie.MoviePosition) || movieFinishCalled)
                {
                    playDecorder.Update(100000000000000000, presscount, pressed, released, out newTime, out newCurrentTime);
                    var customInputInfo = new CustomInputInfo(presscount, pressed, released, inputInfo);
                    currentTime = newCurrentTime;
                    UpdateImpl(newTime + PPDSetting.Setting.AdjustGapTime, customInputInfo);
                }
            }
            else
            {
                UpdateImpl(time, inputInfo);
            }
        }

        private void UpdateImpl(float time, InputInfoBase inputInfo)
        {
            currentTime = (long)(currentTime * playRatio);
            currentUpdateTime = time;
            scriptManager.UpdateManager.MovieTime = time;
            if (state == State.Playing && playDecorder == null)
            {
                inputInfo = new DisabledInputInfo(inputInfo, scriptManager.InputManager);
            }
            if (movie != null)
            {
                movie.Update();
            }

            bool[] b = new bool[10];
            bool[] released = new bool[10];
            for (int k = 0; k < 10; k++)
            {
                b[k] = inputInfo.IsPressed((ButtonType)k);
                released[k] = inputInfo.IsReleased((ButtonType)k);
                if (!allowInput)
                {
                    b[k] = false;
                }
                else if (state == State.Playing)
                {
                    if (b[k])
                    {
                        scriptManager.Call("PPD.Input", new object[] { (MarkType)k, inputInfo.GetPressingFrame((ButtonType)k), b[k], false });
                    }

                    if (released[k])
                    {
                        scriptManager.Call("PPD.Input", new object[] { (MarkType)k, 0, false, true });
                    }
                }
            }
            if (inputInfo.IsPressed(ButtonType.Start) && pauseMenu != null)
            {
                if (state < State.Result)
                {
                    if (state == State.Playing)
                    {
                        state = State.Stop;
                        if (PauseMovieWhenPause)
                        {
                            if (waitingMovieStart)
                            {
                                tempLastTime = time;
                            }
                            movie.Pause();
                            CancelHold(time, b, released);
                        }
                        pauseMenu.Hidden = false;
                        scriptManager.PauseManager.Pause();
                        OnPause();
                        return;
                    }
                }
            }

            switch (state)
            {
                case State.Playing:
                    if (gameHost.IsCloseRequired && pauseMenu != null)
                    {
                        state = State.Stop;
                        if (PauseMovieWhenPause)
                        {
                            if (waitingMovieStart)
                            {
                                tempLastTime = time;
                            }
                            movie.Pause();
                            CancelHold(time, b, released);
                        }
                        scriptManager.PauseManager.Pause();
                        OnPause();
                        pauseMenu.Hidden = false;
                    }
                    else
                    {
                        int[] pressCount = new int[10];
                        for (int i = 0; i < 10; i++)
                        {
                            pressCount[i] = inputInfo.GetPressingFrame((ButtonType)i);
                        }
                        if (playRecorder != null)
                        {
                            playRecorder.Update(time - PPDSetting.Setting.AdjustGapTime, currentTime, pressCount, b, released);
                        }
                        int currentScore = GameResultManager.CurrentScore;
                        int currentLife = GameResultManager.CurrentLife;
                        bool[] tempb = new bool[b.Length];
                        for (int i = 0; i < b.Length; i++)
                        {
                            if (b[i])
                            {
                                var index = randomChangeManager.Invert(i);
                                tempb[index] = true;
                            }
                        }
                        scriptManager.UpdateManager.Update();
                        eventManager.Update(time);
                        kasiManager.Update(time);
                        if (!gameUtility.Profile.MuteSE && !gameUtility.MuteSE)
                        {
                            soundManager.Update(time, tempb, released, playRatio);
                        }
                        //Utility.Start();
                        markManager.Update(time, b, released);
                        //Utility.Stop("MarkManager Update");
                        holdManager.Update(currentTime);
                        if (!dontUpdateExtraProcess)
                        {
                            effectManager.Update();
                            slideManager.Update();
                        }
                        if (currentScore != GameResultManager.CurrentScore)
                        {
                            if (ScoreChanged != null)
                            {
                                ScoreChanged.Invoke(GameResultManager.CurrentScore);
                            }
                            if (gameInterface != null)
                            {
                                gameInterface.ChangeScore(GameResultManager.CurrentScore);
                            }
                        }
                        if (currentLife != GameResultManager.CurrentLife)
                        {
                            if (LifeChanged != null)
                            {
                                LifeChanged.Invoke(GameResultManager.CurrentLife);
                            }
                        }
                        if (!dontUpdateExtraProcess)
                        {
                            hiddenSprite.Update();
                            if (gameInterface != null)
                            {
                                gameInterface.ChangeMoviePosition((float)((time - startTime) / (endTime - startTime) * GameInterfaceBase.MaxMoviePosition));
                                gameInterface.Update();
                            }
                            combo.Update();
                        }

                        if (GameResultManager.IfDeath && !gameUtility.GodMode && !finishCalled)
                        {
                            if (allowInput)
                            {
                                scriptManager.Call("Died", null);
                                allowInput = false;
                            }
                            mistake = true;
                            if (!GameResultManager.SuspendFinish)
                            {
                                OnFinish();
                            }
                        }
                        scriptManager.Update(dontUpdateExtraProcess);
                        if (latencyFixer != null && latencyFixer.Enabled)
                        {
                            var newLatency = latencyFixer.GetNewLatencyCount();
                            if ((latencyUpdater.LatencyCount + latencyUpdater.BaseLantencyCount) != newLatency)
                            {
                                latencyUpdater.LatencyCount = newLatency - latencyUpdater.BaseLantencyCount;
                            }
                        }
                    }

                    break;
                case State.Result:
                    gameResult.Update(inputInfo);
                    break;
                case State.Stop:
                    pauseMenu.Update(inputInfo);
                    break;
            }
        }

        public void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            var time = (float)movie.MoviePosition;
            time += PPDSetting.Setting.MovieLatency + aprilFoolMovieLatency + latencyUpdater.Latency;
            if (waitingMovieStart && state == State.Playing)
            {
                var deftime = Win32API.timeGetTime();
                /*if ((deftime - lasttime) / (double)1000 >= Math.Abs(ppdgameutil.StartTime + util.startgap) && !movie.Playing)
                {
                    movie.Play();
                }*/
                if ((deftime - lastTime) / (double)1000 >= Math.Abs(startTime))
                {
                    waitingMovieStart = false;
                    movie.Play();
                }
                else
                {
                    time = (float)((deftime - lastTime) / (double)1000 + startTime);
                    time += PPDSetting.Setting.MovieLatency + aprilFoolMovieLatency + latencyUpdater.Latency;
                }
            }

            if (inputInfo.Accurate && inputInfo.Actions.Length > 0)
            {
                foreach (InputActionBase action in inputInfo.Actions)
                {
                    var singleInputInfo = new SingleInputInfo(action.ButtonType,
                        action.IsPressed, inputInfo.GetPressingFrame(action.ButtonType));
                    PreUpdate((float)action.GetAccurateTime(time), singleInputInfo);
                }
            }
            else
            {
                PreUpdate(time, inputInfo);
            }

            bool finished = CheckFinish((float)movie.MoviePosition) || movieFinishCalled;
            if (finished && !mistake && !finishCalled && !(holdManager.IsHolding && !holdManager.IsMaxHolding))
            {
                OnFinish();
                mistake = false;
            }

            base.Update();
        }

        protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
        {
            if (state < State.Result)
            {
                hiddenSprite.Draw();
                if (!waitingMovieStart)
                {
                    movie.Draw();
                }
                scriptManager.StageManager.Draw(LayerType.AfterMovie);
                OnDrawTiming(LayerType.AfterMovie);
                holdManager.Draw();
                effectManager.Draw();
                slideManager.Draw();
                scriptManager.StageManager.Draw(LayerType.AfterMarkEffect);
                OnDrawTiming(LayerType.AfterMarkEffect);
                markManager.Draw();
                scriptManager.StageManager.Draw(LayerType.AfterMark);
                OnDrawTiming(LayerType.AfterMark);
                if (gameInterface != null)
                {
                    gameInterface.Draw();
                }
                scriptManager.StageManager.Draw(LayerType.AfterInterface);
                combo.Draw();
                OnDrawTiming(LayerType.AfterInterface);
            }
            else
            {
                gameResult.Draw();
            }
            if (state == State.Stop)
            {
                pauseMenu.Draw();
                scriptManager.StageManager.Draw(LayerType.AfterPauseMenu);
                OnDrawTiming(LayerType.AfterPauseMenu);
            }
        }

        private void CancelHold(float time, bool[] pressed, bool[] released)
        {
            bool[] newPressed = new bool[pressed.Length];
            bool[] newReleased = new bool[released.Length];
            for (int i = 0; i < newReleased.Length; i++)
            {
                newReleased[i] = true;
            }

            markManager.Update(time, newPressed, newReleased);
        }

        public void SpecialPlaySound(int index, bool keep)
        {
            if (gameUtility.Profile.MuteSE || gameUtility.MuteSE) return;
            if (keep) soundManager.SpKeepPlay(index, eventManager.GetVolPercent(index), eventManager.KeepPlayings, playRatio);
            else soundManager.SpPlay(index, eventManager.GetVolPercent(index), playRatio);
        }

        public void SpecialStopSound(int index, bool keep)
        {
            if (gameUtility.Profile.MuteSE) return;
            soundManager.SpStopSound(index);
        }

        void km_KasiChanged(string kasi)
        {
            if (gameInterface != null)
            {
                gameInterface.ChangeKasi(kasi);
            }
        }

        void mm_ChangeCombo(bool gain, Vector2 pos)
        {
            if (gain)
            {
                GameResultManager.CurrentCombo += (int)config.ComboScale;
            }
            else
            {
                GameResultManager.CurrentCombo = 0;
            }
            if (!dontUpdateExtraProcess)
            {
                combo.ChangeCombo(GameResultManager.CurrentCombo, pos);
            }
            if (gameInterface != null)
            {
                gameInterface.ChangeCombo(pos, GameResultManager.CurrentCombo);
            }
            OnComboChanged(GameResultManager.CurrentCombo, pos);
        }

        private void EvaluateCount(EffectType eval, bool isMissPress)
        {
            var markEvaluate = ConvertEffectTypeToMarkEvaluate(eval);
            GameResultManager.CurrentLife += isMissPress ? evaPoint[4] * (1 + (int)markEvaluate) / 8 : evaPoint[(int)markEvaluate];
            GameResultManager.GainScore(markEvaluate, isMissPress, config.ScoreScale);
            if (allowInput)
            {
                if (!isMissPress)
                {
                    GameResultManager.GainEvaluate(markEvaluate, 1);
                }
                else
                {
                    GameResultManager.GainEvaluate(MarkEvaluateType.Worst, 1);
                }
            }
            var ratio = ((float)(GameResultManager.CoolCount + GameResultManager.GoodCount) / GameResultManager.EvaluateSum - 0.7f) / 0.3f;
            if (ratio <= 0) ratio = 0;
            if (ratio >= 1) ratio = 1;
            if (gameInterface != null)
            {
                gameInterface.ChangeEvaluate(markEvaluate, isMissPress, GameResultManager.CurrentLifeAsFloat, ratio, GameResultManager.ResultEvaluateType);
            }
            if (!dontUpdateExtraProcess)
            {
                combo.ChangeState(markEvaluate, isMissPress);
            }
            OnEvaluateChanged(markEvaluate, isMissPress);
        }

        void markManager_PreEvaluate(IMarkInfo markInfo, EffectType effectType, bool missPress, bool release, Vector2 position)
        {
            if (latencyFixer == null)
            {
                return;
            }

            if (!release && (effectType == EffectType.Cool || effectType == EffectType.Fine || effectType == EffectType.Safe || effectType == EffectType.Sad))
            {
                latencyFixer.Add(currentMovieTime - latencyUpdater.Latency - PPDSetting.Setting.MovieLatency
                    - aprilFoolMovieLatency - PPDSetting.Setting.AdjustGapTime - markInfo.Time);
            }
        }

        private MarkEvaluateType ConvertEffectTypeToMarkEvaluate(EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.Cool:
                    return MarkEvaluateType.Cool;
                case EffectType.Fine:
                    return MarkEvaluateType.Fine;
                case EffectType.Safe:
                    return MarkEvaluateType.Safe;
                case EffectType.Sad:
                    return MarkEvaluateType.Sad;
                case EffectType.Worst:
                    return MarkEvaluateType.Worst;
            }
            return MarkEvaluateType.Worst;
        }

        private void OnEvaluateChanged(MarkEvaluateType evaType, bool missPress)
        {
            if (EvaluateChanged != null)
            {
                EvaluateChanged.Invoke(evaType, missPress);
            }
        }

        private void OnComboChanged(int combo, Vector2 pos)
        {
            if (ComboChanged != null)
            {
                ComboChanged.Invoke(combo, pos);
            }
        }

        bool mm_PressingButton(ButtonType buttonType, bool pressing)
        {
            return holdManager.SetPressing(buttonType, pressing, currentTime);
        }

        void hm_ScoreGained(int gain)
        {
            GameResultManager.AddHoldBonus((int)(gain * config.ScoreScale));
        }

        void markManager_Slide(object sender, Vector2 position, int score, bool isRight)
        {
            GameResultManager.AddSlideBonus((int)(score * config.ScoreScale));
            slideManager.AddSlidePoint(sender, position, score, isRight);
        }

        void markManager_MaxSlide(object sender, Vector2 position, int score, bool isRight)
        {
            GameResultManager.AddSlideBonus((int)(score * config.ScoreScale));
            slideManager.AddMaxSlideEffect(sender, position, score);
        }

        public void ChangeMovieVolume(int volume)
        {
            if (movie != null && movie.Initialized)
            {
                movie.Volume = volume;
            }
        }

        protected void OnDrawTiming(LayerType layerType)
        {
            if (Drawed != null)
            {
                Drawed.Invoke(layerType);
            }
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (movie != null)
            {
                movie.Dispose();
            }
            if (soundManager != null)
            {
                soundManager.Dispose();
            }
            if (gameInterface != null)
            {
                gameInterface.Dispose();
            }
            if (eventManager != null)
            {
                eventManager.Dispose();
            }
            if (markManager != null)
            {
                markManager.Dispose();
            }
            if (pauseMenu != null)
            {
                pauseMenu.Dispose();
            }
            if (effectManager != null)
            {
                effectManager.Dispose();
            }
            if (gameResult != null)
            {
                gameResult.Dispose();
            }
            if (holdManager != null)
            {
                holdManager.Dispose();
            }
            if (scriptManager != null)
            {
                scriptManager.Dispose();
            }
            if (combo != null)
            {
                combo.Dispose();
            }
            if (slideManager != null)
            {
                slideManager.Dispose();
            }
        }
    }
}
