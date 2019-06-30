using FlowScriptControl.Controls;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor.DockForm.Script
{
    public partial class FlowTreeViewDockForm : DockContent
    {
        public FlowTreeViewDockForm()
        {
            InitializeComponent();
        }

        public FlowTreeView FlowTreeView
        {
            get
            {
                return flowTreeView1;
            }
        }
    }
}
