using System;
using System.IO;
using System.Windows.Forms;

namespace FolderDiffChecker
{
    public partial class UserForm : Form
    {
        private string lastUserNameFilePath = "lastUser.txt";

        public string UserName
        {
            get
            {
                return userNameTextBox.Text;
            }
        }

        public UserForm()
        {
            InitializeComponent();

            Load += UserForm_Load;
        }

        void UserForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(lastUserNameFilePath))
            {
                userNameTextBox.Text = File.ReadAllText(lastUserNameFilePath);
            }
            else
            {
                userNameTextBox.Focus();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            File.WriteAllText(lastUserNameFilePath, UserName);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
