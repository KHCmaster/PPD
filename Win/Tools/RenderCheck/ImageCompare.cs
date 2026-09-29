using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

/// <summary>
/// 2 枚の画像を画素単位で比較し、差分画像を出力します。
/// 使い方: ImageCompare.exe <基準画像> <比較画像> <差分画像の出力先> [許容する差分画素数]
/// 終了コード: 0 = 許容範囲内, 1 = 許容範囲外, 2 = 引数エラー・サイズ不一致
/// </summary>
static class ImageCompare
{
    static int Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("usage: ImageCompare.exe <expected> <actual> <diffOutput> [allowedPixels]");
            return 2;
        }
        var allowedPixels = args.Length > 3 ? int.Parse(args[3]) : int.MaxValue;
        using (var a = Load(args[0]))
        using (var b = Load(args[1]))
        {
            if (a.Width != b.Width || a.Height != b.Height)
            {
                Console.WriteLine("size mismatch {0}x{1} vs {2}x{3}", a.Width, a.Height, b.Width, b.Height);
                return 2;
            }
            var pa = ReadPixels(a);
            var pb = ReadPixels(b);
            var diff = new byte[pa.Length];
            int differing = 0, over8 = 0, maxDiff = 0;
            for (var i = 0; i < pa.Length; i += 4)
            {
                var d = Math.Max(Math.Abs(pa[i] - pb[i]), Math.Max(Math.Abs(pa[i + 1] - pb[i + 1]), Math.Abs(pa[i + 2] - pb[i + 2])));
                if (d > 0) differing++;
                if (d > 8) over8++;
                maxDiff = Math.Max(maxDiff, d);
                var v = (byte)Math.Min(255, d * 8);
                diff[i] = diff[i + 1] = diff[i + 2] = v;
                diff[i + 3] = 255;
            }
            using (var diffBitmap = new Bitmap(a.Width, a.Height, PixelFormat.Format32bppArgb))
            {
                WritePixels(diffBitmap, diff);
                diffBitmap.Save(args[2], ImageFormat.Png);
            }
            Console.WriteLine("size {0}x{1}, differing pixels {2}, >8 diff {3}, max diff {4}", a.Width, a.Height, differing, over8, maxDiff);
            return over8 <= allowedPixels ? 0 : 1;
        }
    }

    static Bitmap Load(string path)
    {
        using (var image = new Bitmap(path))
        {
            // 画像の DPI 情報によって拡大縮小されないように、画素数を指定して等倍で描画する
            var copy = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(copy))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }
            return copy;
        }
    }

    static byte[] ReadPixels(Bitmap bitmap)
    {
        var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var bytes = new byte[bitmap.Width * bitmap.Height * 4];
            for (var y = 0; y < bitmap.Height; y++)
            {
                Marshal.Copy(data.Scan0 + y * data.Stride, bytes, y * bitmap.Width * 4, bitmap.Width * 4);
            }
            return bytes;
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    static void WritePixels(Bitmap bitmap, byte[] bytes)
    {
        var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            for (var y = 0; y < bitmap.Height; y++)
            {
                Marshal.Copy(bytes, y * bitmap.Width * 4, data.Scan0 + y * data.Stride, bitmap.Width * 4);
            }
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }
}
