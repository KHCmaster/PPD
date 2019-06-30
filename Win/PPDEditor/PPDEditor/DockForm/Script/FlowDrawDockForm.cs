using FlowScriptControl.Controls;
using PPDConfiguration;
using PPDEditor.Forms;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PPDEditor.DockForm.Script
{
    public partial class FlowDrawDockForm : ChangableDockContent
    {
        private string filePath;

        public FlowDrawPanel FlowDrawPanel
        {
            get
            {
                return flowDrawPanel1;
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
                    TabText = Path.GetFileName(filePath);
                    ToolTipText = filePath;
                    if (IsContentChanged)
                    {
                        ContentChanged();
                    }
                }
            }
        }

        public bool CanSave
        {
            get
            {
                if (String.IsNullOrEmpty(FilePath))
                {
                    return false;
                }

                if (Path.GetInvalidPathChars().Any(FilePath.Contains))
                {
                    return false;
                }

                var dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir))
                {
                    return false;
                }

                return true;
            }
        }

        public event EventHandler Modified;

        public FlowDrawDockForm()
        {
            InitializeComponent();

            FormClosing += FlowDrawDockForm_FormClosing;
            Load += FlowDrawDockForm_Load;
            flowDrawPanel1.Modified += flowDrawPanel1_Modified;
        }

        void FlowDrawDockForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsContentChanged)
            {
                var dialogResult = MessageBox.Show(Utility.Language["AskSave"], Utility.Language["Confirm"], MessageBoxButtons.YesNoCancel);
                e.Cancel = dialogResult == DialogResult.Cancel;
                if (dialogResult == DialogResult.Yes)
                {
                    Save();
                }
            }
        }

        void flowDrawPanel1_Modified(object sender, EventArgs e)
        {
            OnModified(this, EventArgs.Empty);
            ContentChanged();
        }

        public void Save()
        {
            if (!CanSave)
            {
                return;
            }

            using (Stream stream = File.Open(FilePath, FileMode.Create))
            {
                FlowDrawPanel.SaveXML(stream);
            }
            ContentSaved();
        }

        public void SetLang()
        {
            保存ToolStripMenuItem.Text = Utility.Language["Save"];
            var filePath = Path.Combine("Lang", String.Format("lang_FlowScript_{0}.ini", PPDStaticSetting.langFileISO));
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine("Lang", String.Format("lang_FlowScript_{0}.ini", "en"));
                if (!File.Exists(filePath))
                {
                    Path.Combine("Lang", String.Format("lang_FlowScript_{0}.ini", "jp"));
                }
            }

            if (!File.Exists(filePath))
            {
                return;
            }

            var sr = new StreamReader(filePath);
            var flowLang = new SettingReader(sr.ReadToEnd());
            sr.Close();
            this.flowDrawPanel1.LanguageProvider = new FlowScriptLanguageProvider(flowLang);
        }

        void FlowDrawDockForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(FilePath))
            {
                using (Stream stream = File.Open(FilePath, FileMode.Open))
                {
                    flowDrawPanel1.LoadXML(stream);
                }

                ContentSaved();
            }
        }

        private void OnModified(object sender, EventArgs e)
        {
            Modified?.Invoke(sender, e);
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void 閉じるToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
