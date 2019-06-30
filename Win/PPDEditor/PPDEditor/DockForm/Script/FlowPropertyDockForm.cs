using PPDEditor.Controls;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor.DockForm.Script
{
    public partial class FlowPropertyDockForm : DockContent
    {
        public FlowPropertyDockForm()
        {
            InitializeComponent();
        }

        public CustomFlowPropertyPanel FlowPropertyPanel
        {
            get
            {
                return flowPropertyPanel1;
            }
        }
    }
}
