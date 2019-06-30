namespace PPDFramework.Shaders
{
    /// <summary>
    /// カラーフィルターの基底クラスです。
    /// </summary>
    public abstract class ColorFilterBase
    {
        /// <summary>
        /// ウェイトを取得、設定します。
        /// </summary>
        public float Weight
        {
            get;
            set;
        }

        /// <summary>
        /// フィルターのタイプを取得します。
        /// </summary>
        public abstract ColorFilterType FilterType
        {
            get;
        }

        /// <summary>
        /// フィルター情報を取得します。
        /// </summary>
        /// <returns></returns>
        public abstract FilterInfo ToFilterInfo();

        /// <summary>
        /// ウェイトをクランプします。
        /// </summary>
        /// <returns></returns>
        protected float ClampWeight()
        {
            return Clamp(Weight, 0, 1);
        }

        /// <summary>
        /// クランプします。
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        protected float Clamp(float val, float min, float max)
        {
            return val >= max ? max : (val <= min ? min : val);
        }
    }
}
