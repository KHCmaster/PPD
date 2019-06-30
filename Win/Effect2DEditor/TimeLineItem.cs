using Effect2D;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Effect2DEditor
{
    public partial class TimeLineItem : CustomUserControl
    {
        const int TimeLineAreaHeight = 20;
        const int ItemHeight = 18;
        const int StringOffsetLeft = 2;
        const int StringOffsetTop = 4;
        bool mousedown;
        Point mousedownpos;
        int swapborderindex = -1;
        int movecount;
        TransParentForm tpf;
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
        public TimeLine TimeLine
        {
            get;
            set;
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
        public TimeLineItem()
        {
            InitializeComponent();
            InitializeBuffer();
        }
        protected override void DrawToBuffer(Graphics g)
        {
            g.Clear(SystemColors.Control);
            if (EffectManager == null || TimeLine == null) return;
            int iter = -TimeLine.VScrollValue;
            var sf = new StringFormat
            {
                Trimming = StringTrimming.EllipsisCharacter
            };
            for (int i = EffectManager.Effects.Count - 1; i >= 0; i--)
            {
                IEffect effect = EffectManager.Effects[i];
                if (effect is BaseEffect be)
                {
                    if (iter >= 0)
                    {
                        var rec = new Rectangle(0, iter * ItemHeight + TimeLineAreaHeight, this.Width - 1, ItemHeight);
                        if (be == SelectedManager.Effect) g.FillRectangle(Brushes.LightBlue, rec);
                        else g.FillRectangle(Brushes.White, rec);
                        g.DrawRectangle(Pens.Black, rec);
                        g.DrawString(Path.GetFileNameWithoutExtension(be.Filename), this.Font, Brushes.Black, new RectangleF(StringOffsetLeft, StringOffsetTop + iter * ItemHeight + TimeLineAreaHeight, this.Width, Font.Height), sf);
                    }
                    iter++;
                }
            }
            if (swapborderindex != -1)
            {
                g.FillRectangle(Brushes.Black, new Rectangle(0, TimeLineAreaHeight + (swapborderindex - TimeLine.VScrollValue) * ItemHeight - 2, this.Width, 4));
            }
        }

        private int ItemIndex(int y)
        {
            return EffectManager.Effects.Count - 1 - (y - TimeLineAreaHeight) / ItemHeight - TimeLine.VScrollValue;
        }
        private void TimeLineItem_MouseDown(object sender, MouseEventArgs e)
        {
            movecount = 0;
            var itemindex = ItemIndex(e.Y);
            if (e.Y >= TimeLineAreaHeight && itemindex >= 0 && itemindex < EffectManager.Effects.Count)
            {
                mousedown = true;
                SelectedManager.Effect = EffectManager.Effects[itemindex];
                tpf = new TransParentForm();
                tpf.Show();
                int hindex = (e.Y - TimeLineAreaHeight) / ItemHeight;
                tpf.SetInfo(this.PointToScreen(new Point(0, TimeLineAreaHeight + hindex * ItemHeight)), new Size(this.Width, ItemHeight + 1));
                tpf.Location = this.PointToScreen(new Point(0, TimeLineAreaHeight + hindex * ItemHeight));
                mousedownpos = new Point(e.X, e.Y);
            }
            else
            {
                mousedown = false;
            }
        }

        private void TimeLineItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousedown && SelectedManager.Effect != null)
            {
                tpf.Location = new Point(tpf.Location.X + e.X - mousedownpos.X, tpf.Location.Y + e.Y - mousedownpos.Y);
                mousedownpos = new Point(e.X, e.Y);
                if (e.Y < TimeLineAreaHeight)
                {
                    movecount++;
                    if (movecount % 10 == 0) TimeLine.VScrollChangeValue(-1);
                }
                else if (e.Y > this.Height)
                {
                    movecount++;
                    if (movecount % 10 == 0) TimeLine.VScrollChangeValue(1);
                }
                int itemindex = (e.Y - TimeLineAreaHeight) / ItemHeight + TimeLine.VScrollValue;
                if (itemindex < 0) itemindex = 0;
                if (itemindex > EffectManager.Effects.Count) itemindex = EffectManager.Effects.Count;
                if (itemindex != swapborderindex)
                {
                    swapborderindex = itemindex;
                    DrawAndRefresh();
                }
            }
        }

        private void TimeLineItem_MouseUp(object sender, MouseEventArgs e)
        {
            if (tpf != null)
            {
                tpf.Close();
                tpf.Dispose();
                tpf = null;
            }
            if (swapborderindex != -1)
            {
                var selectedindex = EffectManager.Effects.IndexOf(SelectedManager.Effect);
                if (selectedindex != -1)
                {
                    swapborderindex = EffectManager.Effects.Count - 1 - swapborderindex;
                    if (swapborderindex == selectedindex || swapborderindex + 1 == selectedindex)
                    {
                    }
                    else
                    {
                        MainForm.ChangeIndex(swapborderindex, selectedindex);
                    }
                    swapborderindex = -1;
                    DrawAndRefresh();
                }
            }
            mousedown = false;
        }

    }
}
