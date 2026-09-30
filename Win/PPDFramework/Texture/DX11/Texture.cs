using System.Numerics;
using PPDMath;
using Vortice.Direct3D11;
using Vortice.Mathematics;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace PPDFramework.Texture.DX11
{
    /// <summary>
    /// DirectX11のテクスチャです。
    /// CPUから書き込まれたデータはCPU側に保持し、描画スレッドで使われるときにGPUへ転送します。
    /// </summary>
    class Texture : TextureBase
    {
        const int BytesPerPixel = 4;

        PPDDevice device;
        bool pa;
        byte[] cpuData;
        readonly object cpuDataLock = new object();
        int dirtyTop;
        int dirtyBottom;

        public override bool PA
        {
            get { return pa; }
        }

        public override SurfaceBase Surface
        {
            get;
            protected set;
        }

        public int Width
        {
            get;
            private set;
        }

        public int Height
        {
            get;
            private set;
        }

        private int Pitch
        {
            get { return Width * BytesPerPixel; }
        }

        public ID3D11Texture2D _Texture
        {
            get;
            private set;
        }

        public ID3D11ShaderResourceView _ShaderResourceView
        {
            get;
            private set;
        }

        public ID3D11RenderTargetView _RenderTargetView
        {
            get;
            private set;
        }

        public Texture(PPDDevice device, ID3D11Texture2D texture, bool pa, bool isRenderTarget, byte[] cpuData)
        {
            this.device = device;
            this.pa = pa;
            this.cpuData = cpuData;
            _Texture = texture;
            Width = (int)texture.Description.Width;
            Height = (int)texture.Description.Height;
            var d3dDevice = (ID3D11Device)device.Device;
            _ShaderResourceView = d3dDevice.CreateShaderResourceView(texture);
            Surface = new Surface();
            if (isRenderTarget)
            {
                _RenderTargetView = d3dDevice.CreateRenderTargetView(texture);
            }
        }

        protected override void DisposeResource()
        {
            if (_ShaderResourceView != null)
            {
                _ShaderResourceView.Dispose();
                _ShaderResourceView = null;
            }
            if (_RenderTargetView != null)
            {
                _RenderTargetView.Dispose();
                _RenderTargetView = null;
            }
            if (_Texture != null)
            {
                _Texture.Dispose();
                _Texture = null;
            }
            if (Surface != null)
            {
                Surface.Dispose();
                Surface = null;
            }
            cpuData = null;
            base.DisposeResource();
        }

        public override void Write(byte[] bytes)
        {
            lock (cpuDataLock)
            {
                EnsureCpuData();
                System.Buffer.BlockCopy(bytes, 0, cpuData, 0, Math.Min(bytes.Length, cpuData.Length));
                MarkDirty(0, Height);
            }
        }

        public override void Write(IntPtr ptr, int length)
        {
            lock (cpuDataLock)
            {
                EnsureCpuData();
                Marshal.Copy(ptr, cpuData, 0, Math.Min(length, cpuData.Length));
                MarkDirty(0, Height);
            }
        }

        public override Stream ToStream()
        {
            var stream = new MemoryStream();
            using (var bitmap = ToBitmap())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            }
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public override TextureDataBase GetTextureData()
        {
            return new TextureData(this);
        }

        public override void Save(string filename)
        {
            using (var bitmap = ToBitmap())
            {
                bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        /// <summary>
        /// CPU側のデータへのアクセスを開始します。<see cref="EndCpuAccess"/>を同じスレッドで呼ぶ必要があります。
        /// </summary>
        /// <returns></returns>
        internal byte[] BeginCpuAccess()
        {
            Monitor.Enter(cpuDataLock);
            EnsureCpuData();
            return cpuData;
        }

        /// <summary>
        /// CPU側のデータへのアクセスを終了します。
        /// </summary>
        internal void EndCpuAccess()
        {
            MarkDirty(0, Height);
            Monitor.Exit(cpuDataLock);
        }

        /// <summary>
        /// 別のテクスチャの内容をCPU側でコピーします。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="sourceRect"></param>
        /// <param name="destPoint"></param>
        internal void CopyFrom(Texture src, Vortice.RawRect? sourceRect, Int2? destPoint)
        {
            var rect = sourceRect ?? new Vortice.RawRect(0, 0, src.Width, src.Height);
            var point = destPoint ?? new Int2(0, 0);
            var srcData = src.GetCpuDataCopy();
            lock (cpuDataLock)
            {
                EnsureCpuData();
                var copyWidth = Math.Min(rect.Right - rect.Left, Width - point.X);
                var copyHeight = Math.Min(rect.Bottom - rect.Top, Height - point.Y);
                if (copyWidth <= 0 || copyHeight <= 0)
                {
                    return;
                }
                for (var y = 0; y < copyHeight; y++)
                {
                    System.Buffer.BlockCopy(srcData,((rect.Top + y) * src.Width + rect.Left) * BytesPerPixel,
                        cpuData, ((point.Y + y) * Width + point.X) * BytesPerPixel,
                        copyWidth * BytesPerPixel);
                }
                MarkDirty(point.Y, point.Y + copyHeight);
            }
        }

        /// <summary>
        /// CPU側で書き込まれたデータをGPUへ転送します。描画スレッドから呼ぶ必要があります。
        /// </summary>
        /// <param name="context"></param>
        internal void Flush(ID3D11DeviceContext context)
        {
            if (dirtyBottom <= dirtyTop)
            {
                return;
            }
            lock (cpuDataLock)
            {
                if (cpuData == null || _Texture == null || dirtyBottom <= dirtyTop)
                {
                    return;
                }
                var handle = GCHandle.Alloc(cpuData, GCHandleType.Pinned);
                try
                {
                    var ptr = handle.AddrOfPinnedObject() + dirtyTop * Pitch;
                    context.UpdateSubresource(_Texture, 0, new Box(0, dirtyTop, 0, Width, dirtyBottom, 1), ptr, (uint)Pitch, 0);
                }
                finally
                {
                    handle.Free();
                }
                dirtyTop = dirtyBottom = 0;
            }
        }

        private void EnsureCpuData()
        {
            if (cpuData == null)
            {
                cpuData = ReadPixels(((PPDFramework.DX11.PPDDevice)device).Context, _Texture);
            }
        }

        private void MarkDirty(int top, int bottom)
        {
            if (dirtyBottom <= dirtyTop)
            {
                dirtyTop = top;
                dirtyBottom = bottom;
            }
            else
            {
                dirtyTop = Math.Min(dirtyTop, top);
                dirtyBottom = Math.Max(dirtyBottom, bottom);
            }
        }

        private byte[] GetCpuDataCopy()
        {
            lock (cpuDataLock)
            {
                if (cpuData != null)
                {
                    return (byte[])cpuData.Clone();
                }
            }
            return ReadPixels(((PPDFramework.DX11.PPDDevice)device).Context, _Texture);
        }

        private System.Drawing.Bitmap ToBitmap()
        {
            return ToBitmap(GetCpuDataCopy(), Width, Height);
        }

        /// <summary>
        /// GPU上のテクスチャの内容を読み出します。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="texture"></param>
        /// <returns></returns>
        internal static byte[] ReadPixels(ID3D11DeviceContext context, ID3D11Texture2D texture)
        {
            var desc = texture.Description;
            var width = (int)desc.Width;
            var height = (int)desc.Height;
            var data = new byte[width * height * BytesPerPixel];
            using (var device = texture.Device)
            using (var staging = device.CreateTexture2D(new Texture2DDescription
            {
                Width = desc.Width,
                Height = desc.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = desc.Format,
                SampleDescription = new Vortice.DXGI.SampleDescription(1, 0),
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CPUAccessFlags = CpuAccessFlags.Read,
                MiscFlags = ResourceOptionFlags.None
            }))
            {
                context.CopySubresourceRegion(staging, 0, 0, 0, 0, texture, 0, null);
                var dataBox = context.Map(staging, 0, MapMode.Read, MapFlags.None);
                try
                {
                    var rowBytes = width * BytesPerPixel;
                    for (var y = 0; y < height; y++)
                    {
                        Marshal.Copy(dataBox.DataPointer + y * (int)dataBox.RowPitch, data, y * rowBytes, rowBytes);
                    }
                }
                finally
                {
                    context.Unmap(staging, 0);
                }
            }
            return data;
        }

        /// <summary>
        /// BGRAのピクセルデータからビットマップを作成します。
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        internal static System.Drawing.Bitmap ToBitmap(byte[] pixels, int width, int height)
        {
            var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            try
            {
                var rowBytes = width * BytesPerPixel;
                for (var y = 0; y < height; y++)
                {
                    Marshal.Copy(pixels, y * rowBytes, bitmapData.Scan0 + y * bitmapData.Stride, rowBytes);
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
            return bitmap;
        }
    }
}
