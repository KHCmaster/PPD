using SharpDX;

namespace PPDFramework.Shaders
{
    /// <summary>
    /// 最大グレースケールのカラーフィルターのクラスです。
    /// </summary>
    public class MaxGrayScaleColorFilter : ColorFilterBase
    {
        /// <summary>
        /// フィルターのタイプを取得します。
        /// </summary>
        public override ColorFilterType FilterType
        {
            get
            {
                return ColorFilterType.MaxGrayScale;
            }
        }

        /// <summary>
        /// フィルタ情報を取得します。
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
