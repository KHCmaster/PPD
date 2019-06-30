using SharpDX;

namespace PPDFramework.Shaders
{
    /// <summary>
    /// インバートのカラーフィルタクラスです。
    /// </summary>
    public class InvertColorFilter : ColorFilterBase
    {
        /// <summary>
        /// フィルターのタイプを取得します。
        /// </summary>
        public override ColorFilterType FilterType
        {
            get
            {
                return ColorFilterType.Invert;
            }
        }

        /// <summary>
        /// フィルター情報を取得します。
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
