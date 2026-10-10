using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PPDEditor.Controls.Resource
{
    public partial class ImagePanel : UserControl
    {
        public ImagePanel()
        {
            InitializeComponent();
            // 画像を拡大縮小しないので、画素がぼやけないようにする
            RenderOptions.SetBitmapScalingMode(pictureBox1, BitmapScalingMode.NearestNeighbor);
        }

        public void OpenFile(string filePath)
        {
            pictureBox1.Source = LoadImage(filePath);
        }

        /// <summary>
        /// ファイルを開いたままにしないで画像を読み込む。
        /// </summary>
        public static BitmapSource LoadImage(string filePath)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.UriSource = new Uri(filePath, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            // DPI が 96 でない画像も、画素の数どおりの大きさで出す
            if (Math.Abs(image.DpiX - 96) < 0.01 && Math.Abs(image.DpiY - 96) < 0.01)
            {
                return image;
            }
            var stride = (image.PixelWidth * image.Format.BitsPerPixel + 7) / 8;
            var pixels = new byte[stride * image.PixelHeight];
            image.CopyPixels(pixels, stride, 0);
            var resized = BitmapSource.Create(image.PixelWidth, image.PixelHeight, 96, 96, image.Format, image.Palette, pixels, stride);
            resized.Freeze();
            return resized;
        }
    }
}
