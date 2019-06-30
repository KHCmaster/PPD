using PPDFramework;
using SharpDX;

namespace PPDCore
{
    /// <summary>
    /// 下のメインゲームUI
    /// </summary>
    class GameInterfaceBottom : GameComponent
    {
        PictureObject bottom;
        PictureObject bottomdanger;
        LifeGage lg;
        TemporaryEvaluate te;
        TextureString kasi;
        bool _danger;

        public bool Danger
        {
            get
            {
                return _danger;
            }
            set
            {
                _danger = value;
                if (_danger)
                {
                    bottom.Hidden = true;
                    bottomdanger.Hidden = false;
                }
                else
                {
                    bottom.Hidden = false;
                    bottomdanger.Hidden = true;
                }
            }
        }

        public float CurrentLife
        {
            set
            {
                lg.CurrentLife = value;
            }
        }

        public float MoviePosDrawX
        {
            set
            {
                te.MoviePosDrawX = value;
            }
        }

        public string Kasi
        {
            set
            {
                kasi.Text = value;
            }
        }

        public GameInterfaceBottom(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PPDGameUtility gameUtility) : base(device)
        {
            bottom = new PictureObject(device, resourceManager, Utility.Path.Combine("bottom.png"));
            bottomdanger = new PictureObject(device, resourceManager, Utility.Path.Combine("bottomred.png"))
            {
                Hidden = true
            };

            kasi = new TextureString(device, "", 20, true, PPDColors.White);

            this.AddChild(kasi);

            if (gameUtility.AutoMode != AutoMode.None)
            {
                this.AddChild(new TextureString(device, "-AUTO-", 20, PPDColors.White)
                {
                    Position = new Vector2(710, 16)
                });
            }

            lg = new LifeGage(device, resourceManager);
            te = new TemporaryEvaluate(device, resourceManager);

            this.AddChild(lg);
            this.AddChild(te);
            this.AddChild(bottomdanger);
            this.AddChild(bottom);
            InitializeComponentPosition();
        }

        private void InitializeComponentPosition()
        {
            kasi.Position = new Vector2(400, 10);
            this.Position = new Vector2(0, 450);
        }

        public void Retry()
        {
            kasi.Text = "";
            lg.Retry();
            te.Retry();
            SetDefault();
            InitializeComponentPosition();
        }

        /// <summary>
        /// 音符作成(ライフゲージ)
        /// </summary>
        /// <param name="num"></param>
        public void CreateOnpu(int num)
        {
            lg.CreateOnpu(num);
        }

        /// <summary>
        /// 現状の評価の変更
        /// </summary>
        /// <param name="currentEvaluateRatio"></param>
        /// <param name="currentResultType"></param>
        public void ChangeTempEvaluate(float currentEvaluateRatio, ResultEvaluateType currentResultType)
        {
            te.Change(currentResultType, currentEvaluateRatio);
        }

        protected override void UpdateImpl()
        {
            this.Position = new Vector2(0, Position.Y + bottom.Height - 2 >= 450 ? Position.Y - 1 : Position.Y);
        }
    }
}
