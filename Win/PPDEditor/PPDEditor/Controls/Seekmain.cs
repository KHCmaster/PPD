using PPDEditor.Command.PPDSheet;
using PPDEditor.Forms;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using System.Numerics;
using PPDMath;
using Point = Vortice.Mathematics.Int2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Keys = System.Windows.Forms.Keys;

namespace PPDEditor.Controls
{
    /// <summary>
    /// タイムラインの、マークを並べるところです。
    /// </summary>
    public class Seekmain : UserControl
    {
        /// <summary>
        /// 描くところです。描く内容は <see cref="Seekmain"/> に任せる。
        /// </summary>
        sealed class TimeLineCanvas(Seekmain owner) : FrameworkElement
        {
            protected override void OnRender(DrawingContext drawingContext)
            {
                owner.Render(drawingContext);
            }
        }

        public event EventHandler Seeked;

        float assistInfoTime;
        AssistType assistType = AssistType.None;
        bool doDragChange;
        Point mouseDownPos;
        // イベントなどの印をドラッグしているときに、その印を書く位置
        Point dragIconPos;
        ToolTip assistToolTip;
        DispatcherTimer assistToolTipTimer;


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
        readonly ScrollBar Hsc;
        readonly TimeLineCanvas canvas;
        // ドラッグ中にマウスの位置を見る（以前と同じく 100 ミリ秒ごと）
        readonly DispatcherTimer timer1 = new() { Interval = TimeSpan.FromMilliseconds(100) };
        readonly DispatcherTimer timer2 = new() { Interval = TimeSpan.FromMilliseconds(100) };
        readonly DispatcherTimer timer3 = new() { Interval = TimeSpan.FromMilliseconds(100) };
        readonly DispatcherTimer timer4 = new() { Interval = TimeSpan.FromMilliseconds(100) };
        MarkData[] copyBuffer;

        public Seekmain()
        {
            ShortcutManager = new PPDEditor.ShortcutManager();
            canvas = new TimeLineCanvas(this);
            Hsc = new ScrollBar
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            Hsc.Scroll += Hsc_Scroll;
            var grid = new Grid();
            grid.Children.Add(canvas);
            grid.Children.Add(Hsc);
            Content = grid;
            Focusable = true;
            ClipToBounds = true;
            // 以前の GDI と同じく、線や丸の縁をぼかさない
            RenderOptions.SetEdgeMode(canvas, EdgeMode.Aliased);

            timer1.Tick += MoveSeek;
            timer2.Tick += MoveMark;
            timer3.Tick += ChangeRec;
            timer4.Tick += MoveAssistInformation;

            PreviewKeyDown += Seekmain_PreviewKeyDown;
            MouseWheel += Seekmain_MouseWheel;
            canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            canvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            canvas.LostMouseCapture += Canvas_LostMouseCapture;
            SizeChanged += (sender, e) => DrawAndRefresh();
        }

