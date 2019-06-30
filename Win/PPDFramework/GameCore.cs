using PPDFrameworkCore;
using SharpDX.Windows;
using System.Windows.Forms;

namespace PPDFramework
{
    /// <summary>
    /// ゲームのコアクラスです。
    /// </summary>
    public abstract class GameCore : GameCoreBase
    {
        /// <summary>
        /// ゲームフォームを取得します。
        /// </summary>
        public IGameForm Form
        {
            get;
            private set;
        }

        /// <summary>
        /// 現在のFPSを設定します。
        /// </summary>
        protected override string CurrentFPS
        {
            set
            {
                Form.MainForm.Invoke(() =>
                {
                    Form.MainForm.Text = value;
                });
            }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="args"></param>
        /// <param name="form"></param>
        protected GameCore(PPDExecuteArg args, IGameForm form) : base(args, new TextEditableControl())
        {
            Form = form;
            gameTimer = new GameTimer();
        }

        /// <summary>
        /// ウィンドウが表示されたときの処理です。
        /// </summary>
        protected virtual void OnWindowShown()
        {
        }

        /// <summary>
        /// ウィンドウが閉じられたときの処理です。
        /// </summary>
        protected virtual void OnWindowClose()
        {
        }

        /// <summary>
        /// 実行します。
        /// </summary>
        public void Run()
        {
            var width = 0;
            var height = 0;
            if (!FullScreen)
            {
                width = PPDSetting.Setting.Width;
                height = PPDSetting.Setting.Height;
            }
            else
            {
                width = Screen.PrimaryScreen.Bounds.Width;
                height = Screen.PrimaryScreen.Bounds.Height;
            }
            if (!InitializeDirectX(width, height, 800, 450))
            {
                OnFailedInitializeDirectX();
                return;
            }
            OnSuccessInitializeDirectX();
            Form.RenderForm.Controls.Add(Control);
            Control.Dock = DockStyle.Fill;
            Form.MainForm.Show();
            OnWindowShown();
            Initialize();
            RenderLoop.Run(Form.RenderForm, Render, true);
            OnWindowClose();
        }

        private void Render()
        {
            Routine();
            if (ShouldBeExit)
            {
                ShouldBeExit = false;
                Form.MainForm.Close();
            }
        }

        /// <summary>
        /// 初期化の処理です。
        /// </summary>
        protected override void InitializeUI()
        {
            base.InitializeUI();
            if (FullScreen)
            {
                Form.RenderForm.ClientSize = new System.Drawing.Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                Form.RenderForm.FormBorderStyle = FormBorderStyle.None;
                Form.RenderForm.WindowState = FormWindowState.Maximized;
            }
        }
    }
}