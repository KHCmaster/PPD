using PPDFramework;
using PPDShareComponent;
using System;

namespace PPD
{
    class HomeTopHeader : FocusableGameComponent
    {
        PPDFramework.Resource.ResourceManager resourceManager;
        HomeBottomMenu hbm;

        TextureString[] strings;
        int drawCount;
        bool operated;

        public HomeTopHeader(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, HomeBottomMenu hbm) : base(device)
        {
            this.resourceManager = resourceManager;
            this.hbm = hbm;

            strings = new TextureString[HomeBottomMenu.ModeArray.Length];
            for (int i = 0; i < strings.Length; i++)
            {
                strings[i] = new TextureString(device, Utility.Language[HomeBottomMenu.ModeArray[i].ToString()], 18, true, PPDColors.White)
                {
                    Position = new SharpDX.Vector2(400, 5)
                };
                strings[i].Alpha = 0;
                this.AddChild(strings[i]);
            }

            hbm.ModeChanged += hbm_ModeChanged;

            this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("l.png"))
            {
                Position = new SharpDX.Vector2(5, 5)
            });
            this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("r.png"))
            {
                Position = new SharpDX.Vector2(760, 5)
            });
            this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("header.png")));
        }

        void hbm_ModeChanged(object sender, EventArgs e)
        {
            operated = true;
            drawCount = 0;
        }

        protected override void UpdateImpl()
        {
            for (int i = 0; i < strings.Length; i++)
            {
                float alpha = (hbm.CurrentMode == (HomeBottomMenu.Mode)i) ? AnimationUtility.IncreaseAlpha(strings[i].Alpha) : AnimationUtility.DecreaseAlpha(strings[i].Alpha);
                if (hbm.CurrentMode == (HomeBottomMenu.Mode)i)
                {
                    if (alpha >= 0.999f)
                    {
                        drawCount++;
                    }
                }
                strings[i].Alpha = alpha;
            }

            if (drawCount >= 60 && operated)
            {
                this.Alpha = AnimationUtility.DecreaseAlpha(this.Alpha, 0.2f);
                this.Position = new SharpDX.Vector2(0, AnimationUtility.GetAnimationValue(this.Position.Y, -30));
            }
            else
            {
                this.Alpha = AnimationUtility.IncreaseAlpha(this.Alpha, 0.2f);
                this.Position = new SharpDX.Vector2(0, AnimationUtility.GetAnimationValue(this.Position.Y, 0));
            }

            operated |= drawCount >= 200;
        }
    }
}
