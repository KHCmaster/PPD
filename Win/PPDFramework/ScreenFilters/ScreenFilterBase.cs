namespace PPDFramework.ScreenFilters
{
    /// <summary>
    /// スクリーンフィルタの基底クラスです。
    /// </summary>
    public abstract class ScreenFilterBase
    {
        /// <summary>
        /// フィルター処理です。
        /// </summary>
        public abstract void Filter(PPDDevice device);
    }
}
