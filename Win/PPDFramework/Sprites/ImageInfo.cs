using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace PPDFramework.Sprites
{
    /// <summary>
    /// 画像の情報を表すクラスです。
    /// </summary>
    public class ImageInfo
    {
        static readonly Regex scaleRegex = new Regex(@"\.(\d+)_(\d+)\.\w+$");

        Func<Image> imageCallback;

        /// <summary>
        /// パスを取得します。
        /// </summary>
        public string Path
        {
            get;
            private set;
        }

        /// <summary>
        /// 幅を取得します。
        /// </summary>
        public int Width
        {
            get;
            private set;
        }

        /// <summary>
        /// 2のべき乗の幅を取得します。
        /// </summary>
        public int WidthPowerOfTwo
        {
            get;
            private set;
        }

        /// <summary>
        /// 高さを取得します。
        /// </summary>
        public int Height
        {
            get;
            private set;
        }

        /// <summary>
        /// 2のべき乗の高さを取得します。
        /// </summary>
        public int HeightPowerOfTwo
        {
            get;
            private set;
        }

        /// <summary>
        /// 最後の書き込み日時を取得します。
        /// </summary>
        public DateTime LastWriteTime
        {
            get;
            private set;
        }

        /// <summary>
        /// スケールの分子を取得します。
        /// </summary>
        public int ScaleNumerator
        {
            get;
            private set;
        }

        /// <summary>
        /// スケールの分母を取得します。
        /// </summary>
        public int ScaleDenominator
        {
            get;
            private set;
        }

        /// <summary>
        /// スケールを取得します。
        /// </summary>
        public ImageScale Scale
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="path"></param>
        public ImageInfo(string path)
        {
            Path = path;
            imageCallback = () => Image.FromFile(path);
            InitializeImage();
            InitializeScale();
            LastWriteTime = File.GetLastWriteTime(path);
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="getStream"></param>
        public ImageInfo(string path, Func<Stream> getStream)
        {
            Path = path;
            imageCallback = () => Image.FromStream(getStream());
            InitializeImage();
            InitializeScale();
        }

        private void InitializeImage()
        {
            using (var img = imageCallback())
            {
                Width = img.Width;
                Height = img.Height;
            }
            WidthPowerOfTwo = Utility.UpperPowerOfTwo(Width);
            HeightPowerOfTwo = Utility.UpperPowerOfTwo(Height);
        }

        private void InitializeScale()
        {
            var match = scaleRegex.Match(Path);
            if (match.Success)
            {
                ScaleNumerator = int.Parse(match.Groups[1].Value);
                ScaleDenominator = int.Parse(match.Groups[2].Value);
            }
            else
            {
                ScaleNumerator = 1;
                ScaleDenominator = 1;
            }
            var gcd = Gcd(ScaleNumerator, ScaleDenominator);
            ScaleNumerator /= gcd;
            ScaleDenominator /= gcd;
            Scale = new ImageScale(ScaleNumerator, ScaleDenominator);
        }

        private int Gcd(int a, int b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                {
                    a %= b;
                }
                else
                {
                    b %= a;
                }
            }

            return a == 0 ? b : a;
        }

        /// <summary>
        /// 画像を取得します。
        /// </summary>
        /// <returns></returns>
        public Image GetImage()
        {
            return imageCallback();
        }
    }
}
