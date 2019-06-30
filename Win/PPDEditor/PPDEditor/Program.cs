using PPDEditor.Forms;
using PPDFramework;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

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
            System.Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            var errorHandler = new ErrorHandlerPPD();
            errorHandler.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Game game = null;
            try
            {
                PPDSetting.Initialize();
                game = new MyGame(new PPDExecuteArg(new string[] { }));
                game.Run();
            }
            catch (Exception e)
            {
                SplashForm.CloseSplash();
                errorHandler.ProcessError(e);
                if (game != null)
                {
                    try
                    {
                        game.Window.RescueData();
                    }
                    catch
                    {

                    }
                    game.Window.Close();
                }
            }
            finally
            {
                if (game != null)
                {
                    game.Dispose();
                    game = null;
                }
            }
        }
    }
}