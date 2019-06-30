using System;
using System.IO;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class ModPublishForm : Form
    {
        public ModPublishForm()
        {
            InitializeComponent();
            var path = Path.GetFullPath("publish_mod");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            textBox5.Text = path;
            textBox3.Text = PPDStaticSetting.AuthorName;
        }

        public void SetLang()
        {
            this.Text = Utility.Language["PublishMod"];
            this.label1.Text = Utility.Language["PublishModLabel1"];
            this.label2.Text = Utility.Language["PublishModLabel2"];
            this.label3.Text = Utility.Language["PublishModLabel3"];
            this.label4.Text = Utility.Language["PublishModLabel4"];
            this.label5.Text = Utility.Language["PublishModLabel5"];
            this.label6.Text = Utility.Language["PublishModLabel6"];
        }

        public string ModFileName
        {
            get
            {
                return textBox1.Text;
            }
        }

        public string ModDisplayName
        {
            get
            {
                return textBox2.Text;
            }
        }

        public string ModAuthorName
        {
            get
            {
                return textBox3.Text;
            }
        }

        public string ModVersion
        {
            get
            {
                return textBox4.Text;
            }
        }

        public string PublishFolder
        {
            get
            {
                return textBox5.Text;
            }
        }

        private bool CheckInput()
        {
            try
            {
                if (!Directory.Exists(PublishFolder))
                {
                    Directory.CreateDirectory(PublishFolder);
                }
                if (String.IsNullOrEmpty(ModFileName))
                {
                    MessageBox.Show(Utility.Language["NoFilename"]);
                    return false;
                }
                if (!Utility.CheckValidFileName(ModFileName))
                {
                    MessageBox.Show(String.Format(Utility.Language["ContainsInvalidChars"], Utility.Language["FileName"]));
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (CheckInput())
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = textBox5.Text;
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.textBox5.Text = folderBrowserDialog1.SelectedPath;
            }
        }
    }
}
