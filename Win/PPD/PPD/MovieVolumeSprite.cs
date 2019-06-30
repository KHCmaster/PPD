using PPDFramework;
using SharpDX;
using System;

namespace PPD
{
    class MovieVolumeSprite : GameComponent
    {
        private const int RectWidth = 8;
        private const int RectMargin = 6;
        private const int RectHeight = 35;

        MyGame myGame;
        PPDFramework.Resource.ResourceManager resourceManager;
        SongInformation lastSongInformation;

        TextureString volumeText;
        RectangleComponent[] rects;

        int timerId = -1;
        int showCount;

        public MovieVolumeSprite(PPDDevice device, MyGame myGame) : base(device)
        {
            this.myGame = myGame;
            resourceManager = new PPDFramework.Resource.ResourceManager();

            var color = PPDColors.Green;
            this.AddChild(new TextureString(device, Utility.Language["Video"], 30, color)
            {
                Position = new Vector2(800, 0),
                Alignment = Alignment.Right
            });
            this.AddChild(volumeText = new TextureString(device, "100", 30, color)
            {
                Position = new Vector2(70, 400),
                Alignment = Alignment.Right
            });
            rects = new RectangleComponent[50];
            for (int i = 0; i < rects.Length; i++)
            {
                rects[i] = new RectangleComponent(device, resourceManager, color)
                {
                    Position = new Vector2(80 + i * (RectWidth + RectMargin), 400),
                    RectangleHeight = RectHeight,
                    RectangleWidth = RectWidth
                };
                this.AddChild(rects[i]);
            }
            Hidden = true;
        }

        private void UpdateVolumeDisplay(int volume)
        {
            for (int i = 0; i < rects.Length; i++)
            {
                bool isOver = false;
                switch (volume)
                {
                    case 0:
                        isOver = false;
                        break;
                    case 100:
                        isOver = true;
                        break;
                    default:
                        isOver = (i + 1) * 2 <= volume;
                        break;
                }
                rects[i].Position = new Vector2(80 + i * (RectWidth + RectMargin), 400 + (isOver ? 0 : RectHeight / 4));
                rects[i].RectangleHeight = isOver ? RectHeight : RectHeight / 2;
            }
            volumeText.Text = String.Format("{0:D}", volume);
        }

        public void Update(MouseInfo mouseInfo)
        {
            if (myGame.CurrentSong != null && lastSongInformation != myGame.CurrentSong)
            {
                lastSongInformation = myGame.CurrentSong;
                if (lastSongInformation != null)
                {
                    UpdateVolumeDisplay(lastSongInformation.UserVolume);
                }
                timerId = -1;
            }
            foreach (var e in mouseInfo.Events)
            {
                if (e.EventType == MouseEvent.MouseEventType.Wheel)
                {
                    if (Hidden)
                    {
                        Hidden = false;
                        showCount = 0;
                    }
                    else if (lastSongInformation != null && myGame.CurrentPlayerBase != null &&
                        myGame.CurrentPlayerBase.Initialized)
                    {
                        lastSongInformation.UserVolume += Math.Sign(e.WheelValue);
                        var targetSongInformation = lastSongInformation;
                        myGame.RemoveTimerCallBack(timerId);
                        timerId = myGame.AddTimerCallBack((index) =>
                        {
                            targetSongInformation.SaveUserVolume();
                        }, 1000, true, false);
                        myGame.CurrentPlayerBase.UserVolume = lastSongInformation.UserVolume;
                        UpdateVolumeDisplay(lastSongInformation.UserVolume);
                        showCount = 0;
                    }
                }
            }
            if (!Hidden)
            {
                showCount++;
                Hidden |= showCount >= 60 * 5;
            }
            Update();
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            resourceManager.Dispose();
        }
    }
}