        void Seekmain_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var sign = Math.Sign(e.Delta);
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                int result = WindowUtility.Editor.DisplayWidth + 50 * sign;
                if (result < 50) result = 50;
                if (result > 300) result = 300;
                WindowUtility.Editor.DisplayWidth = result;
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
            }
            else
            {
                int result = ScrollValue - sign * 100;
                if (result < ScrollMinimum) result = ScrollMinimum;
                if (result > ScrollMaximum) result = ScrollMaximum;
                ScrollValue = result;
                DrawAndRefresh();
            }
            e.Handled = true;
        }

        /// <summary>
        /// 横にスクロールした量（ピクセル）です。
        /// </summary>
        private int ScrollValue
        {
            get { return (int)Hsc.Value; }
            set { Hsc.Value = value; }
        }

        private int ScrollMinimum
        {
            get { return (int)Hsc.Minimum; }
        }

        private int ScrollMaximum
        {
            get { return (int)Hsc.Maximum; }
        }

        private int PixelWidth
        {
            get { return (int)ActualWidth; }
        }

        private int PixelHeight
        {
            get { return (int)ActualHeight; }
        }

        /// <summary>
        /// 描くところの中のマウスの位置です（以前の GetMousePosition()）。
        /// </summary>
        private Point GetMousePosition()
        {
            var pos = Mouse.GetPosition(canvas);
            return new Point((int)Math.Floor(pos.X), (int)Math.Floor(pos.Y));
        }

        /// <summary>
        /// 描き直しを予約します。
        /// </summary>
        public void DrawAndRefresh()
        {
            canvas.InvalidateVisual();
        }

        /// <summary>
        /// 横のスクロールを最初に戻します。
        /// </summary>
        public void ResetScroll()
        {
            ScrollValue = 0;
            DrawAndRefresh();
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

        private void Render(DrawingContext dc)
        {
            if (WindowUtility.TimeLineForm == null || WindowUtility.LayerManager == null || PixelWidth <= 0 || PixelHeight <= 0)
            {
                return;
            }

            TimeLineDrawer.Draw(new TimeLineDrawParameter(dc, PixelWidth, PixelHeight, WindowUtility.TimeLineForm.TimeLineBackground,
                WindowUtility.LayerManager.SelectedPpdSheet, bpm, bpmStart, defaultInterval, DisplayMode, ScrollValue, (float)currentTime, BeatType));
            if (doDragChange)
            {
                var icon = assistType switch
                {
                    AssistType.Event => TimeLineDrawer.EventMark,
                    AssistType.Sound => TimeLineDrawer.SoundMark,
                    AssistType.Lylics => TimeLineDrawer.LylicsMark,
                    _ => null,
                };
                if (icon != null)
                {
                    // ドラッグしている印を半透明で書く
                    dc.PushOpacity(0.7);
                    TimeLineDrawer.DrawIcon(dc, icon, dragIconPos.X - 8, dragIconPos.Y - 8);
                    dc.Pop();
                }
            }
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
            Hsc.ViewportSize = defaultInterval;
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
                if (!WindowUtility.Editor.DrawToggle) return null;
                PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
                if (ppdsheet == null) return null;
                return ppdsheet.SelectedMark;
            }
        }

        public Mark HeadMark
        {
            get
            {
                if (!WindowUtility.Editor.DrawToggle) return null;
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
            // ゲームの画面（WPF の外のウィンドウ）から呼ばれるので、修飾キーはシステムから読む
            var modifiers = GameView.GetModifiers();
            bool shiftKey = modifiers.HasFlag(ModifierKeys.Shift),
                ctrlKey = modifiers.HasFlag(ModifierKeys.Control),
                altKey = modifiers.HasFlag(ModifierKeys.Alt);
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
        private static Point ChangePos(Point p)
        {
            if (WindowUtility.Editor.DisplayGrid)
            {
                SquareGrid Grid = WindowUtility.Editor.Grid;
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
            if (ppdsheet == null) return [];
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
            int val = (int)(Currenttime * bpm / 60 * defaultInterval) - PixelWidth / 2;
            if (val > ScrollMaximum)
            {
                ScrollValue = ScrollMaximum;
            }
            else if (val < ScrollMinimum)
            {
                ScrollValue = ScrollMinimum;
            }
            else
            {
                ScrollValue = val;
            }
            this.DrawAndRefresh();
        }
        private void OnSeeked()
        {
            this.Seeked?.Invoke(this, EventArgs.Empty);
        }
        private void ChangeRec(object sender, EventArgs e)
        {
            var pos = GetMousePosition();
            var num = WindowUtility.TimeLineForm.RowManager.GetNormalRowIndex(pos.Y);
            if (num < 0) num = 0;
            if (num > 9) num = 9;
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            ppdsheet.RecEnd = new CustomPoint((float)(pos.X + ScrollValue) / defaultInterval / bpm * 60, num);
            if (pos.X < 0)
            {
                int gain = pos.X;
                if (ScrollValue + gain < ScrollMinimum)
                {
                    gain = ScrollMinimum - ScrollValue;
                }
                ScrollValue += gain;
            }
            else if (pos.X > PixelWidth)
            {
                int gain = pos.X - PixelWidth;
                if (ScrollValue + gain > ScrollMaximum)
                {
                    gain = ScrollMaximum - ScrollValue;
                }
                ScrollValue += gain;
            }
            DrawAndRefresh();
        }
        private void MoveMark(object sender, EventArgs e)
        {
            if (moveMarkState == MoveMarkState.Same)
            {
                PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
                if (ppdsheet == null) return;
                var pos = GetMousePosition();
                float time = (float)(pos.X + ScrollValue) / defaultInterval / bpm * 60;
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
            var pos = GetMousePosition();
            if (pos.Y >= 0 && pos.Y <= PPDEditorSkin.Skin.TimeLineHeight)
            {
                float time = (float)(ScrollValue + pos.X) / defaultInterval / bpm * 60;
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
                WindowUtility.Editor.Pause();
                WindowUtility.Editor.SeekMovie(currentTime);
                this.DrawAndRefresh();
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            canvas.CaptureMouse();
            var pos = e.GetPosition(canvas);
            var point = new Point((int)Math.Floor(pos.X), (int)Math.Floor(pos.Y));
            ProcessMouseLeftDown(point);
            if (e.ClickCount == 2)
            {
                Seekmain_DoubleClick();
                Seekmain_MouseDoubleClick(point);
            }
            e.Handled = true;
        }

        private void ProcessMouseLeftDown(Point e)
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
                var pos = GetMousePosition();
                float time = (float)(pos.X + ScrollValue) * 60 / defaultInterval / bpm;
                float dt = (float)TimeLineDrawer.CircleWidth / defaultInterval * 60 / bpm / 2;
                if (rowIndex >= 0 && rowIndex < 10)
                {
                    if (ppdsheet.SelectMark(time, rowIndex, dt))
                    {
                        UpdateInfo();
                        SeekToNearSelected();
                        if (!WindowUtility.Editor.Bpmfixed)
                        {
                            timer2.Start();
                        }
                        ppdsheet.RecStart = new CustomPoint(-1, 0);
                        ppdsheet.RecEnd = new CustomPoint(-1, 0);
                        foundpoint = true;
                        WindowUtility.Editor.ChangeAreaSelect(false);
                    }
                    if (Keyboard.Modifiers == ModifierKeys.Control && foundpoint)
                    {
                        moveMarkState = MoveMarkState.CopyToOther;
                    }
                    else if (Keyboard.Modifiers == ModifierKeys.Shift && foundpoint)
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
                        if (Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            moveMarkState = MoveMarkState.CopyToOther;
                            return;
                        }
                        else if (Keyboard.Modifiers == ModifierKeys.Shift)
                        {
                            moveMarkState = MoveMarkState.MoveToOther;
                            return;
                        }
                    }
                    ppdsheet.RecStart = new CustomPoint((float)(pos.X + ScrollValue) / defaultInterval / bpm * 60, rowIndex);
                    ppdsheet.RecEnd = new CustomPoint((float)(pos.X + ScrollValue) / defaultInterval / bpm * 60, rowIndex);
                    WindowUtility.Editor.ChangeAreaSelect(true);
                    timer3.Start();
                }
            }
            else
            {
                float drawstarttime = (float)(ScrollValue - 10) / defaultInterval / bpm * 60;
                float drawendtime = (float)(ScrollValue + PixelWidth + 10) / defaultInterval / bpm * 60;
                mouseDownPos = e;
                assistInfoTime = GetAssistInformationTime(mouseDownPos, drawstarttime, drawendtime, out assistType);
                timer4.Start();
            }
        }

        private void MoveAssistInformation(object sender, EventArgs e)
        {
            var pos = GetMousePosition();
            if (assistType != AssistType.None)
            {
                if (!doDragChange && Math.Abs(pos.X - mouseDownPos.X) >= 20)
                {
                    doDragChange = true;
                }
                if (doDragChange)
                {
                    dragIconPos = pos;
                    DrawAndRefresh();
                }
            }
            bool shouldreflesh = false;
            if (pos.X < 0)
            {
                int gain = pos.X;
                if (ScrollValue + gain < ScrollMinimum)
                {
                    gain = ScrollMinimum - ScrollValue;
                }
                shouldreflesh = true;
                ScrollValue += gain;
            }
            else if (pos.X > PixelWidth)
            {
                int gain = pos.X - PixelWidth;
                if (ScrollValue + gain > ScrollMaximum)
                {
                    gain = ScrollMaximum - ScrollValue;
                }
                shouldreflesh = true;
                ScrollValue += gain;
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
                float pos = eventtime * defaultInterval * bpm / 60 - ScrollValue;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 8) < 8)
                {
                    type = AssistType.Event;
                    return eventtime;
                }
            }
            events = WindowUtility.SoundManager.GetSoundChangeWithinTime(starttime, endtime);
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - ScrollValue;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 22) < 8)
                {
                    type = AssistType.Sound;
                    return eventtime;
                }
            }
            events = WindowUtility.KasiEditor.GetKasiChangeWithinTime(starttime, endtime);
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - ScrollValue;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 36) < 8)
                {
                    type = AssistType.Lylics;
                    return eventtime;
                }
            }
            return -1;
        }
        private static void UpdateInfo()
        {
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null) return;
            if (ppdsheet.SelectedMark != null)
            {
                Mark mk = ppdsheet.SelectedMark;
                if (mk != null)
                {
                    WindowUtility.Editor.SetMarkInfo(mk);
                }
            }
            else
            {
                WindowUtility.Editor.SetMarksInfo(ppdsheet.GetAreaData());
            }
        }
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dragged = doDragChange;
            canvas.ReleaseMouseCapture();
            ProcessMouseLeftUp();
            if (!dragged)
            {
                var pos = e.GetPosition(canvas);
                Seekmain_MouseClick(new Point((int)Math.Floor(pos.X), (int)Math.Floor(pos.Y)));
            }
            e.Handled = true;
        }

        private void Canvas_LostMouseCapture(object sender, MouseEventArgs e)
        {
            // ほかのウィンドウに移ったときなどは、ドラッグをやめる
            if (timer1.IsEnabled || timer2.IsEnabled || timer3.IsEnabled || timer4.IsEnabled)
            {
                ProcessMouseLeftUp();
            }
        }

        private void ProcessMouseLeftUp()
        {
            timer1.Stop();
            timer2.Stop();
            timer3.Stop();
            timer4.Stop();
            var pos = GetMousePosition();
            var num = WindowUtility.TimeLineForm.RowManager.GetRowIndex(pos.Y);
            float time = (float)(ScrollValue + pos.X) / defaultInterval / bpm * 60;
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
                                    else if (marks[i] is not null)
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
                        else if (mark is not null)
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
                WindowUtility.Editor.ChangeAreaSelect(false);
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
            DrawAndRefresh();
            UpdateInfo();
        }

        private void Seekmain_DoubleClick()
        {
            if (Keyboard.Modifiers != ModifierKeys.Shift)
            {
                PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
                if (ppdsheet == null) return;
                var pos = GetMousePosition();
                var num = WindowUtility.TimeLineForm.RowManager.GetRowIndex(pos.Y);
                if (num >= 0 && num < 10)
                {
                    float time = (float)(ScrollValue + pos.X) / defaultInterval / bpm * 60;
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
                                else if (copyBuffer[i] is not null)
                                {
                                    newMk = ppdsheet.AddMark(addtime, copyBuffer[i].X, copyBuffer[i].Y, copyBuffer[i].Angle, addnum, ppdsheet.GetCorrectID(copyBuffer[i]));
                                }
                                newMk?.CopyParameters(copyBuffer[i]);
                            }
                        }
                        ppdsheet.EndGroupCommand();
                        ppdsheet.RecStart = new CustomPoint(-1, -1);
                        WindowUtility.Editor.ChangeAreaSelect(false);
                        DrawAndRefresh();
                    }
                    else
                    {
                        ppdsheet.AddMark(time, num);
                        ppdsheet.RecStart = new CustomPoint(-1, -1);
                        WindowUtility.Editor.ChangeAreaSelect(false);
                        DrawAndRefresh();
                    }
                }
            }
        }
        private float AdjusttimeWhenFixed(float time)
        {
            if (WindowUtility.Editor.Bpmfixed)
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

        private void Seekmain_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            var keyCode = (Keys)KeyInterop.VirtualKeyFromKey(key);
            var modifiers = Keyboard.Modifiers;
            var handled = CheckPreviewKey(keyCode, modifiers.HasFlag(ModifierKeys.Control), modifiers.HasFlag(ModifierKeys.Shift), modifiers.HasFlag(ModifierKeys.Alt));
            // 矢印と Tab は、フォーカスを移す操作にしない（以前の IsInputKey と同じ）
            if (handled || key is Key.Left or Key.Right or Key.Up or Key.Down or Key.Tab)
            {
                e.Handled = true;
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
            Mark[] marks;
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
                var t = AdjusttimeWhenFixed(mark.Time);
                float[] ts;
                if (mark is ExMark exmk)
                {
                    ts = [t, exmk.EndTime - exmk.Time + t];
                }
                else
                {
                    ts = [t];
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
            if (!WindowUtility.Editor.MarkFocus) return;
            PPDSheet ppdsheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (ppdsheet == null || ppdsheet.SelectedMark == null) return;
            float scale = (float)ppdsheet.BPM * WindowUtility.Editor.SpeedScaleAsFloat / Mark.defaultbpm;
            Currenttime = ppdsheet.SelectedMark.Time - (2 / scale) * WindowUtility.Editor.Farness;
            WindowUtility.Editor.SeekMovie(currentTime);
        }
        private static int GetMove(ShortcutType shortcutType)
        {
            return shortcutType switch
            {
                ShortcutType.Left0 or ShortcutType.Right0 or ShortcutType.Up0 or ShortcutType.Down0 => PPDStaticSetting.Moves[0],
                ShortcutType.Left1 or ShortcutType.Right1 or ShortcutType.Up1 or ShortcutType.Down1 => PPDStaticSetting.Moves[1],
                ShortcutType.Left2 or ShortcutType.Right2 or ShortcutType.Up2 or ShortcutType.Down2 => PPDStaticSetting.Moves[2],
                ShortcutType.Left3 or ShortcutType.Right3 or ShortcutType.Up3 or ShortcutType.Down3 => PPDStaticSetting.Moves[3],
                ShortcutType.Left4 or ShortcutType.Right4 or ShortcutType.Up4 or ShortcutType.Down4 => PPDStaticSetting.Moves[4],
                ShortcutType.Left5 or ShortcutType.Right5 or ShortcutType.Up5 or ShortcutType.Down5 => PPDStaticSetting.Moves[5],
                ShortcutType.Left6 or ShortcutType.Right6 or ShortcutType.Up6 or ShortcutType.Down6 => PPDStaticSetting.Moves[6],
                ShortcutType.Left7 or ShortcutType.Right7 or ShortcutType.Up7 or ShortcutType.Down7 => PPDStaticSetting.Moves[7],
                _ => 1,
            };
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
                WindowUtility.Editor.ExecuteScript(shortcutInfo.ScriptPathWithExtension);
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
                        if (ppdsheet.ShiftSelectedMarkInAll(1, [.. WindowUtility.TimeLineForm.RowManager.InverseRowOrders.Select(i => (ButtonType)i)]))
                        {
                            UpdateInfo();
                            SeekToNearSelected();
                            DrawAndRefresh();
                        }
                        break;
                    case ShortcutType.PreviousAll:
                        if (ppdsheet.ShiftSelectedMarkInAll(-1, [.. WindowUtility.TimeLineForm.RowManager.InverseRowOrders.Select(i => (ButtonType)i)]))
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
                            WindowUtility.Editor.ChangeCopyBuffer(true);
                        }
                        break;
                    case ShortcutType.ClearCopyBuffer:
                        if (copyBuffer != null && copyBuffer.Length != 0)
                        {
                            copyBuffer = null;
                            WindowUtility.Editor.ChangeCopyBuffer(false);
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

        private void Seekmain_MouseClick(Point e)
        {
            var rowIndex = WindowUtility.TimeLineForm.RowManager.GetRowIndex(e.Y);
            if (rowIndex >= 0)
            {
                float drawstarttime = (float)(ScrollValue - 10) / defaultInterval / bpm * 60;
                float drawendtime = (float)(ScrollValue + PixelWidth + 10) / defaultInterval / bpm * 60;

                var content = GetAssistInformation(e, drawstarttime, drawendtime);
                if (content != "" && WindowUtility.TimeLineForm != null)
                {
                    ShowAssistToolTip(content);
                }
            }
        }

        /// <summary>
        /// イベントなどの内容を、マウスの位置に 5 秒間出す。
        /// </summary>
        private void ShowAssistToolTip(string content)
        {
            if (assistToolTip != null)
            {
                assistToolTip.IsOpen = false;
            }
            assistToolTipTimer?.Stop();
            assistToolTip = new ToolTip
            {
                Content = content,
                PlacementTarget = canvas,
                Placement = PlacementMode.Mouse,
                IsOpen = true
            };
            assistToolTipTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            var toolTip = assistToolTip;
            assistToolTipTimer.Tick += (sender, e) =>
            {
                ((DispatcherTimer)sender).Stop();
                toolTip.IsOpen = false;
            };
            assistToolTipTimer.Start();
        }

        private string GetAssistInformation(Point mousepos, float starttime, float endtime)
        {
            string ret = "";
            var events = WindowUtility.EventManager.GetEventsWithinTime(starttime, endtime);
            float optionheight = PPDEditorSkin.Skin.TimeLineHeight +
                PPDEditorSkin.Skin.TimeLineRowHeight * WindowUtility.TimeLineForm.RowManager.OrderedVisibleRowsCount;
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - ScrollValue;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 8) < 8)
                {
                    return WindowUtility.EventManager.GetFormattedContent(eventtime);
                }
            }
            events = WindowUtility.SoundManager.GetSoundChangeWithinTime(starttime, endtime);
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - ScrollValue;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 22) < 8)
                {
                    return WindowUtility.SoundManager.GetFormattedContent(eventtime);
                }
            }
            events = WindowUtility.KasiEditor.GetKasiChangeWithinTime(starttime, endtime);
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - ScrollValue;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 36) < 8)
                {
                    return WindowUtility.KasiEditor.GetFormattedContent(eventtime);
                }
            }
            return ret;
        }

        private void Seekmain_MouseDoubleClick(Point e)
        {
            var rowIndex = WindowUtility.TimeLineForm.RowManager.GetRowIndex(e.Y);
            if (rowIndex >= 0)
            {
                float drawstarttime = (float)(ScrollValue - 10) / defaultInterval / bpm * 60;
                float drawendtime = (float)(ScrollValue + PixelWidth + 10) / defaultInterval / bpm * 60;

                SetFocusAssistInformation(e, drawstarttime, drawendtime);
            }
        }

        private void SetFocusAssistInformation(Point mousepos, float starttime, float endtime)
        {
            var events = WindowUtility.EventManager.GetEventsWithinTime(starttime, endtime);
            float optionheight = PPDEditorSkin.Skin.TimeLineHeight +
                PPDEditorSkin.Skin.TimeLineRowHeight * WindowUtility.TimeLineForm.RowManager.OrderedVisibleRowsCount;
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - ScrollValue;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 8) < 8)
                {
                    WindowUtility.EventManager.FocusWithTimeData(eventtime);
                    return;
                }
            }
            events = WindowUtility.SoundManager.GetSoundChangeWithinTime(starttime, endtime);
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - ScrollValue;
                if (Math.Abs(pos - mousepos.X) < 8 && Math.Abs(mousepos.Y - optionheight - 22) < 8)
                {
                    WindowUtility.SoundManager.FocusWithTimeData(eventtime);
                    return;
                }
            }
            events = WindowUtility.KasiEditor.GetKasiChangeWithinTime(starttime, endtime);
            foreach (float eventtime in events)
            {
                float pos = eventtime * defaultInterval * bpm / 60 - ScrollValue;
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
