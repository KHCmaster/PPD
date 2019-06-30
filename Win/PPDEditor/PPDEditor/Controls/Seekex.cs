using PPDFramework;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PPDEditor.Controls
{
    public partial class Seekex : CustomUserControl
    {
        Seekmain sm;
        int num = -1;
        int anchorx;

        //Pens and Brush
        Brush area = Brushes.LightGray;
        Pen border = Pens.Gray;
        Brush text = Brushes.Black;
        Pen hline = Pens.Gray;

        Image[] images;

        public Seekex()
        {
            InitializeComponent();
            InitializeBuffer();
        }

        public void setseekmain(Seekmain sm)
        {
            this.sm = sm;
        }
        public void SetSkin()
        {
            images = new Image[10];
            for (int i = 0; i < 10; i++)
            {
                var path = PPDEditorSkin.Skin.GetSmallMarkColorImagePath((ButtonType)i);
                if (File.Exists(path))
                {
                    images[i] = Image.FromFile(path);
                }
                else
                {
                    images[i] = null;
                }
            }
            DrawAndRefresh();
        }

        protected override void DrawToBuffer(Graphics g)
        {
            if (WindowUtility.TimeLineForm == null || images == null)
            {
                return;
            }

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            area = new SolidBrush(PPDEditorSkin.Skin.TimeLineSeekAreaColor);
            border = new Pen(PPDEditorSkin.Skin.TimeLineSeekAreaBorderColor);
            text = new SolidBrush(PPDEditorSkin.Skin.TimeLineTextColor);
            hline = new Pen(PPDEditorSkin.Skin.TimeLineHorizontalLineColor);
            if (BackgroundImage != null)
            {
                g.DrawImage(BackgroundImage, new Point(0, 0));
            }
            else
            {
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, this.Width, this.Height));
            }
            g.FillRectangle(area, 0, 0, this.Width, PPDEditorSkin.Skin.TimeLineHeight);
            g.DrawLine(border, 0, PPDEditorSkin.Skin.TimeLineHeight - 1, this.Width, PPDEditorSkin.Skin.TimeLineHeight - 1);
            var font = new Font("ＭＳ ゴシック", PPDEditorSkin.Skin.TimeLineHeight - 3);
            g.DrawString("Time Line", font, text, 0, 0);
            int iter = 0;
            foreach (var row in WindowUtility.TimeLineForm.RowManager.OrderedVisibleRows)
            {
                g.FillRectangle(PPDEditorSkin.Skin.TimeLineBackGroundColors[row],
                    new Rectangle(0, PPDEditorSkin.Skin.TimeLineHeight + PPDEditorSkin.Skin.TimeLineRowHeight * iter + 1,
                        this.Width, PPDEditorSkin.Skin.TimeLineRowHeight));
                g.DrawLine(hline, 0, PPDEditorSkin.Skin.TimeLineHeight + PPDEditorSkin.Skin.TimeLineRowHeight * (iter + 1),
                    this.Width, PPDEditorSkin.Skin.TimeLineHeight + PPDEditorSkin.Skin.TimeLineRowHeight * (iter + 1));
                if (images[row] != null)
                {
                    g.DrawImage(images[row], new Rectangle(0, PPDEditorSkin.Skin.TimeLineHeight + PPDEditorSkin.Skin.TimeLineRowHeight * iter,
                        PPDEditorSkin.Skin.TimeLineHeight, PPDEditorSkin.Skin.TimeLineHeight),
                        new Rectangle(0, 0, images[row].Width, images[row].Height), GraphicsUnit.Pixel);
                }
                iter++;
            }
        }
        private void Seekex_MouseDown(object sender, MouseEventArgs e)
        {
            var p = this.PointToClient(Cursor.Position);
            anchorx = p.X;
            if (p.Y >= 0 && p.Y <= PPDEditorSkin.Skin.TimeLineHeight)
            {
                num = int.MaxValue;
            }
            else
            {
                num = WindowUtility.TimeLineForm.RowManager.GetRowIndex(p.Y);
            }
        }
        private void Seekex_MouseMove(object sender, MouseEventArgs e)
        {
            if ((num >= 0 && num < 10) || num == int.MaxValue)
            {
                var pos = this.PointToClient(Cursor.Position);
                sm.MoveLine(num, pos.X - anchorx);
                anchorx = pos.X;
            }
        }

        private void Seekex_MouseUp(object sender, MouseEventArgs e)
        {
            num = -1;
        }
    }
}
