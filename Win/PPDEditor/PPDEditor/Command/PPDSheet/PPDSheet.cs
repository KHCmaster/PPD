using MoreLinq;
using PPDEditorCommon;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDEditor.Command.PPDSheet
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
        readonly int[] defaultDrawingOrder = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        public event EventHandler DisplayDataChanged;
        public event EventHandler CommandChanged;
        string name;
        CustomPoint recstart = new CustomPoint(-1, -1);
        CustomPoint recend = new CustomPoint(-2, -1);
        private float bpm = 100;
        private float bpmstart;
        private int defaultinterval = 240;
        private DisplayLineMode displayMode = DisplayLineMode.Fourth;
        private DisplayBeatType beatType = DisplayBeatType.Fourth;
        int[] focusedmark = { -1, -1 };
        SortedList<float, Mark>[] data;
        SortedList<float, SameTimingMarks> data2;
        List<Mark> dataForColorDraw;
        CommandManager commandManager;
        const int buttonnum = 10;
        public PPDSheet()
        {
            dataForColorDraw = new List<Mark>();
            commandManager = new CommandManager();
            commandManager.CommandChanged += commandManager_CommandChanged;
            data = new SortedList<float, Mark>[buttonnum];
            for (int i = 0; i < buttonnum; i++)
            {
                data[i] = new SortedList<float, Mark>();
            }
            data2 = new SortedList<float, SameTimingMarks>();
        }

        void commandManager_CommandChanged(CommandType commandType)
        {
            if ((commandType & CommandType.Time) == CommandType.Time)
            {
                ChangeDrawData();
            }
            if (CommandChanged != null)
            {
                CommandChanged.Invoke(this, EventArgs.Empty);
            }
        }

        private void ChangeDrawData()
        {
            foreach (SameTimingMarks marks in data2.Values)
            {
                marks.Clear();
            }
            for (int i = 0; i < data.Length; i++)
            {
                foreach (KeyValuePair<float, Mark> pair in data[i])
                {
                    if (!data2.TryGetValue(pair.Key, out SameTimingMarks marks))
                    {
                        marks = new SameTimingMarks();
                        data2.Add(pair.Key, marks);
                    }
                    marks.Add(pair.Value);
                }
            }
            var removes = data2.Where(p => p.Value.Count == 0).Select(p => p.Key).ToArray();
            foreach (var remove in removes)
            {
                data2.Remove(remove);
            }
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
        public DisplayLineMode DisplayMode
        {
            get
            {
                return displayMode;
            }
            set
            {
                displayMode = value;
                if (DisplayDataChanged != null)
                {
                    DisplayDataChanged.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public DisplayBeatType BeatType
        {
            get
            {
                return beatType;
            }
            set
            {
                beatType = value;
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

        public bool Undo()
        {
            if (commandManager.CanUndo)
            {
                commandManager.Undo();
                return commandManager.CanUndo;
            }
            return false;
        }

        public bool CanUndo
        {
            get
            {
                return commandManager.CanUndo;
            }
        }

        public bool Redo()
        {
            if (commandManager.CanRedo)
            {
                commandManager.Redo();
                return commandManager.CanRedo;
            }
            return false;
        }

        public bool CanRedo
        {
            get
            {
                return commandManager.CanRedo;
            }
        }

        public SortedList<float, Mark>[] Data
        {
            get
            {
                return data;
            }
        }

        public Mark[] GetAreaData()
        {
            return GetAreaData(false);
        }

        public Mark[] GetAreaData(bool onlyHead)
        {
            if (recstart.X == -1 || recstart.Y == -1)
            {
                return new Mark[0];
            }
            var rows = WindowUtility.TimeLineForm.RowManager.OrderedVisibleRows;
            var width = Math.Abs(recstart.X - recend.X);
            int starty = (recstart.Y <= recend.Y ? recstart.Y : recend.Y);
            int height = Math.Abs(recstart.Y - recend.Y) + 1;
            var selectedRows = new List<int>();
            for (int i = starty; i < Math.Min(starty + height, rows.Length); i++)
            {
                selectedRows.Add(rows[i]);
            }
            selectedRows.Sort();
            float starttime = (recstart.X <= recend.X ? recstart.X : recend.X);
            float endtime = starttime + width;
            int num = 0;
            int[] startnum = new int[10];
            for (int i = 0; i < startnum.Length; i++)
            {
                startnum[i] = -1;
            }
            foreach (var row in selectedRows)
            {
                bool first = true;
                for (int j = 0; j < data[row].Count; j++)
                {
                    if (data[row].Keys[j] < starttime) continue;
                    if (data[row].Keys[j] > endtime) break;
                    if (first)
                    {
                        first = false;
                        startnum[row] = j;
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
                foreach (var row in selectedRows)
                {
                    if (startnum[row] == -1 || startnum[row] >= data[row].Count)
                    {
                        continue;
                    }
                    if (data[row].Keys[startnum[row]] > endtime)
                    {
                        startnum[row] = -1;
                        continue;
                    }
                    if (minimumtime > data[row].Keys[startnum[row]])
                    {
                        minimumnum = row;
                        minimumtime = data[row].Keys[startnum[row]];
                        continue;
                    }
                }
                if (minimumnum == -1)
                {
                    break;
                }
                else
                {
                    mks[iter] = data[minimumnum].Values[startnum[minimumnum]];
                    iter++;
                    startnum[minimumnum]++;
                    if (onlyHead)
                    {
                        return new Mark[] { mks[0] };
                    }
                }
            }
            return mks;
        }

        public int AreaSelectionCount
        {
            get
            {
                return GetAreaData().Length;
            }
        }

        public Mark AddMark(float time, int num, bool selectMark = true)
        {
            return AddMark(time, 400, 225, 0, num, 0, selectMark);
        }
        public Mark AddMark(float time, float x, float y, float rotation, int num, uint id, bool selectMark = true)
        {
            return AddMark(time, x, y, rotation, num, id, null, selectMark);
        }
        public Mark AddMark(float time, float x, float y, float rotation, int num, uint id, KeyValuePair<string, string>[] parameters, bool selectMark = true)
        {
            var position = Vector2.Clamp(new Vector2(x, y), Vector2.Zero, new Vector2(800, 450));
            rotation = NormalizeAngle(rotation);
            if (num >= 0 && num < 10)
            {
                if (!data[num].ContainsKey(time) && !checklongmark(num, time))
                {
                    var mk = CreateMark(position.X, position.Y, (ButtonType)num, time, rotation, id);
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            mk[parameter.Key] = parameter.Value;
                        }
                    }
                    var command = new AddMarkCommand(data, mk);
                    commandManager.AddCommand(command);
                    if (selectMark)
                    {
                        focusedmark[0] = num;
                        focusedmark[1] = data[num].IndexOfKey(time);
                    }
                    return mk;
                }
            }
            return null;
        }
        public Mark AddExMark(float time, float endtime, int num, bool selectMark = true)
        {
            return AddExMark(time, endtime, 400, 225, 0, num, 0, selectMark);
        }
        public Mark AddExMark(float time, float endtime, float x, float y, float rotation, int num, uint id, bool selectMark = true)
        {
            return AddExMark(time, endtime, x, y, rotation, num, id, null, selectMark);
        }
        public Mark AddExMark(float time, float endtime, float x, float y, float rotation, int num, uint id, KeyValuePair<string, string>[] parameters, bool selectMark = true)
        {
            var position = Vector2.Clamp(new Vector2(x, y), Vector2.Zero, new Vector2(800, 450));
            rotation = NormalizeAngle(rotation);
            if (num >= 0 && num < 10)
            {
                if (!data[num].ContainsKey(time) && !checklongmark(num, time))
                {
                    Mark mk = CreateExMark(position.X, position.Y, (ButtonType)num, time, endtime, rotation, id);
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            mk[parameter.Key] = parameter.Value;
                        }
                    }
                    var command = new AddMarkCommand(data, mk);
                    commandManager.AddCommand(command);
                    if (selectMark)
                    {
                        focusedmark[0] = num;
                        focusedmark[1] = data[num].IndexOfKey(time);
                    }
                    return mk;
                }
            }
            return null;
        }
        public void Swap(ButtonType first, ButtonType second)
        {
            if (first == second) return;
            var command = new SwapLineCommand(data, first, second);
            commandManager.AddCommand(command);
        }
        public bool[] UpdateMark(float time, float speedscale, EventManager em)
        {
            bool[] ret = new bool[buttonnum];
            if (data2 != null)
            {
                var last = WindowUtility.EventManager.GetEvent(0);
                if (last == null)
                {
                    last = new EventData();
                }
                foreach (KeyValuePair<float, SameTimingMarks> pair in data2)
                {
                    var current = WindowUtility.EventManager.GetEvent(pair.Key);
                    if (last != current && current != null)
                    {
                        last = current;
                    }
                    foreach (Mark mk in pair.Value)
                    {
                        if (mk is ExMark exmk)
                        {
                            ret[(int)mk.Type] |= (exmk.ExUpdate(em.GetCorrectTime(time, mk.Time), speedscale * bpm,
                                last.DisplayState, last.NoteType, last.SlideScale, em.ReleaseSound((int)mk.Type), pair.Value.SameTimings) == 1);
                        }
                        else
                        {
                            ret[(int)mk.Type] |= (mk.Update(em.GetCorrectTime(time, mk.Time), speedscale * bpm,
                                last.DisplayState, last.NoteType, pair.Value.SameTimings) == 1);
                        }
                        mk.GraphicUpdate();
                    }
                }
            }
            return ret;
        }
        public void DrawMark()
        {
            if (data2 != null)
            {
                int miny = -1, maxy = -1;
                float minx = -1, maxx = -1;
                var areaDraws = new Dictionary<int, bool>();
                if (AreaSelectionEnabled && recstart.Y != -1 && recend.Y != -1)
                {
                    miny = Math.Min(recstart.Y, recend.Y);
                    maxy = Math.Max(recstart.Y, recend.Y);
                    minx = Math.Min(recstart.X, recend.X);
                    maxx = Math.Max(recstart.X, recend.X);
                    var rows = WindowUtility.TimeLineForm.RowManager.OrderedVisibleRows;
                    for (int i = miny; i < Math.Min(maxy + 1, rows.Length); i++)
                    {
                        areaDraws.Add(rows[i], true);
                    }
                }

                dataForColorDraw.Clear();
                var last = WindowUtility.EventManager.GetEvent(0);
                int[] drawingOrder = last != null ? last.DrawingOrder : defaultDrawingOrder;
                foreach (KeyValuePair<float, SameTimingMarks> pair in data2)
                {
                    var current = WindowUtility.EventManager.GetEvent(pair.Key);
                    if (last != current && current != null)
                    {
                        drawingOrder = current.DrawingOrder;
                        last = current;
                    }
                    foreach (Mark mk in pair.Value.OrderByDescending(m => drawingOrder[(int)m.Type]))
                    {
                        if (mk.Hidden)
                        {
                            if (areaDraws.ContainsKey((int)mk.Type) && minx <= mk.Time && mk.Time <= maxx)
                            {
                                mk.DrawOnlyMark();
                            }
                        }
                        else
                        {
                            dataForColorDraw.Add(mk);
                            mk.Draw();
                        }
                    }
                }
                foreach (Mark mk in dataForColorDraw)
                {
                    if (mk is ExMark exmk)
                    {
                        exmk.gageDraw();
                    }
                }
                foreach (Mark mk in dataForColorDraw)
                {
                    mk.ColorDraw();
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
                    if (focusedmark[1] < 0 || focusedmark[1] >= data[focusedmark[0]].Count)
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
        public void MoveMark(Point pos, int mode, bool shiftKey, bool ctrlKey, bool altKey)
        {
            //0...pos
            //1...angle
            if (focusedmark[0] >= 0 && focusedmark[0] < 10)
            {
                if (data[focusedmark[0]].TryGetValue(data[focusedmark[0]].Keys[focusedmark[1]], out Mark mk))
                {
                    if (mode == 0)
                    {
                        if (pos.X <= 0) pos.X = 0;
                        if (pos.X >= 800) pos.X = 800;
                        if (pos.Y <= 0) pos.Y = 0;
                        if (pos.Y >= 450) pos.Y = 450;

                        var command = commandManager.LastDoneCommand as ChangeMarkPosOrAngleCommand;
                        if (command == null || command.Mark != mk || command.TType != ChangeMarkPosOrAngleCommand.TransType.Position)
                        {
                            command = new ChangeMarkPosOrAngleCommand(mk, ChangeMarkPosOrAngleCommand.TransType.Position, new Vector2(pos.X, pos.Y), 0);
                        }
                        else
                        {
                            command.Position = new Vector2(pos.X, pos.Y);
                        }
                        commandManager.AddCommand(command);
                    }
                    else if (mode == 1)
                    {
                        var rotation = GetNormalizedRotation(new Vector2(pos.X, pos.Y), mk.Position, shiftKey, ctrlKey, altKey);

                        var command = commandManager.LastDoneCommand as ChangeMarkPosOrAngleCommand;
                        if (command == null || command.Mark != mk || command.TType != ChangeMarkPosOrAngleCommand.TransType.Rotation)
                        {
                            command = new ChangeMarkPosOrAngleCommand(mk, ChangeMarkPosOrAngleCommand.TransType.Rotation, Vector2.Zero, rotation);
                        }
                        else
                        {
                            command.Rotation = rotation;
                        }
                        commandManager.AddCommand(command);
                    }
                }
            }
            else
            {
                var marks = GetAreaData();
                if (marks.Length > 0)
                {
                    var rotation = GetNormalizedRotation(new Vector2(pos.X, pos.Y), marks[0].Position, shiftKey, ctrlKey, altKey);
                    var command = commandManager.LastDoneCommand as ChangeMarksAngleCommand;
                    if (command == null || !CheckSameArray(command.Marks, marks))
                    {
                        command = new ChangeMarksAngleCommand(marks, rotation);
                    }
                    else
                    {
                        command.Angle = rotation;
                    }
                    commandManager.AddCommand(command);
                }
            }
        }

        private bool CheckSameArray<T>(T[] array1, T[] array2) where T : class
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        }

        private float GetNormalizedRotation(Vector2 pos1, Vector2 pos2, bool shiftKey, bool ctrlKey, bool altKey)
        {
            float rotation = 0;
            int restriction = 0;
            if (shiftKey && ctrlKey && altKey)
            {
                restriction = PPDStaticSetting.Angles[7];
            }
            else if (shiftKey && ctrlKey)
            {
                restriction = PPDStaticSetting.Angles[4];
            }
            else if (shiftKey && altKey)
            {
                restriction = PPDStaticSetting.Angles[5];
            }
            else if (ctrlKey && altKey)
            {
                restriction = PPDStaticSetting.Angles[6];
            }
            else if (shiftKey)
            {
                restriction = PPDStaticSetting.Angles[1];
            }
            else if (ctrlKey)
            {
                restriction = PPDStaticSetting.Angles[2];
            }
            else if (altKey)
            {
                restriction = PPDStaticSetting.Angles[3];
            }
            else
            {
                restriction = PPDStaticSetting.Angles[0];
            }

            if (restriction == 0)
            {
                var length = (float)Math.Sqrt(Math.Pow(Math.Abs(pos1.X - pos2.X), 2) + Math.Pow(Math.Abs(pos1.Y - pos2.Y), 2));
                var d = (float)Math.Acos((pos1.X - pos2.X) / length);
                if (pos1.Y > pos2.Y)
                {
                    d = (float)(Math.PI * 2 - d);
                }
                rotation = d;
            }
            else
            {
                Vector2 vec = pos1 - pos2;
                var length = vec.Length();
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

                var poses = new List<Vector2>();
                int angleSplit = 90 / restriction;
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
                    Vector2 p = vector - pos1 + pos2;
                    length = p.Length();
                    if (length < nearestlength)
                    {
                        nearestlength = length;
                        nearestindex = index;
                    }
                    index++;
                }
                if (nearestindex >= 0)
                {
                    var d = (float)(Math.PI * ((anglebase + 90 * ((float)nearestindex / angleSplit))) / 180);
                    rotation = (float)(Math.PI * 2 - d);
                }
            }
            return rotation;
        }

        public void InterpolateAngle(bool clockwise)
        {
            var marks = GetAreaData();
            if (marks.Length < 2)
            {
                return;
            }

            float startAngle = marks[0].Rotation, endAngle = marks[marks.Length - 1].Rotation;
            float startTime = marks[0].Time, endTime = marks[marks.Length - 1].Time;
            if (startTime == endTime)
            {
                return;
            }
            if (clockwise)
            {
                endAngle -= (float)(Math.PI * 2);
            }
            StartGroupCommand();
            foreach (Mark mark in marks)
            {
                float angle = (startAngle * (endTime - mark.Time) + endAngle * (mark.Time - startTime)) / (endTime - startTime);
                angle = NormalizeAngle(angle);
                var command = new ChangeMarkPosOrAngleCommand(mark, ChangeMarkPosOrAngleCommand.TransType.Rotation, Vector2.Zero, angle);
                commandManager.AddCommand(command);
            }
            EndGroupCommand();
        }

        public void InterpolatePosition()
        {
            var marks = GetAreaData();
            if (marks.Length < 2)
            {
                return;
            }

            Vector2 startPosition = marks[0].Position, endPosition = marks[marks.Length - 1].Position;
            float startTime = marks[0].Time, endTime = marks[marks.Length - 1].Time;
            if (startTime == endTime)
            {
                return;
            }
            StartGroupCommand();
            foreach (Mark mark in marks)
            {
                var position = new Vector2((startPosition.X * (endTime - mark.Time) + endPosition.X * (mark.Time - startTime)) / (endTime - startTime),
                    (startPosition.Y * (endTime - mark.Time) + endPosition.Y * (mark.Time - startTime)) / (endTime - startTime));

                var command = new ChangeMarkPosOrAngleCommand(mark, ChangeMarkPosOrAngleCommand.TransType.Position, position, 0);
                commandManager.AddCommand(command);
            }
            EndGroupCommand();
        }

        private float NormalizeAngle(float angle)
        {
            double div = angle / (Math.PI * 2);
            if (0 <= div && div <= 1)
            {

            }
            else
            {
                var temp = (int)Math.Floor(div);
                angle -= (float)(temp * Math.PI * 2);
            }
            return angle;
        }

        public Mark[] GetSortedData()
        {
            return Utility.GetSortedData<Mark>(data);
        }

        public MarkData[] GetSortedDataAsMarkData()
        {
            var mks = GetSortedData();
            MarkData[] ret = new MarkData[mks.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = mks[i].Convert();
            }
            return ret;
        }

        public void MoveLine(int num, float dt)
        {
            var command = commandManager.LastDoneCommand as ShiftMarkLineTimeCommand;
            if (command == null || command.Index != num)
            {
                command = new ShiftMarkLineTimeCommand(data, num, dt);
            }
            else
            {
                command.Dt += dt;
            }
            commandManager.AddCommand(command);
        }
        public bool SetData(Vector2[] positions, float[] angles, bool reverse, int mode, bool withangle)
        {
            if (mode == 0)
            {
                if (focusedmark[0] == -1) return false;
                var command = new SetDataFromSelectionCommand(data, positions, angles, reverse, withangle, data[focusedmark[0]].Values[focusedmark[1]]);
                commandManager.AddCommand(command);
            }
            else if (mode == 1)
            {
                var mks = GetAreaData();
                if (mks.Length == 0) return false;
                var command = new SetDataInSelectionCommand(mks, positions, angles, reverse, withangle);
                commandManager.AddCommand(command);
            }
            return true;
        }
        public bool GetSortedData(bool reverse, bool inAreaSelection, out Mark[] mks)
        {
            mks = null;
            //mode 0 is line 1 is area
            if (!inAreaSelection)
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
            if (data[focusedmark[0]].ContainsKey(time))
            {
                return false;
            }
            if (data[focusedmark[0]].TryGetValue(data[focusedmark[0]].Keys[focusedmark[1]], out Mark mk))
            {
                MoveMarkInSame(mk, time);
                return true;
            }
            return false;
        }
        public void MoveMarkInSame(Mark mk, float time)
        {
            var command = new ShiftMarkTimeCommand(data, focusedmark[0], mk, time);
            commandManager.AddCommand(command);
            focusedmark[1] = data[focusedmark[0]].IndexOfKey(mk.Time);
        }
        public bool CopyMarkInSame(float time)
        {
            Mark mk = SelectedMark;
            if (mk != null && !data[focusedmark[0]].ContainsKey(time))
            {
                if (mk is ExMark exmk)
                {
                    if (!checklongmark(focusedmark[0], time))
                    {
                        var newexmk = CreateExMark(exmk.Position.X, exmk.Position.Y, (ButtonType)focusedmark[0], time, time + exmk.EndTime - exmk.Time, exmk.Rotation, GetCorrectID(mk));
                        newexmk.CopyParameters(exmk);
                        var command = new AddMarkCommand(data, newexmk);
                        commandManager.AddCommand(command);
                    }
                }
                else
                {
                    var newmk = CreateMark(mk.Position.X, mk.Position.Y, (ButtonType)focusedmark[0], time, mk.Rotation, GetCorrectID(mk));
                    newmk.CopyParameters(mk);
                    var command = new AddMarkCommand(data, newmk);
                    commandManager.AddCommand(command);
                    focusedmark[1] = data[focusedmark[0]].IndexOfKey(time);
                }
                return true;
            }
            return false;
        }
        public uint GetCorrectID(Mark mark)
        {
            // if id == 0 return 0
            // else getNextID
            if (mark.ID == 0)
            {
                return 0;
            }
            else
            {
                return WindowUtility.LayerManager.IDProvider.Next();
            }
        }
        public uint GetCorrectID(MarkData mark)
        {
            // if id == 0 return 0
            // else getNextID
            if (mark.ID == 0)
            {
                return 0;
            }
            else
            {
                return WindowUtility.LayerManager.IDProvider.Next();
            }
        }
        private bool checklongmark(int num, float time)
        {
            foreach (Mark mk in data[num].Values)
            {
                if (mk is ExMark exmk && exmk.Time <= time && time <= exmk.EndTime)
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
                data[num].TryGetValue(f, out Mark mk);
                if (mk is ExMark exmk)
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
            return ChangeMarkType(SelectedMark, num);
        }
        public bool ChangeMarkType(Mark mk, int num)
        {
            num = Utility.Clamp(num, 0, (int)ButtonType.L);
            if (mk != null)
            {
                if (!data[num].ContainsKey(mk.Time))
                {
                    var command = new ChangeMarkTypeCommand(data, mk, (ButtonType)num);
                    commandManager.AddCommand(command);
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
                    if (mk is ExMark exmk)
                    {
                        var newexmk = CreateExMark(exmk.Position.X, exmk.Position.Y, (ButtonType)num, exmk.Time, exmk.EndTime, exmk.Rotation, GetCorrectID(mk));
                        newexmk.CopyParameters(exmk);
                        var command = new AddMarkCommand(data, newexmk);
                        commandManager.AddCommand(command);
                        focusedmark[0] = num;
                        focusedmark[1] = data[num].IndexOfKey(newexmk.Time);
                    }
                    else
                    {
                        var newmk = CreateMark(mk.Position.X, mk.Position.Y, (ButtonType)num, mk.Time, mk.Rotation, GetCorrectID(mk));
                        newmk.CopyParameters(mk);
                        var command = new AddMarkCommand(data, newmk);
                        commandManager.AddCommand(command);
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
                var command = new DeleteMarksCommand(data, new Mark[] { data[focusedmark[0]].Values[focusedmark[1]] });
                commandManager.AddCommand(command);
                focusedmark[0] = -1;
                focusedmark[1] = -1;
                return true;
            }
            return false;
        }
        public bool DeleteSelectedMarks()
        {
            var mks = GetAreaData();
            if (mks.Length > 0)
            {
                var command = new DeleteMarksCommand(data, mks);
                commandManager.AddCommand(command);
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
        public bool ShiftSelectedMarkInAll(int dn, ButtonType[] typeConverter)
        {
            if (focusedmark[0] < 0 || focusedmark[0] >= 10)
            {
                return false;
            }
            float time = data[focusedmark[0]].Keys[focusedmark[1]];
            var mark = data[focusedmark[0]][time];
            var list = data2[time];
            Mark next;
            if (dn > 0)
            {
                next = list.OrderBy(m => typeConverter[(int)m.Type]).FirstOrDefault(m => typeConverter[(int)m.Type] > typeConverter[(int)mark.Type]);
                if (next == null)
                {
                    // find from next time;
                    var nextTime = data2.Keys.FirstOrDefault(t => t > time);
                    if (nextTime > 0)
                    {
                        list = data2[nextTime];
                        next = list.MinBy(m => typeConverter[(int)m.Type]);
                    }
                }
            }
            else
            {
                next = list.OrderByDescending(m => typeConverter[(int)m.Type]).FirstOrDefault(m => typeConverter[(int)m.Type] < typeConverter[(int)mark.Type]);
                if (next == null)
                {
                    // find from next time;
                    var prevTime = data2.Keys.Reverse().FirstOrDefault(t => t < time);
                    if (prevTime > 0)
                    {
                        list = data2[prevTime];
                        next = list.MaxBy(m => typeConverter[(int)m.Type]);
                    }
                }
            }
            if (next != null)
            {
                focusedmark[0] = (int)next.Type;
                focusedmark[1] = data[focusedmark[0]].IndexOfKey(next.Time);
                return true;
            }
            return false;
        }
        public bool CopyPreviousMarkAngle()
        {
            if (focusedmark[0] >= 0 && focusedmark[0] < 10 && focusedmark[1] > 0)
            {
                Mark beforemk = data[focusedmark[0]].Values[focusedmark[1] - 1];
                Mark mk = data[focusedmark[0]].Values[focusedmark[1]];
                var command = commandManager.LastDoneCommand as ChangeMarkPosOrAngleCommand;
                if (command == null || command.TType != ChangeMarkPosOrAngleCommand.TransType.Rotation || command.Mark != mk)
                {
                    command = new ChangeMarkPosOrAngleCommand(mk, ChangeMarkPosOrAngleCommand.TransType.Rotation, Vector2.Zero, beforemk.Rotation);
                }
                else
                {
                    command.Rotation = beforemk.Rotation;
                }
                commandManager.AddCommand(command);
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
                var command = commandManager.LastDoneCommand as ChangeMarkPosOrAngleCommand;
                if (command == null || command.TType != ChangeMarkPosOrAngleCommand.TransType.Position || command.Mark != mk)
                {
                    command = new ChangeMarkPosOrAngleCommand(mk, ChangeMarkPosOrAngleCommand.TransType.Position, beforemk.Position, 0);
                }
                else
                {
                    command.Position = beforemk.Position;
                }
                commandManager.AddCommand(command);
                return true;
            }
            return false;
        }
        public bool FusionMarks()
        {
            if (recstart.X != -1 && recstart.Y != -1)
            {
                var mks = GetAreaData();
                if (mks.Length == 2 && mks[0].Type == mks[1].Type)
                {
                    //cast check
                    if (!(mks[0] is ExMark) && !(mks[1] is ExMark))
                    {
                        var exmk = CreateExMark(mks[0].Position.X, mks[0].Position.Y, mks[0].Type, mks[0].Time, mks[1].Time, mks[0].Rotation, mks[0].ID);
                        exmk.CopyParameters(mks[0]);
                        var command = new FusionMarkCommand(data, mks[0], mks[1], exmk);
                        commandManager.AddCommand(command);
                        return true;
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
                if (mk is ExMark exmk)
                {
                    var mk1 = CreateMark(exmk.Position.X, exmk.Position.Y, exmk.Type, exmk.Time, exmk.Rotation, exmk.ID);
                    mk1.CopyParameters(exmk);
                    var mk2 = CreateMark(exmk.Position.X, exmk.Position.Y, exmk.Type, exmk.EndTime, exmk.Rotation, 0);
                    var command = new DeFusionMarkCommand(data, mk1, mk2, exmk);
                    commandManager.AddCommand(command);
                    return true;
                }
            }
            return false;
        }
        public bool MoveMark(int dx, Direction dir)
        {
            Mark mk = SelectedMark;
            if (mk != null)
            {
                Vector2 newPos = Vector2.Zero;
                switch (dir)
                {
                    case Direction.Up:
                        newPos = new Vector2(mk.Position.X, (mk.Position.Y >= dx ? mk.Position.Y - dx : 0));
                        break;
                    case Direction.Left:
                        newPos = new Vector2((mk.Position.X >= dx ? mk.Position.X - dx : 0), mk.Position.Y);
                        break;
                    case Direction.Down:
                        newPos = new Vector2(mk.Position.X, (mk.Position.Y <= 450 - dx ? mk.Position.Y + dx : 0));
                        break;
                    case Direction.Right:
                        newPos = new Vector2((mk.Position.X <= 800 - dx ? mk.Position.X + dx : 0), mk.Position.Y);
                        break;
                }
                var command = commandManager.LastDoneCommand as ChangeMarkPosOrAngleCommand;
                if (command == null || command.TType != ChangeMarkPosOrAngleCommand.TransType.Position || command.Mark != mk)
                {
                    command = new ChangeMarkPosOrAngleCommand(mk, ChangeMarkPosOrAngleCommand.TransType.Position, newPos, 0);
                }
                else
                {
                    command.Position = newPos;
                }
                commandManager.AddCommand(command);
                return true;
            }
            return false;
        }
        public bool MoveMark(Mark mark, Vector2 position)
        {
            if (mark != null)
            {
                position = Vector2.Clamp(position, Vector2.Zero, new Vector2(800, 450));
                var command = new ChangeMarkPosOrAngleCommand(mark, ChangeMarkPosOrAngleCommand.TransType.Position, position, 0);
                commandManager.AddCommand(command);
                return true;
            }
            return false;
        }
        public bool MoveMarks(int dx, Direction dir)
        {
            if (recstart.X != -1 && recstart.Y != -1)
            {
                var mks = GetAreaData();
                commandManager.StartGroupCommand();
                for (int i = 0; i < mks.Length; i++)
                {
                    Mark mk = mks[i];
                    Vector2 newPos = Vector2.Zero;
                    switch (dir)
                    {
                        case Direction.Up:
                            newPos = new Vector2(mk.Position.X, (mk.Position.Y >= dx ? mk.Position.Y - dx : 0));
                            break;
                        case Direction.Left:
                            newPos = new Vector2((mk.Position.X >= dx ? mk.Position.X - dx : 0), mk.Position.Y);
                            break;
                        case Direction.Down:
                            newPos = new Vector2(mk.Position.X, (mk.Position.Y <= 450 - dx ? mk.Position.Y + dx : 0));
                            break;
                        case Direction.Right:
                            newPos = new Vector2((mk.Position.X <= 800 - dx ? mk.Position.X + dx : 0), mk.Position.Y);
                            break;
                    }
                    var command = new ChangeMarkPosOrAngleCommand(mk, ChangeMarkPosOrAngleCommand.TransType.Position, newPos, 0);
                    commandManager.AddCommand(command);
                }
                commandManager.EndGroupCommand();
                return true;
            }
            return false;
        }
        public bool RotateMark(Mark mark, float rotation)
        {
            if (mark != null)
            {
                rotation = NormalizeAngle(rotation);
                var command = new ChangeMarkPosOrAngleCommand(mark, ChangeMarkPosOrAngleCommand.TransType.Rotation, Vector2.Zero, rotation);
                commandManager.AddCommand(command);
                return true;
            }
            return false;
        }

        public void AssignID()
        {
            Mark[] marks = null;
            if (SelectedMark != null)
            {
                marks = new Mark[] { SelectedMark };
            }
            else
            {
                marks = GetAreaData();
            }

            if (marks == null && marks.Length == 0)
            {
                return;
            }

            commandManager.StartGroupCommand();
            foreach (Mark mk in marks)
            {
                AssignID(mk);
            }
            commandManager.EndGroupCommand();
        }

        public void AssignID(Mark mk)
        {
            var command = new AssignIDCommand(mk, WindowUtility.LayerManager.IDProvider.Next());
            commandManager.AddCommand(command);
        }

        public void UnassignID(Func<bool> removeParameterCallback)
        {
            Mark[] marks = null;
            if (SelectedMark != null)
            {
                marks = new Mark[] { SelectedMark };
            }
            else
            {
                marks = GetAreaData();
            }

            if (marks == null && marks.Length == 0)
            {
                return;
            }

            var containsHasPatametersMark = marks.Any(m => m.Parameters.Length > 0);
            bool removeParameters = false;
            if (containsHasPatametersMark)
            {
                if (!removeParameterCallback())
                {
                    return;
                }

                removeParameters = true;
            }

            commandManager.StartGroupCommand();
            if (removeParameters)
            {
                foreach (var mark in marks)
                {
                    foreach (var parameter in mark.Parameters)
                    {
                        var command = new RemoveParameterCommand(mark, parameter.Key);
                        commandManager.AddCommand(command);
                    }
                }
            }
            foreach (Mark mk in marks)
            {
                var command = new AssignIDCommand(mk, 0);
                commandManager.AddCommand(command);
            }
            commandManager.EndGroupCommand();
        }

        public void DeleteMark(Mark mk)
        {
            // check selection
            Mark mark = null;
            if (focusedmark[0] >= 0 && focusedmark[0] < 10)
            {
                if (!data[focusedmark[0]].TryGetValue(data[focusedmark[0]].Keys[focusedmark[1]], out mark))
                {
                    mark = null;
                }
            }

            if (mark != null && mark == mk)
            {
                focusedmark[0] = focusedmark[1] = -1;
            }
            var command = new DeleteMarksCommand(data, new Mark[] { mk });
            commandManager.AddCommand(command);
        }

        public void RemoveMark(Mark mark)
        {
            var found = data[(int)mark.Type].Select(p => p.Value).FirstOrDefault(m => m == mark);
            if (found != null)
            {
                var command = new DeleteMarksCommand(data, new Mark[] { mark });
                commandManager.AddCommand(command);
            }
        }

        public void UnassignID(Mark mk)
        {
            foreach (var parameter in mk.Parameters)
            {
                var command = new RemoveParameterCommand(mk, parameter.Key);
                commandManager.AddCommand(command);
            }
            var unassignCommand = new AssignIDCommand(mk, 0);
            commandManager.AddCommand(unassignCommand);
        }

        public void ApplyTrans(Vector2[] poses, Vector2[] dirs, bool applyRotation)
        {
            var mks = GetAreaData();
            if (mks.Length > 0)
            {
                var command = commandManager.LastDoneCommand as ApplyTransCommand;
                if (command == null || !command.CheckSameData(mks, applyRotation))
                {
                    command = new ApplyTransCommand(mks, poses, dirs, applyRotation);
                }
                else
                {
                    command.Poses = poses;
                    command.Dirs = dirs;
                }
                commandManager.AddCommand(command);
            }
        }

        public void DeleteData(int index)
        {
            var command = new DeleteDataCommand(data, index);
            commandManager.AddCommand(command);
        }

        public void ForceToNearBar(Dictionary<Mark, float[]> times)
        {
            var command = new ForceToNearBarCommand(data, times);
            commandManager.AddCommand(command);
        }

        public void ChangeParameter(string key, string value)
        {
            if (focusedmark[0] >= 0 && focusedmark[0] < 10)
            {
                if (data[focusedmark[0]].TryGetValue(data[focusedmark[0]].Keys[focusedmark[1]], out Mark mk))
                {
                    ChangeParameter(mk, key, value);
                }
            }
            else
            {
                var marks = GetAreaData();
                if (marks.Length > 0)
                {
                    foreach (var mk in marks)
                    {
                        ChangeParameter(mk, key, value);
                    }
                }
            }
        }

        public void ChangeParameter(Mark mk, string key, string value)
        {
            if (key == null)
            {
                key = "";
            }
            if (value == null)
            {
                value = "";
            }

            if (mk.ID == 0)
            {
                var assignCommand = new AssignIDCommand(mk, WindowUtility.LayerManager.IDProvider.Next());
                commandManager.AddCommand(assignCommand);
            }
            var command = new ChangeParameterCommand(mk, key, value);
            commandManager.AddCommand(command);
        }

        public void RemoveParameter(string key)
        {

            if (focusedmark[0] >= 0 && focusedmark[0] < 10)
            {
                if (data[focusedmark[0]].TryGetValue(data[focusedmark[0]].Keys[focusedmark[1]], out Mark mk))
                {
                    RemoveParameter(mk, key);
                }
            }
            else
            {
                var marks = GetAreaData();
                if (marks.Length > 0)
                {
                    foreach (var mk in marks)
                    {
                        RemoveParameter(mk, key);
                    }
                }
            }
        }

        public void RemoveParameter(Mark mk, string key)
        {
            var command = new RemoveParameterCommand(mk, key);
            commandManager.AddCommand(command);
        }

        public void ClearHistory()
        {
            commandManager.ClearAll();
        }

        public void StartGroupCommand()
        {
            commandManager.StartGroupCommand();
        }

        public void EndGroupCommand()
        {
            commandManager.EndGroupCommand();
        }

        private static Mark CreateMark(float x, float y, ButtonType type, float time, float angle, uint id)
        {
            return new Mark(Utility.Device, Utility.ResourceManager, PPDEditorSkin.Skin, type, x, y, time, angle, id);
        }
        private static ExMark CreateExMark(float x, float y, ButtonType type, float time, float endtime, float angle, uint id)
        {
            return new ExMark(Utility.Device, Utility.ResourceManager, PPDEditorSkin.Skin, type, x, y, time, endtime, angle, Utility.CirclePoints, id);
        }
    }
}
