using PPDFramework;
using PPDShareComponent;
using System;

namespace PPD
{
    class HomeBottomMenu : FocusableGameComponent
    {
        public event EventHandler ModeChanged;
        public enum Mode
        {
            Setting = 0,
            Movie,
            Game,
            Feed
        }
        public static Mode[] ModeArray = Enum.GetValues(typeof(Mode)) as Mode[];

        Mode mode = Mode.Feed;
        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;

        PictureObject back;
        PictureObject[] icons;
        TextureString[] strings;

        int drawCount;
        bool operated;

        public Mode CurrentMode
        {
            get { return mode; }
            set { mode = value; }
        }

        public HomeBottomMenu(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.resourceManager = resourceManager;
            this.sound = sound;

            back = new PictureObject(device, resourceManager, Utility.Path.Combine("bottom_back.png"));
            back.Position = new SharpDX.Vector2(400 - back.Width / 2, 0);

            icons = new PictureObject[ModeArray.Length];
            strings = new TextureString[ModeArray.Length];
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i] = new PictureObject(device, resourceManager, Utility.Path.Combine(String.Format("{0}_icon.png", ((Mode)i))), true);
                this.AddChild(icons[i]);
                strings[i] = new TextureString(device, Utility.Language[((Mode)i).ToString()], 20, true, PPDColors.White);
                this.AddChild(strings[i]);
            }

            this.AddChild(back);

            this.Position = new SharpDX.Vector2(0, 421);
            HandleOverFocusInput = true;
            Inputed += HomeUnderMenu_Inputed;
        }

        void HomeUnderMenu_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.L))
            {
                operated = true;
                mode--;
                if (mode < 0)
                {
                    mode = ModeArray[ModeArray.Length - 1];
                }
                OnModeChanged(mode);
                sound.Play(PPDSetting.DefaultSounds[3], -1000);
                drawCount = 0;
            }
            else if (args.InputInfo.IsPressed(ButtonType.R))
            {
                operated = true;
                mode++;
                if ((int)mode >= ModeArray.Length)
                {
                    mode = ModeArray[0];
                }
                OnModeChanged(mode);
                sound.Play(PPDSetting.DefaultSounds[3], -1000);
                drawCount = 0;
            }
        }

        private void OnModeChanged(Mode mode)
        {
            if (ModeChanged != null)
            {
                ModeChanged.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void UpdateImpl()
        {
            float nextX = 0;
            for (int i = 0; i < icons.Length; i++)
            {
                float newScale = (int)mode == i ? AnimationUtility.IncreaseAlpha(icons[i].Scale.X) : AnimationUtility.GetAnimationValue(icons[i].Scale.X, 0.5f);
                if ((int)mode == i)
                {
                    if (newScale >= 0.999f)
                    {
                        drawCount++;
                    }
                }

                icons[i].Scale = new SharpDX.Vector2(newScale, newScale);
                icons[i].Position = new SharpDX.Vector2(nextX + newScale * icons[i].Width / 2, 0);
                nextX = icons[i].Position.X + icons[i].Width * newScale / 2 + 10;
            }
            float diff = 400 - (ModeArray.Length % 2 == 0 ? (icons[ModeArray.Length / 2 - 1].Position.X + icons[ModeArray.Length / 2].Position.X) / 2 : icons[ModeArray.Length / 2].Position.X);
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i].Position = new SharpDX.Vector2(icons[i].Position.X + diff, 0);
            }
            for (int i = 0; i < strings.Length; i++)
            {
                strings[i].Position = new SharpDX.Vector2(icons[i].Position.X, -45);
                strings[i].Alpha = (icons[i].Scale.X - 0.5f) * 2;
            }

            if (drawCount >= 60 && operated)
            {
                this.Alpha = AnimationUtility.DecreaseAlpha(this.Alpha, 0.2f);
                this.Position = new SharpDX.Vector2(0, AnimationUtility.GetAnimationValue(this.Position.Y, 480));
            }
            else
            {
                this.Alpha = AnimationUtility.IncreaseAlpha(this.Alpha, 0.2f);
                this.Position = new SharpDX.Vector2(0, AnimationUtility.GetAnimationValue(this.Position.Y, 421));
            }

            operated |= drawCount >= 200;
        }
    }
}
