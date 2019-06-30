using Effect2DEditor.Command;
using WeifenLuo.WinFormsUI.Docking;

namespace Effect2DEditor.DockForm
{
    public partial class HistoryDock : DockContent
    {
        public CommandManager CommandManager
        {
            get
            {
                return historyPanel1.CommandManager;
            }
        }

        public HistoryDock()
        {
            InitializeComponent();
            historyPanel1.SetEvent();
        }
    }
}
