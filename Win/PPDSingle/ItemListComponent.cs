using PPDFramework;
using PPDFramework.Web;
using PPDFrameworkCore;
using PPDShareComponent;
using SharpDX;
using System;
using System.Linq;

namespace PPDSingle
{
    class ItemListComponent : FocusableGameComponent
    {
        enum DialogState
        {
            None,
            WaitIncludeFine,
            WaitUseNotIncludeButton,
            WaitAllowWarnScript,
        }

        const int ClipY = 80;
        const int ClipHeight = 330;
        const int SpriteY = 90;
        const int ItemHeight = 70;
        IGameHost gameHost;
        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;

        SpriteObject mainSprite;
        SpriteObject waitSprite;
        bool loading;
        ItemInfo[] items;
        LineRectangleComponent rectangle;
        int selection;
        StackObject stackObject;
        DialogState dialogState = DialogState.None;

        public ItemInfo UseItem
        {
            get;
            private set;
        }

        private ItemsComponent SelectedComponent
        {
            get
            {
                return (ItemsComponent)mainSprite.GetChildAt(selection);
            }
        }

        public ItemListComponent(PPDDevice device, IGameHost gameHost, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.gameHost = gameHost;
            this.resourceManager = resourceManager;
            this.sound = sound;
            PictureObject back;

            waitSprite = new SpriteObject(device)
            {
                Hidden = true
            };
            this.AddChild(waitSprite);
            waitSprite.AddChild(new TextureString(device, Utility.Language["LoadingItems"], 20, true, PPDColors.White)
            {
                Position = new Vector2(400, 220)
            });
            waitSprite.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.65f
            });

