using PPDFramework;

namespace PPDShareComponent
{
    public class FadableButton : GameComponent
    {
        PictureObject normal;
        PictureObject over;
        TextureString str;

        bool mouseOn;

        public bool Selected
        {
            get;
            set;
        }

        public FadableButton(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PathObject normalImgPath, PathObject overImgPath, string text) : base(device)
        {
            InternalStruct(resourceManager, normalImgPath, overImgPath, new TextureString(device, text, 14, PPDColors.White));
        }

        public FadableButton(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PathObject normalImgPath, PathObject overImgPath, TextureString str) : base(device)
        {
            if (str != null)
            {
                InternalStruct(resourceManager, normalImgPath, overImgPath, str);
            }
            else
            {
                InternalStruct(resourceManager, normalImgPath, overImgPath, new TextureString(device, "", 14, PPDColors.White));
            }
        }

        private void InternalStruct(PPDFramework.Resource.ResourceManager resourceManager, PathObject normalImgPath, PathObject overImgPath, TextureString str)
        {
            normal = new PictureObject(device, resourceManager, normalImgPath, true);
            this.AddChild(normal);
            over = new PictureObject(device, resourceManager, overImgPath, true);
            this.AddChild(over);
            over.Alpha = 0;
            this.str = str;
            this.AddChild(str);
            str.Alpha = 0;
            str.Position = new SharpDX.Vector2(-str.Width / 2, -normal.Height / 2 - str.Height);

            MouseEnter += FadableButton_MouseEnter;
            MouseLeave += FadableButton_MouseLeave;
        }

        void FadableButton_MouseLeave(GameComponent sender, MouseEvent mouseEvent)
        {
            mouseOn = false;
        }

        void FadableButton_MouseEnter(GameComponent sender, MouseEvent mouseEvent)
        {
            mouseOn = true;
        }

        public override float Width
        {
            get
            {
                return normal.Width;
            }
        }

        public override float Height
        {
            get
            {
                return normal.Height;
            }
        }

        protected override void UpdateImpl()
        {
            float newAlpha = (mouseOn || Selected) ? AnimationUtility.DecreaseAlpha(normal.Alpha) : AnimationUtility.IncreaseAlpha(normal.Alpha);
            normal.Alpha = newAlpha;
            over.Alpha = 1 - newAlpha;
            str.Alpha = 1 - newAlpha;
        }

        public override bool HitTest(SharpDX.Vector2 vec)
        {
            return base.HitTest(vec + new SharpDX.Vector2(normal.Width / 2, normal.Height / 2));
        }
    }
}
