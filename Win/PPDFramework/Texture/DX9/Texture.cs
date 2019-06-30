using SharpDX.Direct3D9;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PPDFramework.Texture.DX9
{
    /// <summary>
    /// テクスチャです。
    /// </summary>
    public class Texture : TextureBase
    {
        bool pa;

        /// <summary>
        /// PremultipliedAlphaかどうかを取得します。
        /// </summary>
        public override bool PA
        {
            get { return pa; }
        }

        /// <summary>
        /// サーフェースを取得します。
        /// </summary>
        public override SurfaceBase Surface
        {
            get;
            protected set;
        }

        /// <summary>
        /// テクスチャを取得します。
        /// </summary>
        public SharpDX.Direct3D9.Texture _Texture
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="pa"></param>
        public Texture(SharpDX.Direct3D9.Texture texture, bool pa)
        {
            this.pa = pa;
            _Texture = texture;
            Surface = new Surface(_Texture.GetSurfaceLevel(0));
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            if (Surface != null)
            {
                Surface.Dispose();
                Surface = null;
            }
            if (_Texture != null)
            {
                _Texture.Dispose();
                _Texture = null;
            }
            base.DisposeResource();
        }

        /// <summary>
        /// 書き込みます。
        /// </summary>
        /// <param name="bytes"></param>
        public override void Write(byte[] bytes)
        {
            var rec = _Texture.LockRectangle(0, LockFlags.None);
            Marshal.Copy(bytes, 0, rec.DataPointer, bytes.Length);
            _Texture.UnlockRectangle(0);
        }

        /// <summary>
        /// 書き込みます。
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="length"></param>
        public override void Write(IntPtr ptr, int length)
        {
            var rec = _Texture.LockRectangle(0, LockFlags.None);
            SharpDX.Utilities.CopyMemory(rec.DataPointer, ptr, length);
            _Texture.UnlockRectangle(0);
        }

        /// <summary>
        /// ストリームにします。
        /// </summary>
        /// <returns></returns>
        public override Stream ToStream()
        {
            return SharpDX.Direct3D9.Texture.ToStream(_Texture, ImageFileFormat.Bmp);
        }

        /// <summary>
        /// テクスチャのデータを取得します。
        /// </summary>
        /// <returns></returns>
        public override TextureDataBase GetTextureData()
        {
            return new TextureData(_Texture);
        }

        /// <summary>
        /// 保存します。
        /// </summary>
        /// <param name="filename"></param>
        public override void Save(string filename)
        {
            SharpDX.Direct3D9.Texture.ToFile(_Texture, filename, ImageFileFormat.Png);
        }
    }
}
