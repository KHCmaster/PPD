using PPDFramework;
using System;

namespace PPDShareComponent
{
    public class SelectableComponent : GameComponent
    {
        private bool _checked;

        public event EventHandler CheckChanged;

        public bool Selected
        {
            get;
            set;
        }

        public bool Checked
        {
            get
            {
                return _checked;
            }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    OnCheckChanged(this, EventArgs.Empty);
                }
            }
        }

        public SelectableComponent(PPDDevice device) : base(device)
        {

        }

        public virtual bool Down()
        {
            return false;
        }

        public virtual bool Up()
        {
            return false;
        }

        public virtual bool Left()
        {
            return false;
        }

        public virtual bool Right()
        {
            return false;
        }

        protected virtual void OnCheckChanged(object sender, EventArgs e)
        {
            CheckChanged?.Invoke(sender, e);
        }
    }
}
