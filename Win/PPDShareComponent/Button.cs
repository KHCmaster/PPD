using PPDFramework;
using SharpDX;

namespace PPDShareComponent
{
    public class Button : GameComponent
    {
        private TextureString textureString;
        private EffectObject button;
        private PictureObject disableButton;
        private PPDFramework.Resource.ResourceManager resourceManager;

        private bool disabled;

        public Button(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PathManager pathManager, string text) : base(device)
        {
            this.resourceManager = resourceManager;
            textureString = new TextureString(device, text, 20, true, PPDColors.White)
            {
                Position = new Vector2(0, -10)
            };
            button = new EffectObject(device, resourceManager, pathManager.Combine("difficulty.etd"))
            {
                Alignment = EffectObject.EffectAlignment.Center,
                PlayType = Effect2D.EffectManager.PlayType.Loop
            };
            button.Play();
            disableButton = new PictureObject(device, resourceManager, pathManager.Combine("difficulty.png"), true)
            {
                Hidden = true
            };

            this.AddChild(textureString);
            this.AddChild(button);
            this.AddChild(disableButton);
        }

        public bool Enabled
        {
            get
            {
                return !disabled;
            }
            set
            {
                if (value)
                {
                    textureString.Color = PPDColors.White;
                }
                else
                {
                    textureString.Color = PPDColors.Gray;
                }
                disabled = !value;
            }
        }

        public bool Selected
        {
            get
            {
                return !button.Hidden;
            }
            set
            {
                if (value)
                {
                    button.Hidden = false;
                    disableButton.Hidden = true;
                }
                else
                {
                    button.Hidden = true;
                    disableButton.Hidden = false;
                }
            }
        }

        public override bool HitTest(Vector2 vec)
        {
            return base.HitTest(new Vector2(vec.X + disableButton.Width / 2, vec.Y + disableButton.Height / 2));
        }
    }
}
