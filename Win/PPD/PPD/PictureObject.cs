using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;


namespace testgame
{
    class PictureObject : IDisposable
    {
        //positon
        protected float x;
        protected float y;
        protected float z;
        //size
        protected float width;
        protected float height;
        //alpha
        protected float a = 1.0f;
        //hidden
        protected bool _hidden;
        //rotation
        protected float r = 0;
        //scaling
        protected Vector2 scale = new Vector2(1, 1);
        //rect
        public RectangleF rec;
        //filename
        private string filename;
        //resource
        protected Dictionary<string, Picture> resource;
        //Device
        Device _device;
        //Sprite
        Sprite _sprite;
        //Disposed
        bool disposed;
        //Tecture
        Texture tex;
        bool center;
        public bool special = false;
        public bool drawwithsprite = false;
        public PictureObject(string filename, float x, float y, float z, Dictionary<string, Picture> resource, Device device, Sprite sprite)
        {
            Picture p = null;
            if (resource.TryGetValue(filename, out p))
            {
                this.width = p.width;
                this.height = p.height;
                tex = p.texture;
            }
            this.x = x;
            this.y = y;
            this.z = z;
            this.filename = filename;
            this.resource = resource;
            this._device = device;
            this._sprite = sprite;
            rec = new RectangleF(0, 0, width, height);
        }
        public PictureObject(string filename, float x, float y, float z, bool center, Dictionary<string, Picture> resource, Device device, Sprite sprite)
        {
            this.center = center;
            Picture p = null;
            if (resource.TryGetValue(filename, out p))
            {
                this.width = p.width;
                this.height = p.height;
                tex = p.texture;
            }
            if (!center)
            {
                this.x = x;
                this.y = y;
            }
            else
            {
                this.x = x - p.width / 2;
                this.y = y - p.height / 2;
            }
            this.z = z;
            this.filename = filename;
            this.resource = resource;
            this._device = device;
            this._sprite = sprite;
            rec = new RectangleF(0, 0, width, height);
        }
        public PictureObject(string filename, float x, float y, float z, bool center, bool drawwithsprite, Dictionary<string, Picture> resource, Device device, Sprite sprite)
        {
            Picture p = null;
            if (resource.TryGetValue(filename, out p))
            {
                this.width = p.width;
                this.height = p.height;
                tex = p.texture;
            }
            if (!center)
            {
                this.x = x;
                this.y = y;
            }
            else
            {
                this.x = x - p.width / 2;
                this.y = y - p.height / 2;
            }
            this.z = z;
            this.filename = filename;
            this.resource = resource;
            this._device = device;
            this._sprite = sprite;
            this.drawwithsprite = drawwithsprite;
            rec = new RectangleF(0, 0, width, height);
        }
        public virtual void Update()
        {

        }
        public virtual void Draw()
        {
            if (_hidden || this.a == 0.0f) return;
            Picture p = null;
            if (tex == null)
            {
                if (!this.resource.TryGetValue(this.filename, out p)) return;
                else
                {
                    this.width = p.width;
                    this.height = p.height;
                    tex = p.texture;
                }
            }
            // 頂点の色；テクスチャを貼るポリゴンは白色にしておく
            int color = new Color4(this.a, 1.0f, 1.0f, 1.0f).ToArgb();
            int w = 1;
            var cv = new TransformedColoredTexturedVertex[4]{
        // 左上の頂点
        new TransformedColoredTexturedVertex()
        {
          Position = new Vector4( x, y, z, w ),
          Color = color,
          TextureCoordinates = new Vector2() { X = rec.X/width, Y = rec.Y/height  },
        },
        // 右上の頂点
        new TransformedColoredTexturedVertex()
        {
          Position = new Vector4( x + rec.Width, y, z, w ),
          Color = color,
          TextureCoordinates = new Vector2() { X = (rec.X+rec.Width)/width, Y = rec.Y/height },
        },
        // 左下の頂点
        new TransformedColoredTexturedVertex()
        {
          Position = new Vector4( x, y + rec.Height, z, w ),
          Color = color,
          TextureCoordinates = new Vector2() { X = rec.X/width, Y = (rec.Y+rec.Height)/height },
        },
        // 右下の頂点
        new TransformedColoredTexturedVertex()
        {
          Position = new Vector4( x + rec.Width, y + rec.Height, z, w ),
          Color = color,
          TextureCoordinates = new Vector2() { X = (rec.X+rec.Width)/width, Y = (rec.Y+rec.Height)/height },
        }
                };
            Matrix m = Matrix.Transformation2D((!this.special ? new Vector2(this.x + width / 2f, this.y + height / 2f) : new Vector2(this.x + width / 20f, this.y + height / 20f)), 0f, scale, new Vector2(this.x + width / 2f, this.y + height / 2f), this.r, new Vector2(0, 0));
            for (int i = 0; i < cv.Length; i++)
            {
                cv[i].Position = Vector4.Transform(cv[i].Position, m);

            }
            Matrix tm = _sprite.Transform;
            //_sprite.Transform = m;

            if (drawwithsprite)
            {
                _sprite.Draw(tex, new Vector3(0, 0, 0), new Vector3(this.x, this.y, this.z), new Color4(this.a, 1.0f, 1.0f, 1.0f));
            }
            else
            {
                if (tex != null)
                {
                    this._device.SetTexture(0, tex);
                    this._device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this._device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv);
                }
            }
            //_sprite.Transform = tm;
        }
        public bool hidden
        {
            get
            {
                return this._hidden;
            }
            set
            {
                this._hidden = value;
            }
        }
        public float alpha
        {
            get
            {
                return this.a;
            }
            set
            {
                this.a = value;
            }
        }
        public float rotation
        {
            get
            {
                return this.r;
            }
            set
            {
                this.r = value;
            }
        }
        public Texture texture
        {
            get
            {
                return tex;
            }

        }
        public Vector2 scaling
        {
            get
            {
                return this.scale;
            }
            set
            {
                this.scale = value;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {

                }
            }
            disposed = true;
        }
        public Vector2 position
        {
            get
            {
                if (!center)
                {
                    return new Vector2(this.x, this.y);
                }
                else
                {
                    return new Vector2(this.x + this.width / 2, this.y + this.height / 2);
                }
            }
            set
            {
                if (!center)
                {
                    this.x = value.X;
                    this.y = value.Y;
                }
                else
                {
                    this.x = value.X - this.width / 2;
                    this.y = value.Y - this.height / 2;
                }
            }
        }
        public float Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }
        public float Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
                Picture p = null;
                if (resource.TryGetValue(filename, out p))
                {
                    this.width = p.width;
                    this.height = p.height;
                    tex = p.texture;
                }
            }
        }
    }
}