            this.AddChild(stackObject = new StackObject(device,
                new PictureObject(device, resourceManager, Utility.Path.Combine("delta.png")),
                new TextureString(device, String.Format(":{0}", Utility.Language["CancelUsingItem"]), 15, PPDColors.White)
                )
            {
                IsHorizontal = true
            });
            stackObject.Update();
            stackObject.Position = new Vector2(760 - stackObject.Width, 50);
            rectangle = new LineRectangleComponent(device, resourceManager, PPDColors.Selection)
            {
                Hidden = true,
                RectangleWidth = 700,
                RectangleHeight = ItemHeight
            };
            this.AddChild(rectangle);
            mainSprite = new SpriteObject(device)
            {
                Position = new Vector2(50, SpriteY),
                Clip = new ClipInfo(gameHost)
                {
                    PositionX = 40,
                    PositionY = ClipY,
                    Width = 750,
                    Height = ClipHeight
                }
            };
            this.AddChild(mainSprite);
            this.AddChild(back = new PictureObject(device, resourceManager, Utility.Path.Combine("dialog_back.png")));
            this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.80f
            });
            back.AddChild(new TextureString(device, Utility.Language["ItemList"], 30, PPDColors.White)
            {
                Position = new Vector2(35, 30)
            });

            Inputed += ItemComponent_Inputed;
            GotFocused += ItemComponent_GotFocused;
        }

        void ItemComponent_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (args.FocusObject is LeftMenu)
            {
                Load();
            }
        }

        void ItemComponent_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (!waitSprite.Hidden)
            {
                return;
            }

            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (mainSprite.ChildrenCount > 0 && SelectedComponent.Items.First().IsAvailable)
                {
                    sound.Play(PPDSetting.DefaultSounds[1], -1000);
                    GeneralDialogBase dialog;
                    if (SelectedComponent.ItemType == ItemType.AutoFreePass)
                    {
                        dialog = new SuperAutoDialog(device, resourceManager, sound,
                            String.Format(Utility.Language["UseItemConfirm"],
                            Utility.Language[String.Format("Item{0}Name", ((ItemsComponent)mainSprite.GetChildAt(selection)).Items[0].ItemType)]));
                    }
                    else
                    {
                        dialog = new GeneralDialog(device, resourceManager, sound,
                            String.Format(Utility.Language["UseItemConfirm"],
                            Utility.Language[String.Format("Item{0}Name", ((ItemsComponent)mainSprite.GetChildAt(selection)).Items[0].ItemType)]), GeneralDialog.ButtonTypes.OkCancel);
                    }
                    FocusManager.Focus(dialog);
                    this.InsertChild(dialog, 0);
                    dialog.LostFocused += drd_LostFocused;
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Triangle))
            {
                if (UseItem != null && !UseItem.IsUsed)
                {
                    sound.Play(PPDSetting.DefaultSounds[1], -1000);
                    var gd = new GeneralDialog(device, resourceManager, sound,
                           String.Format(Utility.Language["CancelItemConfirm"],
                           Utility.Language[String.Format("Item{0}Name", UseItem.ItemType)]), GeneralDialog.ButtonTypes.OkCancel);
                    FocusManager.Focus(gd);
                    this.InsertChild(gd, 0);
                    gd.LostFocused += gd_LostFocused;
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                if (mainSprite.ChildrenCount > 0)
                {
                    selection++;
                    if (selection >= mainSprite.ChildrenCount)
                    {
                        selection = 0;
                    }
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                if (mainSprite.ChildrenCount > 0)
                {
                    selection--;
                    if (selection < 0)
                    {
                        selection = mainSprite.ChildrenCount - 1;
                    }
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
            }
        }

        void gd_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            var drd = sender as GeneralDialog;
            if (drd.OK)
            {
                UseItem = null;
                stackObject.Hidden = true;
            }
            this.RemoveChild(drd);
            drd.Dispose();
        }

        void drd_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            var baseDialog = sender as GeneralDialogBase;
            ItemType itemType = ItemType.None;
            if (baseDialog is GeneralDialog && ((GeneralDialog)baseDialog).OK)
            {
                UseItem = ((ItemsComponent)mainSprite.GetChildAt(selection)).Items[0];
                stackObject.Hidden = false;
                itemType = UseItem.ItemType;
            }
            else if (baseDialog is SuperAutoDialog && ((SuperAutoDialog)baseDialog).OK)
            {
                UseItem = ((ItemsComponent)mainSprite.GetChildAt(selection)).Items[0];
                itemType = (ItemType)(((SuperAutoDialog)baseDialog).Result + 1);
                UseItem["SubItemType"] = itemType;
                stackObject.Hidden = false;
            }
            if (itemType == ItemType.Auto3 || itemType == ItemType.Auto4)
            {
                dialogState = DialogState.WaitIncludeFine;
            }
            this.RemoveChild(baseDialog);
            baseDialog.Dispose();
        }

        void dialog_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            var dialog = sender as GeneralDialog;
            if (dialog.OK)
            {
                UseItem["AllowAllButton"] = true;
            }
            this.RemoveChild(dialog);
            dialog.Dispose();
        }

        private void Load()
        {
            if (loading)
            {
                return;
            }
            loading = true;
            waitSprite.Hidden = false;
            ThreadManager.Instance.GetThread(() =>
            {
                items = WebManager.Instance.GetItems();
                loading = false;
            }).Start();
        }

        private void Generate()
        {
            mainSprite.ClearChildren();
            var groups = items.GroupBy(i => i.ItemType).OrderBy(g => g.Key);
            int iter = 0;
            foreach (var g in groups)
            {
                var control = new ItemsComponent(device, resourceManager, g.ToArray())
                {
                    Position = new Vector2(0, iter * ItemHeight)
                };
                mainSprite.AddChild(control);
                iter++;
            }
            rectangle.Hidden = !(mainSprite.ChildrenCount > 0);
            rectangle.Position = mainSprite.Position;
            selection = 0;

            if (UseItem != null && UseItem.IsUsed)
            {
                UseItem = null;
            }
            stackObject.Hidden = UseItem == null;
        }

        private void UpdateScroll()
        {
            var temp = SelectedComponent.ScreenPos;
            var target = 0f;
            if (temp.Y < ClipY && temp.Y + ItemHeight < ClipHeight)
            {
                target = ClipY - temp.Y;
            }
            else if (temp.Y + ItemHeight + 10 > ClipY + ClipHeight)
            {
                target = ClipY + ClipHeight - (temp.Y + ItemHeight + 10);
            }
            mainSprite.Position = new Vector2(mainSprite.Position.X,
                AnimationUtility.GetAnimationValue(mainSprite.Position.Y, mainSprite.Position.Y + target));
        }

        protected override void UpdateImpl()
        {
            if (!loading)
            {
                if (!waitSprite.Hidden)
                {
                    Generate();
                    waitSprite.Hidden = true;
                }
            }
            if (!rectangle.Hidden)
            {
                UpdateScroll();
                rectangle.Position = AnimationUtility.GetAnimationPosition(rectangle.Position, mainSprite.Position + new Vector2(0, ItemHeight * selection));
            }
            if (dialogState != DialogState.None)
            {
                switch (dialogState)
                {
                    case DialogState.WaitIncludeFine:
                        var dialog = new GeneralDialog(device, resourceManager, sound, Utility.Language["IncludeFineConfirm"], GeneralDialog.ButtonTypes.YesNo);
                        FocusManager.Focus(dialog);
                        this.InsertChild(dialog, 0);
                        dialog.LostFocused += (sender, e) =>
                        {
                            if (dialog.OK)
                            {
                                UseItem["IncludeFine"] = true;
                            }
                            this.RemoveChild(dialog);
                            dialog.Dispose();
                            if (UseItem.ItemType == ItemType.Auto4 || (UseItem.ContainsParameter("SubItemType") && (ItemType)UseItem["SubItemType"] == ItemType.Auto4))
                            {
                                dialogState = DialogState.WaitUseNotIncludeButton;
                            }
                        };
                        break;
                    case DialogState.WaitUseNotIncludeButton:
                        dialog = new GeneralDialog(device, resourceManager, sound, Utility.Language["DumpNotesConfirm"], GeneralDialog.ButtonTypes.YesNo);
                        FocusManager.Focus(dialog);
                        this.InsertChild(dialog, 0);
                        dialog.LostFocused += (sender, e) =>
                        {
                            if (dialog.OK)
                            {
                                UseItem["AllowAllButton"] = true;
                            }
                            this.RemoveChild(dialog);
                            dialog.Dispose();
                            if (UseItem.ItemType == ItemType.Auto4 || (UseItem.ContainsParameter("SubItemType") && (ItemType)UseItem["SubItemType"] == ItemType.Auto4))
                            {
                                dialogState = DialogState.WaitAllowWarnScript;
                            }
                        };
                        break;
                    case DialogState.WaitAllowWarnScript:
                        dialog = new GeneralDialog(device, resourceManager, sound, Utility.Language["AllowWarnScriptConfirm"], GeneralDialog.ButtonTypes.YesNo);
                        FocusManager.Focus(dialog);
                        this.InsertChild(dialog, 0);
                        dialog.LostFocused += (sender, e) =>
                        {
                            if (dialog.OK)
                            {
                                UseItem["AllowWarnScript"] = true;
                            }
                            this.RemoveChild(dialog);
                            dialog.Dispose();
                        };
                        break;
                }

                dialogState = DialogState.None;
            }
        }

        protected override bool OnCanUpdate()
        {
            return !Hidden && OverFocused;
        }

        protected override bool OnCanDraw(PPDFramework.Shaders.AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return OverFocused;
        }

        class ItemsComponent : GameComponent
        {
            private PPDFramework.Resource.ResourceManager resourceManager;

            public ItemInfo[] Items
            {
                get;
                private set;
            }

            public ItemType ItemType
            {
                get
                {
                    return Items[0].ItemType;
                }
            }

            public ItemsComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ItemInfo[] items) : base(device)
            {
                this.resourceManager = resourceManager;
                Items = items;

                var first = items.First();
                var color = first.IsAvailable ? PPDColors.White : PPDColors.Gray;
                AddChild(new TextureString(device, Utility.Language[String.Format("Item{0}Name", first.ItemType)], 20, color));
                AddChild(new TextureString(device, String.Format("x{0}", items.Length), 20, color)
                {
                    Position = new Vector2(650, 0)
                });
                AddChild(new TextureString(device, Utility.Language[String.Format("Item{0}Desc", first.ItemType)], 15, 650, 40, true, color)
                {
                    Position = new Vector2(10, 24)
                });
            }
        }
    }
}
