using SharpDX;
using System;

namespace PPDShareComponent
{
    public static class AnimationUtility
    {
        public static float IncreaseAlpha(float alpha)
        {
            return IncreaseAlpha(alpha, 0.1f);
        }

        public static float IncreaseAlpha(float alpha, float accel)
        {
            return GetAnimationValue(alpha, 1, accel);
        }

        public static float DecreaseAlpha(float alpha)
        {
            return DecreaseAlpha(alpha, 0.1f);
        }

        public static float DecreaseAlpha(float alpha, float accel)
        {
            return GetAnimationValue(alpha, 0, accel);
        }

        public static float GetAnimationValue(float value, float destValue)
        {
            return GetAnimationValue(value, destValue, 0.1f);
        }

        public static float GetAnimationValue(float value, float destValue, float accel)
        {
            float newValue = value + (destValue - value) * accel;
            if (Math.Abs(newValue - destValue) <= 0.001f)
            {
                newValue = destValue;
            }
            return newValue;
        }

        public static Vector2 GetAnimationPosition(Vector2 pos, Vector2 destPos)
        {
            return GetAnimationPosition(pos, destPos, 0.1f);
        }

        public static Vector2 GetAnimationPosition(Vector2 pos, Vector2 destPos, float accel)
        {
            return new Vector2(GetAnimationValue(pos.X, destPos.X, accel), GetAnimationValue(pos.Y, destPos.Y, accel));
        }
    }
}
