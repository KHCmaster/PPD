using PPDFramework.Shaders;
using System.Collections.Generic;

namespace PPDFramework.ScreenFilters
{
    /// <summary>
    /// カラーのスクリーンフィルタークラスです。
    /// </summary>
    public class ColorScreenFilter : ScreenFilterBase
    {
        /// <summary>
        /// フィルターの一覧を取得します
        /// </summary>
        public List<ColorFilterBase> Filters
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタです
        /// </summary>
        public ColorScreenFilter()
        {
            Filters = new List<ColorFilterBase>();
        }

        /// <summary>
        /// フィルタ処理です
        /// </summary>
        public override void Filter(PPDDevice device)
        {
            if (Filters.Count == 0)
            {
                return;
            }

            device.GetModule<Impl.ColorScreenFilter>().Draw(device, Filters);
        }
    }
}
