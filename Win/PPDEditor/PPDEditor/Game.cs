using PPDEditor.Forms;
using PPDFramework;
using System;
using System.Windows;
using System.Windows.Threading;

namespace PPDEditor
{
    /// <summary>
    /// エディターのゲームの部分です。メインのウィンドウ（WPF）の中のゲームの画面に描きます。
    /// </summary>
    /// <remarks>
    /// 以前の WinForms の RenderLoop と同じく、画面の更新を待っている処理がないときに 1 フレームずつ進める
    /// （Dispatcher の Background の優先度で、次のフレームを予約し続ける）。
    /// </remarks>
    public class Game : GameCoreBase
    {
        bool running;
        Application application;

        /// <summary>
        /// フレームの処理で起きて、ゲームを止めた例外です。
        /// </summary>
        public Exception FatalException
        {
            get;
            private set;
        }

        public EditorForm Window
        {
            get;
            private set;
        }

        public Game(PPDExecuteArg args) : this(args, new EditorForm())
        {
        }

        private Game(PPDExecuteArg args, EditorForm window) : base(args, window.GameView.RenderHandle)
        {
            Window = window;
            SplashWindow.ShowSplash(Window);
            SplashWindow.ChangeStatus(String.Format(Utility.Language["InitializingStart"], Utility.Language["MainWindow"]));
            Window.Initialize();
        }

        /// <summary>
        /// DirectX を初期化して、ウィンドウを表示し、閉じるまでフレームを進めます。
        /// </summary>
        public void Run(Application application)
        {
            this.application = application;
            if (!InitializeDirectX(PPDSetting.Setting.Width, PPDSetting.Setting.Height, 800, 450))
            {
                OnFailedInitializeDirectX();
                return;
            }
            OnSuccessInitializeDirectX();
            Window.Closed += Window_Closed;
            Window.Show();
            Initialize();
            running = true;
            ScheduleFrame();
            application.Run(Window);
        }

        private void ScheduleFrame()
        {
            Window.Dispatcher.BeginInvoke(DispatcherPriority.Background, RunFrame);
        }

        private void RunFrame()
        {
            if (!running)
            {
                return;
            }
            try
            {
                Routine();
            }
            catch (Exception e)
            {
                // 以前の RenderLoop と同じく、フレームの中の例外ではゲームを止める（Program がデータを退避して閉じる）
                FatalException = e;
                running = false;
                application.Shutdown();
                return;
            }
            if (ShouldBeExit)
            {
                ShouldBeExit = false;
                running = false;
                Window.Close();
                return;
            }
            ScheduleFrame();
        }

        void Window_Closed(object sender, EventArgs e)
        {
            running = false;
            // エディターのウィンドウを閉じたら、アプリを終わらせる。
            // ShutdownMode.OnMainWindowClose はアプリのメインのウィンドウ（Application.MainWindow）が閉じたときだけ終わるので、
            // ほかのウィンドウがメインのウィンドウになっていると、閉じてもプロセスが残る
            application?.Shutdown();
        }
    }
}
