using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;


namespace PPDFramework
{
    /// <summary>
    /// 画像リソースクラス
    /// </summary>
    public class ImageResource : IDisposable
    {
        //size
        int _width;
        int _height;
        //Texture
        Texture _texture;
        /// <summary>
        /// 処分されたか
        /// </summary>
        public bool disposed = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="filename">ファイルパス</param>
        /// <param name="device">デバイス</param>
        public ImageResource(string filename, Device device)
        {
            try
            {
                ImageInformation ii = ImageInformation.FromFile(filename);
                this._width = ii.Width;
                this._height = ii.Height;
                this._texture = Texture.FromFile(device, filename, this._width, this._height, 0, Usage.None, Format.A8R8G8B8, Pool.Managed, Filter.Linear, Filter.Linear, 0);
            }
            catch
            {
                MessageBox.Show(PPDExceptionContentProvider.Provider.GetContent(PPDExceptionType.ImageReadError) + System.Environment.NewLine + filename);
            }
        }

        /// <summary>
        /// 幅
        /// </summary>
        public int Width
        {
            get
            {
                return this._width;
            }
        }

        /// <summary>
        /// 高さ
        /// </summary>
        public int Height
        {
            get
            {
                return this._height;
            }
        }

        /// <summary>
        /// テクスチャ
        /// </summary>
        public Texture Texture
        {
            get
            {
                return this._texture;
            }
        }

        /// <summary>
        /// 処分する
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (_texture != null)
                    {
                        _texture.Dispose();
                        _texture = null;
                    }
                }
            }
            disposed = true;
        }
    }
}
