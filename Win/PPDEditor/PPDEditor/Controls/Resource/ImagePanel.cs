using System;
using System.Drawing;
using System.Windows.Forms;

namespace PPDEditor.Controls.Resource
{
    public partial class ImagePanel : UserControl
    {
        private Image image;

        public ImagePanel()
        {
            InitializeComponent();

            Disposed += ImagePanel_Disposed;
            SizeChanged += ImagePanel_SizeChanged;
            Dock = DockStyle.Fill;
        }

        void ImagePanel_SizeChanged(object sender, EventArgs e)
        {
            ChangeLocation();
        }

        private void ChangeLocation()
        {
            if (image == null)
            {
                return;
            }

            int posX = 0, posY = 0;
            if (image.Width < this.Width)
            {
                posX = (this.Width - image.Width) / 2;
            }
            if (image.Height < this.Height)
            {
                posY = (this.Height - image.Height) / 2;
            }
            pictureBox1.Location = new Point(posX, posY);
        }

        void ImagePanel_Disposed(object sender, EventArgs e)
        {
            DisposeImage();
        }

        private void DisposeImage()
        {
            if (image != null)
            {
                image.Dispose();
                image = null;
            }
        }

        public void OpenFile(string filePath)
        {
            DisposeImage();

            using (Image loadImage = Image.FromFile(filePath))
            {
                image = new Bitmap(loadImage);
            }

            pictureBox1.Image = image;
            pictureBox1.Size = new Size(image.Width, image.Height);
            ChangeLocation();
        }
    }
}
