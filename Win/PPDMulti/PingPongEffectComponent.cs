using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDMulti
{
    class PingPongEffectComponent : GameComponent
    {
        const int maxSpeed = 5;

        PPDFramework.Resource.ResourceManager resourceManager;
        RectangleComponent leftRect;
        RectangleComponent rightRect;
        RectangleComponent ballRect;

        Vector2 ballVector;

        public PingPongEffect Effect
        {
            get;
            set;
        }

        public PingPongEffectComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            this.resourceManager = resourceManager;

            this.AddChild(leftRect = new RectangleComponent(device, resourceManager, PPDColors.White)
            {
                RectangleWidth = 50,
                RectangleHeight = 200,
                Position = new Vector2(50, 0)
            });

            this.AddChild(rightRect = new RectangleComponent(device, resourceManager, PPDColors.White)
            {
                RectangleWidth = 50,
                RectangleHeight = 200,
                Position = new Vector2(700, 250)
            });

            this.AddChild(ballRect = new RectangleComponent(device, resourceManager, PPDColors.White)
            {
                RectangleHeight = 50,
                RectangleWidth = 50,
                Position = new Vector2(375, 200)
            });

            var random = new Random();
            ballVector = new Vector2(random.Next(1, 5), random.Next(1, 5));

            Alpha = 0;
        }

        public bool EffectExpired
        {
            get;
            set;
        }

        private void Check()
        {
            if (ballRect.Position.Y < 0)
            {
                ballRect.Position = new Vector2(ballRect.Position.X, -ballRect.Position.Y);
                ballVector = new Vector2(ballVector.X, -ballVector.Y);
            }
            else if (ballRect.Position.Y + ballRect.RectangleHeight > 450)
            {
                ballRect.Position = new Vector2(ballRect.Position.X, 900 - (ballRect.Position.Y + ballRect.RectangleHeight));
                ballVector = new Vector2(ballVector.X, -ballVector.Y);
            }
            var left = new Rectangle
            {
                Position = leftRect.Position,
                Width = leftRect.RectangleWidth,
                Height = leftRect.RectangleHeight
            };
            var right = new Rectangle
            {
                Position = rightRect.Position,
                Width = rightRect.RectangleWidth,
                Height = rightRect.RectangleHeight
            };
            var ball = new Rectangle
            {
                Position = ballRect.Position,
                Width = ballRect.RectangleWidth,
                Height = ballRect.RectangleHeight
            };
            if (left.Intersect(ball))
            {
                ball.Position = new Vector2(2 * (left.Position.X + left.Width) - ball.Position.X, ball.Position.Y);
                ballVector = new Vector2(-(ballVector.X - 1), ballVector.Y);
            }
            if (right.Intersect(ball))
            {
                ball.Position = new Vector2(2 * right.Position.X - ball.Position.X - ballRect.RectangleWidth, ball.Position.Y);
                ballVector = new Vector2(-(ballVector.X + 1), ballVector.Y);
            }
            if (ballVector.Length() > 100)
            {
                ballVector.Normalize();
                ballVector = ballVector * 100;
            }
        }

        private void Move()
        {
            float ballCenterY = ballRect.Position.Y + ballRect.RectangleHeight / 2;
            float leftCenterY = leftRect.Position.Y + leftRect.RectangleHeight / 2;
            float rightCenterY = rightRect.Position.Y + rightRect.RectangleHeight / 2;
            if (ballRect.Position.X < rightRect.Position.X - 100)
            {
                var diff = ballCenterY - leftCenterY;
                var move = Math.Abs(diff) > maxSpeed ? maxSpeed : Math.Abs(diff);
                leftRect.Position = new Vector2(leftRect.Position.X,
                   Math.Min(450 - leftRect.RectangleHeight, Math.Max(0, leftRect.Position.Y + move * Math.Sign(diff))));
            }
            if (ballRect.Position.X > leftRect.Position.X + leftRect.RectangleWidth + 50)
            {
                var diff = ballCenterY - rightCenterY;
                var move = Math.Abs(diff) > maxSpeed ? maxSpeed : Math.Abs(diff);
                rightRect.Position = new Vector2(rightRect.Position.X,
                   Math.Min(450 - rightRect.RectangleHeight, Math.Max(0, rightRect.Position.Y + move * Math.Sign(diff))));
            }
        }

        protected override void UpdateImpl()
        {
            ballRect.Position += ballVector;
            Check();
            Move();

            if (EffectExpired)
            {
                Alpha = AnimationUtility.DecreaseAlpha(Alpha);
                if (Alpha <= 0)
                {
                    Parent.RemoveChild(this);
                }
            }
            else
            {
                Alpha = AnimationUtility.IncreaseAlpha(Alpha);
            }
        }

        struct Rectangle
        {
            public Vector2 Position;
            public float Width;
            public float Height;

            public bool Intersect(Rectangle rect)
            {
                return (Position.X <= rect.Position.X && rect.Position.X <= Position.X + Width && Position.Y <= rect.Position.Y && rect.Position.Y <= Position.Y + Height)
                    || (Position.X <= rect.Position.X + rect.Width && rect.Position.X + rect.Width <= Position.X + Width && Position.Y <= rect.Position.Y && rect.Position.Y <= Position.Y + Height)
                    || (Position.X <= rect.Position.X && rect.Position.X <= Position.X + Width && Position.Y <= rect.Position.Y + rect.Height && rect.Position.Y + rect.Height <= Position.Y + Height)
                    || (Position.X <= rect.Position.X + rect.Width && rect.Position.X + rect.Width <= Position.X + Width && Position.Y <= rect.Position.Y + rect.Height && rect.Position.Y + rect.Height <= Position.Y + Height);
            }
        }
    }
}
