using PPDFramework;
using PPDShareComponent;

namespace PPD
{
    class ChangeVolumeSprite : FocusableGameComponent
    {
        private const float RectStartX = 120;
        private const float RectWidth = 400;

        PPDFramework.Resource.ResourceManager resourceManager;
        MyGame myGame;
        ISound sound;

        TextureString[] texts;
        RectangleComponent[] rects;
        PictureObject[] spheres;
        PictureObject select;
        int currentIndex;
        bool modified;

        public ChangeVolumeSprite(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, MyGame myGame, ISound sound) : base(device)
        {
            this.resourceManager = resourceManager;
            this.myGame = myGame;
            this.sound = sound;

            texts = new TextureString[3];
            rects = new RectangleComponent[3];
            spheres = new PictureObject[3];
            var sprite = new SpriteObject(device,
                texts[0] = new TextureString(device, Utility.Language["Master"], 18, PPDColors.White),
                texts[1] = new TextureString(device, Utility.Language["Movie"], 18, PPDColors.White)
                {
                    Position = new SharpDX.Vector2(0, 25)
                },
                texts[2] = new TextureString(device, Utility.Language["SE"], 18, PPDColors.White)
                {
                    Position = new SharpDX.Vector2(0, 50)
                },
                rects[0] = new RectangleComponent(device, resourceManager, PPDColors.White) { Position = new SharpDX.Vector2(RectStartX, 10), RectangleHeight = 2, RectangleWidth = RectWidth },
                rects[1] = new RectangleComponent(device, resourceManager, PPDColors.White) { Position = new SharpDX.Vector2(RectStartX, 35), RectangleHeight = 2, RectangleWidth = RectWidth },
                rects[2] = new RectangleComponent(device, resourceManager, PPDColors.White) { Position = new SharpDX.Vector2(RectStartX, 60), RectangleHeight = 2, RectangleWidth = RectWidth },
                spheres[0] = new PictureObject(device, resourceManager, Utility.Path.Combine("sphere.png"), true)
                {
                    Position = new SharpDX.Vector2(RectStartX, 10)
                },
                spheres[1] = new PictureObject(device, resourceManager, Utility.Path.Combine("sphere.png"), true)
                {
                    Position = new SharpDX.Vector2(RectStartX, 35)
                },
                spheres[2] = new PictureObject(device, resourceManager, Utility.Path.Combine("sphere.png"), true)
                {
                    Position = new SharpDX.Vector2(RectStartX, 60)
                },
                select = new PictureObject(device, resourceManager, Utility.Path.Combine("right.png"), true)
                {
                    Position = new SharpDX.Vector2(30, 0),
                    Scale = new SharpDX.Vector2(0.5f, 0.5f)
                }
            )
            {
                Position = new SharpDX.Vector2(160, 225 - 75 / 2f)
            };
            modified = true;

            this.AddChild(sprite);
            AdjustPosition();
            Inputed += ChangeVolumeSprite_Inputed;
            LostFocused += ChangeVolumeSprite_LostFocused;
        }

        void ChangeVolumeSprite_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            SoundMasterControl.Instance.Save();
        }

        private void AdjustPosition()
        {
            AdjustPosition(spheres[0], (SoundMasterControl.Instance.GetVolume(SoundType.Master) - (float)SoundMasterControl.Min) /
                (SoundMasterControl.Max - SoundMasterControl.Min));
            AdjustPosition(spheres[1], (SoundMasterControl.Instance.GetVolume(SoundType.Movie) - (float)SoundMasterControl.Min) /
                (SoundMasterControl.Max - SoundMasterControl.Min));
            AdjustPosition(spheres[2], (SoundMasterControl.Instance.GetVolume(SoundType.Se) - (float)SoundMasterControl.Min) /
                (SoundMasterControl.Max - SoundMasterControl.Min));
        }

        private void AdjustPosition(PictureObject sphere, float ratio)
        {
            sphere.Position = new SharpDX.Vector2(RectStartX + RectWidth * ratio, sphere.Position.Y);
        }

        private void ChangeVolume(int diff)
        {
            SoundType soundType = SoundType.Master;
            switch (currentIndex)
            {
                case 0:
                    soundType = SoundType.Master;
                    break;
                case 1:
                    soundType = SoundType.Movie;
                    break;
                case 2:
                    soundType = SoundType.Se;
                    break;
            }
            var current = SoundMasterControl.Instance.GetVolume(soundType);
            current += diff;
            SoundMasterControl.Instance.ChangeVolume(current, soundType);
        }

        void ChangeVolumeSprite_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                currentIndex--;
                if (currentIndex < 0)
                {
                    currentIndex = 2;
                }
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                currentIndex++;
                if (currentIndex >= 3)
                {
                    currentIndex = 0;
                }
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                if (FocusManager != null)
                {
                    FocusManager.RemoveFocus();
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Left))
            {
                int scale = args.InputInfo.GetPressingFrame(ButtonType.Left) > 60 ? 10 : 1;
                ChangeVolume(-10 * scale);
                AdjustPosition();
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Right))
            {
                int scale = args.InputInfo.GetPressingFrame(ButtonType.Right) > 60 ? 10 : 1;
                ChangeVolume(10 * scale);
                AdjustPosition();
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
        }

        protected override void UpdateImpl()
        {
            if (modified)
            {
                select.Position = new SharpDX.Vector2(-10, texts[currentIndex].Position.Y + 10);
                modified = false;
            }

            select.Position = new SharpDX.Vector2(select.Position.X,
                AnimationUtility.GetAnimationValue(select.Position.Y, texts[currentIndex].Position.Y + 10, 0.2f));
        }
    }
}
