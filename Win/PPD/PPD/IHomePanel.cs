using PPDFramework;
using System;

namespace PPD
{
    abstract class HomePanelBase : FocusableGameComponent
    {
        public event Action<int> LoadProgressed;
        public abstract void Load();

        public int CurrentProgress
        {
            get;
            protected set;
        }

        protected HomePanelBase(PPDDevice device) : base(device)
        {

        }

        protected void OnLoadProgressed(int val)
        {
            CurrentProgress = val;
            LoadProgressed?.Invoke(val);
        }
    }
}
