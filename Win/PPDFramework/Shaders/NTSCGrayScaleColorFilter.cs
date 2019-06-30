using SharpDX;

namespace PPDFramework.Shaders
{
    /// <summary>
    /// NTSCのグレースケールのカラースケールのフィルタです。
    /// </summary>
    public class NTSCGrayScaleColorFilter : ColorFilterBase
    {
        /// <summary>
        /// フィルターのタイプを取得します。
        /// </summary>
        public override ColorFilterType FilterType
        {
            get
            {
                return ColorFilterType.NTSCGrayScale;
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
