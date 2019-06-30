using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;


namespace testgame
{
    class Picture : IDisposable
    {
        //size
        int _width;
        int _height;
        //Texture
        Texture tex;
        //initialized
        bool initok = false;
        //Disposed
        public bool disposed = false;

        public Picture(string filename, Device device)
        {
            try
            {
                ImageInformation ii = ImageInformation.FromFile(filename);
                this._width = ii.Width;
                this._height = ii.Height;
                this.tex = Texture.FromFile(device, filename, this._width, this._height, 0, Usage.None, Format.A8R8G8B8, Pool.Managed, Filter.Linear, Filter.Linear, 0);
                
                initok = true;
            }
            catch (Exception e)
            {
                MessageBox.Show("画像ファイル\n" + filename + "の取得に失敗しました\n" + e.Message);
            }
        }
        public Picture(byte[] data,int width,int height,Device device)
        {
            this._width = width;
            this._height = height;
            try
            {
                this.tex = Texture.FromMemory(device, data, width, height, 0, Usage.None, Format.R8G8B8, Pool.Managed, Filter.Linear, Filter.Linear, 0);
                initok = true;
            }
            catch (Exception e)
            {
                MessageBox.Show("画像データが読み込めませんでした\n" + e.Message);
            }
        }
        public void changetexture(byte[] data)
        {
            Device device = this.tex.Device;
            this.tex.Dispose();
            this.tex = Texture.FromMemory(device, data, this._width, this._height, 0, Usage.None, Format.R8G8B8, Pool.Managed, Filter.Linear, Filter.Linear, 0);
        }
        public int width
        {
            get
            {
                return this._width;
            }
        }
        public int height
        {
            get
            {
                return this._height;
            }
        }
        public Texture texture
        {
            get
            {
                return this.tex;
            }
        }
        public bool initialized
        {
            get
            {
                return this.initok;
            }
        }
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
                    if (tex != null)
                    {
                        tex.Dispose();
                        tex = null;
                    }
                }
            }
            disposed = true;
        }
    }
}
