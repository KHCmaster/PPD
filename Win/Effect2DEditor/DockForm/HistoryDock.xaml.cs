using Effect2DEditor.Command;
using System.Windows.Controls;

namespace Effect2DEditor.DockForm
{
    public partial class HistoryDock : UserControl
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
