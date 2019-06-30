using SharpDX;

namespace PPDFramework.Shaders
{
    /// <summary>
    /// 輝度フィルタのクラスです。
    /// </summary>
    public class BrightnessColorFilter : ColorFilterBase
    {
        /// <summary>
        /// フィルターのタイプを取得します。
        /// </summary>
        public override ColorFilterType FilterType
        {
            get
            {
                return ColorFilterType.Brightness;
            }
        }

        /// <summary>
        /// スケールを取得、設定します。
        /// </summary>
        public float Scale
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
                Arg1 = new Vector4(weight, 1 - weight, Clamp(Scale, 0, float.MaxValue), 0)
            };
        }
    }
}
