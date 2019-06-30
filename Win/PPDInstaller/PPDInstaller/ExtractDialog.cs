using System;
using System.Threading;
using System.Windows.Forms;

namespace PPDInstaller
{
    public partial class ExtractDialog : Form
    {
        public string UnzipFilePath
        {
            get;
            set;
        }

        public string ExtractDir
        {
            get;
            set;
        }

        public ExtractDialog()
        {
            InitializeComponent();

            Load += ExtractDialog_Load;
        }

        void ExtractDialog_Load(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                Utility.Unzip(UnzipFilePath, ExtractDir);
                this.Invoke((Action)(() =>
                {
                    this.Close();
                }));
            }).Start();
        }
    }
}
