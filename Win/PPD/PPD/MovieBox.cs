using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using System.Threading;
using PPDFramework;


namespace testgame
{
    class MovieBox : GameComponent, IMovie
    {
        public event EventHandler FadeOutFinished;
        const int fadestep = 2;
        float rotation = 0;
        Vector4[] box;
        float[] startx;
        float h = 0.5625f;
        double start = -1;
        double end = -1;
        bool waitingseek = false;
        int waitingcount = 0;
        Device device;
        IMovie movie;
        MovieFadeState laststate;
        ImageResource black;
        public MovieBox(Device device)
        {
            this.device = device;
            black = new ImageResource("img\\default\\black.png", device);
            box = new Vector4[]{new Vector4(80,80,80,1),new Vector4(-80,80,80,1),new Vector4(80,-80,80,1),
                new Vector4(80,80,-80,1),new Vector4(-80,-80,80,1),new Vector4(-80,80,-80,1),new Vector4(80,-80,-80,1),new Vector4(-80,-80,-80,1)};
            startx = new float[6];
            Random r = new Random();
            for (int i = 0; i < 6; i++)
            {
                startx[i] = (float)r.NextDouble() * 0.4375f;
            }
            if (PPDSetting.Setting.LightMode)
            {
                Hidden = true;
            }
        }
        public IMovie Movie
        {
            get { return movie; }
            set { movie = value; }
        }
        public float Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }
        public override void Update()
        {
            if (movie != null) movie.Update();
            if (!Initialized) return;
            if (waitingseek) waitingcount++;
            if (laststate == MovieFadeState.FadeOut && FadeState == MovieFadeState.None)
            {
                if (FadeOutFinished != null) FadeOutFinished.Invoke(this, EventArgs.Empty);
            }
            if (movie == null) return;
            laststate = FadeState;
            if (CheckLoopAvailable) CheckLoop();
        }
        public void Turn()
        {
            this.rotation += (float)Math.PI / 360;
        }
        public void UnTurn()
        {
            this.rotation -= (float)Math.PI / 360;
        }
        public void ChangeCut()
        {
            float asp = (float)this.Height / this.Width;
            int top = movie.TrimmingData.Top, left = movie.TrimmingData.Left, right = movie.TrimmingData.Right, bottom = movie.TrimmingData.Bottom;
            h = (1.0f - (float)(top > 0 ? top : 0) / this.Height - (float)(bottom > 0 ? bottom : 0) / this.Height) * movie.MaxV * asp;
            float sx = (float)(left > 0 ? left : 0) / this.Width * movie.MaxU;
            float ex = (1.0f - (float)(right > 0 ? right : 0) / this.Width) * movie.MaxU;
            Random r = new Random();
            for (int i = 0; i < 6; i++)
            {
                startx[i] = sx + (ex - sx - h) * (float)r.NextDouble();
            }
        }
        public override void Draw()
        {
            if (Hidden) return;
            if (movie != null && movie.Initialized) DrawMovieCube();
            else DrawBlackCube();
        }
        private void DrawBlackCube()
        {
            this.rotation += (float)Math.PI / 360;
            Matrix rotate = Matrix.RotationYawPitchRoll((float)Math.PI / 4, (float)Math.PI / 4 + rotation, (float)Math.PI / 4 + rotation);
            Matrix translate = Matrix.Identity;
            Matrix.Translation(350, 250, -300, out translate);
            rotate = Matrix.Multiply(rotate, translate);
            Vector4[] transformed = new Vector4[8];
            for (int i = 0; i < 8; i++)
            {
                transformed[i] = Vector4.Transform(box[i], rotate);
            }
            int max = 0;
            for (int i = 1; i < 8; i++)
            {
                if (transformed[max].Z <= transformed[i].Z)
                {
                    max = i;
                }
            }
            for (int i = 0; i < 8; i++)
            {
                transformed[i].Z = 0.5f;
            }

            int color = new Color4(1.0f, 1, 1, 1).ToArgb();
            TransformedColoredTexturedVertex[] cv0 = new TransformedColoredTexturedVertex[4]
      {
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[6],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[7],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[3],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[5],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
      };
            TransformedColoredTexturedVertex[] cv1 = new TransformedColoredTexturedVertex[4]
      {
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[7],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[4],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[5],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[1],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
      };
            TransformedColoredTexturedVertex[] cv2 = new TransformedColoredTexturedVertex[4]
      {
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[4],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[2],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[1],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[0],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
      };
            TransformedColoredTexturedVertex[] cv3 = new TransformedColoredTexturedVertex[4]
      {
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[2],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[6],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[0],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[3],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
      };
            TransformedColoredTexturedVertex[] cv4 = new TransformedColoredTexturedVertex[4]
      {
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[1],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[0],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[5],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[3],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
      };
            TransformedColoredTexturedVertex[] cv5 = new TransformedColoredTexturedVertex[4]
      {
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[7],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[6],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[4],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[2],
          Color = color,
          TextureCoordinates = new Vector2() {X = 0,Y = 0},
        },
      };



