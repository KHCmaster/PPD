using Effect2D;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Effect2DEditor
{
    /// <summary>
    /// タイムラインの左側の、エフェクトの名前の一覧です。ドラッグで順番を入れ替えます。
    /// </summary>
    public class TimeLineItem : DrawingSurface
    {
        const int TimeLineAreaHeight = 20;
        const int ItemHeight = 18;
        const int StringOffsetLeft = 2;
        const int StringOffsetTop = 2;
        bool mousedown;
        Point mousedownpos;
        Point mousepos;
        int pressedrow;
        int swapborderindex = -1;
        int movecount;
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
        /// <summary>
        /// 順番の入れ替えを履歴に残すエディターです。
        /// </summary>
        public ViewModels.ITimeLineHost Host
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
            Render = Draw;
        }
        private void Draw(DrawingContext g)
        {
            if (EffectManager == null || TimeLine == null) return;
            int iter = -TimeLine.VScrollValue;
            for (int i = EffectManager.Effects.Count - 1; i >= 0; i--)
            {
                IEffect effect = EffectManager.Effects[i];
                if (effect is BaseEffect be)
                {
                    if (iter >= 0)
                    {
                        DrawRow(g, be, iter * ItemHeight + TimeLineAreaHeight);
                    }
                    iter++;
                }
            }
            if (swapborderindex != -1)
            {
                g.DrawRectangle(Brushes.Black, null, new Rect(0, TimeLineAreaHeight + (swapborderindex - TimeLine.VScrollValue) * ItemHeight - 2, ActualWidth, 4));
            }
            if (mousedown && SelectedManager.Effect is BaseEffect selected)
            {
                // ドラッグしている項目を、マウスに付いてくるように半透明で描く
                g.PushOpacity(0.6);
                DrawRow(g, selected, pressedrow * ItemHeight + TimeLineAreaHeight + mousepos.Y - mousedownpos.Y);
                g.Pop();
            }
        }

        private void DrawRow(DrawingContext g, BaseEffect be, double y)
        {
            g.DrawRectangle(be == SelectedManager.Effect ? Brushes.LightBlue : Brushes.White, null, new Rect(0, y, ActualWidth - 1, ItemHeight));
            DrawFrame(g, BlackPen, 0, y, ActualWidth - 1, ItemHeight);
            DrawText(g, Path.GetFileNameWithoutExtension(be.Filename), Brushes.Black, StringOffsetLeft, StringOffsetTop + y, ActualWidth - StringOffsetLeft);
        }

        private int ItemIndex(double y)
        {
            return EffectManager.Effects.Count - 1 - (int)((y - TimeLineAreaHeight) / ItemHeight) - TimeLine.VScrollValue;
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (EffectManager == null) return;
            var pos = e.GetPosition(this);
            movecount = 0;
            var itemindex = ItemIndex(pos.Y);
            if (pos.Y >= TimeLineAreaHeight && itemindex >= 0 && itemindex < EffectManager.Effects.Count)
            {
                mousedown = true;
                CaptureMouse();
                SelectedManager.Effect = EffectManager.Effects[itemindex];
                pressedrow = (int)((pos.Y - TimeLineAreaHeight) / ItemHeight);
                mousedownpos = pos;
                mousepos = pos;
            }
            else
            {
                mousedown = false;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (mousedown && SelectedManager.Effect != null)
            {
                var pos = e.GetPosition(this);
                mousepos = pos;
                if (pos.Y < TimeLineAreaHeight)
                {
                    movecount++;
                    if (movecount % 10 == 0) TimeLine.VScrollChangeValue(-1);
                }
                else if (pos.Y > ActualHeight)
                {
                    movecount++;
                    if (movecount % 10 == 0) TimeLine.VScrollChangeValue(1);
                }
                int itemindex = (int)((pos.Y - TimeLineAreaHeight) / ItemHeight) + TimeLine.VScrollValue;
                if (itemindex < 0) itemindex = 0;
                if (itemindex > EffectManager.Effects.Count) itemindex = EffectManager.Effects.Count;
                swapborderindex = itemindex;
                DrawAndRefresh();
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            ReleaseMouseCapture();
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
                        Host.ChangeIndex(swapborderindex, selectedindex);
                    }
                    swapborderindex = -1;
                }
            }
            mousedown = false;
            DrawAndRefresh();
        }
    }
}
