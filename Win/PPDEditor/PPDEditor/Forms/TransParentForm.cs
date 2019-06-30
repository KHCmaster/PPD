using System.Drawing;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class TransParentForm : Form
    {
        public TransParentForm()
        {
            InitializeComponent();
        }
        public void SetInfo(Control c, Size size)
        {
            this.Size = size;
            var bit = new Bitmap(size.Width, size.Height);
            var g = Graphics.FromImage(bit);
            c.DrawToBitmap(bit, new Rectangle(0, 0, size.Width, size.Height));
            g.Dispose();
            this.BackgroundImage = bit;
        }
        public void SetInfo(Control c, Size size, Point location)
        {
            this.Size = size;
            var bit = new Bitmap(size.Width, size.Height);
            var g = Graphics.FromImage(bit);
            c.DrawToBitmap(bit, new Rectangle(location.X, location.Y, size.Width, size.Height));
            g.Dispose();
            this.BackgroundImage = bit;
        }
        public void SetInfo(Point screenpos, Size size)
        {
            this.Size = size;
            var bit = new Bitmap(size.Width, size.Height);
            var g = Graphics.FromImage(bit);
            g.CopyFromScreen(screenpos, new Point(0, 0), size, CopyPixelOperation.SourceCopy);
            g.Dispose();
            this.BackgroundImage = bit;
        }
    }
}
