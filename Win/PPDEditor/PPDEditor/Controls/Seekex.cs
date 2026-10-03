using PPDFramework;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PPDEditor.Controls
{
    /// <summary>
    /// タイムラインの左の、行の名前（マークの画像）を出すところです。行をドラッグすると、その行のマークをまとめて動かす。
    /// </summary>
    public class Seekex : FrameworkElement
    {
        Seekmain sm;
        int num = -1;
        int anchorx;
        ImageSource[] images;

        public Seekex()
        {
            ClipToBounds = true;
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }

        public void Setseekmain(Seekmain sm)
        {
            this.sm = sm;
        }
        public void SetSkin()
        {
            images = new ImageSource[10];
            for (int i = 0; i < 10; i++)
            {
                var path = PPDEditorSkin.Skin.GetSmallMarkColorImagePath((ButtonType)i);
                if (File.Exists(path))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = new Uri(Path.GetFullPath(path), UriKind.Absolute);
                    image.EndInit();
                    image.Freeze();
                    images[i] = image;
                }
                else
                {
                    images[i] = null;
                }
            }
            DrawAndRefresh();
        }

        public void DrawAndRefresh()
        {
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (WindowUtility.TimeLineForm == null || images == null)
            {
                return;
            }

            var skin = PPDEditorSkin.Skin;
            var width = ActualWidth;
            var area = TimeLineDrawer.ToBrush(skin.TimeLineSeekAreaColor);
            var border = TimeLineDrawer.ToPen(skin.TimeLineSeekAreaBorderColor);
            var text = TimeLineDrawer.ToBrush(skin.TimeLineTextColor);
            var hline = TimeLineDrawer.ToPen(skin.TimeLineHorizontalLineColor);
            dc.DrawRectangle(WindowUtility.TimeLineForm.TimeLineBackground, null, new Rect(0, 0, width, ActualHeight));
            dc.DrawRectangle(area, null, new Rect(0, 0, width, skin.TimeLineHeight));
            TimeLineDrawer.DrawLine(dc, border, 0, skin.TimeLineHeight - 1, width, skin.TimeLineHeight - 1);
            var typeface = new Typeface(new FontFamily("ＭＳ ゴシック"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            // 以前の GDI と同じく、行の高さに合わせたポイントの大きさ
            dc.DrawText(TimeLineDrawer.CreateText("Time Line", typeface, (skin.TimeLineHeight - 3) * 96.0 / 72, text), new Point(0, 0));
            int iter = 0;
            foreach (var row in WindowUtility.TimeLineForm.RowManager.OrderedVisibleRows)
            {
                dc.DrawRectangle(TimeLineDrawer.GetRowBrush(row), null,
                    new Rect(0, skin.TimeLineHeight + skin.TimeLineRowHeight * iter + 1, width, skin.TimeLineRowHeight));
                var y = skin.TimeLineHeight + skin.TimeLineRowHeight * (iter + 1);
                TimeLineDrawer.DrawLine(dc, hline, 0, y, width, y);
                if (images[row] != null)
                {
                    dc.DrawImage(images[row], new Rect(0, skin.TimeLineHeight + skin.TimeLineRowHeight * iter, skin.TimeLineHeight, skin.TimeLineHeight));
                }
                iter++;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var p = e.GetPosition(this);
            anchorx = (int)p.X;
            if (p.Y >= 0 && p.Y <= PPDEditorSkin.Skin.TimeLineHeight)
            {
                num = int.MaxValue;
            }
            else
            {
                num = WindowUtility.TimeLineForm.RowManager.GetRowIndex((int)p.Y);
            }
            CaptureMouse();
            sm?.Focus();
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if ((num >= 0 && num < 10) || num == int.MaxValue)
            {
                var x = (int)e.GetPosition(this).X;
                sm.MoveLine(num, x - anchorx);
                anchorx = x;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            num = -1;
            ReleaseMouseCapture();
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            num = -1;
        }
    }
}
