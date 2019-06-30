using PPDFramework;
using SharpDX;
using System.Collections.Generic;

namespace PPDCore
{
    class SlideManager : GameComponent
    {
        readonly PathObject maxSlideEffectPath = Utility.Path.Combine("slide", "max_slide_effect.etd");
        readonly PathObject hatchRightEffetPath = Utility.Path.Combine("slide", "hatch_right.etd");
        readonly PathObject hatchLeftEffetPath = Utility.Path.Combine("slide", "hatch_left.etd");

        PPDFramework.Resource.ResourceManager resourceManager;
        PPDEffectManager effectManager;
        SpriteObject pointsSprite;
        Dictionary<object, SlidePoint> slidePoints;

        public SlideManager(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            this.resourceManager = resourceManager;
            slidePoints = new Dictionary<object, SlidePoint>();
            effectManager = new PPDEffectManager(device, resourceManager);
            effectManager.CreateEffect(maxSlideEffectPath);
            effectManager.CreateEffect(hatchLeftEffetPath);
            effectManager.CreateEffect(hatchRightEffetPath);

            this.AddChild(effectManager);
            this.AddChild(pointsSprite = new SpriteObject(device));
        }

        public void Retry()
        {
            SetDefault();
            pointsSprite.ClearChildren();
            slidePoints.Clear();
        }

        public void AddSlidePoint(object mark, Vector2 position, int score, bool isRight)
        {
            effectManager.AddEffect(isRight ? hatchRightEffetPath : hatchLeftEffetPath, position);
            RemoveSlidePoint(mark);
            SlidePoint slidePoint;
            pointsSprite.AddChild((slidePoint = new SlidePoint(device, resourceManager, score)
            {
                Position = new Vector2(position.X, position.Y - 20)
            }));
            slidePoints[mark] = slidePoint;
        }

        public void AddMaxSlideEffect(object mark, Vector2 position, int score)
        {
            effectManager.AddEffect(maxSlideEffectPath, position);
            RemoveSlidePoint(mark);
            pointsSprite.AddChild(new MaxSlidePoint(device, resourceManager, score)
            {
                Position = position
            });
        }

        private void RemoveSlidePoint(object mark)
        {
            if (slidePoints.TryGetValue(mark, out SlidePoint slidePoint))
            {
                if (slidePoint.Parent != null)
                {
                    slidePoint.Parent.RemoveChild(slidePoint);
                }
                slidePoints.Remove(mark);
            }
        }

        class SlidePoint : GameComponent
        {
            int showCount;

            public SlidePoint(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, int score) : base(device)
            {
                NumberPictureObject num;
                this.AddChild(num = new NumberPictureObject(device, resourceManager, Utility.Path.Combine("slide", "slide_point.png"))
                {
                    Position = Vector2.Zero,
                    Alignment = Alignment.Left,
                    MaxDigit = -1,
                    Value = (uint)score
                });
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("slide", "slide_point_plus.png"))
                {
                    Position = new Vector2(-num.DigitWidth, 0)
                });
            }

            protected override void UpdateImpl()
            {
                showCount++;
                if (showCount > 10 && Parent != null)
                {
                    Parent.RemoveChild(this);
                }
                Position = new Vector2(Position.X, Position.Y - 1);
            }
        }

        class MaxSlidePoint : GameComponent
        {
            int showCount;

            public MaxSlidePoint(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, int score) : base(device)
            {
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("slide", "max_slide.png"), true));
                NumberPictureObject num;
                this.AddChild(num = new NumberPictureObject(device, resourceManager, Utility.Path.Combine("slide", "slide_point.png"))
                {
                    Position = new Vector2(0, 5),
                    Alignment = Alignment.Left,
                    MaxDigit = -1,
                    Value = (uint)score
                });
                num.Position = new Vector2(-num.DigitWidth * score.ToString().Length / 2f, num.Position.Y);
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("slide", "slide_point_plus.png"))
                {
                    Position = new Vector2(-num.DigitWidth + num.Position.X, num.Position.Y)
                });
            }

            protected override void UpdateImpl()
            {
                showCount++;
                if (showCount > 50 && Parent != null)
                {
                    Parent.RemoveChild(this);
                }
                Position = new Vector2(Position.X, Position.Y - 0.5f);
            }
        }
    }
}
