using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.Logger;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.PPDData;
using PPDFramework.Shaders;
using PPDFrameworkCore;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PPDCore
{
    public class MarkManager : GameComponent, IMarkManager
    {
        public delegate void ChangeComboHandler(bool gain, Vector2 pos);
        public event ChangeComboHandler ChangeCombo;
        public delegate void SpecialSoundHandler(int index, bool keep);
        public event SpecialSoundHandler PlaySound;
        public event SpecialSoundHandler StopSound;
        public delegate void EvaluateCountHandler(EffectType effectType, bool isMissPress);
        public event EvaluateCountHandler EvaluateCount;
        public delegate bool PressingButtonHandler(ButtonType buttonType, bool pressing);
        public event PressingButtonHandler PressingButton;
        public delegate void SlideHandler(object sender, Vector2 position, int score, bool isRight);
        public event SlideHandler Slide;
        public event SlideHandler MaxSlide;
        public delegate void PreEvaludateHandler(PPDCoreModel.IMarkInfo markInfo, EffectType effectType, bool missPress, bool release, Vector2 position);
        public event PreEvaludateHandler PreEvaluate;

        public delegate void EvaluateEffectHandler(EffectType type, Vector2 pos);

        const float defaultbpm = 130;

        int simtype = 1;
        List<int> simpiclist = new List<int>();

        string[] effectfilenames = new string[(int)EffectType.MaxSlide + 1];
        MarkDataBase[] ppdat;
        int iter;
        float sametimeing = 0.01f;
        HashSet<Mark> removeMarks = new HashSet<Mark>();
        Dictionary<float, SameTimingMarks> markgroups = new Dictionary<float, SameTimingMarks>();
        bool initialized;

        PPDGameUtility gameutility;
        Vector2[] circlepoints;
        RandomChangeManager randomChangeManager;
        float[] evals;
        float adjustgaptime;
        PPDEffectManager ppdem;
        FlowScriptManager scriptManager;
        MarkImagePathsBase imagepathes;
        MainGameConfigBase config;

        ExMarkBase[] ACPressing = new ExMarkBase[10];
        PPDFramework.Resource.ResourceManager resourceManager;
        MarkConnectionCommon markConnectionCommon;
        List<MarkData> externalMarks;

        int allMarkCount;
        int allLongMarkCount;
        int[] markCounts = new int[10];

        int currentAllMarkCount;
        int currentAllLongMarkCount;
        int[] currentMarkCounts = new int[10];
        PostDrawManager postDrawManager;

        public SpriteObject MarkLayer
        {
            get;
            private set;
        }

        public SpriteObject ConnectLayer
        {
            get;
            private set;
        }

        public EventManager EventManager
        {
            get;
            set;
        }

        public int AllMarkCount
        {
            get
            {
                return currentAllMarkCount;
            }
        }

        public int AllLongMarkCount
        {
            get
            {
                return currentAllLongMarkCount;
            }
        }

        public MarkDataBase[] Marks
        {
            get
            {
                return ppdat;
            }
        }

        public ScoreDifficultyMeasureResult ScoreDifficultyMeasureResult
        {
            get
            {
                return ScoreDifficultyMeasure.Measure(ppdat);
            }
        }

        public MarkManager(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PPDGameUtility gameutility, RandomChangeManager randomChangeManager, PPDEffectManager ppdem, MarkImagePathsBase imagepathes, MainGameConfigBase config, FlowScriptManager scriptManager) : base(device)
        {
            InnerStruct(gameutility, resourceManager, randomChangeManager, ppdem, imagepathes, config, scriptManager);
            ReadPpdData(gameutility.SongInformation.DirectoryPath, gameutility.Difficulty);
            CheckGroups();
            InitializeRandomChange();
        }

        public MarkManager(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PPDGameUtility gameutility, RandomChangeManager randomChangeManager, PPDEffectManager ppdem, MarkImagePathsBase imagepathes, MainGameConfigBase config, FlowScriptManager scriptManager, Stream stream) : base(device)
        {
            InnerStruct(gameutility, resourceManager, randomChangeManager, ppdem, imagepathes, config, scriptManager);
            ReadPpdData(stream);
            CheckGroups();
            InitializeRandomChange();
        }

        private void InnerStruct(PPDGameUtility gameutility, PPDFramework.Resource.ResourceManager resourceManager, RandomChangeManager randomChangeManager, PPDEffectManager ppdem, MarkImagePathsBase imagepathes, MainGameConfigBase config, FlowScriptManager scriptManager)
        {
            this.gameutility = gameutility;
            this.randomChangeManager = randomChangeManager;
            this.ppdem = ppdem;
            this.imagepathes = imagepathes;
            this.resourceManager = resourceManager;
            this.config = config;
            this.scriptManager = scriptManager;
            externalMarks = new List<MarkData>();

            evals = new float[] { PPDSetting.Setting.CoolArea, PPDSetting.Setting.GoodArea, PPDSetting.Setting.SafeArea, PPDSetting.Setting.SadArea };
            Logger.Instance.AddLog("CoolArea:{0}", PPDSetting.Setting.CoolArea);
            Logger.Instance.AddLog("GoodArea:{0}", PPDSetting.Setting.GoodArea);
            Logger.Instance.AddLog("SafeArea:{0}", PPDSetting.Setting.SafeArea);
            Logger.Instance.AddLog("SadArea:{0}", PPDSetting.Setting.SadArea);
            adjustgaptime = PPDSetting.Setting.AdjustGapTime;
            CreateResource();

            MarkLayer = new SpriteObject(device);
            ConnectLayer = new SpriteObject(device);
            postDrawManager = new PostDrawManager(device);
            markConnectionCommon = new MarkConnectionCommon(device, resourceManager);
            this.AddChild(markConnectionCommon);
            if (PPDSetting.Setting.DrawConnectUnderAllMark)
            {
                this.AddChild(postDrawManager);
                this.AddChild(MarkLayer);
                this.AddChild(ConnectLayer);
            }
            else
            {
                this.AddChild(postDrawManager);
                this.AddChild(ConnectLayer);
                this.AddChild(MarkLayer);
            }
            ConnectLayer.CanDraw += (obj, context, depth, childIndex) =>
            {
                return gameutility.Connect || gameutility.Profile.Connect;
            };
        }

        private void CreateResource()
        {
            var pathes = new List<string>(30);
            var checktracelist = new List<string>(10);
            for (ButtonType type = ButtonType.Square; type < ButtonType.Start; type++)
            {
                pathes.Add(imagepathes.GetMarkImagePath(type));
                pathes.Add(imagepathes.GetMarkColorImagePath(type));
                string tracepath = imagepathes.GetTraceImagePath(type);
                pathes.Add(tracepath);
                if (!checktracelist.Contains(tracepath))
                {
                    checktracelist.Add(tracepath);
                    simpiclist.Add((int)type + 1);
                }
            }
            pathes.Add(imagepathes.GetCircleAxisImagePath());
            pathes.Add(imagepathes.GetClockAxisImagePath());
            imagepathes.GetLongNoteCircleInfo(out PathObject fn, out float inner, out float outer);
            pathes.Add(fn);
            imagepathes.GetHoldInfo(out fn, out float x, out float y);
            pathes.Add(fn);
            CreateCirclePoints(inner, outer);

            for (MarkEvaluateType type = MarkEvaluateType.Cool; type <= MarkEvaluateType.Worst; type++)
            {
                var effectfilename = imagepathes.GetEvaluateEffectPath(type);
                if (File.Exists(effectfilename))
                {
                    ppdem.CreateEffect(effectfilename);
                    effectfilenames[5 - (int)type] = effectfilename;
                }
            }
            var appearname = imagepathes.GetAppearEffectPath();
            if (File.Exists(appearname))
            {
                ppdem.CreateEffect(appearname);
                effectfilenames[(int)EffectType.Appear] = appearname;
            }
            var slidername = imagepathes.GetSlideEffectPath();
            if (File.Exists(slidername))
            {
                ppdem.CreateEffect(slidername);
                effectfilenames[(int)EffectType.Slide] = slidername;
            }
        }

        private void CreateCirclePoints(float smallrad, float bigrad)
        {
            int num = 361;
            circlepoints = new Vector2[num * 2];
            for (int i = 0; i < num; i++)
            {
                circlepoints[i * 2].X = (float)(smallrad * (Math.Cos(Math.PI * i / (num - 1) * 2 - Math.PI / 2)));
                circlepoints[i * 2].Y = (float)(smallrad * (Math.Sin(Math.PI * i / (num - 1) * 2 - Math.PI / 2)));
                circlepoints[i * 2 + 1].X = (float)(bigrad * (Math.Cos(Math.PI * i / (num - 1) * 2 - Math.PI / 2)));
                circlepoints[i * 2 + 1].Y = (float)(bigrad * (Math.Sin(Math.PI * i / (num - 1) * 2 - Math.PI / 2)));
            }
        }

        private void InitializeRandomChange()
        {
            if (gameutility.Random)
            {
                bool hasLR = false, hasDirection = false, hasSymbol = false;
                foreach (MarkDataBase ppdData in ppdat)
                {
                    hasLR |= ppdData.ButtonType == ButtonType.L || ppdData.ButtonType == ButtonType.R;
                    hasDirection |= ppdData.ButtonType == ButtonType.Left || ppdData.ButtonType == ButtonType.Down
                        || ppdData.ButtonType == ButtonType.Right || ppdData.ButtonType == ButtonType.Up;
                    hasSymbol = ppdData.ButtonType == ButtonType.Square || ppdData.ButtonType == ButtonType.Cross
                        || ppdData.ButtonType == ButtonType.Circle || ppdData.ButtonType == ButtonType.Triangle;
                }
                RandomChangeManager.RandomType randomType = RandomChangeManager.RandomType.None;
                if (hasSymbol)
                {
                    randomType |= RandomChangeManager.RandomType.Symbol;
                }
                if (hasDirection)
                {
                    randomType |= RandomChangeManager.RandomType.Direction;
                }
                if (hasLR)
                {
                    randomType |= RandomChangeManager.RandomType.LR;
                }
                randomChangeManager.Initialize(randomType);
            }
        }

        public void AddMark(Vector2 position, float angle, float time, MarkType markType, int id)
        {
            externalMarks.Add(new MarkData(position.X, position.Y, angle, time, (ButtonType)markType, (uint)id));
        }

        public void AddLongMark(Vector2 position, float angle, float time, float endTime, MarkType markType, int id)
        {
            externalMarks.Add(new ExMarkData(position.X, position.Y, angle, time, endTime, (ButtonType)markType, (uint)id));
        }

        public int GetMarkCount(int sameNum)
        {
            if (sameNum >= 0 && sameNum < markCounts.Length)
            {
                return currentMarkCounts[sameNum];
            }
            return 0;
        }

        private void ReadPpdData(string dir, Difficulty difficulty)
        {
            string filename = DifficultyUtility.ConvertDifficulty(difficulty) + ".ppd";
            var path = Path.Combine(dir, filename);
            if (File.Exists(path))
            {
                ppdat = PPDReader.Read(path);
                initialized = true;
            }
            else
            {
                initialized = false;
            }
        }

        private void ReadPpdData(Stream stream)
        {
            try
            {
                ppdat = PPDReader.Read(stream);
                initialized = true;
            }
            catch
            {
                initialized = false;
            }
        }

        private Dictionary<float, SameTimingMarks> GetGroups(IEnumerable<MarkDataBase> list)
        {
            float lastTime = -1;
            var group = new SameTimingMarks();
            var ret = new Dictionary<float, SameTimingMarks>();
            foreach (MarkDataBase ipdata in list)
            {
                if (ipdata.Time >= lastTime + sametimeing)
                {
                    if (group.Count > 1)
                    {
                        ret.Add(lastTime, group);
                        group = new SameTimingMarks();
                    }
                    else
                    {
                        group.Clear();
                    }
                    group.Add(ipdata);
                    lastTime = ipdata.Time;
                }
                else
                {
                    group.Add(ipdata);
                }
            }
            if (group.Count > 1)
            {
                ret.Add(lastTime, group);
            }

            return ret;
        }

        private void CheckGroups()
        {
            if (initialized)
            {
                markgroups = GetGroups(ppdat);
                allMarkCount = ppdat.Length;
                allLongMarkCount = ppdat.OfType<ExMarkData>().Count();
                foreach (SameTimingMarks marks in markgroups.Values)
                {
                    markCounts[marks.Count - 1]++;
                }
                markCounts[0] = allMarkCount;
                for (int i = 1; i < markCounts.Length; i++)
                {
                    markCounts[0] -= markCounts[i] * (i + 1);
                }
                currentAllMarkCount = allMarkCount;
                currentAllLongMarkCount = allLongMarkCount;
                Array.Copy(markCounts, currentMarkCounts, markCounts.Length);
            }
        }

        private void ReadPpdData(float mainTime, float bpm)
        {
            float scale = (float)bpm / defaultbpm;
            float targetTimeDiff = 3 / scale;
            while (iter < ppdat.Length)
            {
                if (ppdat[iter].Time - mainTime < targetTimeDiff)
                {
                    if (!markgroups.TryGetValue(ppdat[iter].Time, out SameTimingMarks group))
                    {
                        CreateMark(ppdat[iter]);
                        iter++;
                    }
                    else
                    {
                        ProcessGroup(group, mainTime);
                        iter += group.Count;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void ReadExternalPpdData(float mainTime)
        {
            if (externalMarks.Count == 0)
            {
                return;
            }
#if DEBUG
            Console.WriteLine("ExternalCount: {0}", externalMarks.Count);
#endif
            externalMarks.Sort((mk1, mk2) =>
            {
                if (mk1.Time == mk2.Time)
                {
                    return mk1.ButtonType - mk2.ButtonType;
                }
                else
                {
                    return Math.Sign(mk1.Time - mk2.Time);
                }
            });

            currentAllMarkCount += externalMarks.Count;
            currentAllLongMarkCount += externalMarks.OfType<ExMarkData>().Count();
            var markGroups = GetGroups(externalMarks);
            int[] tempCounts = new int[10];
            foreach (SameTimingMarks list in markGroups.Values)
            {
                tempCounts[list.Count - 1]++;
            }
            tempCounts[0] += externalMarks.Count;
            for (int i = 1; i < tempCounts.Length; i++)
            {
                tempCounts[0] -= tempCounts[i] * (i + 1);
            }
            for (int i = 0; i < tempCounts.Length; i++)
            {
                currentMarkCounts[i] += tempCounts[i];
            }

            for (int i = 0; i < externalMarks.Count; i++)
            {
                if (!markGroups.TryGetValue(externalMarks[i].Time, out SameTimingMarks group))
                {
                    CreateMark(externalMarks[i]);
                }
                else
                {
                    ProcessGroup(group, mainTime);
                    i += group.Count - 1;
                }
            }

            externalMarks.Clear();
        }

        private void ProcessGroup(SameTimingMarks group, float mainTime)
        {
            var poses = new List<Vector2>(group.Count);
            var colorposes = new List<Vector2>(group.Count);

            int currentSim = simtype;
            group.Sort(EventManager.GetInitlaizeOrder(group[0].Time));
            for (int i = 0; i < group.Count; i++)
            {
                var mk = CreateMark(group[i], true, group.SameTimings);
                mk.UpdateColorPosition(mainTime, EventManager.BPM);
                mk.SimType = currentSim;
                poses.Add(mk.Position);
                colorposes.Add(mk.ColorPosition);
                if (mk.NoteType == NoteType.AC || mk.NoteType == NoteType.ACFT)
                {
                    mk.ChangeColorPosition += mk_ChangeColorPosition;
                }
            }
            simtype++;
            if (simtype > simpiclist.Count)
            {
                simtype = 1;
            }
            if (poses.Count >= 3)
            {
                bool useColorPosition = true;
                for (int i = 0; i < poses.Count; i++)
                {
                    if (poses[i] != poses[0])
                    {
                        useColorPosition = false;
                        break;
                    }
                }

                Vector2[] convex = null;
                if (useColorPosition)
                {
                    convex = ConvexHull.Convex_Hull(colorposes.ToArray());
                }
                else
                {
                    convex = ConvexHull.Convex_Hull(poses.ToArray());
                }

                var mkc = new MarkConnection(device, markConnectionCommon);
                Mark[] orders = new Mark[convex.Length];
                for (int i = 0; i < MarkLayer.ChildrenCount && i < group.Count; i++)
                {
                    var mark = MarkLayer[i] as Mark;
                    int index = -1;
                    if (useColorPosition)
                    {
                        index = Array.IndexOf(convex, mark.ColorPosition);
                    }
                    else
                    {
                        index = Array.IndexOf(convex, mark.Position);
                    }
                    if (index >= 0)
                    {
                        while (index < orders.Length)
                        {
                            if (orders[index] == null)
                            {
                                orders[index] = mark;
                                break;
                            }
                            else
                            {
                                index++;
                            }
                        }
                    }
                    else
                    {
                        mkc.AddIn(mark);
                    }
                }
                foreach (Mark mk in orders)
                {
                    if (mk != null)
                    {
                        mkc.AddConvex(mk);
                    }
                }
                mkc.Initialize();
                ConnectLayer.AddChild(mkc);
            }
            else
            {
                var mkc = new MarkConnection(device, markConnectionCommon);
                for (int i = 0; i < MarkLayer.ChildrenCount && i < group.Count; i++)
                {
                    mkc.AddConvex(MarkLayer[i] as Mark);
                }
                mkc.Initialize();
                ConnectLayer.AddChild(mkc);
            }
        }

        bool mk_CheckColorPosition(PPDCoreModel.IMarkInfo markInfo, float currentTime, float bpm)
        {
            return scriptManager.CalculatePositionManager.Calculate(markInfo, currentTime, bpm);
        }

        bool mk_ChangeMarkEvaluate(PPDCoreModel.IMarkInfo markInfo, EffectType effectType, bool missPress, bool release, Vector2 position)
        {
            PreEvaluate?.Invoke(markInfo, effectType, missPress, release, position);
            return scriptManager.ProcessEvaluateManager.ProcessEvaluate(markInfo, effectType, missPress, release, position);
        }

        Vector2 mk_ChangeColorPosition(float scale, float timediff)
        {
            return new Vector2(300 * timediff * scale, 0);
        }

        private Mark CreateMark(MarkDataBase ipdata)
        {
            return CreateMark(ipdata, false, 0);
        }

        private bool IsScratch(ButtonType buttonType, int sameTimingMarks)
        {
            switch (buttonType)
            {
                case ButtonType.Triangle:
                case ButtonType.Circle:
                    if (HasRLSameTiming(sameTimingMarks))
                    {
                        return true;
                    }
                    break;
                case ButtonType.R:
                    return true;
                case ButtonType.L:
                    return true;
            }
            return false;
        }

        private bool HasRLSameTiming(int sameTimingMarks)
        {
            return ((sameTimingMarks >> (int)ButtonType.R) & 1) == 1 ||
                 ((sameTimingMarks >> (int)ButtonType.L) & 1) == 1;
        }

        private Mark CreateMark(MarkDataBase ipdata, bool hasSameTimingMark, int sameTimingMarks)
        {
            var emd = ipdata as ExMarkData;
            Mark mk = null;
            if (emd != null)
            {
                var noteType = EventManager.GetNoteType(ipdata.Time);
                var displayState = EventManager.GetCorrectDisplaystate(ipdata.Time);
                switch (noteType)
                {
                    case NoteType.Normal:
                        mk = new NormalExMark(device, resourceManager, imagepathes, emd, randomChangeManager[emd.ButtonType],
                            evals, adjustgaptime, circlepoints, displayState, noteType, sameTimingMarks, ipdata.Parameters, postDrawManager);
                        break;
                    case NoteType.AC:
                        mk = new HoldExMark(device, resourceManager, imagepathes, emd, randomChangeManager[emd.ButtonType],
                            evals, adjustgaptime, displayState, noteType, sameTimingMarks, ipdata.Parameters, postDrawManager);
                        break;
                    case NoteType.ACFT:
                        if (IsScratch(ipdata.ButtonType, sameTimingMarks))
                        {
                            var slideScale = EventManager.GetSlideScale(ipdata.Time);
                            mk = new SlideExMark(device, resourceManager, imagepathes, emd, randomChangeManager[emd.ButtonType],
                                evals, adjustgaptime, displayState, noteType, slideScale, sameTimingMarks, ipdata.Parameters, postDrawManager);
                        }
                        else
                        {
                            goto case NoteType.AC;
                        }
                        break;
                }
            }
            else
            {
                var md = ipdata as MarkData;
                mk = new Mark(device, resourceManager, imagepathes, md, randomChangeManager[md.ButtonType], evals, adjustgaptime,
                    EventManager.GetCorrectDisplaystate(ipdata.Time), EventManager.GetNoteType(ipdata.Time), sameTimingMarks, ipdata.Parameters, postDrawManager);
            }
            mk.HasSameTimingMark = hasSameTimingMark;
            if (scriptManager.ProcessMarkBPMManager.Process(mk, out float bpm))
            {
                mk.FixedBPM = bpm;
            }

            if (mk != null)
            {
                MarkLayer.InsertChild(mk, 0);
                mk.Initialize(scriptManager.CreateMarkManager.CreateMark, scriptManager.ProcessAllowedButtonsManager.Process);
                mk.CheckColorPosition += mk_CheckColorPosition;
                mk.ChangeMarkEvaluate += mk_ChangeMarkEvaluate;
            }

            return mk;
        }

        public void Seek(float seektime)
        {
            retry();
            if (!initialized) return;
            while (iter < ppdat.Length)
            {
                float time = ppdat[iter].Time;
                if (time - seektime >= 0)
                {
                    break;
                }
                iter++;
            }
        }
        public void Update(float time, bool[] b, bool[] released)
        {
            if (!initialized) return;
            ReadPpdData(time, SpeedScale * EventManager.BPM);
            ReadExternalPpdData(time);
            Logger.Instance.AddLog("Time:{0}, pressed:{1}, released:{2}", time, String.Join(",", b.Select(v => v ? "1" : "0").ToArray()),
                String.Join(",", released.Select(v => v ? "1" : "0").ToArray()));
            foreach (Mark mk in MarkLayer.Children.Reverse())
            {
                if (removeMarks.Contains(mk))
                {
                    continue;
                }
                var exmk = mk as ExMarkBase;
                var results = new MarkResults();
                int soundType = -1;
                bool remove = false;
                var isAuto = false;
                switch (AutoMode)
                {
                    case AutoMode.All:
                        isAuto = true;
                        break;
                    case AutoMode.ExceptSlide:
                        isAuto = !mk.IsScratch;
                        break;
                }
                if (exmk != null)
                {
                    remove = exmk.ExUpdate(EventManager.GetCorrectTime(time, mk.Time),
                             SpeedScale * EventManager.BPM,
                             ref b, ref released, results,
                             isAuto, ref soundType,
                             CreateEffect
                             );
                    if (isAuto)
                    {
                        if (soundType >= 0 && soundType < 10)
                        {
                            var index = randomChangeManager.Invert(soundType);
                            if (PlaySound != null) PlaySound.Invoke(index, true);
                        }
                        else if (soundType >= 10)
                        {
                            var index = randomChangeManager.Invert(soundType - 10);
                            if (StopSound != null) StopSound.Invoke(index, false);
                            if (EventManager.GetReleaseSound(index) && PlaySound != null) PlaySound.Invoke(index, false);
                        }
                    }
                    else
                    {
                        //manual
                        if (exmk.IsScratch && exmk.IsLong && soundType >= 0 && soundType < 10)
                        {
                            var index = randomChangeManager.Invert(soundType);
                            if (PlaySound != null) PlaySound.Invoke(index, false);
                        }
                        else
                        {
                            var index = randomChangeManager.Invert(soundType);
                            if (EventManager.GetReleaseSound(index) && PlaySound != null) PlaySound.Invoke(index, false);
                        }
                    }
                }
                else
                {
                    remove = mk.Update(EventManager.GetCorrectTime(time, mk.Time),
                          SpeedScale * EventManager.BPM,
                          ref b, results, isAuto,
                          ref soundType,
                          CreateEffect
                          );
                    if (isAuto)
                    {
                        if (soundType != -1)
                        {
                            var index = randomChangeManager.Invert(soundType);
                            if (PlaySound != null) PlaySound.Invoke(index, false);
                        }
                    }
                }
                if (results[EffectType.Cool] || results[EffectType.Fine])
                {
                    if (ACPressing[(int)mk.ButtonType] != null)
                    {
                        OnPressingButton(mk.ButtonType, false);
                        for (int i = 0; i < ACPressing.Length; i++)
                        {
                            if (ACPressing[i] != null)
                            {
                                AddToRemove(ACPressing[i]);
                            }
                        }
                        Array.Clear(ACPressing, 0, ACPressing.Length);
                    }
                    if (ChangeCombo != null) ChangeCombo.Invoke(true, mk.Position);
                }
                if (results[EffectType.Pressing])
                {
                    // 同時にHoldが始まったときに2個目ので1個目のを消す
                    if (ACPressing[(int)mk.ButtonType] != null && ACPressing[(int)mk.ButtonType] != mk)
                    {
                        OnPressingButton(mk.ButtonType, false);
                        for (int i = 0; i < ACPressing.Length; i++)
                        {
                            if (ACPressing[i] != null)
                            {
                                AddToRemove(ACPressing[i]);
                            }
                        }
                        Array.Clear(ACPressing, 0, ACPressing.Length);
                    }
                    if (OnPressingButton(mk.ButtonType, true))
                    {
                        for (int i = 0; i < ACPressing.Length; i++)
                        {
                            if (ACPressing[i] != null)
                            {
                                AddToRemove(ACPressing[i]);
                            }
                        }
                        Array.Clear(ACPressing, 0, ACPressing.Length);
                    }
                    ACPressing[(int)mk.ButtonType] = exmk;
                    RemoveFromConnection(exmk);
                }
                if (results[EffectType.PressReleased])
                {
                    OnPressingButton(mk.ButtonType, false);
                    for (int i = 0; i < ACPressing.Length; i++)
                    {
                        if (ACPressing[i] != null)
                        {
                            AddToRemove(ACPressing[i]);
                        }
                    }
                    Array.Clear(ACPressing, 0, ACPressing.Length);
                }
                if (results[EffectType.Slide])
                {
                    var slideExMark = exmk as SlideExMark;
                    for (var i = slideExMark.ProcessedPressingFrames; i < slideExMark.PressingFrames; i++)
                    {
                        var position = slideExMark.GetPressedSlidePosition(i + 1);
                        if (slideExMark.IsMaxSlide && (i + 1) == slideExMark.ExCount)
                        {
                            // do nothing for max slide
                        }
                        else
                        {
                            var score = 10 * (i + 1);
                            Slide?.Invoke(slideExMark, position, score, exmk.IsRight);
                        }
                    }
                    slideExMark.ProcessedPressingFrames = slideExMark.PressingFrames;
                }
                if (results[EffectType.Safe] || results[EffectType.Sad] || results[EffectType.Worst])
                {
                    if (ChangeCombo != null)
                    {
                        ChangeCombo.Invoke(false, mk.Position);
                    }
                }
                if (results[EffectType.Cool] || results[EffectType.Fine] || results[EffectType.Safe] || results[EffectType.Sad]
                    || results[EffectType.Worst])
                {
                    if (EvaluateCount != null)
                    {
                        EvaluateCount.Invoke(results.First, false);
                    }
                }
                if (remove)
                {
                    if (exmk is SlideExMark && ((SlideExMark)exmk).IsMaxSlide)
                    {
                        var slideExMark = ((SlideExMark)exmk);
                        var score = 1000 + 10 * slideExMark.ExCount;
                        var position = slideExMark.GetPressedSlidePosition(slideExMark.ExCount);
                        MaxSlide?.Invoke(slideExMark, position, score, exmk.IsRight);
                    }
                    AddToRemove(mk);
                }
            }
            foreach (Mark mk in removeMarks)
            {
                MarkLayer.RemoveChild(mk);
                mk.Dispose();
            }
            if (MarkLayer.ChildrenCount > 0 && AutoMode == AutoMode.None)
            {
                for (int i = 0; i < 10; i++)
                {
                    int iter = MarkLayer.ChildrenCount - 1;
                    while (b[i] && iter >= 0)
                    {
                        var mk = MarkLayer[iter] as Mark;
                        if (mk is ExMarkBase exmk && exmk.ExMarkState != ExMarkBase.ExState.Waiting)
                        {
                            iter--;
                            continue;
                        }
                        var eval = mk.Evaluate(time);
                        if (eval == EffectType.None)
                        {
                            iter--;
                            continue;
                        }
                        Action proc = () =>
                        {
                            b[i] = false;
                            ChangeCombo?.Invoke(false, mk.Position);
                            EvaluateCount.Invoke(eval, true);
                            mk_ChangeMarkEvaluate(mk, eval, true, false, mk.Position);
                            MarkLayer.RemoveChild(mk);
                            RemoveFromConnection(mk);
                            mk.Dispose();
                        };
                        if (scriptManager.ProcessMissPressManager.Process(mk, (MarkType)i, out bool isMissPress))
                        {
                            if (isMissPress)
                            {
                                proc();
                                break;
                            }
                        }
                        else if (mk.NoteType == NoteType.AC || (mk.NoteType == NoteType.ACFT && !mk.IsScratch))
                        {
                            proc();
                            break;
                        }
                        iter--;
                    }
                }
            }
            removeMarks.Clear();
            Update();
        }

        private bool OnPressingButton(ButtonType buttonType, bool isPressing)
        {
            if (PressingButton != null)
            {
                return PressingButton(buttonType, isPressing);
            }

            return false;
        }

        private void AddToRemove(Mark mk)
        {
            removeMarks.Add(mk);
            RemoveFromConnection(mk);
        }

        private void RemoveFromConnection(Mark mk)
        {
            if (!gameutility.Connect && !gameutility.Profile.Connect) return;
            MarkConnection foundmc = null;
            foreach (MarkConnection mc in ConnectLayer.Children)
            {
                if (mc.Contains(mk))
                {
                    foundmc = mc;
                    break;
                }
            }
            if (foundmc != null)
            {
                ConnectLayer.RemoveChild(foundmc);
                foundmc.Dispose();
            }
        }

        private void CreateEffect(EffectType type, Vector2 pos)
        {
            string filename = effectfilenames[(int)type];
            ppdem.AddEffect(filename, pos);
        }

        protected override bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return initialized;
        }

        public void retry()
        {
            iter = 0;
            simtype = 1;
            MarkLayer.ClearDisposeChildren();
            ConnectLayer.ClearDisposeChildren();
            externalMarks.Clear();
            Array.Clear(ACPressing, 0, ACPressing.Length);
            currentAllMarkCount = allMarkCount;
            currentAllLongMarkCount = allLongMarkCount;
            Array.Copy(markCounts, currentMarkCounts, markCounts.Length);
            MarkLayer.SetDefault();
            ConnectLayer.SetDefault();
        }

        private AutoMode AutoMode
        {
            get
            {
                if (config.Auto)
                {
                    return AutoMode.All;
                }
                return gameutility.AutoMode;
            }
        }

        private float SpeedScale
        {
            get
            {
                return gameutility.SpeedScale * config.SpeedScale;
            }
        }
    }
}
