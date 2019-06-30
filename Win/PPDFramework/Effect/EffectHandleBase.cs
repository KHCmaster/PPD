namespace PPDFramework.Effect
{
    /// <summary>
    /// エフェクトハンドルのクラスです。
    /// </summary>
    public abstract class EffectHandleBase : DisposableComponent
    {
        /// <summary>
        /// 実際の値です。
        /// </summary>
        public object Value
        {
            get;
            set;
        }
    }
}
