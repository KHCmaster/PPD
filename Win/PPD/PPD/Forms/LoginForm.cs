using PPDFramework.Web;
using PPDFrameworkCore;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace PPD.Forms
{
    public partial class LoginForm : Form
    {
        Thread logingThread;
        volatile bool loginCanceled;

        public bool RememberMe
        {
            get
            {
                return checkBox1.Checked;
            }
        }

        public string Username
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
            }
        }

        public string Password
        {
            get
            {
                return maskedTextBox1.Text;
            }
        }

        public ErrorReason InitialLoginResult
        {
            get;
            set;
        }

        public LoginForm()
        {
            InitializeComponent();
            SetLang();
            FormClosing += LoginForm_FormClosing;
            Shown += LoginForm_Shown;
        }

        public void SetLang()
        {
            this.Text = Utility.Language["Login"];
            this.label1.Text = Utility.Language["DoLogin"];
            this.label2.Text = Utility.Language["IDOrMailAddress"];
            this.label3.Text = Utility.Language["Password"];
            this.linkLabel1.Text = Utility.Language["CreateAccount"];
            this.button1.Text = Utility.Language["Login"];
            this.button2.Text = Utility.Language["Cancel"];
        }

        void LoginForm_Shown(object sender, EventArgs e)
        {
            if (InitialLoginResult != ErrorReason.OK)
            {
                ShowErrorMessage(InitialLoginResult);
            }
        }

        void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (logingThread != null)
            {
                // .NET では Thread.Abort が使えないので、ログインの結果を画面に反映しないようにするだけにする
                loginCanceled = true;
                logingThread = null;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(PPDFramework.Web.WebManager.BaseUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Enabled = false;
            var id = textBox1.Text;
            var password = maskedTextBox1.Text;
            logingThread = ThreadManager.Instance.GetThread(() =>
            {
                var errorReason = WebManager.Instance.Login(id, password);
                if (loginCanceled)
                {
                    return;
                }
                try
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        if (IsDisposed)
                        {
                            return;
                        }
                        if (errorReason == ErrorReason.OK)
                        {
                            this.Close();
                        }
                        else
                        {
                            panel1.Enabled = true;
                            ShowErrorMessage(errorReason);
                        }
                        logingThread = null;
                    }));
                }
                catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidOperationException)
                {
                    // ログインの途中でフォームが閉じられた
                }
            });
            logingThread.Start();
        }

        private void ShowErrorMessage(ErrorReason errorReason)
        {
            string errorText = "";
            switch (errorReason)
            {
                case ErrorReason.AuthFailed:
                    errorText = Utility.Language["LoginAuthFailed"];
                    break;
                case ErrorReason.VersionUnmatch:
                    errorText = Utility.Language["LoginVersionUnmatch"];
                    break;
                default:
                    errorText = Utility.Language["LoginFailed"];
                    break;
            }
            MessageBox.Show(errorText);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
