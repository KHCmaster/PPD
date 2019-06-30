using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using PPDFramework;

namespace testgame
{
    class Reflection
    {
        Device device;
        public Reflection(Device device)
        {
            this.device = device;
        }
        public void Update()
        {

        }
        public void Draw(PictureObject po)
        {
            float zure = 0 * po.Scale.X;
            float press = 0.8f;
            float alpha = 0.4f;
            int w = 1;
            float z = 0.5f;
            Vector4[] positions = new Vector4[4];
            positions[0] = new Vector4(po.Position.X - po.Width / 2, po.Position.Y - po.Height / 2, z, w);
            positions[1] = new Vector4(po.Position.X + po.rec.Width - po.Width / 2, po.Position.Y - po.Height / 2, z, w);
            positions[2] = new Vector4(po.Position.X - po.Width / 2, po.Position.Y + po.rec.Height - po.Height / 2, z, w);
            positions[3] = new Vector4(po.Position.X + po.rec.Width - po.Width / 2, po.Position.Y + po.rec.Height - po.Height / 2, z, w);
            Matrix m = Matrix.Transformation2D(new Vector2(po.Position.X, po.Position.Y), 0f, po.Scale, new Vector2(po.Position.X, po.Position.Y), po.Rotation, new Vector2(0, 0));
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = Vector4.Transform(positions[i], m);
            }
            var cv = new TransformedColoredTexturedVertex[6]{
        // 左上の頂点
        new TransformedColoredTexturedVertex()
        {
          Position = new Vector4( positions[0].X - zure, (positions[2].Y-positions[0].Y)*press + positions[2].Y, z, w ),
          Color = new Color4(0,1,1,1).ToArgb(),
          TextureCoordinates = new Vector2() { X = po.rec.X/po.Width, Y = po.rec.Y/po.Height  },
        },
        // 右上の頂点
        new TransformedColoredTexturedVertex()
        {
          Position = new Vector4( positions[1].X + zure, (positions[2].Y-positions[0].Y)*press + positions[2].Y, z, w ),
          Color = new Color4(0,1,1,1).ToArgb(),
          TextureCoordinates = new Vector2() { X = (po.rec.X+po.rec.Width)/po.Width, Y = po.rec.Y/po.Height },
        },
        // 左真ん中の頂点
        new TransformedColoredTexturedVertex()
        {
          Position = new Vector4( positions[0].X - zure/2, (positions[2].Y-positions[0].Y)*press/2 + positions[2].Y, z, w ),
          Color = new Color4(0,1,1,1).ToArgb(),
          TextureCoordinates = new Vector2() { X = po.rec.X/po.Width, Y = (po.rec.Y+po.rec.Height)/po.Height/2 },
        },
        // 右真ん中の頂点
        new TransformedColoredTexturedVertex()
        {
          Position = new Vector4( positions[1].X + zure/2, (positions[2].Y-positions[0].Y)*press/2 + positions[2].Y, z, w ),
          Color = new Color4(0,1,1,1).ToArgb(),
          TextureCoordinates = new Vector2() { X = (po.rec.X+po.rec.Width)/po.Width, Y = (po.rec.Y+po.rec.Height)/po.Height/2 },
        },
        // 左下の頂点
        new TransformedColoredTexturedVertex()
        {
          Position = new Vector4( positions[2].X, positions[2].Y, z, w ),
          Color = new Color4(alpha,1,1,1).ToArgb(),
          TextureCoordinates = new Vector2() { X = po.rec.X/po.Width, Y = (po.rec.Y+po.rec.Height)/po.Height },
        },
        // 右下の頂点
        new TransformedColoredTexturedVertex()
        {
          Position = new Vector4( positions[3].X, positions[3].Y, z, w ),
          Color = new Color4(alpha,1,1,1).ToArgb(),
          TextureCoordinates = new Vector2() { X = (po.rec.X+po.rec.Width)/po.Width, Y = (po.rec.Y+po.rec.Height)/po.Height },
        }
                };
            Texture t = po.Texture;
            if (t != null)
            {
                this.device.SetTexture(0, t);
                this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 4, cv);
            }
        }
    }
}
