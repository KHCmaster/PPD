using System;
using System.Windows.Forms;

namespace PPDEditor.Controls
{
    class ExToolStripSplitButton : ToolStripSplitButton
    {
        public event EventHandler CheckStateChanged;
        bool _Checked;
        bool _CheckOnClick;
        public bool CheckOnClick
        {
            get { return _CheckOnClick; }
            set
            {
                _CheckOnClick = value;
                if (_CheckOnClick) this.ButtonClick += ExToolStripSplitButton_ButtonClick;
                else this.ButtonClick -= ExToolStripSplitButton_ButtonClick;
            }
        }

        void ExToolStripSplitButton_ButtonClick(object sender, EventArgs e)
        {
            Checked = !Checked;
        }
        public bool Checked
        {
            get
            {
                return _Checked;
            }
            set
            {
                _Checked = value;
                if (CheckStateChanged != null) CheckStateChanged.Invoke(this, EventArgs.Empty);
                this.Invalidate();
            }
        }
    }
}
