using PPDFramework;
using PPDFrameworkCore;
using PPDShareComponent;
using System.Numerics;
using PPDMath;
using System;
using System.Collections.Generic;

namespace PPD
{
    class NotifyControl(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, GameTimer gameTimer) : GameComponent(device)
    {
        private const int startY = 20;
        private const int itemHeight = 25;

        private readonly PPDFramework.Resource.ResourceManager resourceManager = resourceManager;
        private readonly GameTimer gameTimer = gameTimer;

        private readonly Queue<string> textQueue = new();

        public void AddNotify(string text)
        {
            textQueue.Enqueue(text);
        }

        protected override void UpdateImpl()
        {
            while (textQueue.Count > 0)
            {
                this.AddChild(new NotifyComponent(device, resourceManager, gameTimer, textQueue.Dequeue()));
            }
            for (int i = 0; i < ChildrenCount; i++)
            {
                var gc = GetChildAt(i);
                gc.Position = new Vector2(gc.Position.X, AnimationUtility.GetAnimationValue(gc.Position.Y, startY + (ChildrenCount - 1 - i) * itemHeight));
            }
        }

        class NotifyComponent : GameComponent
        {
            private const int ShowMS = 5000;
            private const int HideInterval = 1000;
            private readonly GameTimer gameTimer;

            private readonly long initTime;

            public NotifyComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, GameTimer gameTimer, string text) : base(device)
            {
                this.gameTimer = gameTimer;
                TextureString textureString;
                this.AddChild(textureString = new TextureString(device, text, 10, PPDColors.Black)
                {
                    Position = new Vector2(26, 8)
                });
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("notify_back.png")));
                textureString.Update();
                this.Position = new Vector2(Math.Max(400, 800 - textureString.Width - 40), 0);
                initTime = gameTimer.ElapsedTime;
                this.Alpha = 0;
            }

            protected override void UpdateImpl()
            {
                long diff = gameTimer.ElapsedTime - initTime;
                if (diff <= ShowMS)
                {
                    this.Alpha = AnimationUtility.IncreaseAlpha(this.Alpha, 0.1f);
                }
                else
                {
                    this.Alpha = Math.Max(0, (float)(ShowMS + HideInterval - diff) / HideInterval);
                    if (this.Alpha == 0)
                    {
                        this.Parent.RemoveChild(this);
                    }
                }
            }
        }
    }
}
