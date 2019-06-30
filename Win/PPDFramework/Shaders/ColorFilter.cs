using SharpDX;

namespace PPDFramework.Shaders
{
    /// <summary>
    /// カラーフィルタのクラスです
    /// </summary>
    public class ColorFilter : ColorFilterBase
    {
        /// <summary>
        /// フィルターのタイプを取得します。
        /// </summary>
        public override ColorFilterType FilterType
        {
            get
            {
                return ColorFilterType.Color;
            }
        }

        /// <summary>
        /// カラーを取得、設定します
        /// </summary>
        public Color4 Color
        {
            get;
            set;
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
                Arg1 = new Vector4(Color.Red, Color.Green, Color.Blue, 1),
                Arg2 = new Vector4(weight, 1 - weight, 0, 0)
            };
        }
    }
}
