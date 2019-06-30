using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using PPDFramework;
using PPDEditor.Forms;

namespace PPDEditor
{
    public class PPDSheet
    {
        public enum Direction
        {
            Up = 0,
            Left = 1,
            Down = 2,
            Right = 3
        }
        public event EventHandler DisplayDataChanged;
        string name;
        CustomPoint recstart = new CustomPoint(-1, -1);
        CustomPoint recend = new CustomPoint(-1, -1);
        private float bpm = 100;
        private float bpmstart = 0;
        private int defaultinterval = 240;
        private int deatmodeindex = 0;
        int[] focusedmark = new int[] { -1, -1 };
        SortedList<float, Mark>[] data;
        RingBuffer<SortedList<float, Mark>[]> storeddata;
        const int buttonnum = 10;
        public PPDSheet()
        {
            storeddata = new RingBuffer<SortedList<float, Mark>[]>(200);
            data = new SortedList<float, Mark>[buttonnum];
            for (int i = 0; i < buttonnum; i++)
            {
                data[i] = new SortedList<float, Mark>();
            }
            PreserveData();
        }
        public float BPM
        {
            get
            {
                return bpm;
            }
            set
            {
                bpm = value;
                if (DisplayDataChanged != null)
                {
                    DisplayDataChanged.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public float BPMStart
        {
            get
            {
                return bpmstart;
            }
            set
            {
                bpmstart = value;
                if (DisplayDataChanged != null)
                {
                    DisplayDataChanged.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public string DisplayName
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        public int DisplayWidth
        {
            get
            {
                return defaultinterval;
            }
            set
            {
                defaultinterval = value;
                if (DisplayDataChanged != null)
                {
                    DisplayDataChanged.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public int BeatModeIndex
        {
            get
            {
                return deatmodeindex;
            }
            set
            {
                deatmodeindex = value;
                if (DisplayDataChanged != null)
                {
                    DisplayDataChanged.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public CustomPoint RecStart
        {
            get
            {
                return recstart;
            }
            set
            {
                recstart = value;
            }
        }
        public CustomPoint RecEnd
        {
            get
            {
                return recend;
            }
            set
            {
                recend = value;
            }
        }

        public bool AreaSelectionEnabled
        {
            get
            {
                return recstart.X != recend.X;
            }
        }

        public int[] FocusedMark
        {
            get
            {
                return focusedmark;
            }
            set
            {
                focusedmark = value;
            }
        }
        public SortedList<float, Mark>[] Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }
        public void PreserveData()
        {
            SortedList<float, Mark>[] temp = new SortedList<float, Mark>[10];
            for (int i = 0; i < 10; i++)
            {
                temp[i] = new SortedList<float, Mark>();
                for (int j = 0; j < data[i].Count; j++)
                {
                    ExMark exmk = data[i].Values[j] as ExMark;
                    if (exmk == null)
                    {
                        temp[i].Add(data[i].Values[j].Time, data[i].Values[j].Clone());
                    }
                    else
                    {
                        temp[i].Add(data[i].Values[j].Time, exmk.ExClone());
                    }
                }
            }
            storeddata.add(temp);
        }
        public bool Undo()
        {
            if (storeddata.canundo())
            {
                data = copydata(storeddata.getprevious());
                focusedmark[0] = -1;
                focusedmark[1] = -1;
                return true;
            }
            return false;
        }
        public bool Redo()
        {
            if (storeddata.canredo())
            {
                data = copydata(storeddata.getnext());
                focusedmark[0] = -1;
                focusedmark[1] = -1;
                return true;
            }
            return false;
        }
        private SortedList<float, Mark>[] copydata(SortedList<float, Mark>[] dat)
        {
            SortedList<float, Mark>[] temp = new SortedList<float, Mark>[10];
            for (int i = 0; i < 10; i++)
            {
                temp[i] = new SortedList<float, Mark>();
                for (int j = 0; j < dat[i].Count; j++)
                {
                    ExMark exmk = dat[i].Values[j] as ExMark;
                    if (exmk == null)
                    {
                        temp[i].Add(dat[i].Values[j].Time, dat[i].Values[j].Clone());
                    }
                    else
                    {
                        temp[i].Add(dat[i].Values[j].Time, exmk.ExClone());
                    }
                }
            }
            return temp;
        }
        public Mark[] GetAreaData()
        {
            if (recstart.X == -1 || recstart.Y == -1)
            {
                return new Mark[0];
            }
            float width = Math.Abs(recstart.X - recend.X);
            int starty = (recstart.Y <= recend.Y ? recstart.Y : recend.Y);
            int height = Math.Abs(recstart.Y - recend.Y) + 1;
            float starttime = (recstart.X <= recend.X ? recstart.X : recend.X);
            float endtime = starttime + width;
            int num = 0;
            int[] startnum = new int[height];
            for (int i = 0; i < height; i++)
            {
                startnum[i] = -1;
            }
            for (int i = starty; i < starty + height; i++)
            {
                bool first = true;
                for (int j = 0; j < data[i].Count; j++)
                {
                    if (data[i].Keys[j] < starttime) continue;
                    if (data[i].Keys[j] > endtime) break;
                    if (first)
                    {
                        first = false;
                        startnum[i - starty] = j;
                    }
                    num++;
                }
            }
            Mark[] mks = new Mark[num];
            int iter = 0;
            while (true)
            {
                float minimumtime = float.MaxValue;
                int minimumnum = -1;
                for (int i = starty; i < starty + height; i++)
                {
                    if (startnum[i - starty] == -1 || startnum[i - starty] >= data[i].Count)
                    {
                        continue;
                    }
                    if (data[i].Keys[startnum[i - starty]] > endtime)
                    {
                        startnum[i - starty] = -1;
                        continue;
                    }
                    if (minimumtime > data[i].Keys[startnum[i - starty]])
                    {
                        minimumnum = i;
                        minimumtime = data[i].Keys[startnum[i - starty]];
                        continue;
                    }
                }
                if (minimumnum == -1)
                {
                    break;
                }
                else
                {
                    mks[iter] = data[minimumnum].Values[startnum[minimumnum - starty]];
                    iter++;
                    startnum[minimumnum - starty]++;
                }
            }
            return mks;
        }
        public void ShiftSelectedMarkTime(float dt)
        {
            Mark mk = null;
            if (data[focusedmark[0]].TryGetValue(data[focusedmark[0]].Keys[focusedmark[1]], out mk))
            {
                data[focusedmark[0]].RemoveAt(focusedmark[1]);
                ExMark exmk = mk as ExMark;
                if (exmk != null)
                {
                    exmk.EndTime += dt - mk.Time;
                }
                mk.Time = dt;
                while (data[focusedmark[0]].ContainsKey(mk.Time))
                {
                    mk.Time -= 0.01f;
                }
                data[focusedmark[0]].Add(mk.Time, mk);
                focusedmark[1] = data[focusedmark[0]].IndexOfKey(mk.Time);
            }
        }
        public void AddMark(float time, int num)
        {
            if (!data[num].ContainsKey(time) && !checklongmark(num, time))
            {
                data[num].Add(time, CreateMark(400, 225, num, time, 0));
                focusedmark[0] = num;
                focusedmark[1] = data[num].IndexOfKey(time);
            }
        }
        public void AddMark(float time, float x, float y, float rotation, int num)
        {
            if (num >= 0 && num < 10)
            {
                if (!data[num].ContainsKey(time) && !checklongmark(num, time))
                {
                    data[num].Add(time, CreateMark(x, y, num, time, rotation));
                    focusedmark[0] = num;
                    focusedmark[1] = data[num].IndexOfKey(time);
                }
            }
        }
        public void AddExMark(float time, float endtime, int num)
        {
            if (!data[num].ContainsKey(time) && !checklongmark(num, time))
            {
                data[num].Add(time, CreateExMark(400, 225, num, time, endtime, 0));
                focusedmark[0] = num;
                focusedmark[1] = data[num].IndexOfKey(time);
            }
        }
        public void AddExMark(float time, float endtime, float x, float y, float rotation, int num)
        {
            if (!data[num].ContainsKey(time) && !checklongmark(num, time))
            {
                data[num].Add(time, CreateExMark(x, y, num, time, endtime, rotation));
                focusedmark[0] = num;
                focusedmark[1] = data[num].IndexOfKey(time);
            }
        }
        public void Swap(int first, int second)
        {
            if (first == second) return;
            SortedList<float, Mark> swap = new SortedList<float, Mark>(data[first].Count);
            foreach (Mark mk in this.data[first].Values)
            {
                mk.Type = (byte)second;
                swap.Add(mk.Time, mk);
            }
            this.data[first].Clear();
            foreach (Mark mk in this.data[second].Values)
            {
                mk.Type = (byte)first;
                this.data[first].Add(mk.Time, mk);
            }
            this.data[second].Clear();
            foreach (Mark mk in swap.Values)
            {
                this.data[second].Add(mk.Time, mk);
            }
        }
        public bool[] UpdateMark(float time, float speedscale, EventManager em)
        {
            bool[] ret = new bool[buttonnum];
            if (data != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    foreach (Mark mk in data[i].Values)
                    {
                        ExMark exmk = mk as ExMark;
                        EventData.DisplayState DState;
                        bool AC;
                        em.GetCorrectInfo(mk.Time, out DState, out AC);
                        if (exmk != null)
                        {
                            ret[i] |= (exmk.ExUpdate(em.GetCorrectTime(time, mk.Time), speedscale * bpm, DState, AC, em.ReleaseSound(mk.Type)) == 1);
                        }
                        else
                        {
                            ret[i] |= (mk.Update(em.GetCorrectTime(time, mk.Time), speedscale * bpm, DState, AC) == 1);
                        }
                    }
                }
            }
            return ret;
        }
        public void DrawMark()
        {
            if (data != null)
            {
                int miny = -1, maxy = -1;
                float minx = -1, maxx = -1;
                if (AreaSelectionEnabled && recstart.Y != -1 && recend.Y != -1)
                {
                    miny = Math.Min(recstart.Y, recend.Y);
                    maxy = Math.Max(recstart.Y, recend.Y);
                    minx = Math.Min(recstart.X, recend.X);
                    maxx = Math.Max(recstart.X, recend.X);
                }
                for (int i = 0; i < 10; i++)
                {
                    bool isin = miny <= i && i <= maxy;
                    foreach (Mark mk in data[i].Values)
                    {
                        if (mk.Hidden)
                        {
                            if (isin && minx <= mk.Time && mk.Time <= maxx)
                            {
                                mk.DrawOnlyMark();
                            }
                        }
                        else
                        {
                            ExMark exmk = mk as ExMark;
                            if (exmk != null)
                            {
                                exmk.ExDraw();
                            }
                            else
                            {
                                mk.Draw();
                            }
                        }
                    }
                }
            }
        }
        public Mark SelectedMark
        {
            get
            {
                Mark ret = null;
                if (focusedmark[0] >= 0 && focusedmark[0] < buttonnum)
                {
                    if (focusedmark[1] >= data[focusedmark[0]].Count)
                    {
                        return ret;
                    }
                    if (data[focusedmark[0]].TryGetValue(data[focusedmark[0]].Keys[focusedmark[1]], out ret))
                    {
                        return ret;
                    }
                }
                return ret;
            }
        }
        public int MoveMark(Point pos, int mode, bool shiftKey)
        {
            //0...pos
            //1...angle
            if (focusedmark[0] >= 0 && focusedmark[0] < 10)
            {
                Mark mk = null;
                if (data[focusedmark[0]].TryGetValue(data[focusedmark[0]].Keys[focusedmark[1]], out mk))
                {
                    if (mode == 0)
                    {
                        if (pos.X <= 0) pos.X = 0;
                        if (pos.X >= 800) pos.X = 800;
                        if (pos.Y <= 0) pos.Y = 0;
                        if (pos.Y >= 450) pos.Y = 450;
                        mk.Position = new Vector2(pos.X, pos.Y);
                        //mk.Update((float)currenttime, bpm);
                        return 0;
                    }
                    else if (mode == 1)
                    {
                        if (PPDStaticSetting.RestrictAngle == 0 || !shiftKey)
                        {
                            float length = (float)Math.Sqrt(Math.Pow(Math.Abs(pos.X - mk.Position.X), 2) + Math.Pow(Math.Abs(pos.Y - mk.Position.Y), 2));
                            float d = (float)Math.Acos((pos.X - mk.Position.X) / length);
                            if (pos.Y > mk.Position.Y)
                            {
                                d = (float)(Math.PI * 2 - d);
                            }
                            mk.Rotation = d;
                        }
                        else
                        {
                            Vector2 vec = new Vector2(pos.X - mk.Position.X, pos.Y - mk.Position.Y);
                            float length = vec.Length();
                            int anglebase = 0;
                            if (vec.X >= 0)
                            {
                                if (vec.Y >= 0)
                                {
                                    anglebase = 0;
                                }
                                else
                                {
                                    anglebase = 270;
                                }
                            }
                            else
                            {
                                if (vec.Y >= 0)
                                {
                                    anglebase = 90;
                                }
                                else
                                {
                                    anglebase = 180;
                                }
                            }

                            List<Vector2> poses = new List<Vector2>();
                            int angleSplit = 90 / PPDStaticSetting.RestrictAngle;
                            for (int i = 0; i <= angleSplit; i++)
                            {
                                double angle = Math.PI * ((anglebase + 90 * ((double)i / angleSplit))) / 180;
                                poses.Add(new Vector2((float)(length * Math.Cos(angle)), (float)(length * Math.Sin(angle))));
                            }
                            int nearestindex = -1;
                            float nearestlength = float.MaxValue;
                            int index = 0;
                            foreach (Vector2 vector in poses)
                            {
                                Vector2 p = new Vector2(vector.X - pos.X, vector.Y - pos.Y) + mk.Position;
                                length = p.Length();
                                if (length < nearestlength)
                                {
                                    nearestlength = length;
                                    nearestindex = index;
                                }
                                index++;
                            }
                            Console.WriteLine(nearestindex);
                            if (nearestindex >= 0)
                            {
                                float d = (float)(Math.PI * ((anglebase + 90 * ((float)nearestindex / angleSplit))) / 180);
                                mk.Rotation = (float)(Math.PI * 2 - d);
                            }
                        }
                        //mk.Update((float)currenttime, bpm);
                        return 1;
                    }
                }
            }
            return -1;
        }
        public Mark[] GetSortedData()
        {
            int num = 0;
            for (int i = 0; i < 10; i++)
            {
                num += data[i].Count;
            }
            Mark[] ret = new Mark[num];
            int retiter = 0;
            int[] iters = new int[10];
            while (true)
            {
                int minimum = -1;
                float minimumtime = float.MaxValue;
                for (int i = 0; i < 10; i++)
                {
                    if (iters[i] < data[i].Count)
                    {
                        if (minimumtime >= data[i].Keys[iters[i]])
                        {
                            minimum = i;
                            minimumtime = data[i].Keys[iters[i]];
                        }
                    }
                }
                if (minimum == -1)
                {
                    break;
                }
                else
                {
                    Mark mk = data[minimum].Values[iters[minimum]];
                    ret[retiter] = mk;
                    retiter++;
                    iters[minimum]++;
                }
            }
            return ret;
        }
        public void MoveLine(int num, float dt)
        {
            if (num >= 0 && num < 10)
            {
                SortedList<float, Mark> tempdat = new SortedList<float, Mark>(data[num].Count);
                foreach (Mark mk in data[num].Values)
                {
                    ExMark exmk = mk as ExMark;
                    if (exmk != null)
                    {
                        exmk.EndTime += dt;
                    }
                    mk.Time += dt;
                    tempdat.Add(mk.Time, mk);
                }
                data[num] = tempdat;
            }
            else if (num == int.MaxValue)
            {
                for (int i = 0; i < 10; i++)
                {
                    SortedList<float, Mark> tempdat = new SortedList<float, Mark>(data[i].Count);
                    foreach (Mark mk in data[i].Values)
                    {
                        ExMark exmk = mk as ExMark;
                        if (exmk != null)
                        {
                            exmk.EndTime += dt;
                        }
                        mk.Time += dt;
                        tempdat.Add(mk.Time, mk);
                    }
                    data[i] = tempdat;
                }
            }
        }
        public bool SetData(float[] xs, float[] ys, float[] angles, bool reverse, int mode, bool withangle)
        {
            if (mode == 0)
            {
                if (focusedmark[0] == -1) return false;
                int iter = focusedmark[1];
                if (!reverse)
                {
                    for (int i = 0; i < xs.Length; i++)
                    {
                        Mark mk = data[focusedmark[0]].Values[iter];
                        mk.Position = new Vector2(xs[i], ys[i]);
                        if (withangle)
                        {
                            mk.Rotation = angles[i];
                        }
                        iter++;
                        if (iter >= data[focusedmark[0]].Count) break;
                    }
                }
                else
                {
                    for (int i = 0; i < xs.Length; i++)
                    {
                        Mark mk = data[focusedmark[0]].Values[iter];
                        mk.Position = new Vector2(xs[i], ys[i]);
                        if (withangle)
                        {
                            mk.Rotation = angles[i];
                        }
                        iter--;
                        if (iter < 0) break;
                    }
                }
                return true;
            }
            else if (mode == 1)
            {
                Mark[] mks = GetAreaData();
                int iter;
                if (!reverse)
                {
                    iter = 0;
                    for (int i = 0; i < xs.Length; i++)
                    {
                        mks[iter].Position = new Vector2(xs[i], ys[i]);
                        if (withangle)
                        {
                            mks[iter].Rotation = angles[i];
                        }
                        iter++;
                        if (iter >= mks.Length) break;
                    }
                }
                else
                {
                    iter = mks.Length - 1;
                    for (int i = 0; i < xs.Length; i++)
                    {
                        mks[iter].Position = new Vector2(xs[i], ys[i]);
                        if (withangle)
                        {
                            mks[iter].Rotation = angles[i];
                        }
                        iter--;
                        if (iter < 0) break;
                    }
                }
                return true;
            }
            return false;
        }
        public bool GetSortedData(bool reverse, int mode, out Mark[] mks)
        {
            mks = null;
            //mode 0 is line 1 is area
            if (mode == 0)
            {
                if (focusedmark[0] == -1)
                {
                    return false;
                }
                else
                {
                    if (!reverse)
                    {
                        int num = data[focusedmark[0]].Count - focusedmark[1];
                        mks = new Mark[num];
                        for (int i = 0; i < mks.Length; i++)
                        {
                            mks[i] = data[focusedmark[0]].Values[focusedmark[1] + i];
                        }
                        return true;
                    }
                    else
                    {
                        int num = focusedmark[1] + 1;
                        mks = new Mark[num];
                        for (int i = 0; i < mks.Length; i++)
                        {
                            mks[i] = data[focusedmark[0]].Values[focusedmark[1] - i];
                        }
                        return true;
                    }
                }

            }
            else
            {
                if (recstart.X == -1 || recstart.Y == -1)
                {
                    return false;
                }
                mks = GetAreaData();
                if (reverse)
                {
                    Array.Reverse(mks);
                }
                return true;
            }
        }
        public bool MoveMarkInSame(float time)
        {
            Mark mk = SelectedMark;
            if (mk != null)
            {
                data[focusedmark[0]].RemoveAt(focusedmark[1]);
                ExMark exmk = mk as ExMark;
                if (exmk != null)
                {
                    exmk.EndTime += time - mk.Time;
                }
                mk.Time = time;
                while (data[focusedmark[0]].ContainsKey(mk.Time))
                {
                    mk.Time -= 0.01f;
                }
                data[focusedmark[0]].Add(mk.Time, mk);
                focusedmark[1] = data[focusedmark[0]].IndexOfKey(mk.Time);
                return true;
            }
            return false;
        }
        public bool CopyMarkInSame(float time)
        {
            Mark mk = SelectedMark;
            if (mk != null && !data[focusedmark[0]].ContainsKey(time))
            {
                ExMark exmk = mk as ExMark;
                if (exmk != null)
                {
                    if (!checklongmark(focusedmark[0], time))
                    {
                        ExMark newexmk = CreateExMark(exmk.Position.X, exmk.Position.Y, focusedmark[0], time, time + exmk.EndTime - exmk.Time, exmk.Rotation);
                        data[focusedmark[0]].Add(time, newexmk);
                        focusedmark[1] = data[focusedmark[0]].IndexOfKey(time);
                    }
                }
                else
                {
                    Mark newmk = CreateMark(mk.Position.X, mk.Position.Y, focusedmark[0], time, mk.Rotation);
                    data[focusedmark[0]].Add(time, newmk);
                    focusedmark[1] = data[focusedmark[0]].IndexOfKey(time);
                }
                return true;
            }
            return false;
        }
        private bool checklongmark(int num, float time)
        {
            foreach (Mark mk in data[num].Values)
            {
                ExMark exmk = mk as ExMark;
                if (exmk != null && time == exmk.EndTime)
                {
                    return true;
                }
            }
            return false;
        }
        public bool SelectMark(float time, int num, float dt)
        {
            bool ret = false;
            foreach (float f in data[num].Keys)
            {
                Mark mk;
                data[num].TryGetValue(f, out mk);
                ExMark exmk = mk as ExMark;
                if (exmk != null)
                {
                    if (f <= time && time <= f + exmk.EndTime - exmk.Time)
                    {
                        focusedmark[0] = num;
                        focusedmark[1] = data[num].IndexOfKey(f);
                        recstart.X = -1;
                        recstart.Y = -1;
                        ret = true;
                        break;
                    }
                }
                else
                {
                    if (f - dt <= time && time <= f + dt)
                    {
                        focusedmark[0] = num;
                        focusedmark[1] = data[num].IndexOfKey(f);
                        recstart.X = -1;
                        recstart.Y = -1;
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }
        public bool ChangeMarkType(int num)
        {
            Mark mk = SelectedMark;
            if (SelectedMark != null)
            {
                if (!data[num].ContainsKey(mk.Time))
                {
                    data[focusedmark[0]].Remove(mk.Time);
                    mk.Type = (byte)num;
                    data[num].Add(mk.Time, mk);
                    focusedmark[0] = num;
                    focusedmark[1] = data[num].IndexOfKey(mk.Time);
                    return true;
                }
            }
            return false;
        }
        public bool CopyMark(int num)
        {
            Mark mk = SelectedMark;
            if (SelectedMark != null)
            {
                if (!data[num].ContainsKey(mk.Time))
                {
                    ExMark exmk = mk as ExMark;
                    if (exmk != null)
                    {
                        ExMark newexmk = CreateExMark(exmk.Position.X, exmk.Position.Y, num, exmk.Time, exmk.EndTime, exmk.Rotation);
                        data[num].Add(newexmk.Time, newexmk);
                        focusedmark[0] = num;
                        focusedmark[1] = data[num].IndexOfKey(newexmk.Time);
                    }
                    else
                    {
                        Mark newmk = CreateMark(mk.Position.X, mk.Position.Y, num, mk.Time, mk.Rotation);
                        data[num].Add(newmk.Time, newmk);
                        focusedmark[0] = num;
                        focusedmark[1] = data[num].IndexOfKey(newmk.Time);
                    }
                    return true;
                }
            }
            return false;
        }
        public bool DeleteSelectedMark()
        {
            if (focusedmark[0] >= 0 && focusedmark[0] < 10)
            {
                data[focusedmark[0]].RemoveAt(focusedmark[1]);
                focusedmark[0] = -1;
                focusedmark[1] = -1;
                return true;
            }
            return false;
        }
        public bool DeleteSelectedMarks()
        {
            if (recstart.X != -1 && recstart.Y != -1)
            {
                float width = Math.Abs(recstart.X - recend.X);
                int starty = (recstart.Y <= recend.Y ? recstart.Y : recend.Y);
                int height = Math.Abs(recstart.Y - recend.Y) + 1;
                float starttime = (recstart.X <= recend.X ? recstart.X : recend.X);
                float endtime = starttime + width;
                for (int i = starty; i < starty + height; i++)
                {
                    int iter = 0;
                    if (data[i].Count == 0) continue;
                    do
                    {
                        if (data[i].Keys[iter] >= starttime && data[i].Keys[iter] <= endtime)
                        {
                            data[i].RemoveAt(iter);
                        }
                        else
                        {
                            iter++;
                        }
                    } while (iter < data[i].Count);
                }
                return true;
            }
            return false;
        }
        public bool ShiftSelectedMark(int dn)
        {
            if (focusedmark[0] >= 0 && focusedmark[0] < 10)
            {
                if (dn > 0)
                {
                    if (focusedmark[1] < data[focusedmark[0]].Count - dn)
                    {
                        focusedmark[1] += dn;
                        return true;
                    }
                }
                else if (dn < 0)
                {
                    if (focusedmark[1] > -dn - 1)
                    {
                        focusedmark[1] += dn;
                        return true;
                    }
                }
            }
            return false;
        }
        public bool ShiftSelectedMarkInAll(int dn)
        {
            if (focusedmark[0] >= 0 && focusedmark[0] < 10)
            {
                if (dn > 0)
                {
                    int minimum = -1;
                    int point = 0;
                    float time = data[focusedmark[0]].Keys[focusedmark[1]];
                    float timediff = float.MaxValue;
                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < data[i].Count; j++)
                        {
                            if (i == focusedmark[0] && j == focusedmark[1]) continue;
                            if (time < data[i].Keys[j])
                            {
                                if (timediff > data[i].Keys[j] - time)
                                {
                                    minimum = i;
                                    point = j;
                                    timediff = data[i].Keys[j] - time;
                                }
                                break;
                            }
                            if (time == data[i].Keys[j] && focusedmark[0] < i)
                            {
                                if (timediff > data[i].Keys[j] - time)
                                {
                                    minimum = i;
                                    point = j;
                                    timediff = data[i].Keys[j] - time;
                                }
                                break;
                            }
                        }
                    }
                    if (minimum != -1)
                    {
                        focusedmark[0] = minimum;
                        focusedmark[1] = point;
                        return true;
                    }
                }
                else if (dn < 0)
                {
                    int minimum = -1;
                    int point = 0;
                    float time = data[focusedmark[0]].Keys[focusedmark[1]];
                    float timediff = float.MaxValue;
                    for (int i = 9; i >= 0; i--)
                    {
                        for (int j = data[i].Count - 1; j >= 0; j--)
                        {
                            if (i == focusedmark[0] && j == focusedmark[1]) continue;
                            if (time > data[i].Keys[j])
                            {
                                if (timediff > time - data[i].Keys[j])
                                {
                                    minimum = i;
                                    point = j;
                                    timediff = time - data[i].Keys[j];
                                }
                                break;
                            }
                            if (time == data[i].Keys[j] && focusedmark[0] > i)
                            {
                                if (timediff > data[i].Keys[j] - time)
                                {
                                    minimum = i;
                                    point = j;
                                    timediff = data[i].Keys[j] - time;
                                }
                                break;
                            }
                        }
                    }
                    if (minimum != -1)
                    {
                        focusedmark[0] = minimum;
                        focusedmark[1] = point;
                        return true;
                    }
                }
            }
            return false;
        }
        public bool CopyPreviousMarkAngle()
        {
            if (focusedmark[0] >= 0 && focusedmark[0] < 10 && focusedmark[1] > 0)
            {
                Mark beforemk = data[focusedmark[0]].Values[focusedmark[1] - 1];
                Mark mk = data[focusedmark[0]].Values[focusedmark[1]];
                mk.Rotation = beforemk.Rotation;
                return true;
            }
            return false;
        }
        public bool CopyPreviousMarkPosition()
        {
            if (focusedmark[0] >= 0 && focusedmark[0] < 10 && focusedmark[1] > 0)
            {
                Mark beforemk = data[focusedmark[0]].Values[focusedmark[1] - 1];
                Mark mk = data[focusedmark[0]].Values[focusedmark[1]];
                mk.Position = beforemk.Position;
                return true;
            }
            return false;
        }
        public bool FusionMarks()
        {
            if (recstart.X != -1 && recstart.Y != -1)
            {
                Mark[] mks = GetAreaData();
                if (mks.Length == 2 && mks[0].Type == mks[1].Type)
                {
                    //cast check
                    ExMark exmk = mks[0] as ExMark;
                    if (exmk == null)
                    {
                        exmk = mks[1] as ExMark;
                        if (exmk == null)
                        {
                            exmk = CreateExMark(mks[0].Position.X, mks[0].Position.Y, mks[0].Type, mks[0].Time, mks[1].Time, mks[0].Rotation);
                            data[mks[0].Type].Remove(mks[0].Time);
                            data[mks[0].Type].Remove(mks[1].Time);
                            data[mks[0].Type].Add(mks[0].Time, exmk);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool DeFusionExMark()
        {
            if (focusedmark[0] >= 0 && focusedmark[0] < 10)
            {
                Mark mk = data[focusedmark[0]].Values[focusedmark[1]];
                ExMark exmk = mk as ExMark;
                if (exmk != null)
                {
                    data[focusedmark[0]].Remove(exmk.Time);
                    Mark mk1 = CreateMark(exmk.Position.X, exmk.Position.Y, exmk.Type, exmk.Time, exmk.Rotation);
                    Mark mk2 = CreateMark(exmk.Position.X, exmk.Position.Y, exmk.Type, exmk.EndTime, exmk.Rotation);
                    data[focusedmark[0]].Add(mk1.Time, mk1);
                    data[focusedmark[0]].Add(mk2.Time, mk2);
                    return true;
                }
            }
            return false;
        }
        public bool MoveMark(int dx, Direction dir)
        {
            if (focusedmark[0] >= 0 && focusedmark[0] < 10)
            {
                if (dir == Direction.Up)
                {
                    Mark mk = data[focusedmark[0]].Values[focusedmark[1]];
                    mk.Position = new Vector2(mk.Position.X, (mk.Position.Y >= dx ? mk.Position.Y - dx : 0));
                    return true;
                }
                else if (dir == Direction.Left)
                {
                    Mark mk = data[focusedmark[0]].Values[focusedmark[1]];
                    mk.Position = new Vector2((mk.Position.X >= dx ? mk.Position.X - dx : 0), mk.Position.Y);
                    return true;
                }
                else if (dir == Direction.Down)
                {
                    Mark mk = data[focusedmark[0]].Values[focusedmark[1]];
                    mk.Position = new Vector2(mk.Position.X, (mk.Position.Y <= 450 - dx ? mk.Position.Y + dx : 0));
                    return true;
                }
                else if (dir == Direction.Right)
                {
                    Mark mk = data[focusedmark[0]].Values[focusedmark[1]];
                    mk.Position = new Vector2((mk.Position.X <= 800 - dx ? mk.Position.X + dx : 0), mk.Position.Y);
                    return true;
                }
            }
            return false;
        }
        public bool MoveMarks(int dx, Direction dir)
        {
            if (recstart.X != -1 && recstart.Y != -1)
            {
                Mark[] mks = GetAreaData();
                for (int i = 0; i < mks.Length; i++)
                {
                    if (dir == Direction.Up)
                    {
                        mks[i].Position = new Vector2(mks[i].Position.X, (mks[i].Position.Y >= dx ? mks[i].Position.Y - dx : 0));
                    }
                    else if (dir == Direction.Left)
                    {
                        mks[i].Position = new Vector2((mks[i].Position.X >= dx ? mks[i].Position.X - dx : 0), mks[i].Position.Y);
                    }
                    else if (dir == Direction.Down)
                    {
                        mks[i].Position = new Vector2(mks[i].Position.X, (mks[i].Position.Y <= 450 - dx ? mks[i].Position.Y + dx : 0));
                    }
                    else if (dir == Direction.Right)
                    {
                        mks[i].Position = new Vector2((mks[i].Position.X <= 800 - dx ? mks[i].Position.X + dx : 0), mks[i].Position.Y);
                    }
                }
                return true;
            }
            return false;
        }

        private static Mark CreateMark(float x, float y, int type, float time, float angle)
        {
            return new Mark(Utility.device, Utility.resourceManager, Utility.skin, type, x, y, time, angle);
        }
        private static ExMark CreateExMark(float x, float y, int type, float time, float endtime, float angle)
        {
            return new ExMark(Utility.device, Utility.resourceManager, Utility.skin, type, x, y, time, endtime, angle, Utility.circlepoints);
        }
    }
}
