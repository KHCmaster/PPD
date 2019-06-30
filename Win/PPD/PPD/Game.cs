using PPD.Forms;
using PPDFramework;
using PPDFramework.Web;
using PPDMovie;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace PPD
{
    class Game : GameCore
    {
        public Game(PPDExecuteArg args) : base(args, new MovieForm())
        {
            Form.MainForm.StartPosition = FormStartPosition.Manual;
            try
            {
                Form.MainForm.Location = PPD.Properties.Settings.Default.WindowLocation;
            }
            catch
            {
                PPD.Properties.Settings.Default.Reset();
            }
            Form.MainForm.FormClosing += (sender, e) =>
            {
                PPD.Properties.Settings.Default.WindowLocation = Form.MainForm.Location;
                PPD.Properties.Settings.Default.Save();
            };
        }

        private void Login()
        {
            ErrorReason errorReason = ErrorReason.OK;
            if (!String.IsNullOrEmpty(PPDGeneralSetting.Setting.Username))
            {
                errorReason = WebManager.Instance.Login(PPDGeneralSetting.Setting.Username, PPDGeneralSetting.Setting.Password);
                if (WebManager.Instance.IsLogined)
                {
                    return;
                }
            }

            ShowLoginFormImpl(errorReason);
        }

        public void ShowLoginForm()
        {
            ShowLoginFormImpl(ErrorReason.OK);
        }

        private void ShowLoginFormImpl(ErrorReason errorReason)
        {
            var loginForm = new LoginForm
            {
                Username = PPDGeneralSetting.Setting.Username,
                InitialLoginResult = errorReason
            };
            loginForm.ShowDialog();
            if (WebManager.Instance.IsLogined && loginForm.RememberMe)
            {
                PPDGeneralSetting.Setting.Username = loginForm.Username;
                PPDGeneralSetting.Setting.Password = loginForm.Password;
            }
        }

        protected override void OnWindowShown()
        {
            if (Args.Count == 0 || Args.ContainsKey("login"))
            {
                Login();
            }
        }

        protected override void OnFailedInitializeDirectX()
        {
            MessageBox.Show(Utility.Language["DirectXInitializeFailed"]);
        }

        protected override void OnSuccessInitializeDirectX()
        {
            MovieUtility.Window = Form.RenderForm.Handle;
        }
    }
}