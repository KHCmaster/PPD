using PPDFramework;
using PPDFrameworkCore;
using PPDMultiCommon.Data;
using PPDMultiCommon.Model;
using PPDShareComponent;
using SharpDX;
using System;
using System.Collections.Generic;

namespace PPDMulti
{
    class PlayUserIcon : BindableGameComponent
    {
        PPDFramework.Resource.ResourceManager resourceManager;

        PictureObject userIcon;
        TextureString userNameString;
        NumberPictureObject scorePicture;
        UserPlayState userPlayState;
        LifeGage lifeGage;
        PictureObject[] evals;
        ItemManagerComponent itemManager;
        Dictionary<ItemEffect, PictureObject> itemPictureDict;
        SpriteObject itemSprite;

        int trueScore;
        int score;
        int drawCount;

        private string currentUserIconPath = Utility.Path.Combine("noimage_icon.png");

        public PlayUserIcon(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, UserPlayState userPlayState) : base(device)
        {
            this.resourceManager = resourceManager;
            this.userPlayState = userPlayState;

            InnerStruct();
        }

        public PlayUserIcon(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, UserPlayState userPlayState, ItemManagerComponent itemManager) : base(device)
        {
            this.resourceManager = resourceManager;
            this.userPlayState = userPlayState;
            this.itemManager = itemManager;

            itemManager.EffectAdded += itemManager_EffectAdded;
            itemManager.EffectRemoved += itemManager_EffectRemoved;

            InnerStruct();
        }

