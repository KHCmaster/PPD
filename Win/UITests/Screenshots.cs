using FlaUI.Core.Capturing;
using System;
using System.Drawing;

namespace UITests
{
    /// <summary>
    /// 画面を撮って比べます。DirectX で描くところなど、UI Automation で中身を調べられないところのテストに使う。
    /// </summary>
    /// <remarks>
    /// 画面に見えている部分を撮るので、撮る前にウィンドウを前に出しておく。
    /// </remarks>
    public static class Screenshots
    {
        /// <summary>
        /// 画面の範囲を撮ります。
        /// </summary>
        public static Bitmap Take(Rectangle area)
        {
            return Capture.Rectangle(area).Bitmap;
        }

        /// <summary>
        /// 2 つの画像で、色の違う点の数を返します（大きさは同じであること）。
        /// </summary>
        public static int CountDifferentPixels(Bitmap a, Bitmap b)
        {
            if (a.Size != b.Size)
            {
                throw new ArgumentException("画像の大きさが違います。");
            }
            int count = 0;
            for (int y = 0; y < a.Height; y++)
            {
                for (int x = 0; x < a.Width; x++)
                {
                    if (a.GetPixel(x, y).ToArgb() != b.GetPixel(x, y).ToArgb())
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// 条件に合う色の点の数を返します。
        /// </summary>
        public static int CountPixels(Bitmap image, Func<Color, bool> predicate)
        {
            int count = 0;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if (predicate(image.GetPixel(x, y)))
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}
