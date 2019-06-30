using PPDFramework;
using PPDFrameworkCore;
using PPDMulti.Model;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDMulti
{
    class GameResultComponent : FocusableGameComponent
    {
        const int ClipY = 80;
        const int ClipHeight = 310;
        const int SpriteY = 100;
        const int ScrollBarHeight = 310;
        const int MaxDisplayCount = 8;

        int scrollIndex;

        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;
        IGameHost gameHost;

        PictureObject back;
        SpriteObject resultSprite;
        RectangleComponent scrollBar;

        public GameResultComponent(PPDDevice device, IGameHost gameHost, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.gameHost = gameHost;
            this.resourceManager = resourceManager;
            this.sound = sound;

            this.AddChild(back = new PictureObject(device, resourceManager, Utility.Path.Combine("dialog_back.png")));
            this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.75f
            });

            back.AddChild(resultSprite = new SpriteObject(device)
            {
                Position = new SharpDX.Vector2(50, SpriteY),
                Clip = new ClipInfo(gameHost)
                {
                    PositionX = 40,
                    PositionY = SpriteY,
                    Width = 750,
                    Height = ClipHeight
                }
            });
            back.AddChild(scrollBar = new RectangleComponent(device, resourceManager, PPDColors.White)
            {
                Position = new Vector2(755, SpriteY),
                RectangleHeight = ScrollBarHeight,
                RectangleWidth = 5
            });
            back.AddChild(new TextureString(device, Utility.Language["Result"], 30, PPDColors.White)
            {
                Position = new SharpDX.Vector2(35, 30)
            });
            back.AddChild(new TextureString(device, Utility.Language["Rank"], 14, PPDColors.White)
            {
                Position = new SharpDX.Vector2(50, 80)
            });
            back.AddChild(new TextureString(device, Utility.Language["Player"], 14, PPDColors.White)
            {
                Position = new SharpDX.Vector2(150, 80)
            });
            back.AddChild(new TextureString(device, Utility.Language["Score"], 14, PPDColors.White)
            {
                Position = new SharpDX.Vector2(270, 80)
            });
            back.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("eva", "cool.png"))
            {
                Position = new SharpDX.Vector2(350, 82),
                Scale = new SharpDX.Vector2(0.5f)
            });
            back.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("eva", "good.png"))
            {
                Position = new SharpDX.Vector2(410, 82),
                Scale = new SharpDX.Vector2(0.5f)
            });
            back.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("eva", "safe.png"))
            {
                Position = new SharpDX.Vector2(470, 82),
                Scale = new SharpDX.Vector2(0.5f)
            });
            back.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("eva", "sad.png"))
            {
                Position = new SharpDX.Vector2(530, 82),
                Scale = new SharpDX.Vector2(0.5f)
            });
            back.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("eva", "worst.png"))
            {
                Position = new SharpDX.Vector2(580, 82),
                Scale = new SharpDX.Vector2(0.5f)
            });
            back.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("eva", "combo.png"))
            {
                Position = new SharpDX.Vector2(650, 82),
                Scale = new SharpDX.Vector2(0.5f)
            });

            Inputed += GameResultComponent_Inputed;
            GotFocused += GameResultComponent_GotFocused;
            Alpha = 0;
        }

        public void ClearResult()
        {
            resultSprite.ClearChildren();
            scrollBar.Position = new Vector2(scrollBar.Position.X, SpriteY);
            scrollBar.RectangleHeight = ScrollBarHeight;
        }

        public void AddResult(UserResult result)
        {
            resultSprite.AddChild(new ResultComponent(device, resourceManager, result, resultSprite.ChildrenCount + 1)
            {
                Position = new SharpDX.Vector2(0, 38 * resultSprite.ChildrenCount)
            });
            scrollBar.RectangleHeight = resultSprite.ChildrenCount <= MaxDisplayCount ? ScrollBarHeight : ScrollBarHeight * MaxDisplayCount / resultSprite.ChildrenCount;
        }

        public void ScrollUp()
        {
            scrollIndex--;
            if (scrollIndex < 0)
            {
                scrollIndex = 0;
            }
        }

        public void ScrollDown()
        {
            scrollIndex++;
            if (scrollIndex >= resultSprite.ChildrenCount - 8)
            {
                scrollIndex = resultSprite.ChildrenCount - 8 < 0 ? 0 : resultSprite.ChildrenCount - 8;
            }
        }

        void GameResultComponent_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            back.Position = new SharpDX.Vector2(0, 50);
            Alpha = 0;
        }

        void GameResultComponent_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                ScrollUp();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                ScrollDown();
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
            resultSprite.Clip = new ClipInfo(gameHost)
            {
                PositionX = 40,
                PositionY = (int)(SpriteY + back.Position.Y),
                Width = 750,
                Height = ClipHeight
            };
            resultSprite.Position = new Vector2(resultSprite.Position.X,
                AnimationUtility.GetAnimationValue(resultSprite.Position.Y, +SpriteY - 38 * scrollIndex));
            scrollBar.Position = new Vector2(
                scrollBar.Position.X,
                AnimationUtility.GetAnimationValue(
                    scrollBar.Position.Y,
                    resultSprite.ChildrenCount <= MaxDisplayCount ?
                    SpriteY :
                    SpriteY + ScrollBarHeight * scrollIndex / resultSprite.ChildrenCount));
        }

        class ResultComponent : BindableGameComponent
        {
            PPDFramework.Resource.ResourceManager resourceManager;
            PictureObject userIcon;

            private string currentUserIconPath = Utility.Path.Combine("noimage_icon.png");

            public ResultComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, UserResult result, int rank) : base(device)
            {
                this.resourceManager = resourceManager;

                this.AddChild(new TextureString(device, ConvertRank(rank), 14, PPDColors.White)
                {
                    Position = new SharpDX.Vector2(0, 8)
                });

                this.AddChild(userIcon = new PictureObject(device, resourceManager, PathObject.Absolute(currentUserIconPath))
                {
                    Position = new SharpDX.Vector2(50, 0)
                });
                this.AddChild(new TextureString(device, result.User.Name, 12, 85, PPDColors.White)
                {
                    Position = new SharpDX.Vector2(90, 8)
                });
                this.AddChild(new NumberPictureObject(device, resourceManager, Utility.Path.Combine("numpoint.png"))
                {
                    Position = new SharpDX.Vector2(240, 4),
                    Alignment = PPDFramework.Alignment.Center,
                    MaxDigit = 7,
                    Value = (uint)result.Result.Score,
                    Scale = new SharpDX.Vector2(0.5f, 1)
                });
                this.AddChild(new NumberPictureObject(device, resourceManager, Utility.Path.Combine("numpoint.png"))
                {
                    Position = new SharpDX.Vector2(317, 4),
                    Alignment = Alignment.Center,
                    MaxDigit = -1,
                    Value = (uint)result.Result.CoolCount,
                    Scale = new SharpDX.Vector2(0.5f, 1)
                });
                this.AddChild(new NumberPictureObject(device, resourceManager, Utility.Path.Combine("numpoint.png"))
                {
                    Position = new SharpDX.Vector2(380, 4),
                    Alignment = Alignment.Center,
                    MaxDigit = -1,
                    Value = (uint)result.Result.GoodCount,
                    Scale = new SharpDX.Vector2(0.5f, 1)
                });
                this.AddChild(new NumberPictureObject(device, resourceManager, Utility.Path.Combine("numpoint.png"))
                {
                    Position = new SharpDX.Vector2(440, 4),
                    Alignment = Alignment.Center,
                    MaxDigit = -1,
                    Value = (uint)result.Result.SafeCount,
                    Scale = new SharpDX.Vector2(0.5f, 1)
                });
                this.AddChild(new NumberPictureObject(device, resourceManager, Utility.Path.Combine("numpoint.png"))
                {
                    Position = new SharpDX.Vector2(497, 4),
                    Alignment = Alignment.Center,
                    MaxDigit = -1,
                    Value = (uint)result.Result.SadCount,
                    Scale = new SharpDX.Vector2(0.5f, 1)
                });
                this.AddChild(new NumberPictureObject(device, resourceManager, Utility.Path.Combine("numpoint.png"))
                {
                    Position = new SharpDX.Vector2(557, 4),
                    Alignment = Alignment.Center,
                    MaxDigit = -1,
                    Value = (uint)result.Result.WorstCount,
                    Scale = new SharpDX.Vector2(0.5f, 1)
                });
                this.AddChild(new NumberPictureObject(device, resourceManager, Utility.Path.Combine("numpoint.png"))
                {
                    Position = new SharpDX.Vector2(645, 4),
                    Alignment = Alignment.Center,
                    MaxDigit = -1,
                    Value = (uint)result.Result.MaxCombo,
                    Scale = new SharpDX.Vector2(0.5f, 1)
                });

                AddBinding(new Binding(result.User, "ImagePath", this, "UserImagePath"));
                ChangeUserIconScale();
            }

            private string ConvertRank(int rank)
            {
                switch (rank)
                {
                    case 1:
                        return "1st";
                    case 2:
                        return "2nd";
                    case 3:
                        return "3rd";
                }
                return String.Format("{0}th", rank);
            }

            public string UserImagePath
            {
                get
                {
                    return currentUserIconPath;
                }
                set
                {
                    if (currentUserIconPath != value)
                    {
                        currentUserIconPath = value;
                        this.RemoveChild(userIcon);
                        userIcon = new PictureObject(device, resourceManager, PathObject.Absolute(currentUserIconPath))
                        {
                            Position = new SharpDX.Vector2(50, 0)
                        };
                        this.InsertChild(userIcon, 0);
                        ChangeUserIconScale();
                    }
                }
            }

            private void ChangeUserIconScale()
            {
                userIcon.Scale = new SharpDX.Vector2(32 / userIcon.Width, 32 / userIcon.Height);
            }
        }
    }
}
