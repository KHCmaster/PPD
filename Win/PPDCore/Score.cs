using PPDFramework;
using SharpDX;
using System;

namespace PPDCore
{
    /// <summary>
    /// スコア表示クラス
    /// </summary>
    class Score : GameComponent
    {
        uint score;
        uint truescore;
        NumberPictureObject display;
        public Score(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            display = new NumberPictureObject(device, resourceManager, Utility.Path.Combine("numpoint.png"))
            {
                Position = new Vector2(790, 5),
                Alignment = Alignment.Right,
                MaxDigit = 7
            };
            this.AddChild(display);
        }

        public int ScorePoint
        {
            get
            {
                return (int)truescore;
            }
        }

        public int CurrentScore
        {
            set
            {
                truescore = (uint)value;
            }
        }

        public void Retry()
        {
            truescore = 0;
            score = 0;
            SetDefault();
        }

        protected override void UpdateImpl()
        {
            // 遅延増加させる
            if (score < truescore)
            {
                if (truescore - score < 99)
                {
                    score = truescore;
                }
                else
                {
                    score += Math.Max((truescore - score) / 60, 99);
                }
            }
            display.Value = score;
        }
    }
}
