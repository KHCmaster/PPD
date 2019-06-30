using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

using SlimDX;
using SlimDX.Direct3D9;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.PPDData;
using PPDFramework.Resource;

namespace PPD
{
    public class MarkManager : UpdatableGameComponent
    {
        public delegate void ChangeComboHandler(bool gain, Vector2 pos);
        public event ChangeComboHandler ChangeCombo;
        public delegate void SpecialSoundHandler(int index, bool keep);
        public event SpecialSoundHandler PlaySound;
        public event SpecialSoundHandler StopSound;
        public delegate void EvaluateCountHandler(int index, bool isMissPress);
        public event EvaluateCountHandler EvaluateCount;
        public delegate bool PressingButtonHandler(ButtonType buttonType, bool pressing);
        public event PressingButtonHandler PressingButton;

        public enum EffectType
        {
            None = 0,
            Worst = 1,
            Sad = 2,
            Safe = 3,
            Fine = 4,
            Cool = 5,
            Appear = 6,
            Pressing = 7,
            PressReleased = 8
        }
        public delegate void EvaluateEffectHandler(EffectType type, Vector2 pos);

        const float defaultbpm = 130;

        Device device;
        int simtype = 1;
        List<int> simpiclist = new List<int>();

        string[] effectfilenames = new string[7];
        IPPDData[] ppdat;
        int iter = 0;
        ArrayList marks = new ArrayList(100);
        ArrayList removemarks = new ArrayList(10);
        List<MarkConnection> connections = new List<MarkConnection>(10);
        float sametimeing = 0.01f;
        Hashtable markgroups = new Hashtable();
        bool initialized = false;

        PPDGameUtility gameutility;
        Vector2[] circlepoints;
        int[] keychange;
        float[] evals;
        float adjustgaptime;
        EventManager em;
        PPDEffectManager ppdem;
        CMarkImagePaths imagepathes;

        ExMark[] ACPressing = new ExMark[10];
        PPDFramework.Resource.ResourceManager resourceManager;
        ConnectionDrawer cd;
        public MarkManager(Device device, EventManager em, PPDGameUtility gameutility, int[] keychange, PPDEffectManager ppdem, CMarkImagePaths imagepathes, PPDFramework.Resource.ResourceManager resourceManager)
        {
            InnerStruct(device, em, gameutility, keychange, ppdem, imagepathes, resourceManager);
            readppddata(gameutility.SongInformation.DirectoryPath, gameutility.Difficulty);
            CheckGroups();
            readppddata(gameutility.SongInformation.StartTime);
        }

        public MarkManager(Device device, EventManager em, PPDGameUtility gameutility, int[] keychange, PPDEffectManager ppdem, CMarkImagePaths imagepathes, Stream stream, PPDFramework.Resource.ResourceManager resourceManager)
        {
            InnerStruct(device, em, gameutility, keychange, ppdem, imagepathes, resourceManager);
            readppddata(stream);
            CheckGroups();
            readppddata(gameutility.SongInformation.StartTime);
        }

        private void InnerStruct(Device device, EventManager em, PPDGameUtility gameutility, int[] keychange, PPDEffectManager ppdem, CMarkImagePaths imagepathes, PPDFramework.Resource.ResourceManager resourceManager)
        {
            this.device = device;
            this.gameutility = gameutility;
            this.em = em;
            this.keychange = keychange;
            this.ppdem = ppdem;
            this.imagepathes = imagepathes;
            this.resourceManager = resourceManager;
            cd = new ConnectionDrawer(device, 4);
            evals = new float[] { PPDSetting.Setting.CoolArea, PPDSetting.Setting.GoodArea, PPDSetting.Setting.SafeArea, PPDSetting.Setting.SadArea };
            adjustgaptime = PPDSetting.Setting.AdjustGapTime;
            CreateResource();
        }

