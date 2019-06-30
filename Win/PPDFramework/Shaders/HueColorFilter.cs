using SharpDX;

namespace PPDFramework.Shaders
{
    /// <summary>
    /// 色相カラーフィルターのクラスです。
    /// </summary>
    public class HueColorFilter : ColorFilterBase
    {
        /// <summary>
        /// フィルターのタイプを取得します。
        /// </summary>
        public override ColorFilterType FilterType
        {
            get
            {
                return ColorFilterType.Hue;
            }
        }

        /// <summary>
        /// 回転を取得、設定します。
        /// </summary>
        public float Rotation
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
                Arg1 = new Vector4(weight, 1 - weight, Rotation, 0)
            };
        }
    }
}
