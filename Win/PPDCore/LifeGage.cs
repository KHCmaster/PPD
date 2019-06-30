using PPDFramework;
using SharpDX;
using System;
using System.Collections.Generic;


namespace PPDCore
{
    /// <summary>
    /// ライフゲージ表示UI
    /// </summary>
    class LifeGage : GameComponent
    {
        enum State
        {
            Danger = 0,
            Normal = 1,
            Full = 2
        }

        State state = State.Normal;

        SpriteObject flareSprite;
        SpriteObject backSprite;
        SpriteObject overraySprite;
        SpriteObject onpuSprite;

        PictureObject frame;
        PictureObject black;
        List<OnpuObject> onpus;
        float currentlife = 50;
        PPDFramework.Resource.ResourceManager resourceManager;


        public LifeGage(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            this.resourceManager = resourceManager;
            black = new PictureObject(device, resourceManager, Utility.Path.Combine("lifegage", "black.png"))
            {
                Position = new Vector2(0, 2)
            };
            frame = new PictureObject(device, resourceManager, Utility.Path.Combine("lifegage", "frame.png"))
            {
                Scale = new SharpDX.Vector2(0.99f, 0.95f)
            };
            this.AddChild(frame);
            this.AddChild(black);
            flareSprite = new SpriteObject(device);
            backSprite = new SpriteObject(device);
            overraySprite = new SpriteObject(device);
            onpuSprite = new SpriteObject(device);
            this.AddChild(flareSprite);
            this.AddChild(overraySprite);
            this.AddChild(onpuSprite);
            this.AddChild(backSprite);
            for (int i = 0; i < 3; i++)
            {
                string content = "";
                switch (i)
                {
                    case 0:
                        content = "danger";
                        break;
                    case 1:
                        content = "normal";
                        break;
                    case 2:
                        content = "full";
                        break;
                }
                var flare = new EffectObject(device, resourceManager, Utility.Path.Combine("lifegage", String.Format("{0}{1}", content, "flare.etd")))
                {
                    Position = new Vector2(56, 11)
                };
                flare.PlayType = Effect2D.EffectManager.PlayType.ReverseLoop;
                flare.Play();
                flareSprite.AddChild(flare);
                backSprite.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("lifegage", String.Format("{0}{1}", content, "back.png")))
                {
                    Position = new Vector2(2, 2)
                });
                overraySprite.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("lifegage", String.Format("{0}{1}", content, "overray.png")))
                {
                    Position = new Vector2(2, 2)
                });
                flareSprite[i].Hidden = overraySprite[i].Hidden = backSprite[i].Hidden = true;
            }

            flareSprite[(int)state].Hidden = overraySprite[(int)state].Hidden = backSprite[(int)state].Hidden = false;

            onpus = new List<OnpuObject>();
            CurrentLife = 50;

            InitializeComponentPosition();
        }

        private void InitializeComponentPosition()
        {
            this.Position = new Vector2(2, 16);
        }

        public void Retry()
        {
            CurrentLife = 50;
            state = State.Normal;
            SetDefault();
            InitializeComponentPosition();
        }

        public float CurrentLife
        {
            set
            {
                float previouslife = currentlife;
                currentlife = value;
                black.Rectangle = new RectangleF(0, 0, black.Width * (1 - value / 100f), black.Height);
                black.Position = new SharpDX.Vector2(Position.X + 2 + black.Width - black.Rectangle.Width, black.Position.Y);

                flareSprite[(int)state].Hidden = overraySprite[(int)state].Hidden = backSprite[(int)state].Hidden = true;

                if (currentlife >= 100) state = State.Full;
                else if (currentlife < 20) state = State.Danger;
                else state = State.Normal;

                flareSprite[(int)state].Hidden = overraySprite[(int)state].Hidden = backSprite[(int)state].Hidden = false;
            }
        }

        public void CreateOnpu(int num)
        {
            var r = new Random();
            for (int i = 0; i < num; i++)
            {
                var po = new OnpuObject(device, resourceManager, Utility.Path.Combine("lifegage", "onpu", String.Format("{0}.png", r.Next(1, 6))), 5 + i * 3, 12, true);
                var scale = (float)(r.NextDouble() / 2 + 0.5);
                po.Scale = new Vector2(scale, scale);
                var rotation = (float)((r.NextDouble() - 0.5) * Math.PI / 8);
                po.Rotation = rotation;
                po.VelX = (float)((r.NextDouble() + 0.5) * 2);
                onpus.Add(po);
                onpuSprite.AddChild(po);
            }
        }

        protected override void UpdateImpl()
        {
            for (int i = onpus.Count - 1; i >= 0; i--)
            {
                OnpuObject onpu = onpus[i];
                onpu.Position = new SharpDX.Vector2(onpu.Position.X + onpu.VelX, onpu.Position.Y);
                onpu.Alpha = (black.Position.X - onpu.Position.X) / (black.Width - black.Rectangle.Width);
                if (onpu.Alpha <= 0)
                {
                    onpuSprite.RemoveChild(onpus[i]);
                    onpus.RemoveAt(i);
                }
            }
        }
    }
}
