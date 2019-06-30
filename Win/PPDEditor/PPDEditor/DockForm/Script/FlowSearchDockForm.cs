using FlowScriptControl.Controls;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor.DockForm.Script
{
    public partial class FlowSearchDockForm : DockContent
    {
        public FlowSearchPanel FlowSearchPanel
        {
            get
            {
                return flowSearchPanel1;
            }
        }

        public FlowSearchDockForm()
        {
            InitializeComponent();
        }
    }
}
