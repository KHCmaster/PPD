namespace PPDFramework
{
    /// <summary>
    /// 動画のトリミングデータクラス
    /// </summary>
    public class MovieTrimmingData
    {
        /// <summary>
        /// 上
        /// </summary>
        public float Top
        {
            get;
            set;
        }

        /// <summary>
        /// 左
        /// </summary>
        public float Left
        {
            get;
            set;
        }

        /// <summary>
        /// 右
        /// </summary>
        public float Right
        {
            get;
            set;
        }

        /// <summary>
        /// 下
        /// </summary>
        public float Bottom
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MovieTrimmingData()
        {

        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="top">上</param>
        /// <param name="left">左</param>
        /// <param name="right">右</param>
        /// <param name="bottom">下</param>
        public MovieTrimmingData(float top, float left, float right, float bottom)
        {
            Top = top;
            Left = left;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        /// 左のトリミングを取得します。
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        public float GetLeftTrimming(float width)
        {
            return GetTrimming(Left, width);
        }

        /// <summary>
        /// 右のトリミングを取得します。
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        public float GetRightTrimming(float width)
        {
            return GetTrimming(Right, width);
        }

        /// <summary>
        /// 上のトリミングを取得します。
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public float GetTopTrimming(float height)
        {
            return GetTrimming(Top, height);
        }

        /// <summary>
        /// 下のトリミングを取得します。
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public float GetBottomTrimming(float height)
        {
            return GetTrimming(Bottom, height);
        }

        private float GetTrimming(float ratio_or_pixel, float value)
        {
            if (IsRatio(ratio_or_pixel))
            {
                return value * ratio_or_pixel;
            }
            return ratio_or_pixel;
        }

        private bool IsRatio(float trim)
        {
            return -1 < trim && trim < 1;
        }
    }
}
