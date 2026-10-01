using System.Numerics;
using PPDMath;
using Vortice.Direct3D11;
using Vortice.DXGI;
using System.IO;
using System.Runtime.InteropServices;

namespace PPDFramework.Texture.DX11
{
    class TextureFactory : ITextureFactory
    {
        internal const Format TextureFormat = Format.B8G8R8A8_UNorm;

        public TextureBase Create(PPDDevice device, int width, int height, int level, bool pa)
        {
            return CreateFromPixels(device, new byte[width * height * 4], width, height, pa, true);
        }

        public TextureBase CreateRenderTarget(PPDDevice device, int width, int height, int level, bool pa)
        {
            var description = new Texture2DDescription
            {
                Width = (uint)width,
                Height = (uint)height,
                MipLevels = 1,
                ArraySize = 1,
                Format = TextureFormat,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            };
            var texture = ((ID3D11Device)device.Device).CreateTexture2D(description);
            return new Texture(device, texture, pa, true, null);
        }

        public TextureBase FromStream(PPDDevice device, Stream stream, int width, int height, bool pa)
        {
            return FromStream(device, stream, width, height, pa, false);
        }

        /// <summary>
        /// ストリームから作成します。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="stream"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pa"></param>
        /// <param name="keepCpuData">CPU側にデータを保持するかどうか。</param>
        /// <returns></returns>
        public static TextureBase FromStream(PPDDevice device, Stream stream, int width, int height, bool pa, bool keepCpuData)
        {
            using var bitmap = new System.Drawing.Bitmap(stream);
            return FromBitmap(device, bitmap, width, height, pa, keepCpuData);
        }

        public TextureBase FromFile(PPDDevice device, string filename, int width, int height, bool pa)
        {
            using var stream = File.OpenRead(filename);
            return FromStream(device, stream, width, height, pa, false);
        }

        private static TextureBase FromBitmap(PPDDevice device, System.Drawing.Bitmap bitmap, int width, int height, bool pa, bool keepCpuData)
        {
            if (width <= 0)
            {
                width = bitmap.Width;
            }
            if (height <= 0)
            {
                height = bitmap.Height;
            }
            System.Drawing.Bitmap source = bitmap;
            try
            {
                if (bitmap.Width != width || bitmap.Height != height || bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    source = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using var graphics = System.Drawing.Graphics.FromImage(source);
                    graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    graphics.DrawImage(bitmap, new System.Drawing.Rectangle(0, 0, width, height));
                }
                var pixels = new byte[width * height * 4];
                var bitmapData = source.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                try
                {
                    var rowBytes = width * 4;
                    for (var y = 0; y < height; y++)
                    {
                        Marshal.Copy(bitmapData.Scan0 + y * bitmapData.Stride, pixels, y * rowBytes, rowBytes);
                    }
                }
                finally
                {
                    source.UnlockBits(bitmapData);
                }
                return CreateFromPixels(device, pixels, width, height, pa, keepCpuData);
            }
            finally
            {
                if (source != bitmap)
                {
                    source.Dispose();
                }
            }
        }

        private static TextureBase CreateFromPixels(PPDDevice device, byte[] pixels, int width, int height, bool pa, bool keepCpuData)
        {
            var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                var description = new Texture2DDescription
                {
                    Width = (uint)width,
                    Height = (uint)height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = TextureFormat,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.ShaderResource,
                    CPUAccessFlags = CpuAccessFlags.None,
                    MiscFlags = ResourceOptionFlags.None
                };
                var texture = ((ID3D11Device)device.Device).CreateTexture2D(description, new SubresourceData(handle.AddrOfPinnedObject(), (uint)width * 4, 0));
                return new Texture(device, texture, pa, false, keepCpuData ? pixels : null);
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
