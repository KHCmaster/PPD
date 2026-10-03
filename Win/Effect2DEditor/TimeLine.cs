using Effect2D;
using PPDConfiguration;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Effect2DEditor
{
    /// <summary>
    /// ステートを並べるタイムラインです。
    /// </summary>
    public class TimeLine : UserControl
    {
        //TODO
        //Undo Redo
        //delete state
        //bezier moving(X,Y)
        //easy transform bezier(rotaton,mirror,etc)
        public enum KeyOperationMode
        {
            None = 0,
            Move = 1,
            FixMove = 2,
            LeftMoveOnly = 3,
            RightMoveOnly = 4
        }
        const int TimeLineAreaHeight = 20;
        const int ItemHeight = 18;
        const int KeyWidth = 10;
        const int CircleDiameter = 8;

        static readonly Brush selectedBrush = Brushes.LightBlue;
        static readonly Brush timeLineAreaBrush = Brushes.WhiteSmoke;

        readonly DrawingSurface surface;
        readonly ScrollBar hScrollBar1;
        readonly ScrollBar vScrollBar1;
        readonly MenuItem キーステートを挿入ToolStripMenuItem;
        readonly MenuItem ノーマルステートを挿入ToolStripMenuItem;
        readonly MenuItem ステートを削除ToolStripMenuItem;

        bool mousedown;
        KeyOperationMode keyopmode = KeyOperationMode.None;

        int pressedkeyindex;
        int presseditemindex;

        //statemovecommand
        int beforeframe;
        int newframe;
        //

        bool ignorescroll;

        int currentframe;
        bool movetimeline;

        public EffectManager EffectManager
        {
            get;
            set;
        }
        public SelectedManager SelectedManager
        {
            get;
            set;
        }
        public MainWindow MainForm
        {
            get;
            set;
        }
        public int CurrentFrame
        {
            get
            {
                return currentframe;
            }
            set
            {
                if (currentframe != value)
                {
                    currentframe = value;
                    EffectManager?.Update(currentframe, null);
                    MainForm?.RefreshCanvas();
                    DrawAndRefresh();
                }
            }
        }
        public void SetUtility(SelectedManager SelectedManager)
        {
            this.SelectedManager = SelectedManager;
            SelectedManager.EffectChanged += SelectedManager_EffectChanged;
        }

        void SelectedManager_EffectChanged(object sender, EventArgs e)
        {
            DrawAndRefresh();
        }

        public TimeLine()
        {
            surface = new DrawingSurface { Render = Draw };
            hScrollBar1 = new ScrollBar { Orientation = Orientation.Horizontal, Minimum = 0, Maximum = 0, SmallChange = 1, LargeChange = 1 };
            vScrollBar1 = new ScrollBar { Orientation = Orientation.Vertical, Minimum = 0, Maximum = 0, SmallChange = 1, LargeChange = 1 };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.Children.Add(surface);
            Grid.SetColumn(vScrollBar1, 1);
            grid.Children.Add(vScrollBar1);
            Grid.SetRow(hScrollBar1, 1);
            grid.Children.Add(hScrollBar1);
            Content = grid;

            キーステートを挿入ToolStripMenuItem = new MenuItem { Header = "キーステートを挿入" };
            キーステートを挿入ToolStripMenuItem.Click += キーステートを挿入ToolStripMenuItem_Click;
            ノーマルステートを挿入ToolStripMenuItem = new MenuItem { Header = "ノーマルステートを挿入" };
            ノーマルステートを挿入ToolStripMenuItem.Click += ノーマルステートを挿入ToolStripMenuItem_Click;
            ステートを削除ToolStripMenuItem = new MenuItem { Header = "ステートを削除" };
            ステートを削除ToolStripMenuItem.Click += ステートを削除ToolStripMenuItem_Click;
            surface.ContextMenu = new ContextMenu
            {
                Items = { キーステートを挿入ToolStripMenuItem, ノーマルステートを挿入ToolStripMenuItem, ステートを削除ToolStripMenuItem }
            };
            surface.ContextMenuOpening += ContextMenu_Opening;

            vScrollBar1.Scroll += VScrollBar1_Scroll;
            hScrollBar1.Scroll += HScrollBar1_Scroll;
            surface.SizeChanged += TimeLine_SizeChanged;
            surface.MouseWheel += TimeLine_MouseWheel;
            surface.MouseDown += TimeLine_MouseDown;
            surface.MouseMove += TimeLine_MouseMove;
            surface.MouseUp += TimeLine_MouseUp;
        }

        public void SetLang(SettingReader setting)
        {
            if (setting != null)
            {
                キーステートを挿入ToolStripMenuItem.Header = setting["InsertKeyState"];
                ノーマルステートを挿入ToolStripMenuItem.Header = setting["InsertNormalState"];
                ステートを削除ToolStripMenuItem.Header = setting["DeleteState"];
            }
        }

        public void DrawAndRefresh()
        {
            surface.DrawAndRefresh();
        }

        private int HValue
        {
            get { return (int)hScrollBar1.Value; }
        }

        private int VValue
        {
            get { return (int)vScrollBar1.Value; }
        }

        void HScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            hScrollBar1.Value = Math.Round(hScrollBar1.Value);
            DrawAndRefresh();
        }

        void TimeLine_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int num = VValue;
            if (e.Delta >= 0) num--;
            else num++;
            if (num >= vScrollBar1.Minimum && num <= vScrollBar1.Maximum)
            {
                vScrollBar1.Value = num;
                MainForm.RefreshTimeLine();
            }
        }

        void TimeLine_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (EffectManager == null) return;
            ignorescroll = true;
            AdjustVScrollBar();
            AdjustHScrollBar();
            ignorescroll = false;
            MainForm.RefreshTimeLine();
        }
        public void MoveVScroll(int lineindex)
        {
            ignorescroll = true;
            AdjustVScrollBar();
            ignorescroll = false;
            if (lineindex >= 0 && lineindex < vScrollBar1.Maximum) vScrollBar1.Value = lineindex;
        }
        private void AdjustVScrollBar()
        {
            int maxvalue = MaxItemNumber - ShownItemNumber;
            if (maxvalue < 0) maxvalue = 0;
            vScrollBar1.Maximum = maxvalue;
            if (!ignorescroll) vScrollBar1.Value = maxvalue;
        }
        public void AdjustHScrollBar()
        {
            int maxvalue = SurfaceWidth / KeyWidth;
            int maxframe = MaxKeyFrame;
            int val = maxvalue - maxframe;
            if (val < maxvalue / 2)
            {
                hScrollBar1.Maximum = maxframe + maxvalue / 2 - maxvalue;
            }
            else
            {
                hScrollBar1.Maximum = 0;
            }
        }
        private int SurfaceWidth
        {
            get { return (int)surface.ActualWidth; }
        }
        private int SurfaceHeight
        {
            get { return (int)surface.ActualHeight; }
        }
        private int MaxKeyFrame
        {
            get
            {
                if (EffectManager == null) return 0;
                int max = 0;
                foreach (IEffect effect in EffectManager.Effects)
                {
                    if (max < effect.StartFrame + effect.FrameLength) max = effect.StartFrame + effect.FrameLength;
                }
                return max;
            }
        }
        private int MaxVisibleFrameCount
        {
            get
            {
                return SurfaceWidth / KeyWidth;
            }
        }
        private int MaxItemNumber
        {
            get
            {
                return EffectManager.Effects.Count;
            }
        }
        private int ShownItemNumber
        {
            get
            {
                return Math.Max(0, Math.Min((SurfaceHeight - TimeLineAreaHeight) / ItemHeight, EffectManager.Effects.Count));
            }
        }
        public int VScrollValue
        {
            get
            {
                return VValue;
            }
        }
        public void VScrollChangeValue(int diff)
        {
            int num = VValue + diff;
            if (vScrollBar1.Minimum <= num && num <= vScrollBar1.Maximum) vScrollBar1.Value = num;
            MainForm.RefreshTimeLine();
        }
        void VScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            vScrollBar1.Value = Math.Round(vScrollBar1.Value);
            MainForm.RefreshTimeLine();
        }
        private void Draw(DrawingContext g)
        {
            if (EffectManager == null) return;
            int itemcount = VValue == (int)vScrollBar1.Maximum ? ShownItemNumber : ShownItemNumber + 1;
            DrawGrid(g, itemcount);
            int iter = -VValue;
            for (int i = EffectManager.Effects.Count - 1; i >= 0; i--)
            {
                IEffect effect = EffectManager.Effects[i];
                if (effect is BaseEffect be)
                {
                    bool first = true;
                    if (iter >= 0)
                    {
                        foreach (EffectStateRatioSet set in be.Sets.Values)
                        {
                            if (first) DrawKey((set.StartFrame - HValue) * KeyWidth, iter * ItemHeight + TimeLineAreaHeight, g, set.StartState == SelectedManager.State, true);
                            DrawKey((set.EndFrame - HValue) * KeyWidth, iter * ItemHeight + TimeLineAreaHeight, g, set.EndState == SelectedManager.State, IsKey(set));
                            var x = (set.StartFrame + 1 - HValue) * KeyWidth;
                            var y = iter * ItemHeight + TimeLineAreaHeight;
                            var width = (set.EndFrame - set.StartFrame - 1) * KeyWidth;
                            if (width > 0)
                            {
                                g.DrawRectangle(Brushes.White, null, new Rect(x, y, width, ItemHeight));
                            }
                            DrawingSurface.DrawFrame(g, DrawingSurface.BlackPen, x, y, width, ItemHeight);
                            first = false;
                        }
                    }
                    iter++;
                }
            }
            DrawTimeLine(g, itemcount);
        }
        private static bool IsKey(EffectStateRatioSet set)
        {
            return set[RatioType.X] is ConstantRatioMaker;
        }
        private void DrawTimeLine(DrawingContext g, int itemcount)
        {
            g.DrawRectangle(timeLineAreaBrush, null, new Rect(0, 0, SurfaceWidth, TimeLineAreaHeight));
            DrawingSurface.DrawFrame(g, DrawingSurface.LightGrayPen, 0, 0, SurfaceWidth, TimeLineAreaHeight);
            g.DrawRectangle(Brushes.Salmon, null, new Rect((currentframe - HValue) * KeyWidth, 0, KeyWidth, TimeLineAreaHeight));
            int maxnum = SurfaceWidth / KeyWidth;
            for (int i = 0; i <= maxnum; i++)
            {
                DrawingSurface.DrawLine(g, DrawingSurface.LightGrayPen, i * KeyWidth, TimeLineAreaHeight - 4, i * KeyWidth, TimeLineAreaHeight - 2);
                if ((i + HValue) % 5 == 0) surface.DrawText(g, (i + HValue).ToString(), Brushes.Black, i * KeyWidth, 3);
            }
            DrawingSurface.DrawFrame(g, DrawingSurface.RedPen, (currentframe - HValue) * KeyWidth, 0, KeyWidth, TimeLineAreaHeight);
            var lineX = (currentframe + 0.5 - HValue) * KeyWidth;
            DrawingSurface.DrawLine(g, DrawingSurface.RedPen, lineX, TimeLineAreaHeight, lineX, TimeLineAreaHeight + itemcount * ItemHeight);
        }
        private void DrawGrid(DrawingContext g, int itemcount)
        {
            g.DrawRectangle(Brushes.White, null, new Rect(0, TimeLineAreaHeight, SurfaceWidth, itemcount * ItemHeight));
            int maxnum = SurfaceWidth / KeyWidth;
            for (int i = 0; i < maxnum; i++)
            {
                if ((i + HValue) % 5 == 0) g.DrawRectangle(timeLineAreaBrush, null, new Rect(KeyWidth * i, TimeLineAreaHeight, KeyWidth, itemcount * ItemHeight));
                DrawingSurface.DrawLine(g, DrawingSurface.SilverPen, KeyWidth * i, TimeLineAreaHeight, KeyWidth * i, TimeLineAreaHeight + itemcount * ItemHeight);
            }
            for (int i = 0; i < itemcount; i++)
            {
                DrawingSurface.DrawLine(g, DrawingSurface.SilverPen, 0, TimeLineAreaHeight + ItemHeight * i, SurfaceWidth, TimeLineAreaHeight + ItemHeight * i);
            }
        }
        private static void DrawKey(int x, int y, DrawingContext g, bool selected, bool IsKey)
        {
            g.DrawRectangle(selected ? selectedBrush : Brushes.White, null, new Rect(x, y, KeyWidth, ItemHeight));
            DrawingSurface.DrawFrame(g, DrawingSurface.BlackPen, x, y, KeyWidth, ItemHeight);
            var center = new Point(x + KeyWidth / 2.0 + 0.5, y + ItemHeight / 2.0 + 0.5);
            if (IsKey) g.DrawEllipse(Brushes.Black, null, center, CircleDiameter / 2.0, CircleDiameter / 2.0);
            else g.DrawEllipse(null, DrawingSurface.BlackPen, center, CircleDiameter / 2.0, CircleDiameter / 2.0);
        }
        private int KeyIndex(double x)
        {
            return (int)(x / KeyWidth) + HValue;
        }
        private int ItemIndex(double y)
        {
            return EffectManager.Effects.Count - 1 - (int)((y - TimeLineAreaHeight) / ItemHeight) - VValue;
        }
        private void TimeLine_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (EffectManager == null) return;
            var pos = e.GetPosition(surface);
            if (e.ChangedButton == MouseButton.Right)
            {
                pressedkeyindex = KeyIndex(pos.X);
                presseditemindex = ItemIndex(pos.Y);
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                surface.CaptureMouse();
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) keyopmode = KeyOperationMode.FixMove;
                else keyopmode = KeyOperationMode.Move;
                var itemindex = ItemIndex(pos.Y);
                if (pos.Y >= TimeLineAreaHeight && itemindex >= 0 && itemindex < EffectManager.Effects.Count)
                {
                    mousedown = true;
                    MainForm.PropertyDock.IsRatioMakerAvailable = false;
                    SelectedManager.Effect = EffectManager.Effects[itemindex];
                    FindKey(itemindex, pos.X);
                }
                else if (pos.Y >= 0 && pos.Y <= TimeLineAreaHeight)
                {
                    var keyindex = KeyIndex(pos.X);
                    if (keyindex < 0) keyindex = 0;
                    CurrentFrame = keyindex;
                    movetimeline = true;
                }
                else
                {
                    mousedown = false;
                }
            }
        }
        private void FindKey(int itemindex, double x)
        {
            var keyindex = KeyIndex(x);
            foreach (EffectStateRatioSet set in EffectManager.Effects[itemindex].Sets.Values)
            {
                if (set.StartFrame == keyindex)
                {
                    beforeframe = keyindex;
                    newframe = keyindex;
                    SelectedManager.Set = set;
                    SelectedManager.State = set.StartState;
                    MainForm.PropertyDock.IsRatioMakerAvailable = false;
                    return;
                }
                else if (set.EndFrame == keyindex)
                {
                    beforeframe = keyindex;
                    newframe = keyindex;
                    MainForm.PropertyDock.IsRatioMakerAvailable = !IsKey(set);
                    SelectedManager.Set = set;
                    SelectedManager.State = set.EndState;
                    return;
                }
            }
            mousedown = false;
        }

        private void TimeLine_MouseMove(object sender, MouseEventArgs e)
        {
            if (EffectManager == null) return;
            var pos = e.GetPosition(surface);
            if (movetimeline)
            {
                var keyindex = KeyIndex(pos.X);
                if (keyindex < 0) keyindex = 0;
                CurrentFrame = keyindex;
            }
            if (SelectedManager.Set != null && mousedown)
            {
                if (SelectedManager.Set.StartState == SelectedManager.State && SelectedManager.Set.StartFrame == 0)
                {
                    return;
                }

                var keyindex = KeyIndex(pos.X);
                switch (keyopmode)
                {
                    case KeyOperationMode.Move:
                        if (keyindex < 0) keyindex = 0;
                        if (SelectedManager.Set.StartState == SelectedManager.State)
                        {
                            if (keyindex >= SelectedManager.Set.EndFrame) keyindex = SelectedManager.Set.EndFrame - 1;
                            SelectedManager.Set.StartFrame = keyindex;
                            newframe = keyindex;
                        }
                        else
                        {
                            if (keyindex <= SelectedManager.Set.StartFrame) keyindex = SelectedManager.Set.StartFrame + 1;
                            var selectedindex = SelectedManager.Effect.Sets.IndexOfValue(SelectedManager.Set);
                            if (selectedindex >= 0 && selectedindex < SelectedManager.Effect.Sets.Count - 1)
                            {
                                if (keyindex >= SelectedManager.Effect.Sets.Values[selectedindex + 1].EndFrame) keyindex = SelectedManager.Effect.Sets.Values[selectedindex + 1].EndFrame - 1;
                                EffectStateRatioSet temp = SelectedManager.Effect.Sets.Values[selectedindex + 1];
                                temp.StartFrame = keyindex;
                                SelectedManager.Effect.Sets.RemoveAt(selectedindex + 1);
                                SelectedManager.Effect.Sets.Add(temp.StartFrame, temp);
                            }
                            SelectedManager.Set.EndFrame = keyindex;
                            newframe = keyindex;
                        }
                        SelectedManager.Effect.CheckFrameLength();
                        AdjustHScrollBar();
                        if (keyindex - HValue < 0) GainHScroll(-1);
                        else if (keyindex - HValue > MaxVisibleFrameCount) GainHScroll(1);
                        DrawAndRefresh();
                        break;
                    case KeyOperationMode.FixMove:
                        if (SelectedManager.Set.StartState == SelectedManager.State)
                        {
                            keyopmode = KeyOperationMode.RightMoveOnly;
                        }
                        else
                        {
                            keyopmode = KeyOperationMode.LeftMoveOnly;
                        }

                        break;
                    case KeyOperationMode.LeftMoveOnly:
                        if (keyindex <= SelectedManager.Set.StartFrame) keyindex = SelectedManager.Set.StartFrame + 1;
                        newframe = keyindex;
                        int diff = keyindex - SelectedManager.Set.EndFrame;
                        if (diff != 0)
                        {
                            SelectedManager.Set.EndFrame += diff;
                            var pool = new List<EffectStateRatioSet>();
                            var removekey = new List<int>();
                            bool found = false;
                            foreach (KeyValuePair<int, EffectStateRatioSet> pair in SelectedManager.Effect.Sets)
                            {
                                if (found)
                                {
                                    pair.Value.StartFrame += diff;
                                    pair.Value.EndFrame += diff;
                                    pool.Add(pair.Value);
                                    removekey.Add(pair.Key);
                                }
                                found |= pair.Value == SelectedManager.Set;
                            }
                            foreach (int val in removekey)
                            {
                                SelectedManager.Effect.Sets.Remove(val);
                            }
                            foreach (EffectStateRatioSet set in pool)
                            {
                                SelectedManager.Effect.Sets.Add(set.StartFrame, set);
                            }
                            SelectedManager.Effect.CheckFrameLength();
                            AdjustHScrollBar();
                            DrawAndRefresh();
                        }

                        break;
                    case KeyOperationMode.RightMoveOnly:
                        if (keyindex < 0) keyindex = 0;
                        newframe = keyindex;
                        diff = keyindex - SelectedManager.Set.StartFrame;
                        if (diff != 0)
                        {
                            var pool = new List<EffectStateRatioSet>(SelectedManager.Effect.Sets.Values);
                            SelectedManager.Effect.Sets.Clear();
                            foreach (EffectStateRatioSet set in pool)
                            {
                                set.StartFrame += diff;
                                set.EndFrame += diff;
                            }
                            foreach (EffectStateRatioSet set in pool)
                            {
                                SelectedManager.Effect.Sets.Add(set.StartFrame, set);
                            }
                            SelectedManager.Effect.CheckFrameLength();
                            AdjustHScrollBar();
                            DrawAndRefresh();
                        }

                        break;
                }
            }
        }
        private void GainHScroll(int diff)
        {
            int val = diff + HValue;
            if (hScrollBar1.Minimum <= val && val <= hScrollBar1.Maximum) hScrollBar1.Value = val;
        }
        private void TimeLine_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }
            surface.ReleaseMouseCapture();
            if (mousedown && beforeframe != newframe && keyopmode != KeyOperationMode.FixMove && keyopmode != KeyOperationMode.None)
            {
                SelectedManager.Effect.CheckFrameLength();
                MainForm.MoveState(beforeframe, newframe, keyopmode);
            }
            mousedown = false;
            movetimeline = false;
        }

        private void キーステートを挿入ToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainForm.AddState(presseditemindex, pressedkeyindex, true);
        }

        private void ノーマルステートを挿入ToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainForm.AddState(presseditemindex, pressedkeyindex, false);
        }

        private void ContextMenu_Opening(object sender, ContextMenuEventArgs e)
        {
            if (EffectManager == null || presseditemindex < 0 || presseditemindex >= EffectManager.Effects.Count)
            {
                e.Handled = true;
                return;
            }
            if (pressedkeyindex < 0)
            {
                e.Handled = true;
                return;
            }
            foreach (EffectStateRatioSet set in EffectManager.Effects[presseditemindex].Sets.Values)
            {
                if (set.StartFrame == pressedkeyindex || set.EndFrame == pressedkeyindex)
                {
                    キーステートを挿入ToolStripMenuItem.IsEnabled = false;
                    ノーマルステートを挿入ToolStripMenuItem.IsEnabled = false;
                    ステートを削除ToolStripMenuItem.IsEnabled = true;
                    return;
                }
            }
            キーステートを挿入ToolStripMenuItem.IsEnabled = true;
            ノーマルステートを挿入ToolStripMenuItem.IsEnabled = true;
            ステートを削除ToolStripMenuItem.IsEnabled = false;
        }

        private void ステートを削除ToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (EffectManager.Effects[presseditemindex].Sets.Count <= 1)
            {
                MessageBox.Show("削除するには少なくとも3つ以上のステートが必要です");
                return;
            }
            MainForm.DeleteState(presseditemindex, pressedkeyindex);
        }
    }
}
