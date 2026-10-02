using PPDFramework.Texture;
using System.Numerics;
using PPDMath;
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
        // テクスチャを別のスレッドで作るので、できたかどうかと破棄の状態はロックで守る
        readonly System.Threading.Lock stateLock = new();
        volatile bool ready;
        volatile bool stateDisposed;
        PPDDevice device;
        Direct2DImageEncoder encoder;
        SizeTextureManager sizeTextureManager;

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

        /// <summary>
        /// テクスチャができているかどうかを取得します。大きさ（Width、Height）は、できる前から正しい値になっています。
        /// </summary>
        public bool Ready
        {
            get
            {
                return ready;
            }
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            lock (stateLock)
            {
                stateDisposed = true;
                if (PPDSetting.Setting.CharacterTexturePackingDisabled)
                {
                    Texture?.Dispose();
                    Texture = null;
                }
                else
                {
                    availableSpace?.Dispose();
                }
            }
        }

        internal CharCacheInfo(float fontsize, string facename, char c)
        {
            FontSize = fontsize;
            FaceName = facename;
            Character = c;
        }

        /// <summary>
        /// テクスチャを作ります。worker を渡したときは、大きさだけをすぐに求め、描く処理は worker のスレッドで行います。
        /// </summary>
        internal void CreateTexture(PPDDevice device, Direct2DImageEncoder encoder, SizeTextureManager sizeTextureManager, CharTextureWorker worker)
        {
            using var profile = FrameProfiler.Measure(FrameSection.CharTexture);
            fontScale = PPDSetting.Setting.FontScaleDisabled ? 1 : Math.Max(1, device.Scale.X);
            ActualFontSize = FontSize * fontScale;
            if (worker != null && PPDSetting.Setting.TextureCharMode == TextureCharMode.D2D && d2dFailedCount <= 10)
            {
                try
                {
                    // 幅と高さは、描いたときと同じ DirectWrite の計測で求める（文字列の配置は、テクスチャができる前から正しい）
                    encoder.Measure(new string(Character, 1), FaceName, ActualFontSize, out int measuredWidth, out int measuredHeight);
                    SetSize(measuredWidth, measuredHeight);
                    this.device = device;
                    this.encoder = encoder;
                    this.sizeTextureManager = sizeTextureManager;
                    worker.Enqueue(this);
                    return;
                }
                catch
                {
                    // 計測できなければ、その場で作る
                }
            }
            CreateTextureNow(device, encoder, sizeTextureManager);
        }

        /// <summary>
        /// worker のスレッドから呼ばれ、テクスチャを作ります。
        /// </summary>
        internal void CreateTextureOnWorker()
        {
            if (stateDisposed)
            {
                return;
            }
            var pixels = RenderByD2D(encoder, out int width, out int height);
            lock (stateLock)
            {
                if (stateDisposed)
                {
                    return;
                }
                if (pixels != null)
                {
                    d2dFailedCount = 0;
                    PublishPixels(device, sizeTextureManager, pixels, width, height);
                }
                else
                {
                    d2dFailedCount++;
                    PublishTexture(sizeTextureManager, CreateTextureByWinAPI(device));
                }
            }
            device = null;
            encoder = null;
            sizeTextureManager = null;
        }

        private void CreateTextureNow(PPDDevice device, Direct2DImageEncoder encoder, SizeTextureManager sizeTextureManager)
        {
            lock (stateLock)
            {
                switch (PPDSetting.Setting.TextureCharMode)
                {
                    case TextureCharMode.D2D:
                        if (d2dFailedCount > 10)
                        {
                            goto case TextureCharMode.WinAPI;
                        }
                        var pixels = RenderByD2D(encoder, out int width, out int height);
                        if (pixels == null)
                        {
                            d2dFailedCount++;
                            goto case TextureCharMode.WinAPI;
                        }
                        d2dFailedCount = 0;
                        PublishPixels(device, sizeTextureManager, pixels, width, height);
                        break;
                    case TextureCharMode.WinAPI:
                        PublishTexture(sizeTextureManager, CreateTextureByWinAPI(device));
                        break;
                }
            }
        }

        private void SetSize(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.t_width = Utility.UpperPowerOfTwo(width);
            this.t_height = Utility.UpperPowerOfTwo(height);
        }

        /// <summary>
        /// Direct2D で描いた画素を、文字のテクスチャに書き込みます（一時的なテクスチャは作らない）。
        /// </summary>
        private void PublishPixels(PPDDevice device, SizeTextureManager sizeTextureManager, byte[] pixels, int width, int height)
        {
            SetSize(width, height);
            if (PPDSetting.Setting.CharacterTexturePackingDisabled)
            {
                PublishTexture(sizeTextureManager, PPDFramework.Texture.DX11.TextureFactory.CreateFromPixels(device, pixels, width, height, true, true));
                return;
            }
            var sizeTexture = sizeTextureManager.WritePixels(pixels, width, height, t_width, t_height, out Vector2 uv, out Vector2 uvSize, out availableSpace);
            Texture = sizeTexture.Texture;
            UV = uv;
            UVSize = uvSize;
            ActualUVSize = new Vector2(uvSize.X * width / t_width, uvSize.Y * height / t_height);
            HalfPixel = SizeTexture.HalfPixel;
            ready = true;
        }

        private void PublishTexture(SizeTextureManager sizeTextureManager, TextureBase texture)
        {
            if (texture == null)
            {
                return;
            }
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
                HalfPixel = SizeTexture.HalfPixel;
            }
            ready = true;
        }

        private byte[] RenderByD2D(Direct2DImageEncoder encoder, out int width, out int height)
        {
            for (var retryCount = 0; retryCount < 5; retryCount++)
            {
                try
                {
                    var pixels = encoder.RenderBgra(new string(Character, 1), FaceName, ActualFontSize, out width, out height);
                    Utility.PreMultiplyAlpha(pixels);
                    return pixels;
                }
                catch
                {
                }
            }
            width = height = 0;
            return null;
        }

        private TextureBase CreateTextureByWinAPI(PPDDevice device)
        {
            var metrics = new NativeMethods.GLYPHMETRICS();
            var tmetrics = new NativeMethods.TEXTMETRIC();
            var matrix = new NativeMethods.MAT2();
            TextureBase texture = null;
            matrix.eM11.value = 1;
            matrix.eM12.value = 0;
            matrix.eM21.value = 0;
            matrix.eM22.value = 1;
            using (var font = new System.Drawing.Font(FaceName, ActualFontSize))
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                var hdc = g.GetHdc();
                var prev = NativeMethods.SelectObject(hdc, font.ToHfont());
                var bufferSize = (int)NativeMethods.GetGlyphOutlineW(hdc, Character, (uint)6, out metrics, 0, IntPtr.Zero, ref matrix);
                var buffer = Marshal.AllocHGlobal(bufferSize);
                try
                {
                    uint ret;
                    if ((ret = NativeMethods.GetGlyphOutlineW(hdc, Character, (uint)6, out metrics, (uint)bufferSize, buffer, ref matrix)) > 0 && NativeMethods.GetTextMetrics(hdc, ref tmetrics))
                    {
                        int iBmp_w = metrics.gmBlackBoxX + (4 - (metrics.gmBlackBoxX % 4)) % 4;
                        int iBmp_h = metrics.gmBlackBoxY;
                        int iOfs_x = metrics.gmptGlyphOrigin.x, iOfs_y = tmetrics.ascent - metrics.gmptGlyphOrigin.y;
                        width = metrics.gmCellIncX;
                        height = tmetrics.height;
                        t_width = Utility.UpperPowerOfTwo(Math.Max(width, iBmp_w + iOfs_x));
                        t_height = Utility.UpperPowerOfTwo(height);
                        texture = CreateTemporaryTexture(device, t_width, t_height);

                        using var textureData = texture.GetTextureData();
                        var rec = textureData.DataStream;
                        try
                        {
                            byte[] data = new byte[bufferSize];
                            Marshal.Copy(buffer, data, 0, bufferSize);
                            int offset1 = (iOfs_x + iOfs_y * t_width) * 4;
                            int offset2 = (iOfs_x + t_width - iOfs_x - iBmp_w) * 4;
                            NativeMethods.ZeroMemory(rec.DataPointer, (uint)rec.Length);
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
                finally
                {
                    NativeMethods.SelectObject(hdc, prev);
                    _ = NativeMethods.DeleteObject(prev);
                    g.ReleaseHdc(hdc);
                    Marshal.FreeHGlobal(buffer);
                }
            }
            return texture;
        }

        private static TextureBase CreateTemporaryTexture(PPDDevice device, int width, int height)
        {
            return TextureFactoryManager.Factory.Create(device, width, height, 1, true);
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

        public Vortice.Mathematics.Rect GetActualUVRectangle(float left, float top, float right, float bottom)
        {
            var topLeft = GetActualUV(new Vector2(left, top));
            var bottomRight = GetActualUV(new Vector2(right, bottom));
            var halfPixel = HalfPixel;
            return new Vortice.Mathematics.Rect(topLeft.X + halfPixel.X, topLeft.Y + halfPixel.Y,
                bottomRight.X - topLeft.X - halfPixel.X,
                bottomRight.Y - topLeft.Y - halfPixel.Y);

        }
    }
}
