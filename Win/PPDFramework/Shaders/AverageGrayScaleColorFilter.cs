using SharpDX;

namespace PPDFramework.Shaders
{
    /// <summary>
    /// 平均グレイスケールのフィルターです。
    /// </summary>
    public class AverageGrayScaleColorFilter : ColorFilterBase
    {
        /// <summary>
        /// フィルターのタイプを取得します。
        /// </summary>
        public override ColorFilterType FilterType
        {
            get
            {
                return ColorFilterType.AverageGrayScale;
            }
        }

        /// <summary>
        /// フィルター情報に変換します。
        /// </summary>
        /// <returns></returns>
        public override FilterInfo ToFilterInfo()
        {
            var weight = ClampWeight();
            return new FilterInfo
            {
                Arg1 = new Vector4(weight, 1 - weight, 0, 0)
            };
        }
    }
}
