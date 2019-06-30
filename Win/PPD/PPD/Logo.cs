using PPDFramework;
using PPDFramework.Resource;
using PPDFramework.Scene;
using PPDFramework.Shaders;
using PPDFramework.Vertex;
using PPDFrameworkCore;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPD
{
    class Logo : SceneBase
    {
        enum State
        {
            Loading = 0,
            Complete,
            Fading,
            Expanding,
            Hiding,
            Showing
        }

        PictureObject outCircle;
        PictureObject bootImage;
        PictureObject completeImage;
        PictureObject provideImage;
        PictureObject percentImage;
        NumberPictureObject loadPercentImage;
        Circle circle;
        RectangleComponent rect;
        float currentRatio;
        float drawRatio;
        State state = State.Loading;

        public bool Loading { get; set; }

        SpriteObject topBlack;
        SpriteObject bottomBlack;

        public HomeScene HomeScene
        {
            get;
            private set;
        }

        public Logo(PPDDevice device) : base(device)
        {

        }

        public override bool Load()
        {
            HomeScene = new HomeScene(device)
            {
                ResourceManager = this.ResourceManager,
                Sound = this.Sound,
                SceneManager = this.SceneManager,
                GameHost = this.GameHost
            };
            HomeScene.LoadProgressed += HomeScene_LoadProgressed;

            outCircle = new PictureObject(device, ResourceManager, Utility.Path.Combine("logo", "outcircle.png"), true)
            {
                Position = new Vector2(400, 225)
            };
            bootImage = new PictureObject(device, ResourceManager, Utility.Path.Combine("logo", "boot_text.png"), true)
            {
                Position = new Vector2(400, 205)
            };
            completeImage = new PictureObject(device, ResourceManager, Utility.Path.Combine("logo", "complete_text.png"), true)
            {
                Position = new Vector2(400, 205),
                Alpha = 0
            };
            provideImage = new PictureObject(device, ResourceManager, Utility.Path.Combine("logo", "provide_text.png"), false)
            {
                Position = new Vector2(-400, 185)
            };
            percentImage = new PictureObject(device, ResourceManager, Utility.Path.Combine("logo", "percent.png"), false)
            {
                Position = new Vector2(420, 235)
            };
            loadPercentImage = new NumberPictureObject(device, ResourceManager, Utility.Path.Combine("logo", "number.png"))
            {
                Position = new Vector2(415, 235),
                Alignment = Alignment.Right,
                MaxDigit = -1
            };
            rect = new RectangleComponent(device, ResourceManager, PPDColors.White);

            topBlack = new SpriteObject(device);
            topBlack.AddChild(new RectangleComponent(device, ResourceManager, PPDColors.Black)
            {
                RectangleHeight = 225,
                RectangleWidth = 800
            });
            topBlack.AddChild(new RectangleComponent(device, ResourceManager, PPDColors.White)
            {
                Position = new Vector2(0, 223),
                RectangleHeight = 4,
                RectangleWidth = 800
            });

            bottomBlack = new SpriteObject(device)
            {
                Position = new Vector2(0, 225)
            };
            bottomBlack.AddChild(new RectangleComponent(device, ResourceManager, PPDColors.Black)
            {
                RectangleHeight = 225,
                RectangleWidth = 800
            });
            bottomBlack.AddChild(new RectangleComponent(device, ResourceManager, PPDColors.White)
            {
                Position = new Vector2(0, -2),
                RectangleHeight = 4,
                RectangleWidth = 800
            });

            this.AddChild(circle = new Circle(device, ResourceManager));
            this.AddChild(outCircle);
            this.AddChild(loadPercentImage);
            this.AddChild(percentImage);
            this.AddChild(completeImage);
            this.AddChild(bootImage);
            this.AddChild(provideImage);
            this.AddChild(rect);
            this.AddChild(topBlack);
            this.AddChild(bottomBlack);

            var thread = ThreadManager.Instance.GetThread(() => { HomeScene.Load(); });
            thread.Start();

            return true;
        }

        void HomeScene_LoadProgressed(int obj)
        {
            currentRatio = obj;
        }

        public override void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            switch (state)
            {
                case State.Loading:
                    drawRatio = AnimationUtility.GetAnimationValue(drawRatio, currentRatio / 100);
                    if (drawRatio == 1)
                    {
                        state = State.Complete;
                        bootImage.Alpha = 0;
                    }
                    rect.Position = new Vector2(400 - drawRatio * 100, 223);
                    rect.RectangleHeight = 4;
                    rect.RectangleWidth = drawRatio * 200;
                    break;
                case State.Complete:
                    completeImage.Alpha = AnimationUtility.IncreaseAlpha(completeImage.Alpha);
                    if (completeImage.Alpha == 1)
                    {
                        state = State.Fading;
                    }
                    break;
                case State.Fading:
                    completeImage.Alpha = loadPercentImage.Alpha = percentImage.Alpha = outCircle.Alpha = AnimationUtility.DecreaseAlpha(outCircle.Alpha);
                    if (completeImage.Alpha == 0)
                    {
                        state = State.Expanding;
                    }
                    break;
                case State.Expanding:
                    rect.Position = new Vector2(400 - drawRatio * 100, 223);
                    rect.RectangleHeight = 4;
                    rect.RectangleWidth = drawRatio * 200;
                    drawRatio = AnimationUtility.GetAnimationValue(drawRatio, 4);
                    provideImage.Position = AnimationUtility.GetAnimationPosition(provideImage.Position, new Vector2(20, provideImage.Position.Y));
                    if (drawRatio == 4)
                    {
                        state = State.Hiding;
                    }
                    break;
                case State.Hiding:
                    provideImage.Position = AnimationUtility.GetAnimationPosition(provideImage.Position, new Vector2(900, provideImage.Position.Y));
                    if (provideImage.Position.X >= 800)
                    {
                        this.AddChild(HomeScene);
                        state = State.Showing;
                    }
                    break;
                case State.Showing:

                    topBlack.Position = AnimationUtility.GetAnimationPosition(topBlack.Position, new Vector2(0, -230));
                    bottomBlack.Position = AnimationUtility.GetAnimationPosition(bottomBlack.Position, new Vector2(0, 460));

                    if (topBlack.Position.Y <= -225)
                    {
                        SceneManager.CurrentScene = HomeScene;
                    }

                    HomeScene.Update(EmptyInputInfo.Instance, MouseInfo.Empty);
                    break;
            }

            HomeScene.Hidden = topBlack.Hidden = bottomBlack.Hidden = state < State.Showing;
            rect.Hidden = state >= State.Showing;
            loadPercentImage.Value = (uint)(drawRatio * 100);
            circle.Ratio = state >= State.Fading ? completeImage.Alpha : drawRatio;
            circle.Inverse = state >= State.Fading;
            Update();
        }

        class Circle : GameComponent
        {
            ImageResourceBase loadCircleImageResource;
            VertexInfo loadCircleImageVertex;
            VertexInfo invLoadCircleImageVertex;
            int displayNum;

            public bool Inverse
            {
                get;
                set;
            }

            public float Ratio
            {
                get;
                set;
            }

            public Circle(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
            {
                loadCircleImageResource = resourceManager.GetResource<ImageResourceBase>(Utility.Path.Combine("logo", "circle.png"));

                loadCircleImageVertex = device.GetModule<ShaderCommon>().CreateVertex(362);
                invLoadCircleImageVertex = device.GetModule<ShaderCommon>().CreateVertex(362);
                var tempLoadCircleVertices = new ColoredTexturedVertex[362];
                var tempInvLoadCircleVertices = new ColoredTexturedVertex[362];
                for (var i = 0; i < tempLoadCircleVertices.Length; i++)
                {
                    tempLoadCircleVertices[i] = new ColoredTexturedVertex(Vector3.Zero);
                    tempInvLoadCircleVertices[i] = new ColoredTexturedVertex(Vector3.Zero);
                }
                tempLoadCircleVertices[0].Position = new Vector3(400, 225, 0.5f);
                tempLoadCircleVertices[0].TextureCoordinates = loadCircleImageResource.GetActualUV(new Vector2(0.5f));
                for (int i = 1; i < tempLoadCircleVertices.Length; i++)
                {
                    var rad = (float)((i - 1) * Math.PI / 180);
                    var uv = new Vector2((float)Math.Sin(rad), -(float)Math.Cos(rad));
                    tempLoadCircleVertices[i].Position = new Vector3(
                        400 + loadCircleImageResource.Width / 2 * uv.X,
                        225 + loadCircleImageResource.Height / 2 * uv.Y,
                        0.5f);
                    tempLoadCircleVertices[i].TextureCoordinates = loadCircleImageResource.GetActualUV((uv / 2 + new Vector2(0.5f)));
                }

                Array.Copy(tempLoadCircleVertices, 1, tempInvLoadCircleVertices, 0, 361);
                Array.Reverse(tempInvLoadCircleVertices, 0, tempInvLoadCircleVertices.Length);
                tempInvLoadCircleVertices[0] = tempLoadCircleVertices[0];
                loadCircleImageVertex.Write(tempLoadCircleVertices);
                invLoadCircleImageVertex.Write(tempInvLoadCircleVertices);
            }

            protected override void UpdateImpl()
            {
                displayNum = (int)(361 * Ratio);
            }

            protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
            {
                alphaBlendContext.Texture = loadCircleImageResource.Texture;
                alphaBlendContext.Vertex = Inverse ? invLoadCircleImageVertex : loadCircleImageVertex;
                device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext, PrimitiveType.TriangleFan, displayNum - 1, 0, displayNum + 1);
            }

            protected override bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
            {
                return displayNum >= 2;
            }
        }
    }
}
