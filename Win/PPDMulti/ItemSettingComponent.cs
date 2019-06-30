using PPDFramework;
using PPDMultiCommon.Data;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDMulti
{
    class ItemSettingComponent : FocusableGameComponent
    {
        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;
        PictureObject back;

        SpriteObject manualUseSprite;
        SpriteObject autoUseSprite;

        UseItemComponent current;

        private bool CurrentIsInManual
        {
            get
            {
                return manualUseSprite.ContainsChild(current);
            }
        }

        public ItemSettingComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.resourceManager = resourceManager;
            this.sound = sound;

            this.AddChild(back = new PictureObject(device, resourceManager, Utility.Path.Combine("dialog_back.png")));
            this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.75f
            });
            back.AddChild(new TextureString(device, Utility.Language["ManualUse"], 16, true, PPDColors.White)
            {
                Position = new Vector2(200, 80)
            });
            back.AddChild(new TextureString(device, Utility.Language["AutoUse"], 16, true, PPDColors.White)
            {
                Position = new Vector2(600, 80)
            });
            back.AddChild(new LineRectangleComponent(device, resourceManager, PPDColors.White)
            {
                Position = new SharpDX.Vector2(50, 100),
                RectangleHeight = 310,
                RectangleWidth = 300
            });
            back.AddChild(new LineRectangleComponent(device, resourceManager, PPDColors.White)
            {
                Position = new SharpDX.Vector2(450, 100),
                RectangleHeight = 310,
                RectangleWidth = 300
            });
            back.AddChild(manualUseSprite = new SpriteObject(device) { Position = new SharpDX.Vector2(60, 105) });
            back.AddChild(autoUseSprite = new SpriteObject(device) { Position = new SharpDX.Vector2(460, 105) });

            back.AddChild(new TextureString(device, Utility.Language["ItemUseSetting"], 30, PPDColors.White)
            {
                Position = new Vector2(35, 30)
            });

            Inputed += ItemSettingComponent_Inputed;
            GotFocused += ItemSettingComponent_GotFocused;

            for (int i = 0; i < ItemUseManager.Manager.AutoUseItemTypes.Length; i++)
            {
                autoUseSprite.AddChild(new UseItemComponent(device, resourceManager, ItemUseManager.Manager.AutoUseItemTypes[i])
                {
                    Position = new SharpDX.Vector2(0, i * 20)
                });
            }

            var itemTypeArray = (ItemType[])Enum.GetValues(typeof(ItemType));
            int addedCount = 0;
            for (int i = 1; i < itemTypeArray.Length; i++)
            {
                if (Array.IndexOf(ItemUseManager.Manager.AutoUseItemTypes, itemTypeArray[i]) < 0)
                {
                    manualUseSprite.AddChild(new UseItemComponent(device, resourceManager, itemTypeArray[i])
                    {
                        Position = new SharpDX.Vector2(0, addedCount * 20)
                    });
                    addedCount++;
                }
            }

            if (manualUseSprite.ChildrenCount > 0)
            {
                current = manualUseSprite[0] as UseItemComponent;
            }
            else
            {
                current = autoUseSprite[0] as UseItemComponent;
            }
            current.Selected = true;
        }

        void ItemSettingComponent_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            back.Position = new SharpDX.Vector2(0, 50);
            Alpha = 0;
        }

        void ItemSettingComponent_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                current.Selected = false;
                SpriteObject targetSprite = CurrentIsInManual ? manualUseSprite : autoUseSprite;
                var index = targetSprite.IndexOf(current);
                index++;
                if (index >= targetSprite.ChildrenCount)
                {
                    index = 0;
                }
                current = targetSprite[index] as UseItemComponent;
                current.Selected = true;
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                current.Selected = false;
                SpriteObject targetSprite = CurrentIsInManual ? manualUseSprite : autoUseSprite;
                var index = targetSprite.IndexOf(current);
                index--;
                if (index < 0)
                {
                    index = targetSprite.ChildrenCount - 1;
                }
                current = targetSprite[index] as UseItemComponent;
                current.Selected = true;
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Left))
            {
                if (!CurrentIsInManual && manualUseSprite.ChildrenCount > 0)
                {
                    current.Selected = false;
                    var index = autoUseSprite.IndexOf(current);
                    if (index >= manualUseSprite.ChildrenCount)
                    {
                        index = manualUseSprite.ChildrenCount - 1;
                    }
                    current = manualUseSprite[index] as UseItemComponent;
                    current.Selected = true;
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Right))
            {
                if (CurrentIsInManual && autoUseSprite.ChildrenCount > 0)
                {
                    current.Selected = false;
                    var index = manualUseSprite.IndexOf(current);
                    if (index >= autoUseSprite.ChildrenCount)
                    {
                        index = autoUseSprite.ChildrenCount - 1;
                    }
                    current = autoUseSprite[index] as UseItemComponent;
                    current.Selected = true;
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle) ||
                args.InputInfo.IsPressed(ButtonType.Triangle) ||
                args.InputInfo.IsPressed(ButtonType.Square))
            {
                bool isInManual = CurrentIsInManual;
                current.Parent.RemoveChild(current);

                if (isInManual)
                {
                    autoUseSprite.AddChild(current);
                }
                else
                {
                    manualUseSprite.AddChild(current);
                }

                AdjustPositions();
                ItemUseManager.Manager.ToggleAutoUse(current.ItemType);
                sound.Play(PPDSetting.DefaultSounds[3], -1000);
            }
        }

        private void AdjustPositions()
        {
            int iter = 0;
            foreach (GameComponent gc in manualUseSprite.Children)
            {
                gc.Position = new Vector2(0, iter * 20);
                iter++;
            }

            iter = 0;
            foreach (GameComponent gc in autoUseSprite.Children)
            {
                gc.Position = new Vector2(0, iter * 20);
                iter++;
            }
        }

        protected override void UpdateImpl()
        {
            if (OverFocused)
            {
                back.Position = new SharpDX.Vector2(0, AnimationUtility.GetAnimationValue(back.Position.Y, 0));
                Alpha = AnimationUtility.IncreaseAlpha(Alpha);
            }
            else
            {
                back.Position = new SharpDX.Vector2(0, AnimationUtility.GetAnimationValue(back.Position.Y, 50));
                Alpha = AnimationUtility.DecreaseAlpha(Alpha);
            }
        }

        class UseItemComponent : GameComponent
        {
            PPDFramework.Resource.ResourceManager resourceManager;

            bool selected;
            RectangleComponent rectangle;
            TextureString nameString;

            public UseItemComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ItemType itemType) : base(device)
            {
                this.resourceManager = resourceManager;
                ItemType = itemType;

                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("item", String.Format("{0}.png", itemType)))
                {
                    Position = new Vector2(0, 2)
                });
                this.AddChild(nameString = new TextureString(device, Utility.Language[itemType.ToString()], 16, PPDColors.White)
                {
                    Position = new Vector2(20, 0)
                });
                this.AddChild(rectangle = new RectangleComponent(device, resourceManager, PPDColors.White)
                {
                    Position = new Vector2(20, 0),
                    RectangleHeight = 20,
                    RectangleWidth = nameString.Width,
                    Hidden = true
                });
            }

            public ItemType ItemType
            {
                get;
                set;
            }

            public bool Selected
            {
                get
                {
                    return selected;
                }
                set
                {
                    if (selected != value)
                    {
                        selected = value;
                        nameString.Color = selected ? PPDColors.Black : PPDColors.White;
                        rectangle.Hidden = !selected;
                    }
                }
            }
        }
    }
}
