using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;


namespace testgame
{
    class Border
    {
        public float thickness = 1;
        public Color4 color = new Color4(1, 0, 0, 0);
    }
    class DxString : IDisposable
    {
        float x;
        float y;
        float z;

        float maxwidth;
        bool allowscroll = false;
        bool scrollmode = false;
        int scrollcount = 0;
        const int scrollspeed = 5;
        int speedcount = 0;
        int wait = 600;
        bool disposed;
        Color4 color;
        int height;
        string text;
        Font font;

        Device device;
        Sprite sprite;

        public DxString(string text, float x, float y, float z, int height, Color4 color, Device device, Sprite sprite)
        {
            this.x = x * 2;
            this.y = y * 2;
            this.z = z;
            this.text = text;
            this.height = height;
            this.color = color;
            this.device = device;
            this.sprite = sprite;
            font = new Font(device, FontUtl.fontsizeratio * height * 2 / FontUtl.basesize, 0, FontUtl.fontweight, 0, false, CharacterSet.ShiftJIS, Precision.Default, FontQuality.Antialiased, PitchAndFamily.Default, FontUtl.fontname);
            this.maxwidth = this.width;
        }
        public DxString(string text, float x, float y, float z, int height, int maxwidth, Color4 color, Device device, Sprite sprite)
        {
            this.x = x * 2;
            this.y = y * 2;
            this.z = z;
            this.text = text;
            this.height = height;
            this.color = color;
            this.device = device;
            this.sprite = sprite;
            this.maxwidth = maxwidth;
            scrollmode = true;
            font = new Font(device, FontUtl.fontsizeratio * height * 2 / FontUtl.basesize, 0, FontUtl.fontweight, 0, false, CharacterSet.ShiftJIS, Precision.Default, FontQuality.Antialiased, PitchAndFamily.Default, FontUtl.fontname);
            if (this.width > maxwidth)
            {
                allowscroll = true;
            }
        }
        public virtual void Update()
        {


        }
        public virtual void Draw()
        {
            Matrix m = Matrix.AffineTransformation2D(0.5f, new Vector2(0, 0), 0, new Vector2(0, 0));
            Matrix tmpm = this.sprite.Transform;
            this.sprite.Transform = m;
            if (scrollmode && width > maxwidth)
            {
                if (allowscroll)
                {
                    if (wait > 600)
                    {
                        font.DrawString(this.sprite, this.text.Substring(scrollcount), new System.Drawing.Rectangle((int)this.x, (int)this.y, (int)this.maxwidth * 2, this.height * 2), DrawTextFormat.Left, this.color);
                    }
                    else
                    {
                        font.DrawString(this.sprite, this.text, new System.Drawing.Rectangle((int)this.x, (int)this.y, (int)(this.maxwidth - this.height * 3 / 2) * 2, this.height * 2), DrawTextFormat.Left, this.color);
                        font.DrawString(this.sprite, "...", new System.Drawing.Rectangle((int)this.x + (int)(this.maxwidth - this.height * 3 / 2) * 2, (int)this.y, (int)this.maxwidth * 2, this.height * 2), DrawTextFormat.Left, this.color);
                    }
                }
                else
                {
                    font.DrawString(this.sprite, this.text, new System.Drawing.Rectangle((int)this.x, (int)this.y, (int)(this.maxwidth - this.height * 3 / 2) * 2, this.height * 2), DrawTextFormat.Left, this.color);
                    font.DrawString(this.sprite, "...", new System.Drawing.Rectangle((int)this.x + (int)(this.maxwidth - this.height * 3 / 2) * 2, (int)this.y, (int)this.maxwidth * 2, this.height * 2), DrawTextFormat.Left, this.color);
                }
                if (allowscroll)
                {
                    wait++;
                    if (wait >= 600)
                    {
                        speedcount++;
                        if (speedcount > scrollspeed)
                        {
                            speedcount = 0;
                            scrollcount++;
                        }
                        if (scrollcount >= this.text.Length)
                        {
                            scrollcount = 0;
                            wait = 0;
                        }
                    }
                }
            }
            else
            {
                if (border != null)
                {
                    font.DrawString(this.sprite, this.text, (int)(this.x - border.thickness * 2), (int)this.y, border.color);
                    font.DrawString(this.sprite, this.text, (int)(this.x + border.thickness * 2), (int)this.y, border.color);
                    font.DrawString(this.sprite, this.text, (int)this.x, (int)(this.y - border.thickness * 2), border.color);
                    font.DrawString(this.sprite, this.text, (int)this.x, (int)(this.y + border.thickness * 2), border.color);
                }
                font.DrawString(this.sprite, this.text, (int)this.x, (int)this.y, this.color);
            }


            this.sprite.Transform = tmpm;
        }
        public float alpha
        {
            get
            {
                return this.color.Alpha;
            }
            set
            {
                this.color.Alpha = value;
            }
        }
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                if (scrollmode)
                {
                    if (this.width > this.maxwidth)
                    {
                        allowscroll = true;
                    }
                }
            }
        }
        public bool AllowScroll
        {
            get
            {
                return allowscroll;
            }
            set
            {
                allowscroll = value;
                scrollcount = 0;
                wait = 600;
            }
        }
        public Vector2 position
        {
            get
            {
                return new Vector2(this.x / 2, this.y / 2);
            }
            set
            {
                this.x = value.X * 2;
                this.y = value.Y * 2;
            }
        }
        public int width
        {
            get
            {
                DrawTextFormat dtf = DrawTextFormat.SingleLine;
                return font.MeasureString(sprite, text, dtf).Width / 2;
            }
        }
        public int justwidth
        {
            get
            {
                DrawTextFormat dtf = DrawTextFormat.SingleLine;
                int w = font.MeasureString(sprite, text, dtf).Width / 2;
                if (allowscroll)
                {
                    return w > maxwidth ? (int)maxwidth : width;
                }
                return w;
            }
        }
        public Border border
        {
            get;
            set;
        }
        public void changefont(int height, int width, FontWeight weight, string facename, Color4 color)
        {
            Font old = font;
            font = new Font(device, FontUtl.fontsizeratio * height * 2 / FontUtl.basesize, 0, weight, 0, false, CharacterSet.ShiftJIS, Precision.Default, FontQuality.Antialiased, PitchAndFamily.Default, facename);
            old.Dispose();
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
                    font.Dispose();
                }
            }
            disposed = true;
        }
    }
}
