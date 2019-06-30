using FlowScriptControl.Controls;
using System;
using System.IO;
using System.Windows.Forms;

namespace FlowScriptControlTest
{
    public class FlowDrawTab : TabPage
    {
        private string filePath;
        FlowDrawPanel flowDrawPanel;

        public FlowDrawPanel FlowDrawPanel
        {
            get
            {
                return flowDrawPanel;
            }
        }

        public string FilePath
        {
            get { return filePath; }
            set
            {
                if (filePath != value)
                {
                    filePath = value;
                    this.Text = Path.GetFileName(filePath);
                }
            }
        }

        public FlowDrawTab()
        {
            flowDrawPanel = new FlowDrawPanel
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(flowDrawPanel);
            FilePath = Guid.NewGuid().ToString();
        }
    }
}
