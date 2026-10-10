namespace PPDFramework.Sprites
{
    /// <summary>
    /// 画像の拡大率（分数）です。
    /// </summary>
    public struct ImageScale
    {
        /// <summary>
        /// 分子です。
        /// </summary>
        public int Numerator;

        /// <summary>
        /// 分母です。
        /// </summary>
        public int Denominator;

        /// <summary>
        /// 拡大率（分子 / 分母）です。
        /// </summary>
        public float Ratio;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="numerator">分子。</param>
        /// <param name="denominator">分母。</param>
        public ImageScale(int numerator, int denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
            Ratio = (float)Numerator / Denominator;
        }

        /// <summary>
        /// 拡大率が小さいかどうかを返します。
        /// </summary>
        /// <param name="left">左辺。</param>
        /// <param name="right">右辺。</param>
        /// <returns>左辺のほうが小さければ true。</returns>
        public static bool operator <(ImageScale left, ImageScale right)
        {
            return left.Ratio < right.Ratio;
        }

        /// <summary>
        /// 拡大率が大きいかどうかを返します。
        /// </summary>
        /// <param name="left">左辺。</param>
        /// <param name="right">右辺。</param>
        /// <returns>左辺のほうが大きければ true。</returns>
        public static bool operator >(ImageScale left, ImageScale right)
        {
            return left.Ratio > right.Ratio;
        }

        /// <summary>
        /// 拡大率が同じか小さいかどうかを返します。
        /// </summary>
        /// <param name="left">左辺。</param>
        /// <param name="right">右辺。</param>
        /// <returns>左辺が右辺以下なら true。</returns>
        public static bool operator <=(ImageScale left, ImageScale right)
        {
            return left.Ratio <= right.Ratio;
        }

        /// <summary>
        /// 拡大率が同じか大きいかどうかを返します。
        /// </summary>
        /// <param name="left">左辺。</param>
        /// <param name="right">右辺。</param>
        /// <returns>左辺が右辺以上なら true。</returns>
        public static bool operator >=(ImageScale left, ImageScale right)
        {
            return left.Ratio >= right.Ratio;
        }
    }
}
