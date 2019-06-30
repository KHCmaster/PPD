using PPDFramework;
using SharpDX;
using System;

namespace PPDCore
{
    /// <summary>
    /// 上部のゲームUI
    /// </summary>
    class GameInterfaceTop : GameComponent
    {
        PictureObject top;
        PictureObject topdanger;
        PictureObject onpu;
        Score sco;
        TextureString songname;
        TextureString difficulty;

        public bool Danger
        {
            get
            {
                return top.Hidden;
            }
            set
            {
                if (value)
                {
                    top.Hidden = true;
                    topdanger.Hidden = false;
                }
                else
                {
                    top.Hidden = false;
                    topdanger.Hidden = true;
                }
            }
        }

        public int CurrentScore
        {
            set
            {
                sco.CurrentScore = value;
            }
        }

        public GameInterfaceTop(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PPDGameUtility ppdgameutility) : base(device)
        {
            top = new PictureObject(device, resourceManager, Utility.Path.Combine("top.png"));
            topdanger = new PictureObject(device, resourceManager, Utility.Path.Combine("topred.png"))
            {
                Hidden = true
            };
            onpu = new PictureObject(device, resourceManager, Utility.Path.Combine("toponpu.png"));
            songname = new TextureString(device, ppdgameutility.SongInformation.DirectoryName, 20, 270, PPDColors.White);
            sco = new Score(device, resourceManager);
            difficulty = new TextureString(device, String.Format("-{0}-", ppdgameutility.Difficulty.ToString().ToUpper()), 20, true, PPDColors.White);

            this.AddChild(difficulty);
            this.AddChild(songname);
            this.AddChild(onpu);
            this.AddChild(sco);
            this.AddChild(top);
            this.AddChild(topdanger);
            InitializeComponentPosition();
        }

        private void InitializeComponentPosition()
        {
            onpu.Position = new Vector2(4, 4);
            songname.Position = Position = new Vector2(35, 4);
            difficulty.Position = new Vector2(555, 4);
            Position = new Vector2(0, -top.Height);
        }

        public void Retry()
        {
            topdanger.Hidden = true;
            sco.Retry();
            SetDefault();
            InitializeComponentPosition();
        }

        protected override void UpdateImpl()
        {
            this.Position = new Vector2(0, this.Position.Y + 1 > 0 ? 0 : this.Position.Y + 1);
        }
    }
}
