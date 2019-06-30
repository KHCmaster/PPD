using System;
using System.IO;

namespace PPDFramework.Texture
{
    /// <summary>
    /// テクスチャのインターフェースです。
    /// </summary>
    public abstract class TextureBase : DisposableComponent
    {
        /// <summary>
        /// PremultipliedAlphaかどうかを取得します。
        /// </summary>
        public abstract bool PA { get; }

        /// <summary>
        /// サーフェースを取得します。
        /// </summary>
        public abstract SurfaceBase Surface { get; protected set; }

        /// <summary>
        /// 書き込みます。
        /// </summary>
        /// <param name="bytes"></param>
        public abstract void Write(byte[] bytes);

        /// <summary>
        /// 書き込みます。
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="length"></param>
        public abstract void Write(IntPtr ptr, int length);

        /// <summary>
        /// ストリームにします。
        /// </summary>
        /// <returns></returns>
        public abstract Stream ToStream();

        /// <summary>
        /// テクスチャのデータを取得します。
        /// </summary>
        /// <returns></returns>
        public abstract TextureDataBase GetTextureData();

        /// <summary>
        /// 保存します。
        /// </summary>
        /// <param name="filename"></param>
        public abstract void Save(string filename);
    }
}
