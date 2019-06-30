using SharpDX;

namespace PPDFramework.Shaders
{
    /// <summary>
    /// HDTV仕様のグレイスケールのカラーフィルターです。
    /// </summary>
    public class HDTVGrayScaleColorFilter : ColorFilterBase
    {
        /// <summary>
        /// フィルターのタイプを取得します。
        /// </summary>
        public override ColorFilterType FilterType
        {
            get
            {
                return ColorFilterType.HDTVGrayScale;
            }
        }

        /// <summary>
        /// ガンマ補正を取得、設定します。
        /// </summary>
        public float GammaCorrection
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public HDTVGrayScaleColorFilter()
        {
            GammaCorrection = 2.2f;
        }

        /// <summary>
        /// フィルター情報を取得、設定します。
        /// </summary>
        /// <returns></returns>
        public override FilterInfo ToFilterInfo()
        {
            var weight = ClampWeight();
            return new FilterInfo
            {
                Arg1 = new Vector4(weight, 1 - weight, GammaCorrection, 0)
            };
        }
    }
}