            this.device.SetTexture(0, black.Texture);
            switch (max)
            {
                case 0:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv2);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv3);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv4);
                    break;
                case 1:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv1);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv2);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv4);
                    break;
                case 2:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv2);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv3);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv5);
                    break;
                case 3:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv0);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv3);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv4);
                    break;
                case 4:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv1);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv2);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv5);
                    break;
                case 5:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv0);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv1);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv4);
                    break;
                case 6:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv0);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv3);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv5);
                    break;
                case 7:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv0);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv1);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv5);
                    break;

            }
        }
        private void DrawMovieCube()
        {
            float asp = (float)this.Height / this.Width;
            this.rotation += (float)Math.PI / 360;
            Matrix rotate = Matrix.RotationYawPitchRoll((float)Math.PI / 4, (float)Math.PI / 4 + rotation, (float)Math.PI / 4 + rotation);
            Matrix translate = Matrix.Identity;
            Matrix.Translation(350, 250, -300, out translate);
            rotate = Matrix.Multiply(rotate, translate);
            Vector4[] transformed = new Vector4[8];
            for (int i = 0; i < 8; i++)
            {
                transformed[i] = Vector4.Transform(box[i], rotate);
            }
            int max = 0;
            for (int i = 1; i < 8; i++)
            {
                if (transformed[max].Z <= transformed[i].Z)
                {
                    max = i;
                }
            }
            for (int i = 0; i < 8; i++)
            {
                transformed[i].Z = 0.5f;
            }
            int top = movie.TrimmingData.Top, left = movie.TrimmingData.Left, right = movie.TrimmingData.Right, bottom = movie.TrimmingData.Bottom;
            float maxu = movie.MaxU, maxv = movie.MaxV;
            float alpha = movie.Alpha;
            int color = new Color4(1.0f, alpha, alpha, alpha).ToArgb();
            TransformedColoredTexturedVertex[] cv0 = new TransformedColoredTexturedVertex[4]
      {
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[6],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[0], Y = (float)(top>0?top:0)/this.Height*maxv/asp },
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[7],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[0]+this.h, Y = (float)(top>0?top:0)/this.Height*maxv/asp },
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[3],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[0], Y = (float)(top>0?top:0)/this.Height*maxv + this.h/asp },
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[5],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[0]+this.h, Y = (float)(top>0?top:0)/this.Height*maxv + this.h/asp },
        },
      };
            TransformedColoredTexturedVertex[] cv1 = new TransformedColoredTexturedVertex[4]
      {
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[7],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[1], Y = (float)(top>0?top:0)/this.Height*maxv/asp },
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[4],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[1]+this.h, Y = (float)(top>0?top:0)/this.Height*maxv/asp },
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[5],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[1], Y = (float)(top>0?top:0)/this.Height*maxv + this.h/asp},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[1],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[1]+this.h, Y = (float)(top>0?top:0)/this.Height*maxv+ this.h/asp },
        },
      };
            TransformedColoredTexturedVertex[] cv2 = new TransformedColoredTexturedVertex[4]
      {
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[4],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[2], Y = (float)(top>0?top:0)/this.Height*maxv/asp },
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[2],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[2]+this.h, Y = (float)(top>0?top:0)/this.Height*maxv/asp },
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[1],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[2], Y =(float)(top>0?top:0)/this.Height*maxv+ this.h/asp },
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[0],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[2]+this.h, Y = (float)(top>0?top:0)/this.Height*maxv+ this.h/asp },
        },
      };
            TransformedColoredTexturedVertex[] cv3 = new TransformedColoredTexturedVertex[4]
      {
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[2],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[3], Y = (float)(top>0?top:0)/this.Height*maxv/asp },
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[6],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[3]+this.h, Y = (float)(top>0?top:0)/this.Height*maxv/asp },
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[0],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[3], Y = (float)(top>0?top:0)/this.Height*maxv + this.h/asp},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[3],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[3]+this.h, Y = (float)(top>0?top:0)/this.Height*maxv + this.h/asp},
        },
      };
            TransformedColoredTexturedVertex[] cv4 = new TransformedColoredTexturedVertex[4]
      {
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[1],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[4], Y =(float)(top>0?top:0)/this.Height*maxv/asp },
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[0],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[4]+this.h, Y = (float)(top>0?top:0)/this.Height*maxv/asp },
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[5],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[4], Y = (float)(top>0?top:0)/this.Height*maxv + this.h/asp},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[3],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[4]+this.h, Y = (float)(top>0?top:0)/this.Height*maxv + this.h/asp},
        },
      };
            TransformedColoredTexturedVertex[] cv5 = new TransformedColoredTexturedVertex[4]
      {
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[7],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[5], Y = (float)(top>0?top:0)/this.Height*maxv/asp },
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[6],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[5]+this.h, Y = (float)(top>0?top:0)/this.Height*maxv/asp },
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[4],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[5], Y = (float)(top>0?top:0)/this.Height*maxv + this.h/asp},
        },
        new TransformedColoredTexturedVertex()
        {
          Position = transformed[2],
          Color = color,
          TextureCoordinates = new Vector2() { X = startx[5]+this.h, Y = (float)(top>0?top:0)/this.Height*maxv + this.h/asp},
        },
      };



            this.device.SetTexture(0, movie.Texture);
            switch (max)
            {
                case 0:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv2);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv3);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv4);
                    break;
                case 1:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv1);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv2);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv4);
                    break;
                case 2:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv2);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv3);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv5);
                    break;
                case 3:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv0);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv3);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv4);
                    break;
                case 4:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv1);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv2);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv5);
                    break;
                case 5:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv0);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv1);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv4);
                    break;
                case 6:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv0);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv3);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv5);
                    break;
                case 7:
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv0);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv1);
                    this.device.VertexFormat = TransformedColoredTexturedVertex.Format;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, 2, cv5);
                    break;

            }
        }
        public void SetLoop(double start, double end)
        {
            this.start = start;
            if (end >= movie.Length) end = movie.Length;
            this.end = end;
            Seek(this.start);
            Play();
            Pause();
            Seek(this.start);
            Play();
            Pause();
            Seek(this.start);
            CheckLoopAvailable = true;
        }
        public void CheckLoop()
        {
            double check = movie.MoviePosition;
            if (this.end - 2 / fadestep <= check)
            {
                if (this.FadeState != MovieFadeState.FadeOut)
                {
                    this.FadeOut();
                }
                if (this.end <= check)
                {
                    this.Stop();
                    movie.Seek(this.start);
                    this.Play();
                    this.Pause();
                    this.waitingseek = true;
                    //this.fadein();
                }
            }
            if (waitingseek && waitingcount > 60)
            {
                waitingseek = false;
                this.Play();
                this.FadeIn();
            }
        }

        public override float Alpha
        {
            get;
            set;
        }

        public override bool Hidden
        {
            get;
            set;
        }

        public override Vector2 Position
        {
            get;
            set;
        }

        protected override void DisposeResource()
        {
            if (movie != null)
            {
                movie.Dispose();
                movie = null;
                black.Dispose();
                black = null;
            }
        }

        #region IMovie メンバ

        public void FadeIn()
        {
            if (movie == null) return;
            movie.FadeIn();
        }

        public void FadeOut()
        {
            if (movie == null) return;
            movie.FadeOut();
        }

        public string FileName
        {
            get { return movie.FileName; }
            set { movie.FileName = value; }
        }

        public int Height
        {
            get { return movie.Height; }
        }

        public int Initialize()
        {
            if (movie == null) return 1;
            return movie.Initialize();
        }

        public bool Initialized
        {
            get
            {
                if (movie == null) return false;
                return movie.Initialized;
            }
        }

        public double Length
        {
            get { return movie.Length; }
        }

        public double MoviePosition
        {
            get { return movie.MoviePosition; }
        }

        public void Pause()
        {
            if (movie == null) return;
            movie.Pause();
        }

        public void Play()
        {
            if (movie == null) return;
            movie.Play();
        }

        public bool Playing
        {
            get { return movie.Playing; }
        }

        public void Seek(double time)
        {
            if (movie == null) return;
            movie.Seek(time);
        }

        public void SetDefaultVisible()
        {
            if (movie == null) return;
            movie.SetDefaultVisible();
        }

        public void SetVolume(int vol)
        {
            if (movie == null) return;
            movie.SetVolume(vol);
        }

        public void Stop()
        {
            if (movie == null) return;
            movie.Stop();
        }

        public Texture Texture
        {
            get { return movie.Texture; }
        }

        public int Width
        {
            get { return movie.Width; }
        }
        public float MaxU
        {
            get { return movie.MaxU; }
        }

        public float MaxV
        {
            get { return movie.MaxV; }
        }

        public MovieTrimmingData TrimmingData
        {
            get
            {
                return movie.TrimmingData;
            }
            set
            {
                movie.TrimmingData = value;
            }
        }
        public MovieFadeState FadeState
        {
            get
            {
                if (movie == null) return MovieFadeState.None;
                return movie.FadeState;
            }
        }
        public bool CheckLoopAvailable
        {
            get;
            set;
        }
        #endregion
    }
}
