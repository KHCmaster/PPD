namespace PPDFramework.Sprites
{
    public struct ImageScale
    {
        public int Numerator;
        public int Denominator;
        public float Ratio;

        public ImageScale(int numerator, int denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
            Ratio = (float)Numerator / Denominator;
        }

        public static bool operator <(ImageScale left, ImageScale right)
        {
            return left.Ratio < right.Ratio;
        }

        public static bool operator >(ImageScale left, ImageScale right)
        {
            return left.Ratio > right.Ratio;
        }

        public static bool operator <=(ImageScale left, ImageScale right)
        {
            return left.Ratio <= right.Ratio;
        }

        public static bool operator >=(ImageScale left, ImageScale right)
        {
            return left.Ratio >= right.Ratio;
        }
    }
}
