using System.Windows.Forms;

namespace Effect2DEditor
{
    class ExToolStripSplitButton : ToolStripSplitButton
    {
        bool _Checked;
        public bool Checked
        {
            get
            {
                return _Checked;
            }
            set
            {
                _Checked = value;
                this.Invalidate();
            }
        }
    }
}
