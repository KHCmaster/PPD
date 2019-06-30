using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPD
{
    class GameListPanel : HomePanelBase
    {
        public event EventHandler<GameEventArgs> GameStarted;
        GameList gameList;
        SpriteObject listSprite;
        LineRectangleComponent select;
        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;

        int currentIndex;
        int scrollIndex;
        public GameListPanel(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.resourceManager = resourceManager;
            this.sound = sound;
        }

        public override void Load()
        {
            OnLoadProgressed(0);
            gameList = new GameList();
            HandleOverFocusInput = true;

            listSprite = new SpriteObject(device);
            this.AddChild((select = new LineRectangleComponent(device, resourceManager, PPDColors.Selection) { RectangleWidth = 800, RectangleHeight = 90 }));
            OnLoadProgressed(20);
            this.AddChild(listSprite);
            OnLoadProgressed(30);

            int iter = 0;
            foreach (GameLoader gl in gameList.List)
            {
                Add(gl);
                iter++;
                OnLoadProgressed(30 + 60 * iter / gameList.List.Length);
            }

            Inputed += GameListPanel_Inputed;
            OnLoadProgressed(100);
        }

        private void Add(GameLoader gl)
        {
            var component = new GameInfoComponent(device, resourceManager, gl)
            {
                PlayingTime = String.Format("{0:F1}{1}", ConvertSecond(GetPlayingSecond(gl)), Utility.Language["Hour"]),
                Position = new SharpDX.Vector2(0, listSprite.ChildrenCount * 90)
            };
            listSprite.AddChild(component);
        }

        private string ConvertSecond(int second)
        {
            return String.Format("{0:F1}", (float)second / 3600);
        }

        public int GetPlayingSecond(GameLoader gameLoader)
        {
            string lastValue = PPDGeneralSetting.Setting[gameLoader.GetFullName()];
            if (!int.TryParse(lastValue, out int value))
            {
                value = 0;
            }
            return value;
        }

        public void UpdatePlayingTime(GameLoader gameLoader, int time)
        {
            foreach (GameInfoComponent gameInfoComponent in listSprite.Children)
            {
                if (gameInfoComponent.GameLoader == gameLoader)
                {
                    gameInfoComponent.PlayingTime = String.Format("{0:F1}{1}", ConvertSecond(time), Utility.Language["Hour"]);
                    break;
                }
            }
        }

        void GameListPanel_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (GameStarted != null)
                {
                    sound.Play(PPDSetting.DefaultSounds[1], -1000);
                    GameStarted.Invoke(this, new GameEventArgs(gameList.List[currentIndex]));
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                currentIndex++;
                if (currentIndex >= gameList.List.Length)
                {
                    currentIndex = 0;
                }
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                currentIndex--;
                if (currentIndex < 0)
                {
                    currentIndex = gameList.List.Length - 1;
                }
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            scrollIndex = currentIndex - scrollIndex >= 5 ? scrollIndex + 1 : scrollIndex;
            scrollIndex = scrollIndex > currentIndex ? currentIndex : scrollIndex;
        }

        protected override void UpdateImpl()
        {
            select.Position = new SharpDX.Vector2(0, AnimationUtility.GetAnimationValue(select.Position.Y, 90 * currentIndex, 0.2f));
            this.Position = new SharpDX.Vector2(0, AnimationUtility.GetAnimationValue(this.Position.Y, -90 * scrollIndex));
        }

        class GameInfoComponent : GameComponent
        {
            TextureString playingTimeText;
            public GameInfoComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, GameLoader gameLoader) : base(device)
            {
                this.GameLoader = gameLoader;

                var gameInfo = gameLoader.GetGameInformation();
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("gameicon", gameInfo.GameIconPath))
                {
                    Position = new Vector2(13, 13)
                });
                this.AddChild(new TextureString(device, gameInfo.GameName, 20, PPDColors.White)
                {
                    Position = new Vector2(102, 5)
                });
                this.AddChild(new TextureString(device, gameInfo.GameDescription, 14, 500, 60, true, PPDColors.White)
                {
                    Position = new Vector2(102, 32)
                });
                this.AddChild(new TextureString(device, Utility.Language["PlayingTime"], 12, PPDColors.White)
                {
                    Position = new Vector2(630, 8)
                });
                playingTimeText = new TextureString(device, "", 12, PPDColors.White)
                {
                    Position = new Vector2(795, 8),
                    Alignment = Alignment.Right
                };
                this.AddChild(playingTimeText);
                this.AddChild(new TextureString(device, Utility.Language["Version"], 12, PPDColors.White)
                {
                    Position = new Vector2(630, 25)
                });
                var vertionText = new TextureString(device, gameLoader.Version, 12, PPDColors.White)
                {
                    Position = new Vector2(795, 25),
                    Alignment = Alignment.Right
                };
                this.AddChild(vertionText);
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("gamelist_back.png")));
            }

            public string PlayingTime
            {
                set
                {
                    playingTimeText.Text = value;
                }
            }

            public GameLoader GameLoader
            {
                get;
                private set;
            }
        }
    }

    class GameEventArgs : EventArgs
    {
        public GameEventArgs(GameLoader gl)
        {
            GameLoader = gl;
        }

        public GameLoader GameLoader
        {
            get;
            private set;
        }
    }
}
