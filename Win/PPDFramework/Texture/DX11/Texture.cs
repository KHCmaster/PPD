using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.IO;

namespace PPDFramework.Texture.DX11
{
    class Texture : TextureBase
    {
        PPDDevice device;
        bool pa;

        public override bool PA
        {
            get { return pa; }
        }

        public override SurfaceBase Surface
        {
            get;
            protected set;
        }

        public SharpDX.Direct3D11.Texture2D _Texture
        {
            get;
            private set;
        }

        public SharpDX.Direct3D11.ShaderResourceView _ShaderResourceView
        {
            get;
            private set;
        }

        public SharpDX.Direct3D11.RenderTargetView _RenderTargetView
        {
            get;
            private set;
        }

        public Texture(PPDDevice device, SharpDX.Direct3D11.Texture2D texture, bool pa, bool isRenderTarget)
        {
            this.device = device;
            this.pa = pa;
            _Texture = texture;
            _ShaderResourceView = new ShaderResourceView((SharpDX.Direct3D11.Device)((PPDFramework.DX11.PPDDevice)device).Device, texture);
            Surface = new Surface();
            if (isRenderTarget)
            {
                _RenderTargetView = new RenderTargetView((SharpDX.Direct3D11.Device)((PPDFramework.DX11.PPDDevice)device).Device, texture);
            }
        }

        protected override void DisposeResource()
        {
            if (_Texture != null)
            {
                _Texture.Dispose();
                _Texture = null;
            }
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
            base.DisposeResource();
        }

        public override void Write(byte[] bytes)
        {
            var databox = ((PPDFramework.DX11.PPDDevice)device).Context.MapSubresource(_Texture, 0, 0, MapMode.WriteDiscard, MapFlags.None, out DataStream dataStream);
            dataStream.Position = 0;
            dataStream.WriteRange(bytes, 0, bytes.Length);
            dataStream.Close();
            dataStream.Dispose();
            ((PPDFramework.DX11.PPDDevice)device).Context.UnmapSubresource(_Texture, 0);
        }

        public override void Write(IntPtr ptr, int length)
        {
            var databox = ((PPDFramework.DX11.PPDDevice)device).Context.MapSubresource(_Texture, 0, 0, MapMode.WriteDiscard, MapFlags.None, out DataStream dataStream);
            dataStream.Position = 0;
            dataStream.WriteRange(ptr, length);
            dataStream.Close();
            dataStream.Dispose();
            ((PPDFramework.DX11.PPDDevice)device).Context.UnmapSubresource(_Texture, 0);
        }

        public override Stream ToStream()
        {
            var stream = new MemoryStream();
            /* TODO
            Texture2D.ToStream(((PPDFramework.DX11.PPDDevice)device).Context, _Texture, ImageFileFormat.Bmp, stream);
            */
            return stream;
        }

        public override TextureDataBase GetTextureData()
        {
            return new TextureData(_Texture);
        }

        public override void Save(string filename)
        {
            // TODO
        }
    }
}
