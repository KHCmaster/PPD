using System.Drawing;
using System.Windows.Forms;

namespace Effect2DEditor
{
    public partial class TransParentForm : Form
    {
        public TransParentForm()
        {
            InitializeComponent();
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
