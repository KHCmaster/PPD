using System;
using WeifenLuo.WinFormsUI.Docking;

namespace PPDEditor.DockForm.Script
{
    public partial class FlowLogDockForm : DockContent
    {
        public FlowLogDockForm()
        {
            InitializeComponent();
        }

        public void SetLang()
        {
            clearToolStripMenuItem.Text = Utility.Language["Clear"];
            Column1.HeaderText = Utility.Language["LogType"];
            Column2.HeaderText = Utility.Language["Message"];
        }

        public void AddLog(string logType, string logText)
        {
            dataGridView1.Rows.Add(logType, logText);
            dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
        }

        public void ClearLogs()
        {
            dataGridView1.Rows.Clear();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearLogs();
        }
    }
}
