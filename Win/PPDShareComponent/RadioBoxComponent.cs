using PPDFramework;
using SharpDX;
using System;

namespace PPDShareComponent
{
    public class RadioBoxComponent : SelectableComponent
    {
        private PictureObject check;
        private PictureObject checkBox;

        public RadioBoxComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PathManager pathManager, string text) : base(device)
        {
            this.AddChild(check = new PictureObject(device, resourceManager, pathManager.Combine("optioncheck.png"))
            {
                Position = new Vector2(0, -5),
                Scale = new Vector2(0.5f),
                Hidden = true
            });
            this.AddChild(checkBox = new PictureObject(device, resourceManager, pathManager.Combine("optionradiobox.png"))
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

            CheckChanged += RadioComponent_CheckChanged;
        }

        protected override void UpdateImpl()
        {
            check.Hidden = !Checked;
            checkBox.Alpha = Selected ? AnimationUtility.IncreaseAlpha(checkBox.Alpha) : AnimationUtility.DecreaseAlpha(checkBox.Alpha);
        }

        void RadioComponent_CheckChanged(object sender, EventArgs e)
        {
            if (!this.Checked)
            {
                return;
            }

            if (this.Parent != null)
            {
                foreach (GameComponent child in this.Parent.Children)
                {
                    if (child is SelectableComponent selectableChild && selectableChild != this)
                    {
                        selectableChild.Checked = false;
                    }
                }
            }
        }
    }
}
