using PPDFramework;
using SharpDX;

namespace PPDShareComponent
{
    public class CheckBoxComponent : SelectableComponent
    {
        private PictureObject check;
        private PictureObject checkBox;

        public CheckBoxComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PathManager pathManager, string text) : base(device)
        {
            this.AddChild(check = new PictureObject(device, resourceManager, pathManager.Combine("optioncheck.png"))
            {
                Position = new Vector2(0, -5),
                Scale = new Vector2(0.5f),
                Hidden = true
            });
            this.AddChild(checkBox = new PictureObject(device, resourceManager, pathManager.Combine("optioncheckbox.png"))
            {
                Position = new Vector2(7, 7),
                Scale = new Vector2(0.5f),
                Alpha = 0
            });
            TextureString str;
            this.AddChild(str = new TextureString(device, text, 20, PPDColors.White)
            {
                Position = new Vector2(30, 0)
            });
        }

        protected override void UpdateImpl()
        {
            check.Hidden = !Checked;
            checkBox.Alpha = Selected ? AnimationUtility.IncreaseAlpha(checkBox.Alpha) : AnimationUtility.DecreaseAlpha(checkBox.Alpha);
        }
    }
}
