using Effect2DEditor.Command;
using System.Windows.Controls;

namespace Effect2DEditor.DockForm
{
    /// <summary>
    /// 編集の履歴です。チェックボックスを押すと、そこまで元に戻す・やり直します。
    /// </summary>
    public partial class HistoryDock : UserControl
    {
        public HistoryDock()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 表示する編集の履歴です。
        /// </summary>
        public CommandManager CommandManager
        {
            get { return historyPanel1.CommandManager; }
            set { historyPanel1.CommandManager = value; }
        }
    }
}
