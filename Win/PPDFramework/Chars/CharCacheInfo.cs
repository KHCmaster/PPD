using PPDFramework.Texture;
using SharpDX;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace PPDFramework.Chars
{
    class CharCacheInfo : DisposableComponent
    {
        static int d2dFailedCount;

        int count;
        int width;
        int height;
        int t_width;
        int t_height;
        AvailableSpace availableSpace;
        float fontScale;

        public float FontSize
        {
            get;
            private set;
        }

        public float ActualFontSize
        {
            get;
            private set;
        }

        public string FaceName
        {
            get;
            private set;
        }

        public Char Character
        {
            get;
            private set;
        }

        public TextureBase Texture
        {
            get;
            private set;
        }

        public float Width
        {
            get
            {
                return width / fontScale;
            }
        }

        public float Height
        {
            get
            {
                return height / fontScale;
            }
        }

        public Vector2 UV
        {
            get;
            private set;
        }

        public Vector2 UVSize
        {
            get;
            private set;
        }

        public Vector2 ActualUVSize
        {
            get;
            private set;
        }

        public Vector2 HalfPixel
        {
            get;
            private set;
        }

        internal int Count
        {
            get
            {
                return count;
            }
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (PPDSetting.Setting.CharacterTexturePackingDisabled)
            {
                if (Texture != null)
                {
                    Texture.Dispose();
                    Texture = null;
                }
            }
            else
            {
                if (availableSpace != null)
                {
                    availableSpace.Dispose();
                }
            }
        }

        internal CharCacheInfo(float fontsize, string facename, char c)
        {
            FontSize = fontsize;
            FaceName = facename;
            Character = c;
        }

        internal void CreateTexture(PPDDevice device, Direct2DImageEncoder encoder, SizeTextureManager sizeTextureManager)
        {
            fontScale = PPDSetting.Setting.FontScaleDisabled ? 1 : Math.Max(1, device.Scale.X);
            ActualFontSize = FontSize * fontScale;
            TextureBase texture = null;
            switch (PPDSetting.Setting.TextureCharMode)
            {
                case TextureCharMode.D2D:
                    if (d2dFailedCount > 10)
                    {
                        goto case TextureCharMode.WinAPI;
                    }
                    texture = CreateTextureByD2D(device, encoder);
                    if (texture == null)
                    {
                        d2dFailedCount++;
                        goto case TextureCharMode.WinAPI;
                    }
                    else
                    {
                        d2dFailedCount = 0;
                    }
                    break;
                case TextureCharMode.WinAPI:
                    texture = CreateTextureByWinAPI(device);
                    break;
            }
            if (texture != null)
            {
                if (PPDSetting.Setting.CharacterTexturePackingDisabled)
                {
                    Texture = texture;
                    UV = Vector2.Zero;
                    UVSize = Vector2.One;
                    ActualUVSize = Vector2.One;
                    HalfPixel = new Vector2(0.5f / t_width, 0.5f / t_height);
                }
                else
                {
                    var sizeTexture = sizeTextureManager.Write(texture, t_width, t_height, out Vector2 uv, out Vector2 uvSize, out availableSpace);
                    texture.Dispose();
                    Texture = sizeTexture.Texture;
                    UV = uv;
                    UVSize = uvSize;
                    ActualUVSize = new Vector2(uvSize.X * width / t_width, uvSize.Y * height / t_height);
                    HalfPixel = sizeTexture.HalfPixel;
                }
            }
        }

        private TextureBase CreateTextureByD2D(PPDDevice device, Direct2DImageEncoder encoder)
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        encoder.Save(stream, Direct2DImageFormat.Png, new string(Character, 1), FaceName, ActualFontSize, out int width, out int height);
                        stream.Seek(0, SeekOrigin.Begin);
                        var bitmap = new Bitmap(stream);
                        Utility.PreMultiplyAlpha(bitmap);
                        var tempStream = new MemoryStream();
                        bitmap.Save(tempStream, System.Drawing.Imaging.ImageFormat.Png);
                        tempStream.Seek(0, SeekOrigin.Begin);
                        var texture = ((PPDFramework.Texture.DX9.TextureFactory)TextureFactoryManager.Factory).FromStream(device, tempStream, width, height, SharpDX.Direct3D9.Pool.SystemMemory, true);
                        this.width = width;
                        this.height = height;
                        this.t_width = Utility.UpperPowerOfTwo(width);
                        this.t_height = Utility.UpperPowerOfTwo(height);
                        return texture;
                    }
                }
                catch
                {
                    retryCount++;
                    if (retryCount >= 5)
                    {
                        break;
                    }
                }
            }
            return null;
        }

        private TextureBase CreateTextureByWinAPI(PPDDevice device)
        {
            var metrics = new WinAPI.GLYPHMETRICS();
            var tmetrics = new WinAPI.TEXTMETRIC();
            var matrix = new WinAPI.MAT2();
            TextureBase texture = null;
            matrix.eM11.value = 1;
            matrix.eM12.value = 0;
            matrix.eM21.value = 0;
            matrix.eM22.value = 1;
            using (var font = new System.Drawing.Font(FaceName, ActualFontSize))
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                var hdc = g.GetHdc();
                var prev = WinAPI.SelectObject(hdc, font.ToHfont());
                var bufferSize = (int)WinAPI.GetGlyphOutlineW(hdc, Character, (uint)6, out metrics, 0, IntPtr.Zero, ref matrix);
                var buffer = Marshal.AllocHGlobal(bufferSize);
                try
                {
                    uint ret;
                    if ((ret = WinAPI.GetGlyphOutlineW(hdc, Character, (uint)6, out metrics, (uint)bufferSize, buffer, ref matrix)) > 0 && WinAPI.GetTextMetrics(hdc, ref tmetrics))
                    {
                        int iBmp_w = metrics.gmBlackBoxX + (4 - (metrics.gmBlackBoxX % 4)) % 4;
                        int iBmp_h = metrics.gmBlackBoxY;
                        int iOfs_x = metrics.gmptGlyphOrigin.x, iOfs_y = tmetrics.ascent - metrics.gmptGlyphOrigin.y;
                        width = metrics.gmCellIncX;
                        height = tmetrics.height;
                        t_width = Utility.UpperPowerOfTwo(Math.Max(width, iBmp_w + iOfs_x));
                        t_height = Utility.UpperPowerOfTwo(height);
                        texture = ((PPDFramework.Texture.DX9.TextureFactory)TextureFactoryManager.Factory).Create(device, t_width, t_height, 1, SharpDX.Direct3D9.Pool.SystemMemory, true);

                        using (var textureData = texture.GetTextureData())
                        {
                            var rec = textureData.DataStream;
                            try
                            {
                                byte[] data = new byte[bufferSize];
                                Marshal.Copy(buffer, data, 0, bufferSize);
                                int offset1 = (iOfs_x + iOfs_y * t_width) * 4;
                                int offset2 = (iOfs_x + t_width - iOfs_x - iBmp_w) * 4;
                                WinAPI.ZeroMemory(rec.DataPointer, (uint)rec.Length);
                                var writedata = new byte[] { (byte)255, (byte)255, (byte)255, (byte)255 };
                                for (int i = 0; i < iBmp_h; i++)
                                {
                                    for (int j = 0; j < iBmp_w; j++)
                                    {
                                        var alpha = (byte)((255 * data[j + iBmp_w * i]) / 64);
                                        writedata[0] = writedata[1] = writedata[2] = alpha;
                                        writedata[3] = (byte)alpha;
                                        if (j == 0)
                                        {
                                            if (i == 0)
                                            {
                                                rec.Seek(offset1, System.IO.SeekOrigin.Current);
                                            }
                                            else
                                            {
                                                rec.Seek(offset2, System.IO.SeekOrigin.Current);
                                            }
                                        }
                                        rec.Write(writedata, 0, writedata.Length);
                                    }
                                }
                            }
                            catch
                            {
                                Console.WriteLine("CharTextureError");
                            }
                        }
                    }
                }
                finally
                {
                    WinAPI.SelectObject(hdc, prev);
                    WinAPI.DeleteObject(prev);
                    g.ReleaseHdc(hdc);
                    Marshal.FreeHGlobal(buffer);
                }
            }
            return texture;
        }

        public void Increment()
        {
            count++;
        }

        public void Decrement()
        {
            count--;
        }

        public Vector2 GetActualUV(Vector2 uv)
        {
            return UV + ActualUVSize * uv;
        }

        public SharpDX.RectangleF GetActualUVRectangle(float left, float top, float right, float bottom)
        {
            var topLeft = GetActualUV(new Vector2(left, top));
            var bottomRight = GetActualUV(new Vector2(right, bottom));
            var halfPixel = HalfPixel;
            return new SharpDX.RectangleF(topLeft.X + halfPixel.X, topLeft.Y + halfPixel.Y,
                bottomRight.X - topLeft.X - halfPixel.X,
                bottomRight.Y - topLeft.Y - halfPixel.Y);

        }
    }
}
