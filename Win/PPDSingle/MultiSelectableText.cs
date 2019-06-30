using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDSingle
{
    class MultiSelectableText : FocusableGameComponent
    {
        const int FramePerMenu = 60;
        IGameHost gameHost;
        PPDFramework.Resource.ResourceManager resourceManager;
        RectangleComponent rectangle;
        TextureString startTextString;
        TextureString[] menuTextStrings;
        int fontHeight;
        int pressingCount;

        private float MaxMenuWidth
        {
            get
            {
                return menuTextStrings.Max(m => m.Width);
            }
        }

        private float CurrentMenuWidth
        {
            get
            {
                return menuTextStrings[CurrentMenuIndex].Width;
            }
        }

        private float CurrentProgress
        {
            get
            {
                return Math.Min(1, (float)pressingCount / (60 * menuTextStrings.Length));
            }
        }

        private int CurrentMenuIndex
        {
            get
            {
                return Math.Min(menuTextStrings.Length - 1, pressingCount / 60);
            }
        }

        public int SelectedIndex
        {
            get;
            private set;
        }

        public event Action Selected;

        public MultiSelectableText(PPDDevice device, IGameHost gameHost, PPDFramework.Resource.ResourceManager resourceManager, string startText, string[] menuTexts, int fontHeight) : base(device)
        {
            this.gameHost = gameHost;
            this.resourceManager = resourceManager;
            this.fontHeight = fontHeight;

            this.AddChild(startTextString = new TextureString(device, startText, fontHeight, PPDColors.White));
            var menus = new List<TextureString>();
            foreach (var menuText in menuTexts)
            {
                var obj = new TextureString(device, menuText, fontHeight, PPDColors.White)
                {
                    Alpha = 0
                };
                this.AddChild(obj);
                menus.Add(obj);
            }
            menuTextStrings = menus.ToArray();
            this.AddChild(rectangle = new RectangleComponent(device, resourceManager, PPDColors.Active)
            {
                Position = new Vector2(0, -fontHeight / 2),
                RectangleHeight = fontHeight + fontHeight,
                RectangleWidth = 0,
                Alpha = 0.75f
            });

            GotFocused += MultiSelectableText_GotFocused;
            LostFocused += MultiSelectableText_LostFocused;
            Inputed += MultiSelectableText_Inputed;
        }

        void MultiSelectableText_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            rectangle.RectangleWidth = 0;
            startTextString.Alpha = 1;
            foreach (var menuTextString in menuTextStrings)
            {
                menuTextString.Alpha = 0;
            }
        }

        void MultiSelectableText_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            rectangle.RectangleWidth = 0;
            pressingCount = 0;
            startTextString.Alpha = 0;
            foreach (var menuTextString in menuTextStrings)
            {
                menuTextString.Alpha = 0;
            }
            if (menuTextStrings.Length > 0)
            {
                menuTextStrings[0].Alpha = 1;
            }
        }

        void MultiSelectableText_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsReleased(ButtonType.Circle) || CurrentProgress == 1)
            {
                SelectedIndex = CurrentMenuIndex;
                FocusManager.RemoveFocus();
                OnSelected();
                return;
            }
            var prevObject = menuTextStrings[CurrentMenuIndex];
            pressingCount = args.InputInfo.GetPressingFrame(ButtonType.Circle);
            var currentObject = menuTextStrings[CurrentMenuIndex];
            if (prevObject != currentObject)
            {
                prevObject.Alpha = 0;
                currentObject.Alpha = 1;
            }
        }

        protected override void UpdateImpl()
        {
            rectangle.RectangleWidth = AnimationUtility.GetAnimationValue(rectangle.RectangleWidth, CurrentMenuWidth * CurrentProgress);
        }

        protected override bool OnCanUpdate()
        {
            return Focused;
        }

        protected void OnSelected()
        {
            Selected?.Invoke();
        }
    }
}
