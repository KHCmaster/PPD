using PPDFramework;
using PPDFrameworkCore;
using System;

namespace PPDSingle
{
    class PlayUserIcon : BindableGameComponent
    {
        PPDFramework.Resource.ResourceManager resourceManager;

        PictureObject userIcon;
        TextureString userNameString;
        NumberPictureObject scorePicture;
        UserPlayState userPlayState;
        MarkEvaluateType evaluate;
        bool isMissPress;
        LifeGage lifeGage;
        PictureObject[] evals;

        int trueScore;
        int score;
        int drawCount;
        bool showScore;
        bool showEvaluate;
        bool showLife;

        private string currentUserIconPath = Utility.Path.Combine("noimage_icon.png");

        public PlayUserIcon(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, UserPlayState userPlayState,
            bool showScore, bool showEvaluate, bool showLife) : base(device)
        {
            this.resourceManager = resourceManager;
            this.userPlayState = userPlayState;
            this.showScore = showScore;
            this.showEvaluate = showEvaluate;
            this.showLife = showLife;

            this.AddChild((userIcon = new PictureObject(device, resourceManager, PathObject.Absolute(currentUserIconPath), true)
            {
                Position = new SharpDX.Vector2(19, 19)
            }));
            this.AddChild((userNameString = new TextureString(device, "", 9, 75, PPDColors.White)
            {
                Position = new SharpDX.Vector2(35, 3)
            }));
            this.AddChild(scorePicture = new NumberPictureObject(device, resourceManager, Utility.Path.Combine("play_user_score.png"))
            {
                Position = new SharpDX.Vector2(35, 24),
                Alignment = Alignment.Left,
                MaxDigit = 7,
                Hidden = !showScore
            });

            var filenames = new string[] {
                 "cool.png",
                 "good.png",
                 "safe.png",
                 "sad.png",
                 "worst.png",
                 "misscool.png",
                 "missgood.png",
                 "misssafe.png",
                 "misssad.png"
            };
            evals = new PictureObject[filenames.Length];
            for (int i = 0; i < filenames.Length; i++)
            {
                evals[i] = new PictureObject(device, resourceManager, Utility.Path.Combine("eva", filenames[i]))
                {
                    Position = new SharpDX.Vector2(35, 14),
                    Hidden = true,
                    Scale = new SharpDX.Vector2(0.4f, 0.4f)
                };
                this.AddChild(evals[i]);
            }
            this.AddChild(lifeGage = new LifeGage(device, resourceManager)
            {
                Position = new SharpDX.Vector2(37, 32),
                Hidden = !showLife
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
            AddBinding(new Binding(userPlayState, "IsMissPress", this, "IsMissPress"));
            AddBinding(new Binding(userPlayState, "LifeAsFloat", this, "LifeAsFloat"));

            ChangeUserIconScale();
        }

        public void Retry()
        {
            score = 0;
            trueScore = 0;
        }

        public UserPlayState UserPlayState
        {
            get
            {
                return userPlayState;
            }
        }

        public MarkEvaluateType Evaluate
        {
            get { return evaluate; }
            set
            {
                evaluate = value;
                UpdateEvaluate();
            }
        }

        public bool IsMissPress
        {
            get { return isMissPress; }
            set
            {
                isMissPress = value;
                UpdateEvaluate();
            }
        }

        public void UpdateEvaluate()
        {
            drawCount = 0;
            foreach (GameComponent gc in evals)
            {
                gc.Hidden = true;
            }

            if (showEvaluate && Evaluate >= 0)
            {
                var index = (int)(Evaluate + (IsMissPress ? 5 : 0));
                if (index >= evals.Length)
                {
                    index = (int)MarkEvaluateType.Worst;
                }
                evals[index].Hidden = false;
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
                        Position = new SharpDX.Vector2(19, 19)
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
