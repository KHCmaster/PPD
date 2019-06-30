using PPDFramework;

namespace PPDMulti
{
    class StripeEffectComponent : GameComponent
    {
        PPDFramework.Resource.ResourceManager resourceManager;

        int updateCount;

        public StripeFilterEffect Effect
        {
            get;
            set;
        }

        public StripeEffectComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            this.resourceManager = resourceManager;

            CreateStripe(false);
        }

        private void CreateStripe(bool thick)
        {
            int thickness = thick ? 40 : 30;
            this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                Position = new SharpDX.Vector2(-thickness, 0),
                RectangleWidth = thickness,
                RectangleHeight = 450
            });
        }

        public bool EffectExpired
        {
            get;
            set;
        }

        protected override void UpdateImpl()
        {
            updateCount++;

            if (!EffectExpired && updateCount % 60 == 0)
            {
                CreateStripe(Effect.EffectLevel == StripeFilterEffect.MaxEffectLevel);
            }

            for (int i = ChildrenCount - 1; i >= 0; i--)
            {
                this[i].Position = new SharpDX.Vector2(this[i].Position.X + 1, this[i].Position.Y);
                if (this[i].Position.X - this[i].Width / 2 >= 800)
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
