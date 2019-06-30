using Effect2D;
using PPDFramework;
using System;

namespace PPDMulti
{
    class FogEffectComponent : GameComponent
    {
        PPDFramework.Resource.ResourceManager resourceManager;

        int updateCount;

        public FogFilterEffect Effect
        {
            get;
            set;
        }

        public FogEffectComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            this.resourceManager = resourceManager;

            CreateFog();
        }

        private void CreateFog()
        {
            var random = new Random();
            int number = random.Next(0, 10), x = random.Next(0, 800);

            PictureObject temp;
            this.AddChild(temp = new PictureObject(device, resourceManager, Utility.Path.Combine(String.Format("cloud{0}.png", new Random().Next(0, 10))), true)
            {
                BlendMode = BlendMode.LinearDodge
            });
            temp.Position = new SharpDX.Vector2(x, 450 + temp.Height / 2);
        }

        public bool EffectExpired
        {
            get;
            set;
        }

        protected override void UpdateImpl()
        {
            updateCount++;

            if (!EffectExpired && updateCount % 30 == 0)
            {
                for (int i = 0; i < Effect.EffectLevel; i++)
                {
                    CreateFog();
                }
            }

            for (int i = ChildrenCount - 1; i >= 0; i--)
            {
                this[i].Position = new SharpDX.Vector2(this[i].Position.X, this[i].Position.Y - 1);
                if (this[i].Position.Y <= -this[i].Height / 2)
                {
                    this.RemoveChild(this[i]);
                }
            }

            if (EffectExpired && ChildrenCount == 0)
            {
                Parent.RemoveChild(this);
            }
        }
    }
}
