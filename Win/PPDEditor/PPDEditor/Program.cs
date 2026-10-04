using PPDEditor.Controls;
using PPDEditor.Forms;
using PPDFramework;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace PPDEditor
{
    static class Program
    {
        public static string AppDir
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        public static string PPDExePath
        {
            get
            {
                string dir = AppDir;
#if DEBUG
                for (int i = 0; i < 5; i++)
                {
                    dir = Directory.GetParent(dir).FullName;
                }
                dir = Path.Combine(dir, @"PPD\PPD\bin\x64\Debug");
#endif
                return Path.Combine(dir, "PPD.exe");
            }
        }

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Environment.CurrentDirectory = AppDir;
            var errorHandler = new ErrorHandlerPPD();
            errorHandler.Initialize();
            var application = new Application
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };
            // 画面の操作の中で起きた例外は、以前と同じく知らせてから続ける（ゲームのフレームの中の例外は Game が止めて、下で処理する）
            application.DispatcherUnhandledException += (sender, e) =>
            {
                errorHandler.ProcessError(e.Exception);
                e.Handled = true;
            };
            Game game = null;
            try
            {
                PPDSetting.Initialize();
                game = new MyGame(new PPDExecuteArg([]));
                game.Run(application);
                if (game.FatalException != null)
                {
                    throw new InvalidOperationException("An error occurred in the game loop.", game.FatalException);
                }
            }
            catch (Exception e)
            {
                SplashWindow.CloseSplash();
                errorHandler.ProcessError(e.InnerException ?? e);
                if (game != null)
                {
                    try
                    {
                        game.Window.ViewModel.RescueData();
                    }
                    catch
                    {

                    }
                    game.Window.ForceClose();
                }
            }
            finally
            {
                game?.Dispose();
                game?.Window.Dispose();
                GameView.DestroyRenderWindow();
            }
        }
    }
}
