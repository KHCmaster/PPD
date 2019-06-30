using PPDEditor.Command.PPDSheet;
using PPDEditor.Forms;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor.Controls
{
    public partial class Seekmain : CustomUserControl
    {
        public event EventHandler Seeked;

        float assistInfoTime;
        AssistType assistType = AssistType.None;
        bool doDragChange;
        Point mouseDownPos;
        TransParentForm tpf;


        MarkSelectMode markSelectMode = MarkSelectMode.None;
        MarkSelectMode onMouseMarkSelectMode = MarkSelectMode.Angle;
        Point pressedPos;
        Point markStartPos;
        MoveMarkState moveMarkState = MoveMarkState.Same;
        int defaultInterval = 240;
        double length = 10;
        double currentTime;
        private float speedScale = 1.0f;
        private float bpm = 100;
        private float bpmStart;
        HScrollBar Hsc;
        MarkData[] copyBuffer;
        TimeLineDrawer timeLineDrawer;

        public Seekmain()
        {
            timeLineDrawer = new TimeLineDrawer();
            ShortcutManager = new PPDEditor.ShortcutManager();
            Hsc = new HScrollBar();
            this.Controls.Add(Hsc);
            Hsc.Scroll += Hsc_Scroll;
            InitializeComponent();
            InitializeBuffer();
            timer1.Interval = 100;
            timer1.Tick += MoveSeek;
            timer2.Interval = 100;
            timer2.Tick += MoveMark;
            timer3.Interval = 100;
            timer3.Tick += ChangeRec;
            timer4.Interval = 100;
            timer4.Tick += MoveAssistInformation;

            for (int i = 0; i < this.Controls.Count; i++)
            {
                this.Controls[i].PreviewKeyDown += Seekmain_PreviewKeyDown;
            }
            this.MouseWheel += Seekmain_MouseWheel;
            this.SizeChanged += Seekmain_SizeChanged;
        }

        void Seekmain_MouseWheel(object sender, MouseEventArgs e)
        {
            var sign = Math.Sign(e.Delta);
            if ((ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                int result = WindowUtility.MainForm.DisplayWidth + 50 * sign;
                if (result < 50) result = 50;
                if (result > 300) result = 300;
                WindowUtility.MainForm.DisplayWidth = result;
            }
            else if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
            }
            else
            {
                int result = Hsc.Value - sign * 100;
                if (result < Hsc.Minimum) result = Hsc.Minimum;
                if (result > Hsc.Maximum) result = Hsc.Maximum;
                if (Hsc.Visible)
                {
                    Hsc.Value = result;
                    DrawAndRefresh();
                }
            }
        }

        public ShortcutManager ShortcutManager
        {
            get;
            private set;
        }

        public DisplayLineMode DisplayMode
        {
            get;
            set;
        }

        public DisplayBeatType BeatType
        {
            get;
            set;
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
                ChangeWidth();
            }
        }
        public float BPMSTART
        {
            get
            {
                return bpmStart;
            }
            set
            {
                bpmStart = value;
                ChangeWidth();
            }
        }
        public int DisplayWidth
        {
            get
            {
                return defaultInterval;
            }
            set
            {
                defaultInterval = value;
                ChangeWidth();
            }
        }

        public void SetSkin()
        {
        }
        public void SetSelectedSheetInfo(float bpm, float bpmOffset, int displayWidth, DisplayLineMode displayMode, DisplayBeatType beatType)
        {
            this.bpm = bpm == 0 ? 1 : bpm;
            this.bpmStart = bpmOffset;
            this.defaultInterval = displayWidth <= 0 ? 1 : displayWidth;
            this.DisplayMode = displayMode;
            this.BeatType = beatType;
            ChangeWidth();
            ChangeSeekPoint();
        }
        public float SpeedScale
        {
            get
            {
                return speedScale;
            }
            set
            {
                speedScale = value;
            }
        }
        void Hsc_Scroll(object sender, ScrollEventArgs e)
        {
            DrawAndRefresh();
        }
        protected override void DrawToBuffer(System.Drawing.Graphics g)
        {
            if (g == null || Hsc == null || WindowUtility.TimeLineForm == null)
            {
                return;
            }

            timeLineDrawer.Draw(new TimeLineDrawParameter(g, Width, Height, BackgroundImage, WindowUtility.LayerManager.SelectedPpdSheet,
                bpm, bpmStart, defaultInterval, DisplayMode, Hsc.Value, (float)currentTime, BeatType));
        }
        public double Length
        {
            get
            {
                return length;
            }
            set
            {
                length = value;
                ChangeWidth();
            }
        }
        private void ChangeWidth()
        {
            Hsc.Maximum = (int)(defaultInterval * bpm * length / 60);
            Hsc.Minimum = 0;
            Hsc.SmallChange = defaultInterval;
            Hsc.LargeChange = defaultInterval;
            DrawAndRefresh();
        }
        public double Currenttime
        {
            get
            {
                return this.currentTime;
            }
            set
            {
                this.currentTime = value;
                ChangeSeekPoint();
                this.OnSeeked();
            }
        }

        public void AssignID()
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            ppdsheet.AssignID();
            UpdateInfo();
        }

        public void UnassignID(Func<bool> removeParameterCallback)
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            ppdsheet.UnassignID(removeParameterCallback);
            UpdateInfo();
        }

        public void DeleteData(int num)
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            ppdsheet.DeleteData(num);
            DrawAndRefresh();
        }
        public void AddData(double time, int num)
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            if (num >= 0 && num < 10)
            {
                time = AdjusttimeWhenFixed((float)time);
                if (!ppdsheet.Data[num].ContainsKey((float)time))
                {
                    ppdsheet.AddMark((float)time, num);
                }
            }
        }
        public void AddData(float time, float x, float y, float rotation, int num, uint id, KeyValuePair<string, string>[] parameters, bool selectMark = true)
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            if (num >= 0 && num < 10)
            {
                if (!ppdsheet.Data[num].ContainsKey((float)time))
                {
                    ppdsheet.AddMark(time, x, y, rotation, num, id, parameters, selectMark);
                }
            }
        }
        public void ExAddData(float time, float endtime, float x, float y, float rotation, int num, uint id, KeyValuePair<string, string>[] parameters, bool selectMark = true)
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return; if (num >= 0 && num < 10)
            {
                if (!ppdsheet.Data[num].ContainsKey((float)time))
                {
                    ppdsheet.AddExMark(time, endtime, x, y, rotation, num, id, parameters, selectMark);
                }
            }
        }
        public void Swap(int first, int second)
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            ppdsheet.Swap((ButtonType)first, (ButtonType)second);
            DrawAndRefresh();
        }
        public bool[] UpdateMark(double time)
        {
            bool[] ret = new bool[10];
            foreach (var sheet in WindowUtility.LayerManager.Sheets)
            {
                var temp = sheet.UpdateMark((float)time, speedScale, WindowUtility.EventManager);
                for (int j = 0; j < ret.Length; j++)
                {
                    ret[j] |= temp[j];
                }
            }
            return ret;
        }
        public void DrawMark()
        {
            foreach (var sheet in WindowUtility.LayerManager.Sheets)
            {
                sheet.DrawMark();
            }
        }
        public Mark SelectedMark
        {
            get
            {
                if (!WindowUtility.MainForm.DrawToggle) return null;
                PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
                if (ppdsheet == null) return null;
                return ppdsheet.SelectedMark;
            }
        }

        public Mark HeadMark
        {
            get
            {
                if (!WindowUtility.MainForm.DrawToggle) return null;
                PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
                if (ppdsheet == null) return null;
                var marks = ppdsheet.GetAreaData(true);
                if (marks.Length > 0)
                {
                    return marks[0];
                }
                return null;
            }
        }

        public MarkSelectMode MarkSelectMode
        {
            get
            {
                return markSelectMode;
            }
        }
        public MarkSelectMode OnMouseMarkSelectMode
        {
            get
            {
                return onMouseMarkSelectMode;
            }
        }
        public void MoveMark(Point pos, int width, int height)
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            var truepos = new Point(pos.X * 800 / width, pos.Y * 450 / height);
            Mark mk = ppdsheet.SelectedMark;
            var marks = ppdsheet.GetAreaData();
            bool shiftKey = ModifierKeys.HasFlag(Keys.Shift),
                ctrlKey = ModifierKeys.HasFlag(Keys.Control),
                altKey = ModifierKeys.HasFlag(Keys.Alt);
            if (mk != null)
            {
                switch (markSelectMode)
                {
                    case MarkSelectMode.None:
                        pressedPos = truepos;
                        markStartPos = new Point((int)mk.Position.X, (int)mk.Position.Y);
                        if (truepos.X >= mk.Position.X - 25 && truepos.X <= mk.Position.X + 25 && truepos.Y >= mk.Position.Y - 25 && truepos.Y <= mk.Position.Y + 25)
                        {
                            markSelectMode = MarkSelectMode.Area;
                        }
                        else if (!PPDStaticSetting.HideToggleArrow && mk.Position.X - 9 <= truepos.X && truepos.X <= mk.Position.X + 9 && mk.Position.Y - 80 <= truepos.Y && truepos.Y <= mk.Position.Y)
                        {
                            markSelectMode = MarkSelectMode.Up;
                        }
                        else if (!PPDStaticSetting.HideToggleArrow && mk.Position.X <= truepos.X && truepos.X <= mk.Position.X + 80 && mk.Position.Y - 9 <= truepos.Y && truepos.Y <= mk.Position.Y + 9)
                        {
                            markSelectMode = MarkSelectMode.Right;
                        }
                        else
                        {
                            markSelectMode = MarkSelectMode.Angle;
                        }

                        break;
                    case MarkSelectMode.Up:
                        int y = (int)markStartPos.Y + truepos.Y - pressedPos.Y;
                        if (y >= 450) y = 450;
                        if (y <= 0) y = 0;
                        var p = ChangePos(new Point((int)(mk.Position.X), y));
                        ppdsheet.MoveMark(p, 0, shiftKey, ctrlKey, altKey);
                        break;
                    case MarkSelectMode.Right:
                        int x = (int)markStartPos.X + truepos.X - pressedPos.X;
                        if (x >= 800) x = 800;
                        if (x <= 0) x = 0;
                        p = ChangePos(new Point(x, (int)(mk.Position.Y)));
                        ppdsheet.MoveMark(p, 0, shiftKey, ctrlKey, altKey);
                        break;
                    case MarkSelectMode.Area:
                        ppdsheet.MoveMark(ChangePos(truepos), 0, shiftKey, ctrlKey, altKey);
                        break;
                    case MarkSelectMode.Angle:
                        ppdsheet.MoveMark(truepos, 1, shiftKey, ctrlKey, altKey);
                        break;
                }

                UpdateInfo();
            }
            else if (marks.Length > 0)
            {
                if (markSelectMode == MarkSelectMode.None)
                {
                    markSelectMode = MarkSelectMode.Angle;
                }
                else if (markSelectMode == MarkSelectMode.Angle)
                {
                    ppdsheet.MoveMark(truepos, 1, shiftKey, ctrlKey, altKey);
                }
                UpdateInfo();
            }
        }
        private Point ChangePos(Point p)
        {
            if (WindowUtility.MainForm.DisplayGrid)
            {
                SquareGrid Grid = WindowUtility.MainForm.Grid;
                int offsetx = Grid.NormalizedOffsetX;
                int offsety = Grid.NormalizedOffsetY;
                p = new Point((p.X + Grid.Width / 2) / Grid.Width * Grid.Width + offsetx, (p.Y + Grid.Height / 2) / Grid.Height * Grid.Height + offsety);
            }
            return p;
        }
        public void OnMouseMove(Point pos, int width, int height)
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            var truepos = new Point(pos.X * 800 / width, pos.Y * 450 / height);
            Mark mk = ppdsheet.SelectedMark;
            var marks = ppdsheet.GetAreaData();
            if (mk != null)
            {
                if (truepos.X >= mk.Position.X - 25 && truepos.X <= mk.Position.X + 25 && truepos.Y >= mk.Position.Y - 25 && truepos.Y <= mk.Position.Y + 25)
                {
                    onMouseMarkSelectMode = MarkSelectMode.Area;
                }
                else if (!PPDStaticSetting.HideToggleArrow && mk.Position.X - 9 <= truepos.X && truepos.X <= mk.Position.X + 9 && mk.Position.Y - 80 <= truepos.Y && truepos.Y <= mk.Position.Y)
                {
                    onMouseMarkSelectMode = MarkSelectMode.Up;
                }
                else if (!PPDStaticSetting.HideToggleArrow && mk.Position.X <= truepos.X && truepos.X <= mk.Position.X + 80 && mk.Position.Y - 9 <= truepos.Y && truepos.Y <= mk.Position.Y + 9)
                {
                    onMouseMarkSelectMode = MarkSelectMode.Right;
                }
                else
                {
                    onMouseMarkSelectMode = MarkSelectMode.Angle;
                }
            }
            else if (marks.Length > 0)
            {
                onMouseMarkSelectMode = MarkSelectMode.Angle;
            }
        }
        public void FinishMarkEdit()
        {
            markSelectMode = MarkSelectMode.None;
        }
        public MarkData[] GetSorteData()
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return new MarkData[0];
            return ppdsheet.GetSortedDataAsMarkData();
        }
        public void MoveLine(int linenum, int diff)
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            float dt = (float)diff / defaultInterval;
            ppdsheet.MoveLine(linenum, dt);
            DrawAndRefresh();
        }
        public bool SetData(Vector2[] positions, float[] angles, bool reverse, int mode, bool withangle)
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return false;
            if (ppdsheet.SetData(positions, angles, reverse, mode, withangle))
            {
                return true;
            }
            return false;
        }
        public bool GetSortedData(bool reverse, bool inAreaSelection, out Mark[] mks)
        {
            mks = null;
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return false;
            var ret = ppdsheet.GetSortedData(reverse, inAreaSelection, out mks);
            return ret;
        }
        private void ChangeSeekPoint()
        {
            float bpminterval = defaultInterval;
            int offset = -(int)(bpmStart * bpminterval / bpm);
            int val = (int)(Currenttime * bpm / 60 * defaultInterval) - this.Width / 2;
            if (val > Hsc.Maximum)
            {
                Hsc.Value = Hsc.Maximum;
            }
            else if (val < Hsc.Minimum)
            {
                Hsc.Value = Hsc.Minimum;
            }
            else
            {
                Hsc.Value = val;
            }
            this.DrawAndRefresh();
        }
        private void OnSeeked()
        {
            this.Seeked?.Invoke(this, EventArgs.Empty);
        }
        private void ChangeRec(object sender, EventArgs e)
        {
            var pos = this.PointToClient(Cursor.Position);
            var num = WindowUtility.TimeLineForm.RowManager.GetNormalRowIndex(pos.Y);
            if (num < 0) num = 0;
            if (num > 9) num = 9;
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            ppdsheet.RecEnd = new CustomPoint((float)(pos.X + Hsc.Value) / defaultInterval / bpm * 60, num);
            if (pos.X < 0)
            {
                int gain = pos.X;
                if (Hsc.Value + gain < Hsc.Minimum)
                {
                    gain = Hsc.Minimum - Hsc.Value;
                }
                Hsc.Value += gain;
            }
            else if (pos.X > this.Width)
            {
                int gain = pos.X - this.Width;
                if (Hsc.Value + gain > Hsc.Maximum)
                {
                    gain = Hsc.Maximum - Hsc.Value;
                }
                Hsc.Value += gain;
            }
            DrawAndRefresh();
        }
        private void MoveMark(object sender, EventArgs e)
        {
            if (moveMarkState == MoveMarkState.Same)
            {
                PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
                if (ppdsheet == null) return;
                var pos = this.PointToClient(Cursor.Position);
                float time = (float)(pos.X + Hsc.Value) / defaultInterval / bpm * 60;
                if (time <= 0)
                {
                    time = 0;
                }
                else if (time > length)
                {
                    time = (float)length;
                }
                if (ppdsheet.MoveMarkInSame(time))
                {
                    DrawAndRefresh();
                }
            }
        }
        private void MoveSeek(object sender, EventArgs e)
        {
            var pos = this.PointToClient(Cursor.Position);
            if (pos.Y >= 0 && pos.Y <= PPDEditorSkin.Skin.TimeLineHeight)
            {
                float time = (float)(Hsc.Value + pos.X) / defaultInterval / bpm * 60;
                if (time < 0)
                {
                    Currenttime = 0;
                }
                else if (time > length)
                {
                    Currenttime = length;
                }
                else
                {
                    Currenttime = time;
                }
                WindowUtility.MainForm.Pause();
                WindowUtility.MainForm.SeekMovie(currentTime);
                this.DrawAndRefresh();
            }
        }

        private void Seekmain_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                ProcessMouseLeftDown(e);
            }
        }

        private void ProcessMouseLeftDown(MouseEventArgs e)
        {
            assistType = AssistType.None;
            doDragChange = false;
            if (e.Y >= 0 && e.Y <= PPDEditorSkin.Skin.TimeLineHeight)
            {
                timer1.Start();
                return;
            }
            var rowIndex = WindowUtility.TimeLineForm.RowManager.GetRowIndex(e.Y);
            if (rowIndex >= 0 && rowIndex < 10)
            {
                PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
                if (ppdsheet == null) return;
                bool foundpoint = false;
                var pos = this.PointToClient(Cursor.Position);
                float time = (float)(pos.X + Hsc.Value) * 60 / defaultInterval / bpm;
                float dt = (float)TimeLineDrawer.CircleWidth / defaultInterval * 60 / bpm / 2;
                if (rowIndex >= 0 && rowIndex < 10)
                {
                    if (ppdsheet.SelectMark(time, rowIndex, dt))
                    {
                        UpdateInfo();
                        SeekToNearSelected();
                        if (!WindowUtility.MainForm.Bpmfixed)
                        {
                            timer2.Start();
                        }
                        ppdsheet.RecStart = new CustomPoint(-1, 0);
                        ppdsheet.RecEnd = new CustomPoint(-1, 0);
                        foundpoint = true;
                        WindowUtility.MainForm.ChangeAreaSelect(false);
                    }
                    if (ModifierKeys == Keys.Control && foundpoint)
                    {
                        moveMarkState = MoveMarkState.CopyToOther;
                    }
                    else if (ModifierKeys == Keys.Shift && foundpoint)
                    {
                        moveMarkState = MoveMarkState.MoveToOther;
                    }
                    DrawAndRefresh();
                }
                if (!foundpoint)
                {
                    ppdsheet.FocusedMark[0] = -1;
                    ppdsheet.FocusedMark[0] = -1;
                    rowIndex = WindowUtility.TimeLineForm.RowManager.GetNormalRowIndex(e.Y);
                    int miny = Math.Min(ppdsheet.RecStart.Y, ppdsheet.RecEnd.Y), height = Math.Abs(ppdsheet.RecStart.Y - ppdsheet.RecEnd.Y);
                    float minx = Math.Min(ppdsheet.RecStart.X, ppdsheet.RecEnd.X), width = Math.Abs(ppdsheet.RecStart.X - ppdsheet.RecEnd.X);
                    if (ppdsheet.AreaSelectionEnabled && miny <= rowIndex && rowIndex <= miny + height && minx <= time && time <= minx + width)
                    {
                        if (ModifierKeys == Keys.Control)
                        {
                            moveMarkState = MoveMarkState.CopyToOther;
                            return;
                        }
                        else if (ModifierKeys == Keys.Shift)
                        {
                            moveMarkState = MoveMarkState.MoveToOther;
                            return;
                        }
                    }
                    ppdsheet.RecStart = new CustomPoint((float)(pos.X + Hsc.Value) / defaultInterval / bpm * 60, rowIndex);
                    ppdsheet.RecEnd = new CustomPoint((float)(pos.X + Hsc.Value) / defaultInterval / bpm * 60, rowIndex);
                    WindowUtility.MainForm.ChangeAreaSelect(true);
                    timer3.Start();
                }
            }
            else
            {
                float drawstarttime = (float)(Hsc.Value - 10) / defaultInterval / bpm * 60;
                float drawendtime = (float)(Hsc.Value + this.Width + 10) / defaultInterval / bpm * 60;
                mouseDownPos = new Point(e.X, e.Y);
                assistInfoTime = GetAssistInformationTime(mouseDownPos, drawstarttime, drawendtime, out assistType);
                timer4.Start();
            }
        }

        private void MoveAssistInformation(object sender, EventArgs e)
        {
            var pos = this.PointToClient(Cursor.Position);
            if (assistType != AssistType.None)
            {
                if (!doDragChange)
                {
                    if (Math.Abs(pos.X - mouseDownPos.X) >= 20)
                    {
                        doDragChange = true;
                        tpf = new TransParentForm();
                        var pb = new PictureBox();
                        switch (assistType)
                        {
                            case AssistType.Event:
                                pb.Image = PPDEditor.Properties.Resources.eventmark;
                                break;
                            case AssistType.Sound:
                                pb.Image = PPDEditor.Properties.Resources.soundmark;
                                break;
                            case AssistType.Lylics:
                                pb.Image = PPDEditor.Properties.Resources.lylicsmark;
                                break;
                        }
                        pb.Location = new System.Drawing.Point(-2, -1);
                        tpf.Controls.Add(pb);
                        tpf.Show();
                        tpf.ClientSize = new System.Drawing.Size(12, 14);
                        tpf.Location = this.PointToScreen(new System.Drawing.Point(mouseDownPos.X - 8, mouseDownPos.Y - 8));
                    }
                }
                else
                {
                    tpf.Location = this.PointToScreen(new System.Drawing.Point(pos.X - 6, pos.Y - 7));
                }
            }
            bool shouldreflesh = false;
            if (pos.X < 0)
            {
                int gain = pos.X;
                if (Hsc.Value + gain < Hsc.Minimum)
                {
                    gain = Hsc.Minimum - Hsc.Value;
                }
                shouldreflesh = true;
                Hsc.Value += gain;
            }
            else if (pos.X > this.Width)
            {
                int gain = pos.X - this.Width;
                if (Hsc.Value + gain > Hsc.Maximum)
                {
                    gain = Hsc.Maximum - Hsc.Value;
                }
                shouldreflesh = true;
                Hsc.Value += gain;
            }
            if (shouldreflesh) DrawAndRefresh();
        }
        private float GetAssistInformationTime(Point mousepos, float starttime, float endtime, out AssistType type)
        {
            type = AssistType.None;
            var events = WindowUtility.EventManager.GetEventsWithinTime(starttime, endtime);
            float optionheight = PPDEditorSkin.Skin.TimeLineHeight +
                PPDEditorSkin.Skin.TimeLineRowHeight * WindowUtility.TimeLineForm.RowManager.OrderedVisibleRowsCount;
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - Hsc.Value;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 8) < 8)
                {
                    type = AssistType.Event;
                    return eventtime;
                }
            }
            events = WindowUtility.SoundManager.GetSoundChangeWithinTime(starttime, endtime);
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - Hsc.Value;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 22) < 8)
                {
                    type = AssistType.Sound;
                    return eventtime;
                }
            }
            events = WindowUtility.KasiEditor.GetKasiChangeWithinTime(starttime, endtime);
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - Hsc.Value;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 36) < 8)
                {
                    type = AssistType.Lylics;
                    return eventtime;
                }
            }
            return -1;
        }
        private void UpdateInfo()
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            if (ppdsheet.SelectedMark != null)
            {
                Mark mk = ppdsheet.SelectedMark;
                if (mk != null)
                {
                    WindowUtility.MainForm.SetMarkInfo(mk);
                }
            }
            else
            {
                WindowUtility.MainForm.SetMarksInfo(ppdsheet.GetAreaData());
            }
        }
        private void Seekmain_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                ProcessMouseLeftUp(e);
            }
        }

        private void ProcessMouseLeftUp(MouseEventArgs e)
        {
            timer1.Stop();
            timer2.Stop();
            timer3.Stop();
            timer4.Stop();
            var pos = PointToClient(Cursor.Position);
            var num = WindowUtility.TimeLineForm.RowManager.GetRowIndex(pos.Y);
            float time = (float)(Hsc.Value + pos.X) / defaultInterval / bpm * 60;
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null)
            {
                moveMarkState = MoveMarkState.Same;
                return;
            }

            if (moveMarkState == MoveMarkState.CopyToOther || moveMarkState == MoveMarkState.MoveToOther)
            {
                if (ppdsheet.AreaSelectionEnabled)
                {
                    var marks = ppdsheet.GetAreaData();
                    if (marks.Length > 0)
                    {
                        int minimum = int.MaxValue;
                        float starttime = marks[0].Time;
                        var rows = WindowUtility.TimeLineForm.RowManager.OrderedVisibleRows;
                        num = WindowUtility.TimeLineForm.RowManager.GetNormalRowIndex(pos.Y);
                        for (int i = 0; i < marks.Length; i++)
                        {
                            var temp = WindowUtility.TimeLineForm.RowManager.GetRowIndexFromType((int)marks[i].Type);
                            if (minimum > temp)
                            {
                                minimum = temp;
                            }
                        }
                        if (num >= 0 && num < 10 && marks.Length > 0)
                        {
                            ppdsheet.StartGroupCommand();
                            if (moveMarkState == MoveMarkState.MoveToOther)
                            {
                                ppdsheet.DeleteSelectedMarks();
                            }
                            for (int i = 0; i < marks.Length; i++)
                            {
                                int addnum = (num + WindowUtility.TimeLineForm.RowManager.GetRowIndexFromType((int)marks[i].Type) - minimum) % rows.Length;
                                addnum = WindowUtility.TimeLineForm.RowManager.GetTypeFromRowIndex(addnum);
                                float addtime = time + marks[i].Time - starttime;
                                addtime = AdjusttimeWhenFixed(addtime);
                                if (addtime >= 0 && addtime <= length)
                                {
                                    var parameters = marks[i].Parameters;
                                    if (marks[i] is ExMark)
                                    {
                                        var exmk = marks[i] as ExMark;
                                        ppdsheet.AddExMark(addtime, addtime + exmk.EndTime - exmk.Time, exmk.Position.X, exmk.Position.Y,
                                            exmk.Rotation, addnum, moveMarkState == MoveMarkState.MoveToOther ? marks[i].ID : ppdsheet.GetCorrectID(marks[i]), parameters);
                                    }
                                    else if (marks[i] is Mark)
                                    {
                                        ppdsheet.AddMark(addtime, marks[i].Position.X, marks[i].Position.Y,
                                            marks[i].Rotation, addnum, moveMarkState == MoveMarkState.MoveToOther ? marks[i].ID : ppdsheet.GetCorrectID(marks[i]), parameters);
                                    }
                                }
                            }
                            ppdsheet.EndGroupCommand();
                            DrawAndRefresh();
                        }
                    }
                }
                else if (num >= 0 && num < 10)
                {
                    if (time < 0) time = 0;
                    if (time > length) time = (float)length;
                    time = AdjusttimeWhenFixed(time);
                    if (PPDStaticSetting.EnableToChangeMarkTypeAndTime)
                    {
                        var mark = ppdsheet.SelectedMark;
                        var parameters = mark.Parameters;
                        ppdsheet.StartGroupCommand();
                        if (moveMarkState == MoveMarkState.MoveToOther)
                        {
                            ppdsheet.DeleteSelectedMark();
                        }
                        if (mark is ExMark)
                        {
                            var exmk = mark as ExMark;
                            ppdsheet.AddExMark(time, time + exmk.EndTime - exmk.Time, exmk.Position.X, exmk.Position.Y, exmk.Rotation,
                                num, moveMarkState == MoveMarkState.MoveToOther ? mark.ID : ppdsheet.GetCorrectID(mark), parameters);
                        }
                        else if (mark is Mark)
                        {
                            ppdsheet.AddMark(time, mark.Position.X, mark.Position.Y, mark.Rotation, num,
                                moveMarkState == MoveMarkState.MoveToOther ? mark.ID : ppdsheet.GetCorrectID(mark), parameters);
                        }
                        ppdsheet.EndGroupCommand();
                        DrawAndRefresh();
                    }
                    else
                    {
                        if (num != ppdsheet.FocusedMark[0])
                        {
                            if (moveMarkState == MoveMarkState.MoveToOther && ppdsheet.ChangeMarkType(num))
                            {
                                DrawAndRefresh();
                            }
                            else if (moveMarkState == MoveMarkState.CopyToOther && ppdsheet.CopyMark(num))
                            {
                                DrawAndRefresh();
                            }
                        }
                        else
                        {
                            if (moveMarkState == MoveMarkState.MoveToOther && ppdsheet.MoveMarkInSame(time))
                            {
                                DrawAndRefresh();
                            }
                            else if (moveMarkState == MoveMarkState.CopyToOther && ppdsheet.CopyMarkInSame(time))
                            {
                                DrawAndRefresh();
                            }
                        }
                    }
                }
            }
            moveMarkState = MoveMarkState.Same;

            if (!ppdsheet.AreaSelectionEnabled)
            {
                WindowUtility.MainForm.ChangeAreaSelect(false);
                DrawAndRefresh();
            }

            if (doDragChange)
            {
                doDragChange = false;
                switch (assistType)
                {
                    case AssistType.Event:
                        WindowUtility.EventManager.ChangeTime(assistInfoTime, time);
                        break;
                    case AssistType.Sound:
                        WindowUtility.SoundManager.ChangeTime(assistInfoTime, time);
                        break;
                    case AssistType.Lylics:
                        WindowUtility.KasiEditor.ChangeTime(assistInfoTime, time);
                        break;
                }
                assistType = AssistType.None;
            }
            if (tpf != null)
            {
                tpf.Close();
                tpf.Dispose();
                tpf = null;
            }
            UpdateInfo();
        }

        private void Seekmain_SizeChanged(object sender, EventArgs e)
        {
            Hsc.Location = new System.Drawing.Point(0, this.Height - Hsc.Height);
            Hsc.Width = this.Width;
        }

        private void Seekmain_DoubleClick(object sender, EventArgs e)
        {
            if (ModifierKeys != Keys.Shift)
            {
                PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
                if (ppdsheet == null) return;
                var pos = this.PointToClient(Cursor.Position);
                var num = WindowUtility.TimeLineForm.RowManager.GetRowIndex(pos.Y);
                if (num >= 0 && num < 10)
                {
                    float time = (float)(Hsc.Value + pos.X) / defaultInterval / bpm * 60;
                    time = AdjusttimeWhenFixed(time);
                    if (copyBuffer != null && copyBuffer.Length > 0)
                    {
                        var rows = WindowUtility.TimeLineForm.RowManager.OrderedVisibleRows;
                        num = WindowUtility.TimeLineForm.RowManager.GetNormalRowIndex(pos.Y);
                        float starttime = copyBuffer[0].Time;
                        int minimum = int.MaxValue;
                        for (int i = 0; i < copyBuffer.Length; i++)
                        {
                            var temp = WindowUtility.TimeLineForm.RowManager.GetRowIndexFromType((int)copyBuffer[i].ButtonType);
                            if (minimum > temp)
                            {
                                minimum = temp;
                            }
                        }
                        ppdsheet.StartGroupCommand();
                        for (int i = 0; i < copyBuffer.Length; i++)
                        {
                            int addnum = (num + WindowUtility.TimeLineForm.RowManager.GetRowIndexFromType((int)copyBuffer[i].ButtonType) - minimum) % rows.Length;
                            addnum = WindowUtility.TimeLineForm.RowManager.GetTypeFromRowIndex(addnum);
                            float addtime = time + copyBuffer[i].Time - starttime;
                            addtime = AdjusttimeWhenFixed(addtime);
                            if (addtime >= 0 && addtime <= length)
                            {
                                Mark newMk = null;
                                if (copyBuffer[i] is ExMarkData)
                                {
                                    var exmk = copyBuffer[i] as ExMarkData;
                                    newMk = ppdsheet.AddExMark(addtime, addtime + exmk.EndTime - exmk.Time, exmk.X, exmk.Y, exmk.Angle, addnum, ppdsheet.GetCorrectID(copyBuffer[i]));
                                }
                                else if (copyBuffer[i] is MarkData)
                                {
                                    newMk = ppdsheet.AddMark(addtime, copyBuffer[i].X, copyBuffer[i].Y, copyBuffer[i].Angle, addnum, ppdsheet.GetCorrectID(copyBuffer[i]));
                                }
                                if (newMk != null)
                                {
                                    newMk.CopyParameters(copyBuffer[i]);
                                }
                            }
                        }
                        ppdsheet.EndGroupCommand();
                        ppdsheet.RecStart = new CustomPoint(-1, -1);
                        WindowUtility.MainForm.ChangeAreaSelect(false);
                        DrawAndRefresh();
                    }
                    else
                    {
                        ppdsheet.AddMark(time, num);
                        ppdsheet.RecStart = new CustomPoint(-1, -1);
                        WindowUtility.MainForm.ChangeAreaSelect(false);
                        DrawAndRefresh();
                    }
                }
            }
        }
        private float AdjusttimeWhenFixed(float time)
        {
            if (WindowUtility.MainForm.Bpmfixed)
            {
                double size = 1;
                switch (DisplayMode)
                {
                    case DisplayLineMode.Fourth:
                        size = 1;
                        break;
                    case DisplayLineMode.Eigth:
                        size = (double)1 / 2;
                        break;
                    case DisplayLineMode.Sixteenth:
                        size = (double)1 / 4;
                        break;
                    case DisplayLineMode.ThirtySecond:
                        size = (double)1 / 8;
                        break;
                    case DisplayLineMode.SixtyFourth:
                        size = (double)1 / 16;
                        break;
                    case DisplayLineMode.Twelfth:
                        size = (double)1 / 3;
                        break;
                    case DisplayLineMode.TwentyFourth:
                        size = (double)1 / 6;
                        break;
                    case DisplayLineMode.FourthEighth:
                        size = (double)1 / 12;
                        break;
                }

                double bittime = 60 / (double)bpm;
                var a = (int)Math.Floor((time - 60 / bpm * bpmStart / bpm) / bittime / size);
                double st = a * bittime * size + bittime * bpmStart / bpm;
                double en = (a + 1) * bittime * size + bittime * bpmStart / bpm;
                if (Math.Abs(time - st) > Math.Abs(time - en))
                {
                    time = (float)en;
                }
                else
                {
                    time = (float)st;
                }
            }
            return time;
        }

        private void Seekmain_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (CheckPreviewKey(e.KeyCode, e.Control, e.Shift, e.Alt))
            {
                e.IsInputKey = true;
            }
        }
        public void Undo()
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            if (ppdsheet.CanUndo)
            {
                ppdsheet.Undo();
                DrawAndRefresh();
            }
        }
        public void Redo()
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            if (ppdsheet.CanRedo)
            {
                ppdsheet.Redo();
                DrawAndRefresh();
            }
        }
        public void ForceToNearBar(bool onlyInSelection)
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            Mark[] marks = null;
            if (onlyInSelection)
            {
                if (!ppdsheet.AreaSelectionEnabled)
                {
                    return;
                }
                marks = ppdsheet.GetAreaData();
            }
            else
            {
                marks = ppdsheet.GetSortedData();
            }
            var times = new Dictionary<Mark, float[]>();
            foreach (var mark in marks)
            {
                var exmk = mark as ExMark;
                var t = AdjusttimeWhenFixed(mark.Time);
                float[] ts;
                if (exmk != null)
                {
                    ts = new float[] { t, exmk.EndTime - exmk.Time + t };
                }
                else
                {
                    ts = new float[] { t };
                }
                times.Add(mark, ts);
            }
            if (times.Count == 0)
            {
                return;
            }

            ppdsheet.ForceToNearBar(times);
            DrawAndRefresh();
        }

        public void AngleInterpolation(bool clockwise)
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            ppdsheet.InterpolateAngle(clockwise);
        }

        public void PositionInterpolation()
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            ppdsheet.InterpolatePosition();
        }

        public void SeekToNearSelected()
        {
            if (!WindowUtility.MainForm.MarkFocus) return;
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null || ppdsheet.SelectedMark == null) return;
            float scale = (float)ppdsheet.BPM * WindowUtility.MainForm.SpeedScaleAsFloat / Mark.defaultbpm;
            Currenttime = ppdsheet.SelectedMark.Time - (2 / scale) * WindowUtility.MainForm.Farness;
            WindowUtility.MainForm.SeekMovie(currentTime);
        }
        private int GetMove(ShortcutType shortcutType)
        {
            switch (shortcutType)
            {
                case ShortcutType.Left0:
                case ShortcutType.Right0:
                case ShortcutType.Up0:
                case ShortcutType.Down0:
                    return PPDStaticSetting.Moves[0];
                case ShortcutType.Left1:
                case ShortcutType.Right1:
                case ShortcutType.Up1:
                case ShortcutType.Down1:
                    return PPDStaticSetting.Moves[1];
                case ShortcutType.Left2:
                case ShortcutType.Right2:
                case ShortcutType.Up2:
                case ShortcutType.Down2:
                    return PPDStaticSetting.Moves[2];
                case ShortcutType.Left3:
                case ShortcutType.Right3:
                case ShortcutType.Up3:
                case ShortcutType.Down3:
                    return PPDStaticSetting.Moves[3];
                case ShortcutType.Left4:
                case ShortcutType.Right4:
                case ShortcutType.Up4:
                case ShortcutType.Down4:
                    return PPDStaticSetting.Moves[4];
                case ShortcutType.Left5:
                case ShortcutType.Right5:
                case ShortcutType.Up5:
                case ShortcutType.Down5:
                    return PPDStaticSetting.Moves[5];
                case ShortcutType.Left6:
                case ShortcutType.Right6:
                case ShortcutType.Up6:
                case ShortcutType.Down6:
                    return PPDStaticSetting.Moves[6];
                case ShortcutType.Left7:
                case ShortcutType.Right7:
                case ShortcutType.Up7:
                case ShortcutType.Down7:
                    return PPDStaticSetting.Moves[7];
                default:
                    return 1;
            }
        }
        public bool CheckPreviewKey(Keys key, bool Control, bool Shift, bool Alt)
        {
            bool ret = false;
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return ret;
            ret |= key == Keys.Tab;
            var shortcutInfo = ShortcutManager.GetShortcut(key, Control, Shift, Alt);
            if (shortcutInfo == null)
            {
                return ret;
            }
            if (shortcutInfo.ShortcutType == ShortcutType.Custom)
            {
                WindowUtility.MainForm.ExecuteScript(shortcutInfo.ScriptPathWithExtension);
            }
            else
            {
                switch (shortcutInfo.ShortcutType)
                {
                    case ShortcutType.Left0:
                    case ShortcutType.Left1:
                    case ShortcutType.Left2:
                    case ShortcutType.Left3:
                    case ShortcutType.Left4:
                    case ShortcutType.Left5:
                    case ShortcutType.Left6:
                    case ShortcutType.Left7:
                        var move = GetMove(shortcutInfo.ShortcutType);
                        if (ppdsheet.MoveMark(move, PPDSheet.Direction.Left))
                        {
                            UpdateInfo();
                            ret = true;
                        }
                        else if (ppdsheet.MoveMarks(move, PPDSheet.Direction.Left))
                        {
                            UpdateInfo();
                            ret = true;
                        }
                        break;
                    case ShortcutType.Right0:
                    case ShortcutType.Right1:
                    case ShortcutType.Right2:
                    case ShortcutType.Right3:
                    case ShortcutType.Right4:
                    case ShortcutType.Right5:
                    case ShortcutType.Right6:
                    case ShortcutType.Right7:
                        move = GetMove(shortcutInfo.ShortcutType);
                        if (ppdsheet.MoveMark(move, PPDSheet.Direction.Right))
                        {
                            UpdateInfo();
                            ret = true;
                        }
                        else if (ppdsheet.MoveMarks(move, PPDSheet.Direction.Right))
                        {
                            UpdateInfo();
                            ret = true;
                        }
                        break;
                    case ShortcutType.Up0:
                    case ShortcutType.Up1:
                    case ShortcutType.Up2:
                    case ShortcutType.Up3:
                    case ShortcutType.Up4:
                    case ShortcutType.Up5:
                    case ShortcutType.Up6:
                    case ShortcutType.Up7:
                        move = GetMove(shortcutInfo.ShortcutType);
                        if (ppdsheet.MoveMark(move, PPDSheet.Direction.Up))
                        {
                            UpdateInfo();
                            ret = true;
                        }
                        else if (ppdsheet.MoveMarks(move, PPDSheet.Direction.Up))
                        {
                            UpdateInfo();
                            ret = true;
                        }
                        break;
                    case ShortcutType.Down0:
                    case ShortcutType.Down1:
                    case ShortcutType.Down2:
                    case ShortcutType.Down3:
                    case ShortcutType.Down4:
                    case ShortcutType.Down5:
                    case ShortcutType.Down6:
                    case ShortcutType.Down7:
                        move = GetMove(shortcutInfo.ShortcutType);
                        if (ppdsheet.MoveMark(move, PPDSheet.Direction.Down))
                        {
                            UpdateInfo();
                            ret = true;
                        }
                        else if (ppdsheet.MoveMarks(move, PPDSheet.Direction.Down))
                        {
                            UpdateInfo();
                            ret = true;
                        }
                        break;
                    case ShortcutType.DeleteMark:
                        if (ppdsheet.DeleteSelectedMark())
                        {
                            DrawAndRefresh();
                        }
                        if (ppdsheet.DeleteSelectedMarks())
                        {
                            DrawAndRefresh();
                        }
                        break;
                    case ShortcutType.Previous:
                        if (ppdsheet.ShiftSelectedMark(-1))
                        {
                            UpdateInfo();
                            SeekToNearSelected();
                            DrawAndRefresh();
                        }
                        break;
                    case ShortcutType.Next:
                        if (ppdsheet.ShiftSelectedMark(1))
                        {
                            UpdateInfo();
                            SeekToNearSelected();
                            DrawAndRefresh();
                        }
                        break;
                    case ShortcutType.NextAll:
                        if (ppdsheet.ShiftSelectedMarkInAll(1, WindowUtility.TimeLineForm.RowManager.InverseRowOrders.Select(i => (ButtonType)i).ToArray()))
                        {
                            UpdateInfo();
                            SeekToNearSelected();
                            DrawAndRefresh();
                        }
                        break;
                    case ShortcutType.PreviousAll:
                        if (ppdsheet.ShiftSelectedMarkInAll(-1, WindowUtility.TimeLineForm.RowManager.InverseRowOrders.Select(i => (ButtonType)i).ToArray()))
                        {
                            UpdateInfo();
                            SeekToNearSelected();
                            DrawAndRefresh();
                        }
                        break;
                    case ShortcutType.CopyAngle:
                        if (ppdsheet.CopyPreviousMarkAngle())
                        {
                            UpdateInfo();
                        }
                        break;
                    case ShortcutType.CopyPosition:
                        if (ppdsheet.CopyPreviousMarkPosition())
                        {
                            UpdateInfo();
                        }
                        break;
                    case ShortcutType.Fusion:
                        if (ppdsheet.FusionMarks())
                        {
                            DrawAndRefresh();
                        }
                        break;
                    case ShortcutType.Defusion:
                        if (ppdsheet.DeFusionExMark())
                        {
                            DrawAndRefresh();
                        }
                        break;
                    case ShortcutType.Undo:
                        Undo();
                        break;
                    case ShortcutType.Redo:
                        Redo();
                        break;
                    case ShortcutType.CopyMark:
                        var mks = ppdsheet.GetAreaData();
                        copyBuffer = new MarkData[mks.Length];
                        for (int i = 0; i < copyBuffer.Length; i++)
                        {
                            if (mks[i] is ExMark exmk)
                            {
                                copyBuffer[i] = new ExMarkData(exmk.Position.X, exmk.Position.Y, exmk.Rotation, exmk.Time, exmk.EndTime, (ButtonType)exmk.Type, exmk.ID);
                            }
                            else
                            {
                                copyBuffer[i] = new MarkData(mks[i].Position.X, mks[i].Position.Y, mks[i].Rotation, mks[i].Time, mks[i].Type, mks[i].ID);
                            }
                            foreach (var parameter in mks[i].Parameters)
                            {
                                copyBuffer[i].AddParameter(parameter.Key, parameter.Value);
                            }
                        }
                        if (copyBuffer.Length > 0)
                        {
                            WindowUtility.MainForm.ChangeCopyBuffer(true);
                        }
                        break;
                    case ShortcutType.ClearCopyBuffer:
                        if (copyBuffer != null && copyBuffer.Length != 0)
                        {
                            copyBuffer = null;
                            WindowUtility.MainForm.ChangeCopyBuffer(false);
                        }
                        break;
                    case ShortcutType.InterpolateAngleClockwise:
                        AngleInterpolation(true);
                        break;
                    case ShortcutType.InterpolateAngleUnclockwise:
                        AngleInterpolation(false);
                        break;
                    case ShortcutType.InterpolatePosition:
                        PositionInterpolation();
                        break;
                }
            }
            return ret;
        }

        private void Seekmain_MouseClick(object sender, MouseEventArgs e)
        {
            var rowIndex = WindowUtility.TimeLineForm.RowManager.GetRowIndex(e.Y);
            if (rowIndex >= 0)
            {
                float drawstarttime = (float)(Hsc.Value - 10) / defaultInterval / bpm * 60;
                float drawendtime = (float)(Hsc.Value + this.Width + 10) / defaultInterval / bpm * 60;

                var content = GetAssistInformation(new Point(e.X, e.Y), drawstarttime, drawendtime);
                if (content != "" && WindowUtility.TimeLineForm != null)
                {
                    toolTip1.Show(content, GetWindowTarget(), GetScreenPoint(), 5000);
                }
            }
        }

        public IWin32Window GetWindowTarget()
        {
            if (WindowUtility.TimeLineForm.DockState == DockState.Float) return this;
            return WindowUtility.MainForm;
        }
        public System.Drawing.Point GetScreenPoint()
        {
            var p = new System.Drawing.Point(Cursor.Position.X + 10, Cursor.Position.Y + 10);
            if (WindowUtility.TimeLineForm.DockState == DockState.Float) return this.PointToClient(p);
            return WindowUtility.MainForm.PointToClient(p);
        }

        private string GetAssistInformation(Point mousepos, float starttime, float endtime)
        {
            string ret = "";
            var events = WindowUtility.EventManager.GetEventsWithinTime(starttime, endtime);
            float optionheight = PPDEditorSkin.Skin.TimeLineHeight +
                PPDEditorSkin.Skin.TimeLineRowHeight * WindowUtility.TimeLineForm.RowManager.OrderedVisibleRowsCount;
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - Hsc.Value;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 8) < 8)
                {
                    return WindowUtility.EventManager.GetFormattedContent(eventtime);
                }
            }
            events = WindowUtility.SoundManager.GetSoundChangeWithinTime(starttime, endtime);
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - Hsc.Value;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 22) < 8)
                {
                    return WindowUtility.SoundManager.GetFormattedContent(eventtime);
                }
            }
            events = WindowUtility.KasiEditor.GetKasiChangeWithinTime(starttime, endtime);
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - Hsc.Value;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 36) < 8)
                {
                    return WindowUtility.KasiEditor.GetFormattedContent(eventtime);
                }
            }
            return ret;
        }

        private void Seekmain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var rowIndex = WindowUtility.TimeLineForm.RowManager.GetRowIndex(e.Y);
            if (rowIndex >= 0)
            {
                float drawstarttime = (float)(Hsc.Value - 10) / defaultInterval / bpm * 60;
                float drawendtime = (float)(Hsc.Value + this.Width + 10) / defaultInterval / bpm * 60;

                SetFocusAssistInformation(new Point(e.X, e.Y), drawstarttime, drawendtime);
            }
        }

        private void SetFocusAssistInformation(Point mousepos, float starttime, float endtime)
        {
            var events = WindowUtility.EventManager.GetEventsWithinTime(starttime, endtime);
            float optionheight = PPDEditorSkin.Skin.TimeLineHeight +
                PPDEditorSkin.Skin.TimeLineRowHeight * WindowUtility.TimeLineForm.RowManager.OrderedVisibleRowsCount;
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - Hsc.Value;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 8) < 8)
                {
                    WindowUtility.EventManager.FocusWithTimeData(eventtime);
                    return;
                }
            }
            events = WindowUtility.SoundManager.GetSoundChangeWithinTime(starttime, endtime);
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - Hsc.Value;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 22) < 8)
                {
                    WindowUtility.SoundManager.FocusWithTimeData(eventtime);
                    return;
                }
            }
            events = WindowUtility.KasiEditor.GetKasiChangeWithinTime(starttime, endtime);
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - Hsc.Value;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 36) < 8)
                {
                    WindowUtility.KasiEditor.FocusWithTimeData(eventtime);
                    return;
                }
            }
        }

        public void ClearHistory()
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            ppdsheet.ClearHistory();
        }
    }
}
