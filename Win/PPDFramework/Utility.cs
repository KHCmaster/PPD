using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PPDFramework
{
    static class Utility
    {
        public static int UpperPowerOfTwo(int value)
        {
            --value;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return ++value;
        }


        public static void PreMultiplyAlpha(Bitmap bitmap)
        {
            var data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            byte[] buf = new byte[bitmap.Width * bitmap.Height * 4];
            Marshal.Copy(data.Scan0, buf, 0, buf.Length);
            for (int i = 0; i < buf.Length;)
            {
                var alpha = buf[i + 3];
                buf[i] = (byte)(buf[i] * alpha / 255f);
                buf[i + 1] = (byte)(buf[i + 1] * alpha / 255f);
                buf[i + 2] = (byte)(buf[i + 2] * alpha / 255f);
                i += 4;
            }
            Marshal.Copy(buf, 0, data.Scan0, buf.Length);
            bitmap.UnlockBits(data);
        }
    }
}
