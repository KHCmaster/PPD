using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace PPDEditor.DockForm.Script
{
    /// <summary>
    /// スクリプトを実行したときのログを出します。
    /// </summary>
    public partial class FlowLogPanel : UserControl
    {
        readonly ObservableCollection<KeyValuePair<string, string>> logs = [];

        public FlowLogPanel()
        {
            InitializeComponent();
            dataGridView1.ItemsSource = logs;
        }

        public void SetLang()
        {
            clearMenuItem.Header = Utility.Language["Clear"];
            Column1.Header = Utility.Language["LogType"];
            Column2.Header = Utility.Language["Message"];
        }

        public void AddLog(string logType, string logText)
        {
            logs.Add(new(logType, logText));
            dataGridView1.ScrollIntoView(logs[^1]);
        }

        public void ClearLogs()
        {
            logs.Clear();
        }

        private void ClearMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClearLogs();
        }
    }
}
