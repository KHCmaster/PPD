using PPDFrameworkCore;
using System.Collections.Generic;

namespace PPDFramework
{
    /// <summary>
    /// バインド可能なゲームコンポーネントです。
    /// </summary>
    public abstract class BindableGameComponent : GameComponent
    {
        List<Binding> bindings;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        protected BindableGameComponent(PPDDevice device) : base(device)
        {
            bindings = new List<Binding>();
        }

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