        private void InnerStruct()
        {
            Hidden = true;
            itemPictureDict = new Dictionary<ItemEffect, PictureObject>();

            this.AddChild(itemSprite = new SpriteObject(device)
            {
                Position = new SharpDX.Vector2(92, 1)
            });
            this.AddChild((userIcon = new PictureObject(device, resourceManager, PathObject.Absolute(currentUserIconPath), true)
            {
                Position = new Vector2(19, 19)
            }));
            this.AddChild((userNameString = new TextureString(device, "", 9, 75, PPDColors.White)
            {
                Position = new Vector2(35, 3)
            }));
            this.AddChild(scorePicture = new NumberPictureObject(device, resourceManager, Utility.Path.Combine("play_user_score.png"))
            {
                Position = new Vector2(35, 24),
                Alignment = Alignment.Left,
                MaxDigit = 7
            });

            var filenames = new string[] {
              "cool.png",
              "good.png",
              "safe.png",
              "sad.png",
              "worst.png",
              "misscool.png",
              "missgood.png"
            };
            evals = new PictureObject[filenames.Length];
            for (int i = 0; i < filenames.Length; i++)
            {
                evals[i] = new PictureObject(device, resourceManager, Utility.Path.Combine("eva", filenames[i]))
                {
                    Position = new Vector2(35, 14),
                    Hidden = true,
                    Scale = new SharpDX.Vector2(0.4f, 0.4f)
                };
                this.AddChild(evals[i]);
            }
            this.AddChild(lifeGage = new LifeGage(device, resourceManager)
            {
                Position = new SharpDX.Vector2(37, 32)
            });

            if (userPlayState.User.IsSelf)
            {
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("play_user_back_self.png")));
            }
            else
            {
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("play_user_back.png")));
            }

            AddBinding(new Binding(userPlayState.User, "Name", this, "UserName"));
            AddBinding(new Binding(userPlayState.User, "ImagePath", this, "UserImagePath"));
            AddBinding(new Binding(userPlayState, "Score", this, "Score"));
            AddBinding(new Binding(userPlayState, "Evaluate", this, "Evaluate"));
            AddBinding(new Binding(userPlayState, "LifeAsFloat", this, "LifeAsFloat"));
            AddBinding(new Binding(userPlayState, "IsStealth", this, "IsStealth"));
            AddBinding(new Binding(userPlayState, "Loaded", this, "Loaded"));

            ChangeUserIconScale();
        }

        void itemManager_EffectRemoved(ItemEffect obj)
        {
            if (itemPictureDict.TryGetValue(obj, out PictureObject pic))
            {
                pic.Parent.RemoveChild(pic);
            }

            ChangeItemPosition();
        }

        void itemManager_EffectAdded(ItemEffect obj)
        {
            var pic = new PictureObject(device, resourceManager, Utility.Path.Combine("item", String.Format("{0}.png", obj.ItemType)))
            {
                Scale = new SharpDX.Vector2(0.75f)
            };
            itemSprite.AddChild(pic);
            itemPictureDict.Add(obj, pic);
            ChangeItemPosition();
        }

        private void ChangeItemPosition()
        {
            int iter = 0;
            foreach (PictureObject pic in itemSprite.Children)
            {
                pic.Hidden = iter >= 6;
                pic.Position = new SharpDX.Vector2(iter % 2 == 0 ? 0 : 12, 12 * (iter / 2));
                iter++;
            }
        }

        public bool Loaded
        {
            set
            {
                Hidden = !value;
            }
        }

        public UserPlayState UserPlayState
        {
            get
            {
                return userPlayState;
            }
        }

        public MarkEvaluateTypeEx Evaluate
        {
            set
            {
                drawCount = 0;
                foreach (GameComponent gc in evals)
                {
                    gc.Hidden = true;
                }

#pragma warning disable RECS0093 // Convert 'if' to '&&' expression
                if (value >= 0 && value <= MarkEvaluateTypeEx.MissGood)
#pragma warning restore RECS0093 // Convert 'if' to '&&' expression
                {
                    evals[(int)value].Hidden = false;
                }
            }
        }

        public int Score
        {
            set
            {
                trueScore = value;
            }
        }

        public int DisplayScore
        {
            get
            {
                return score;
            }
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
                    userIcon = new PictureObject(device, resourceManager, PathObject.Absolute(currentUserIconPath), true)
                    {
                        Position = new Vector2(19, 19)
                    };
                    this.InsertChild(userIcon, 0);
                    ChangeUserIconScale();
                }
            }
        }

        public string UserName
        {
            get
            {
                return userNameString.Text;
            }
            set
            {
                userNameString.Text = value;
            }
        }

        public float LifeAsFloat
        {
            set
            {
                lifeGage.Percent = value;
            }
        }

        private void ChangeUserIconScale()
        {
            userIcon.Scale = new SharpDX.Vector2(32 / userIcon.Width, 32 / userIcon.Height);
        }

        protected override void UpdateImpl()
        {
            drawCount++;

            if (drawCount >= 60)
            {
                foreach (GameComponent gc in evals)
                {
                    gc.Hidden = true;
                }
            }

            if (score < trueScore)
            {
                if (trueScore - score < 99)
                {
                    score = trueScore;
                }
                else
                {
                    score += Math.Max((trueScore - score) / 60, 99);
                }
            }
            if (scorePicture.Value != score)
            {
                scorePicture.Value = (uint)score;
            }

            this.Alpha = userPlayState.IsStealth ? AnimationUtility.DecreaseAlpha(this.Alpha) : AnimationUtility.IncreaseAlpha(this.Alpha);
        }

        class LifeGage : GameComponent
        {
            const int MaxWidth = 76;
            RectangleComponent rectangle;

            float gageWidth;

            public LifeGage(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
            {
                this.AddChild(rectangle = new RectangleComponent(device, resourceManager, PPDColors.LifeGage)
                {
                    RectangleHeight = 3,
                    RectangleWidth = MaxWidth
                });

                gageWidth = MaxWidth;
            }

            public float Percent
            {
                set
                {
                    gageWidth = MaxWidth * value / 100;
                }
            }

            protected override void UpdateImpl()
            {
                rectangle.RectangleWidth = gageWidth;
            }
        }
    }
}
