namespace PPDFramework
{
    /// <summary>
    /// リセット可能なコンポーネントです。
    /// </summary>
    public class ResettableComponent : DisposableComponent
    {
        /// <summary>
        /// デバイスです。
        /// </summary>
        protected PPDDevice device;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        public ResettableComponent(PPDDevice device)
        {
            this.device = device;
            device.AddResettableComponent(this);
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();
            device.RemoveResettableComponent(this);
        }

        /// <summary>
        /// デバイスがロストしたときの処理です。
        /// </summary>
        public virtual void OnLostDevice()
        {

        }

        /// <summary>
        /// デバイスがリセットされたときの処理です。
        /// </summary>
        public virtual void OnResetDevice()
        {

        }
    }
}
