using FlowScriptControl.Controls;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor.DockForm.Script
{
    public partial class FlowExecutingPropertyDockForm : DockContent
    {
        public FlowExecutingPropertyPanel FlowExecutingPropertyPanel
        {
            get
            {
                return flowExecutingPropertyPanel1;
            }
        }

        public FlowExecutingPropertyDockForm()
        {
            InitializeComponent();
        }
    }
}
