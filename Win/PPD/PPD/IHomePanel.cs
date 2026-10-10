using PPDFramework;
using System;

namespace PPD
{
    abstract class HomePanelBase(PPDDevice device) : FocusableGameComponent(device)
    {
        public event Action<int> LoadProgressed;
        public abstract void Load();

        public int CurrentProgress
        {
            get;
            protected set;
        }

        protected void OnLoadProgressed(int val)
        {
            CurrentProgress = val;
            LoadProgressed?.Invoke(val);
        }
    }
}