        private void CreateResource()
        {
            List<string> pathes = new List<string>(30);
            List<string> checktracelist = new List<string>(10);
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
            string fn;
            float inner, outer;
            imagepathes.GetLongNoteCircleInfo(out fn, out inner, out outer);
            pathes.Add(fn);
            float x, y;
            imagepathes.GetHoldInfo(out fn, out x, out y);
            pathes.Add(fn);
            CreateCirclePoints(inner, outer);

            for (int i = 0; i < pathes.Count; i++)
            {
                resourceManager.Add(pathes[i], new ImageResource(pathes[i], device));
            }
            for (MarkEvaluateType type = MarkEvaluateType.Cool; type <= MarkEvaluateType.Worst; type++)
            {
                string effectfilename = imagepathes.GetEvaluateEffectPath(type);
                if (File.Exists(effectfilename))
                {
                    ppdem.CreateEffect(effectfilename);
                    effectfilenames[5 - (int)type] = effectfilename;
                }
            }
            string appearname = imagepathes.GetAppearEffectPath();
            if (File.Exists(appearname))
            {
                ppdem.CreateEffect(appearname);
                effectfilenames[(int)EffectType.Appear] = appearname;
            }

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

        private void readppddata(string dir, Difficulty difficulty)
        {
            string filename = DifficultyUtility.ConvertDifficulty(difficulty) + ".ppd";
            string path = Path.Combine(dir, filename);
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

        private void readppddata(Stream stream)
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

        private void CheckGroups()
        {
            if (initialized)
            {
                float lasttime = -1;
                List<IPPDData> group = new List<IPPDData>();
                foreach (IPPDData ipdata in ppdat)
                {
                    if (ipdata.Time >= lasttime + sametimeing)
                    {
                        if (group.Count > 1)
                        {
                            markgroups.Add(lasttime, group);
                            group = new List<IPPDData>();
                        }
                        else
                        {
                            group.Clear();
                        }
                        group.Add(ipdata);
                        lasttime = ipdata.Time;
                    }
                    else
                    {
                        group.Add(ipdata);
                    }
                }
                if (group.Count > 1)
                {
                    markgroups.Add(lasttime, group);
                    group = new List<IPPDData>();
                }
            }
        }

        private void readppddata(float maintime)
        {
            while (iter < ppdat.Length)
            {
                if (ppdat[iter].Time - 10 < maintime)
                {
                    List<IPPDData> group = markgroups[ppdat[iter].Time] as List<IPPDData>;
                    if (group != null)
                    {
                        List<Vector2> poses = new List<Vector2>(group.Count);
                        List<Vector2> colorposes = new List<Vector2>(group.Count);

                        int currentSim = simtype;
                        MarkComparer.Comparer.Table = em.GetInitlaizeOrder(ppdat[iter].Time);
                        group.Sort(MarkComparer.Comparer);
                        for (int i = 0; i < group.Count; i++)
                        {
                            Mark mk = CreateMark(group[i]);
                            mk.CaliculateColorPosition(maintime, em.BPM);
                            mk.SimType = currentSim;
                            poses.Add(mk.Position);
                            colorposes.Add(mk.ColorPosition);
                            if (mk.AC)
                            {
                                mk.ChangeColorPosition += new Mark.ColorPositionDelegate(mk_ChangeColorPosition);
                            }
                            iter++;
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

                            MarkConnection mkc = new MarkConnection();
                            Mark[] orders = new Mark[convex.Length];
                            for (int i = marks.Count - 1; i >= 0 && marks.Count - i <= group.Count; i--)
                            {
                                int index = -1;
                                if (useColorPosition)
                                {
                                    index = Array.IndexOf(convex, (marks[i] as Mark).ColorPosition);
                                }
                                else
                                {
                                    index = Array.IndexOf(convex, (marks[i] as Mark).Position);
                                }
                                if (index >= 0)
                                {
                                    while (index < orders.Length)
                                    {
                                        if (orders[index] == null)
                                        {
                                            orders[index] = marks[i] as Mark;
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
                                    mkc.AddIn(marks[i] as Mark);
                                }
                            }
                            foreach (Mark mk in orders)
                            {
                                if (mk != null)
                                {
                                    mkc.AddConvex(mk);
                                }
                            }
                            connections.Add(mkc);
                        }
                        else
                        {
                            MarkConnection mkc = new MarkConnection();
                            for (int i = marks.Count - 1; i >= 0 && marks.Count - i <= group.Count; i--)
                            {
                                mkc.AddConvex(marks[i] as Mark);
                            }
                            connections.Add(mkc);
                        }
                    }
                    else
                    {
                        CreateMark(ppdat[iter]);
                        iter++;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        Vector2 mk_ChangeColorPosition(float scale, float timediff)
        {
            return new Vector2(300 * timediff * scale, 0);
        }

        private Mark CreateMark(IPPDData ipdata)
        {
            ExMarkData emd = ipdata as ExMarkData;
            if (emd != null)
            {
                ExMark ret = new ExMark(device, resourceManager, imagepathes, emd, (ButtonType)keychange[(int)emd.ButtonType],
                    evals, adjustgaptime, circlepoints, em.GetCorrectDisplaystate(ipdata.Time), em.GetACMode(ipdata.Time));
                marks.Add(ret);
                return ret;
            }
            else
            {
                MarkData md = ipdata as MarkData;
                Mark ret = new Mark(device, resourceManager, imagepathes, md, (ButtonType)keychange[(int)md.ButtonType], evals, adjustgaptime, em.GetCorrectDisplaystate(ipdata.Time), em.GetACMode(ipdata.Time));
                marks.Add(ret);
                return ret;
            }
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
        public void Update(float time, bool[] b, bool[] released, bool fadeout)
        {
            if (!initialized) return;
            cd.Update();
            readppddata(time);
            foreach (Mark mk in marks)
            {
                if (removemarks.Contains(mk))
                {
                    continue;
                }
                ExMark exmk = mk as ExMark;
                EffectType result = 0;
                int soundtype = -1;
                bool remove = false;
                if (exmk != null)
                {
                    remove = exmk.ExUpdate(em.GetCorrectTime(time, mk.Time),
                             gameutility.SpeedScale * em.BPM,
                             ref b, ref released, ref result,
                             gameutility.Auto, ref soundtype,
                             CreateEffect
                             );
                    if (gameutility.Auto)
                    {
                        if (soundtype >= 0 && soundtype < 10)
                        {
                            int index = Array.IndexOf(keychange, soundtype);
                            if (PlaySound != null) PlaySound.Invoke(index, true);
                        }
                        else if (soundtype >= 10)
                        {
                            int index = Array.IndexOf(keychange, soundtype - 10);
                            if (StopSound != null) StopSound.Invoke(index, false);
                            if (em.GetReleaseSound(index) && PlaySound != null) PlaySound.Invoke(index, false);
                        }
                    }
                    else
                    {
                        //manual
                        int index = Array.IndexOf(keychange, soundtype);
                        if (em.GetReleaseSound(index) && PlaySound != null) PlaySound.Invoke(index, false);
                    }
                }
                else
                {
                    remove = mk.Update(em.GetCorrectTime(time, mk.Time),
                          gameutility.SpeedScale * em.BPM,
                          ref b, ref result, gameutility.Auto,
                          ref soundtype,
                          CreateEffect
                          );
                    if (gameutility.Auto)
                    {
                        if (soundtype != -1)
                        {
                            int index = Array.IndexOf(keychange, soundtype);
                            if (PlaySound != null) PlaySound.Invoke(index, false);
                        }
                    }
                }
                if (result != EffectType.None)
                {
                    if (result == EffectType.Cool || result == EffectType.Fine)
                    {
                        if (gameutility.Auto && ACPressing[(int)mk.ButtonType] != null)
                        {
                            if (PressingButton != null) PressingButton.Invoke(mk.ButtonType, false);
                            for (int i = 0; i < ACPressing.Length; i++)
                            {
                                if (ACPressing[i] != null)
                                {
                                    AddRemove(ACPressing[i]);
                                }
                            }
                            Array.Clear(ACPressing, 0, ACPressing.Length);
                        }
                        if (ChangeCombo != null) ChangeCombo.Invoke(true, new Vector2(mk.Position.X + 10, mk.Position.Y - 60));
                    }
                    else if (result == EffectType.Pressing)
                    {
                        if (PressingButton != null)
                            if (PressingButton.Invoke(mk.ButtonType, true))
                            {
                                for (int i = 0; i < ACPressing.Length; i++)
                                {
                                    if (ACPressing[i] != null)
                                    {
                                        AddRemove(ACPressing[i]);
                                    }
                                }
                                Array.Clear(ACPressing, 0, ACPressing.Length);
                            }
                        ACPressing[(int)mk.ButtonType] = exmk;
                        CheckConnection(exmk);
                    }
                    else if (result == EffectType.PressReleased)
                    {
                        PressingButton.Invoke(mk.ButtonType, false);
                        for (int i = 0; i < ACPressing.Length; i++)
                        {
                            if (ACPressing[i] != null)
                            {
                                AddRemove(ACPressing[i]);
                            }
                        }
                        Array.Clear(ACPressing, 0, ACPressing.Length);
                    }
                    else
                    {
                        if (ChangeCombo != null) ChangeCombo.Invoke(false, new Vector2(mk.Position.X + 10, mk.Position.Y - 60));
                    }
                    if (!fadeout && result != EffectType.Pressing && result != EffectType.PressReleased)
                    {
                        if (EvaluateCount != null) EvaluateCount.Invoke((int)result - 1, false);
                    }
                }
                if (remove)
                {
                    AddRemove(mk);
                }
            }
            foreach (Mark mk in removemarks)
            {
                marks.Remove(mk);
            }
            if (marks.Count > 0 && !gameutility.Auto)
            {
                int iter = 0;
                for (int i = 0; i < 10; i++)
                {
                    if (b[i])
                    {
                        Mark mk = null;
                        while (iter < marks.Count)
                        {
                            mk = marks[iter] as Mark;
                            ExMark exmk = mk as ExMark;
                            if (exmk != null && exmk.ExMarkState != ExMark.ExState.waitingpress)
                            {
                                iter++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (iter >= marks.Count || mk == null)
                        {
                            break;
                        }
                        if (mk.AC)
                        {
                            int eval = mk.Evaluate(time);
                            if (eval > 0)
                            {
                                if (ChangeCombo != null) ChangeCombo(false, new Vector2(mk.Position.X + 10, mk.Position.Y - 60));
                                EvaluateCount.Invoke(eval - 1, true);
                                marks.Remove(mk);
                                CheckConnection(mk);
                            }
                        }
                    }
                }
            }
            removemarks.Clear();
        }

        private void AddRemove(Mark mk)
        {
            removemarks.Add(mk);
            CheckConnection(mk);
        }

        private void CheckConnection(Mark mk)
        {
            if (!gameutility.Connect && !gameutility.Profile.Connect) return;
            MarkConnection foundmc = null;
            foreach (MarkConnection mc in connections)
            {
                if (mc.Contains(mk))
                {
                    foundmc = mc;
                    break;
                }
            }
            if (foundmc != null)
            {
                connections.Remove(foundmc);
            }
        }
        private void CreateEffect(EffectType type, Vector2 pos)
        {
            string filename = effectfilenames[(int)type];
            ppdem.AddEffect(filename, pos);
        }
        public void Draw()
        {
            if (!initialized) return;
            foreach (Mark mk in marks)
            {
                mk.Draw();
            }
            foreach (Mark mk in marks)
            {
                ExMark exmk = mk as ExMark;
                if (exmk != null)
                {
                    exmk.gageDraw();
                }
            }
            if (gameutility.Connect || gameutility.Profile.Connect)
            {
                foreach (MarkConnection mc in connections)
                {
                    mc.DrawConnection(DrawConnect);
                }
            }
            foreach (Mark mk in marks)
            {
                mk.colorDraw();
            }
        }

        private void DrawConnect(Mark mk1, Mark mk2)
        {
            if (mk1.Hidden || mk2.Hidden) return;
            Vector2 vec = Vector2.Subtract(mk1.ColorPosition, mk2.ColorPosition);
            if (vec == Vector2.Zero) return;
            vec.Normalize();
            Vector2 center = new Vector2((mk1.ColorPosition.X + mk2.ColorPosition.X) / 2, (mk1.ColorPosition.Y + mk2.ColorPosition.Y) / 2);
            float rotation = 0;
            if (vec.Y < 0)
            {
                rotation = (float)(Math.PI * 2 - (float)Math.Acos(vec.X));
            }
            else
            {
                rotation = (float)Math.Acos(vec.X);
            }
            float scale = Vector2.Distance(mk1.ColorPosition, mk2.ColorPosition) / ConnectionDrawer.MaxWidth;
            cd.Matrix = Matrix.Transformation2D(Vector2.Zero, 0, new Vector2(scale, 1), Vector2.Zero, rotation, center);
            cd.Draw();
        }

        public void retry()
        {
            iter = 0;
            simtype = 1;
            marks.Clear();
            connections.Clear();
            Array.Clear(ACPressing, 0, ACPressing.Length);
        }

        public override void Update()
        {

        }

        protected override void DisposeResource()
        {
            cd.Dispose();
        }

        class MarkComparer : IComparer<IPPDData>
        {
            public static MarkComparer Comparer = new MarkComparer();
            #region IComparer<IPPDData> メンバ

            public int Compare(IPPDData x, IPPDData y)
            {
                return -(Array.IndexOf(Table, x.ButtonType) - Array.IndexOf(Table, y.ButtonType));
            }

            public ButtonType[] Table
            {
                get;
                set;
            }

            #endregion
        }
    }
}
