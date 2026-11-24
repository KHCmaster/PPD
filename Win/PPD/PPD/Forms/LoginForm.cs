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
        // 進行中のブラウザ経由のログイン（キャンセルや新しい試行で入れ替わる）
        volatile object browserAttempt;

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

        /// <summary>
        /// ブラウザ経由でログインしたときに受け取ったログインキー。パスワードでログインしたときは null です。
        /// </summary>
        public string LoginKey
        {
            get;
            private set;
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
            this.button3.Text = Utility.Language["LoginWithBrowser"];
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

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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

        private void Button1_Click(object sender, EventArgs e)
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

        // ブラウザ経由のログイン: サイトの /device を開いて、表示したコードを入力・承認してもらうのを待つ。
        // パスキーだけのアカウントはパスワードを入力できないので、こちらでログインする。
        // 待っている間は ID・パスワードの欄を隠してコードを大きく出し、キャンセルで元の画面に戻る。
        private void Button3_Click(object sender, EventArgs e)
        {
            var attempt = new object();
            browserAttempt = attempt;
            bool Canceled() => loginCanceled || browserAttempt != attempt;
            panel1.Enabled = false;
            logingThread = ThreadManager.Instance.GetThread(() =>
            {
                var errorReason = WebManager.Instance.StartDeviceLogin(out var request);
                string loginKey = null;
                if (errorReason == ErrorReason.OK && !Canceled())
                {
                    InvokeIfOpen(() =>
                    {
                        if (Canceled())
                        {
                            return;
                        }
                        ShowBrowserWaiting(request.UserCode);
                        try
                        {
                            Process.Start(new ProcessStartInfo(request.VerificationUrl) { UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    });
                    var deadline = DateTime.Now.AddSeconds(request.ExpiresIn);
                    do
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(Math.Max(1, request.Interval)));
                        if (Canceled())
                        {
                            return;
                        }
                        errorReason = WebManager.Instance.PollDeviceLogin(request.DeviceCode, out loginKey);
                    } while (errorReason == ErrorReason.DeviceLoginPending && DateTime.Now < deadline);
                    if (errorReason == ErrorReason.DeviceLoginPending)
                    {
                        errorReason = ErrorReason.DeviceLoginExpired;
                    }
                }
                if (Canceled())
                {
                    return;
                }
                InvokeIfOpen(() =>
                {
                    if (Canceled())
                    {
                        return;
                    }
                    if (errorReason == ErrorReason.OK)
                    {
                        LoginKey = loginKey;
                        this.Close();
                    }
                    else
                    {
                        ShowLoginInputs();
                        ShowErrorMessage(errorReason);
                    }
                    logingThread = null;
                });
            });
            logingThread.Start();
        }

        // ShowBrowserWaiting は ID・パスワードの欄の代わりに、ブラウザで入力するコードを大きく出す。
        // 「ログイン情報を記憶する」とキャンセルは使えるままにする。
        private void ShowBrowserWaiting(string userCode)
        {
            label1.Text = Utility.Language["BrowserLoginWaiting"];
            label4.Text = userCode;
            foreach (Control c in new Control[] { label2, textBox1, label3, maskedTextBox1, button1, button3, linkLabel1 })
            {
                c.Visible = false;
            }
            label4.Visible = true;
            AcceptButton = null;
            panel1.Enabled = true;
        }

        // ShowLoginInputs は通常のログイン画面に戻す。
        private void ShowLoginInputs()
        {
            browserAttempt = null;
            label1.Text = Utility.Language["DoLogin"];
            label4.Visible = false;
            foreach (Control c in new Control[] { label2, textBox1, label3, maskedTextBox1, button1, button3, linkLabel1 })
            {
                c.Visible = true;
            }
            AcceptButton = button1;
            panel1.Enabled = true;
            logingThread = null;
        }

        // InvokeIfOpen はフォームが開いている間だけ UI のスレッドで action を実行する。
        private void InvokeIfOpen(Action action)
        {
            try
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    if (!IsDisposed)
                    {
                        action();
                    }
                }));
            }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidOperationException)
            {
                // 途中でフォームが閉じられた
            }
        }

        private static void ShowErrorMessage(ErrorReason errorReason)
        {
            string errorText = errorReason switch
            {
                ErrorReason.AuthFailed => Utility.Language["LoginAuthFailed"],
                ErrorReason.VersionUnmatch => Utility.Language["LoginVersionUnmatch"],
                ErrorReason.DeviceLoginDenied => Utility.Language["LoginDenied"],
                ErrorReason.DeviceLoginExpired => Utility.Language["LoginExpired"],
                _ => Utility.Language["LoginFailed"],
            };
            MessageBox.Show(errorText);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (label4.Visible)
            {
                // ブラウザでのログインをやめて元の画面に戻る（フォームは閉じない）
                DialogResult = DialogResult.None;
                ShowLoginInputs();
                return;
            }
            this.Close();
        }
    }
}
