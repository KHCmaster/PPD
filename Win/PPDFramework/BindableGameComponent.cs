using PPDFrameworkCore;
using System.Collections.Generic;

namespace PPDFramework
{
    /// <summary>
    /// バインド可能なゲームコンポーネントです。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="device"></param>
    public abstract class BindableGameComponent(PPDDevice device) : GameComponent(device)
    {
        readonly List<Binding> bindings = [];

        /// <summary>
        /// バインディングを追加します。
        /// </summary>
        /// <param name="binding"></param>
        protected void AddBinding(Binding binding)
        {
            if (binding != null)
            {
                bindings.Add(binding);
            }
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            foreach (Binding binding in bindings)
            {
                binding.Release();
            }
            bindings.Clear();
            base.DisposeResource();
        }
    }
}
