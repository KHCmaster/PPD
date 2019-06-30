using Effect2D;
using PPDConfiguration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Effect2DEditor
{
    public partial class TimeLine : CustomUserControl
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
        public MainForm MainForm
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
                    if (EffectManager != null) EffectManager.Update(currentframe, null);
                    if (MainForm != null) MainForm.RefreshCanvas();
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
            InitializeComponent();
            InitializeBuffer();
            hScrollBar1.Maximum = 0;
            hScrollBar1.Minimum = 0;
            hScrollBar1.SmallChange = 1;
            hScrollBar1.LargeChange = 1;
            vScrollBar1.Maximum = 0;
            vScrollBar1.Minimum = 0;
            vScrollBar1.LargeChange = 1;
            vScrollBar1.SmallChange = 1;
            vScrollBar1.Scroll += vScrollBar1_Scroll;
            hScrollBar1.Scroll += hScrollBar1_Scroll;
            this.SizeChanged += TimeLine_SizeChanged;
            this.MouseWheel += TimeLine_MouseWheel;
        }

        public void SetLang(SettingReader setting)
        {
            if (setting != null)
            {
                キーステートを挿入ToolStripMenuItem.Text = setting["InsertKeyState"];
                ノーマルステートを挿入ToolStripMenuItem.Text = setting["InsertNormalState"];
                ステートを削除ToolStripMenuItem.Text = setting["DeleteState"];
            }
        }

        void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            DrawAndRefresh();
        }

        void TimeLine_MouseWheel(object sender, MouseEventArgs e)
        {
            int num = vScrollBar1.Value;
            if (e.Delta >= 0) num--;
            else num++;
            if (num >= vScrollBar1.Minimum && num <= vScrollBar1.Maximum)
            {
                vScrollBar1.Value = num;
                MainForm.RefreshTimeLine();
            }
        }

        void TimeLine_SizeChanged(object sender, EventArgs e)
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
            int maxvalue = this.Width / KeyWidth;
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
                return this.Width / KeyWidth;
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
                return Math.Min((this.Height - hScrollBar1.Height - TimeLineAreaHeight) / ItemHeight, EffectManager.Effects.Count);
            }
        }
        public int VScrollValue
        {
            get
            {
                return vScrollBar1.Value;
            }
        }
        public void VScrollChangeValue(int diff)
        {
            int num = vScrollBar1.Value + diff;
            if (vScrollBar1.Minimum <= num && num <= vScrollBar1.Maximum) vScrollBar1.Value = num;
            MainForm.RefreshTimeLine();
        }
        void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            MainForm.RefreshTimeLine();
        }
        protected override void DrawToBuffer(Graphics g)
        {
            g.Clear(SystemColors.Control);
            if (EffectManager == null) return;
            int itemcount = vScrollBar1.Value == vScrollBar1.Maximum ? ShownItemNumber : ShownItemNumber + 1;
            DrawGrid(g, itemcount);
            int iter = -vScrollBar1.Value;
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
                            if (first) DrawKey(new Point((set.StartFrame - hScrollBar1.Value) * KeyWidth, iter * ItemHeight + TimeLineAreaHeight), g, set.StartState == SelectedManager.State, true);
                            DrawKey(new Point((set.EndFrame - hScrollBar1.Value) * KeyWidth, iter * ItemHeight + TimeLineAreaHeight), g, set.EndState == SelectedManager.State, IsKey(set));
                            var rec = new Rectangle((set.StartFrame + 1 - hScrollBar1.Value) * KeyWidth, iter * ItemHeight + TimeLineAreaHeight, (set.EndFrame - set.StartFrame - 1) * KeyWidth, ItemHeight);
                            g.FillRectangle(Brushes.White, rec);
                            g.DrawRectangle(Pens.Black, rec);
                            first = false;
                        }
                    }
                    iter++;
                }
            }
            DrawTimeLine(g, itemcount);
        }
        private bool IsKey(EffectStateRatioSet set)
        {
            return set[RatioType.X] is ConstantRatioMaker;
        }
        private void DrawTimeLine(Graphics g, int itemcount)
        {
            g.FillRectangle(Brushes.WhiteSmoke, new Rectangle(0, 0, this.Width, TimeLineAreaHeight));
            g.DrawRectangle(Pens.LightGray, new Rectangle(0, 0, this.Width, TimeLineAreaHeight));
            g.FillRectangle(Brushes.Salmon, new Rectangle((currentframe - hScrollBar1.Value) * KeyWidth, 0, KeyWidth, TimeLineAreaHeight));
            int maxnum = this.Width / KeyWidth;
            for (int i = 0; i <= maxnum; i++)
            {
                g.DrawLine(Pens.LightGray, i * KeyWidth, TimeLineAreaHeight - 4, i * KeyWidth, TimeLineAreaHeight - 2);
                if ((i + hScrollBar1.Value) % 5 == 0) g.DrawString((i + hScrollBar1.Value).ToString(), Font, Brushes.Black, new PointF(i * KeyWidth, 3));
            }
            g.DrawRectangle(Pens.Red, new Rectangle((currentframe - hScrollBar1.Value) * KeyWidth, 0, KeyWidth, TimeLineAreaHeight));
            g.DrawLine(Pens.Red, (currentframe + 0.5f - hScrollBar1.Value) * KeyWidth, TimeLineAreaHeight, (currentframe + 0.5f - hScrollBar1.Value) * KeyWidth, TimeLineAreaHeight + itemcount * ItemHeight);
        }
        private void DrawGrid(Graphics g, int itemcount)
        {
            g.FillRectangle(Brushes.White, new Rectangle(0, TimeLineAreaHeight, this.Width, itemcount * ItemHeight));
            int maxnum = this.Width / KeyWidth;
            for (int i = 0; i < maxnum; i++)
            {
                if ((i + hScrollBar1.Value) % 5 == 0) g.FillRectangle(Brushes.WhiteSmoke, new Rectangle(KeyWidth * i, TimeLineAreaHeight, KeyWidth, itemcount * ItemHeight));
                g.DrawLine(Pens.Silver, KeyWidth * i, TimeLineAreaHeight, KeyWidth * i, TimeLineAreaHeight + itemcount * ItemHeight);
            }
            for (int i = 0; i < itemcount; i++)
            {
                g.DrawLine(Pens.Silver, 0, TimeLineAreaHeight + ItemHeight * i, this.Width, TimeLineAreaHeight + ItemHeight * i);
            }
        }
        private void DrawKey(Point pos, Graphics g, bool selected, bool IsKey)
        {
            var rec = new Rectangle(pos, new Size(KeyWidth, ItemHeight));
            if (selected) g.FillRectangle(Brushes.LightBlue, pos.X, pos.Y, KeyWidth, ItemHeight);
            else g.FillRectangle(Brushes.White, pos.X, pos.Y, KeyWidth, ItemHeight);
            g.DrawRectangle(Pens.Black, rec);
            rec = new Rectangle(pos.X + (KeyWidth - CircleDiameter) / 2, pos.Y + (ItemHeight - CircleDiameter) / 2, CircleDiameter, CircleDiameter);
            if (IsKey) g.FillEllipse(Brushes.Black, rec);
            else g.DrawEllipse(Pens.Black, rec);
        }
        private int KeyIndex(int x)
        {
            return x / KeyWidth + hScrollBar1.Value;
        }
        private int ItemIndex(int y)
        {
            return EffectManager.Effects.Count - 1 - (y - TimeLineAreaHeight) / ItemHeight - vScrollBar1.Value;
        }
        private void TimeLine_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                pressedkeyindex = KeyIndex(e.X);
                presseditemindex = ItemIndex(e.Y);
            }
            else
            {
                if ((ModifierKeys & Keys.Control) == Keys.Control) keyopmode = KeyOperationMode.FixMove;
                else keyopmode = KeyOperationMode.Move;
                var itemindex = ItemIndex(e.Y);
                if (e.Y >= TimeLineAreaHeight && itemindex >= 0 && itemindex < EffectManager.Effects.Count)
                {
                    mousedown = true;
                    MainForm.PropertyDock.IsRatioMakerAvailable = false;
                    SelectedManager.Effect = EffectManager.Effects[itemindex];
                    FindKey(itemindex, e.X);
                }
                else if (e.Y >= 0 && e.Y <= TimeLineAreaHeight)
                {
                    var keyindex = KeyIndex(e.X);
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
        private void FindKey(int itemindex, int x)
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
            if (movetimeline)
            {
                var keyindex = KeyIndex(e.X);
                if (keyindex < 0) keyindex = 0;
                CurrentFrame = keyindex;
            }
            if (SelectedManager.Set != null && mousedown)
            {
                if (SelectedManager.Set.StartState == SelectedManager.State && SelectedManager.Set.StartFrame == 0)
                {
                    return;
                }

                var keyindex = KeyIndex(e.X);
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
                        if (keyindex - hScrollBar1.Value < 0) GainHScroll(-1);
                        else if (keyindex - hScrollBar1.Value > MaxVisibleFrameCount) GainHScroll(1);
                        DrawAndRefresh();
                        break;
                    case KeyOperationMode.FixMove:
                        if (keyindex < 0) keyindex = 0;
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
            int val = diff + hScrollBar1.Value;
            if (hScrollBar1.Minimum <= val && val <= hScrollBar1.Maximum) hScrollBar1.Value = val;
        }
        private void TimeLine_MouseUp(object sender, MouseEventArgs e)
        {
            if (mousedown && beforeframe != newframe && keyopmode != KeyOperationMode.FixMove && keyopmode != KeyOperationMode.None)
            {
                SelectedManager.Effect.CheckFrameLength();
                MainForm.MoveState(beforeframe, newframe, keyopmode);
            }
            mousedown = false;
            movetimeline = false;
        }

        private void キーステートを挿入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainForm.AddState(presseditemindex, pressedkeyindex, true);
        }

        private void ノーマルステートを挿入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainForm.AddState(presseditemindex, pressedkeyindex, false);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (presseditemindex < 0 || presseditemindex >= EffectManager.Effects.Count)
            {
                e.Cancel = true;
                //MessageBox.Show("範囲外です:EffectIndex");
                return;
            }
            if (pressedkeyindex < 0)
            {
                e.Cancel = true;
                //MessageBox.Show("負のフレーム数です");
                return;
            }
            foreach (EffectStateRatioSet set in EffectManager.Effects[presseditemindex].Sets.Values)
            {
                if (set.StartFrame == pressedkeyindex || set.EndFrame == pressedkeyindex)
                {
                    キーステートを挿入ToolStripMenuItem.Enabled = false;
                    ノーマルステートを挿入ToolStripMenuItem.Enabled = false;
                    ステートを削除ToolStripMenuItem.Enabled = true;
                    return;
                }
            }
            キーステートを挿入ToolStripMenuItem.Enabled = true;
            ノーマルステートを挿入ToolStripMenuItem.Enabled = true;
            ステートを削除ToolStripMenuItem.Enabled = false;
        }

        private void ステートを削除ToolStripMenuItem_Click(object sender, EventArgs e)
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
