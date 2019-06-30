using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;
using PPDFramework;
using SlimDX;

namespace PPD
{
    class ConnectionDrawer : GameComponent
    {
        public const int MaxWidth = 800;
        const int splitNumber = 100;
        const int width = 32;
        const int diff = 1;
        const int thres = 2;
        Texture t;
        List<ColoredTexturedVertex[]> connects;
        Random r;
        Device device;
        ColoredTexturedVertex[] copy;
        int iter = 0;
        public ConnectionDrawer(Device device, int connectionNum)
        {
            this.device = device;
            t = Texture.FromFile(device, "img\\default\\connectpic.png");
            r = new Random();
            connects = new List<ColoredTexturedVertex[]>(connectionNum);
            copy = new ColoredTexturedVertex[splitNumber];
            for (int i = 0; i < connectionNum; i++)
            {
                connects.Add(new ColoredTexturedVertex[splitNumber]);
                for (int j = 0; j < splitNumber; j++)
                {
                    float alpha = 1f - Math.Abs(j - splitNumber / 2) / ((float)splitNumber / 2);
                    int color = new Color4(alpha, 1, 1, 1).ToArgb();
                    int x = -400 + 8 * j;
                    int zure = r.Next(-diff, diff + 1);
                    connects[i][j] = new ColoredTexturedVertex(
                         new Vector3(x, -width / 2 + zure, 0.5f),
                         color,
                           new Vector2(0, 0)
                       );
                    j++;
                    connects[i][j] = new ColoredTexturedVertex(
                             new Vector3(x, width / 2 + zure, 0.5f),
                             color,
                             new Vector2(0, 1)
                      );
                }
            }
            Matrix = Matrix.Transformation2D(Vector2.Zero, 0, new Vector2(1, 1), Vector2.Zero, 0, new Vector2(400, 225));
        }

        public override void Update()
        {
            iter++;
            if (iter % 3 != 0) return;
            foreach (ColoredTexturedVertex[] vertexs in connects)
            {
                int zure = r.Next(-diff, diff + 1);
                float val = Convert((vertexs[splitNumber - 2].Position.Y + vertexs[splitNumber - 1].Position.Y) / 2 + zure);
                vertexs[0].Position = new Vector3(vertexs[0].Position.X, val - width / 2, 0.5f);
                vertexs[1].Position = new Vector3(vertexs[1].Position.X, val + width / 2, 0.5f);
                for (int i = 2; i < splitNumber; i++)
                {
                    zure = r.Next(-diff, diff + 1);
                    vertexs[i].Position = new Vector3(vertexs[i].Position.X, vertexs[i - 2].Position.Y + zure, 0.5f);
                    i++;
                    vertexs[i].Position = new Vector3(vertexs[i].Position.X, vertexs[i - 2].Position.Y + zure, 0.5f);
                }
            }
        }

        private float Convert(float val)
        {
            if (val >= thres)
            {
                return thres;
            }
            else if (val <= -thres)
            {
                return -thres;
            }
            else
            {
                return val;
            }
        }

        public override void Draw()
        {
            device.SetTexture(0, t);
            device.VertexFormat = ColoredTexturedVertex.Format;
            foreach (ColoredTexturedVertex[] vertexs in connects)
            {
                Array.Copy(vertexs, copy, copy.Length);
                for (int i = 0; i < copy.Length; i++)
                {
                    copy[i].Position = Vector3.TransformCoordinate(copy[i].Position, Matrix);
                }
                device.DrawUserPrimitives(PrimitiveType.TriangleStrip, splitNumber - 2, copy);
            }
        }

        protected override void DisposeResource()
        {
            t.Dispose();
        }

        public Matrix Matrix
        {
            get;
            set;
        }
    }
}
