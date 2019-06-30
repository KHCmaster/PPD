using PPD.Update;
using PPDFramework;
using PPDFrameworkCore;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace PPD
{
    static class Program
    {
        public static string AsmDir
        {
            get;
            private set;
        }

        static Process expansionProcess;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            AsmDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
#if DEBUG
            try
            {
                Initialize(args);
            }
            finally
            {
                ThreadManager.Instance.Dispose();
            }
#else
            var mutex = new System.Threading.Mutex(false, "PPD");
            try
            {
                if (mutex.WaitOne(0, false) == false)
                {
                    MessageBox.Show("PPD is already running.");
                    return;
                }
                var ps = Process.GetProcessesByName("PPD");
                if (ps != null && ps.Length > 0)
                {
                    foreach (var p in ps)
                    {
                        if (Process.GetCurrentProcess().Id != p.Id)
                        {
                            p.Kill();
                        }
                    }
                }

                Initialize(args);
            }
            finally
            {
                try
                {
                    ThreadManager.Instance.Dispose();
                }
                finally
                {
                    mutex.ReleaseMutex();
                    mutex.Close();
                }
            }
#endif
            Environment.Exit(0);
        }

        static void Initialize(string[] args)
        {
            var arg = new PPDExecuteArg(args);
            PPDSetting.Initialize(
                isDebug: arg.Count > 0,
                songDir: arg.ContainsKey("songdir") ? arg["songdir"] : "",
                langIso: arg.ContainsKey("lang") ? arg["lang"] : null,
                disableExpansion: arg.ContainsKey("disableexpansion"),
                disableShader: arg.ContainsKey("disableshader"));
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var errorHandler = new SendErrorHandler();
            errorHandler.Initialize();
            if (args == null || args.Length == 0)
            {
                if (!CheckInstallInfo())
                {
                    MessageBox.Show(Utility.Language["InstallInfoError"]);
                    return;
                }
                if (!CheckVersion())
                {
                    MessageBox.Show(Utility.Language["UpdateExists"]);
#if DEBUG
#else
                    if (File.Exists("PPDUpdater.exe"))
                    {
                        var psi = new ProcessStartInfo();
                        psi.FileName = "PPDUpdater.exe";
                        Process.Start(psi);
                    }
#endif
                    return;
                }
                if (!CheckAssembly(out string errors))
                {
                    MessageBox.Show(String.Format("{0}:\r\n{1}", Utility.Language["AssemblyError"], errors));
                    return;
                }
            }
            RunExpansion();
            using (MyGame game = new MyGame(arg))
            {
                try
                {
                    game.Run();
                }
                catch (Exception e)
                {
                    errorHandler.ProcessError(e);
                }
            }

            if (expansionProcess != null && !expansionProcess.HasExited)
            {
                expansionProcess.CloseMainWindow();
                if (!expansionProcess.WaitForExit(1000))
                {
                    expansionProcess.Kill();
                }
            }
        }

        static void RunExpansion()
        {
            if (!PPDSetting.Setting.RunExpansion)
            {
                return;
            }
            try
            {
                expansionProcess = Process.Start(Path.Combine(AsmDir, "PPDExpansion.exe"), PPDSetting.Setting.ExpansionWaitPort.ToString());
            }
            catch
            {
            }
        }

        static bool CheckInstallInfo()
        {
            var checker = new Checker();
            return checker.CheckInstallInfo();
        }

        static bool CheckVersion()
        {
            var checker = new Checker();
            return checker.CheckUpdate();
        }

        static bool CheckAssembly(out string errors)
        {
            var checker = new Checker();
            return checker.CheckAssembly(out errors);
        }
    }
}