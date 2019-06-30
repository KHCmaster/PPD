using PPDFramework;
using SharpDX;
using System;

namespace PPDShareComponent
{
    public class ListBoxComponent : SelectableComponent
    {
        private object[] items;

        private PictureObject left;
        private PictureObject right;
        private TextureString str;
        private TextureString[] strItems;
        private int selectedIndex;

        public Object SelectedItem
        {
            get
            {
                return items[selectedIndex];
            }
            set
            {
                SelectedIndex = Array.IndexOf(items, value);
            }
        }

        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    AdjustPosition();
                }
            }
        }

        public ListBoxComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PathManager pathManager, string text, params object[] items) : base(device)
        {
            if (items.Length == 1 && items[0].GetType().IsArray)
            {
                var array = (Array)items[0];
                this.items = new object[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    this.items[i] = array.GetValue(i);
                }
            }
            else
            {
                this.items = items;
            }
            this.AddChild(right = new PictureObject(device, resourceManager, pathManager.Combine("optionlistboxright.png"))
            {
                Scale = new Vector2(0.5f),
                Alpha = 0
            });
            this.AddChild(left = new PictureObject(device, resourceManager, pathManager.Combine("optionlistboxleft.png"))
            {
                Scale = new Vector2(0.5f),
                Alpha = 0
            });
            this.AddChild(str = new TextureString(device, text, 20, PPDColors.White));
            str.Update();
            this.strItems = new TextureString[this.items.Length];
            for (int i = 0; i < this.items.Length; i++)
            {
                this.AddChild(strItems[i] = new TextureString(device, this.items[i].ToString(), 20, PPDColors.White)
                {
                    Hidden = true
                });
                strItems[i].Update();
            }
            left.Position = new Vector2(str.Width + 20, -5);
            AdjustPosition();
        }

        private void AdjustPosition()
        {
            for (int i = 0; i < strItems.Length; i++)
            {
                strItems[i].Hidden = true;
            }
            strItems[selectedIndex].Hidden = false;
            strItems[selectedIndex].Position = new Vector2(left.Position.X + 40, 0);
            right.Position = new Vector2(strItems[selectedIndex].Width + strItems[selectedIndex].Position.X + 5, -5);
        }

        protected override void UpdateImpl()
        {
            right.Alpha = left.Alpha = Selected ? AnimationUtility.IncreaseAlpha(left.Alpha) : AnimationUtility.DecreaseAlpha(left.Alpha);
        }

        public override bool Left()
        {
            selectedIndex--;
            if (selectedIndex < 0)
            {
                selectedIndex = strItems.Length - 1;
            }
            AdjustPosition();
            return true;
        }

        public override bool Right()
        {
            selectedIndex++;
            if (selectedIndex >= strItems.Length)
            {
                selectedIndex = 0;
            }
            AdjustPosition();
            return true;
        }
    }
}
