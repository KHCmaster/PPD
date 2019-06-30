using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PPDInstaller.Executor
{
    class IPAFontInstallExecutor : CExecutor
    {
        public static Uri ipafonturi = new Uri("http://ossipedia.ipa.go.jp/ipafont/ipag00303.php");
        System.Net.WebClient downloadClient = new System.Net.WebClient();
        string filePath;
        string unzipPath;
        public IPAFontInstallExecutor(string filePath, string unzipPath, Control control)
            : base(control)
        {
            this.filePath = filePath;
            this.unzipPath = unzipPath;
        }

        public override void Execute()
        {
            base.Execute();
            try
            {
                downloadClient.DownloadProgressChanged += IPAFontDownloadProgressChanged;
                downloadClient.DownloadFileCompleted += IPAFontDownloadFileCompleted;
                downloadClient.DownloadFileAsync(ipafonturi, filePath);
                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                ErrorLog = string.Format("IPAFont Install Error:\n{0}\n{1}", e.Message, e.StackTrace);
                OnFinish();
            }
        }

        void IPAFontDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage.ToString();
            OnProgress();
        }
        void IPAFontDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                downloadClient.DownloadProgressChanged -= IPAFontDownloadProgressChanged;
                downloadClient.DownloadFileCompleted -= IPAFontDownloadFileCompleted;
                Utility.Unzip(filePath, unzipPath);
                var cachefolder = Path.GetDirectoryName(unzipPath);
                var dir = Path.Combine(cachefolder, "ipag00302\\ipag00303");
                foreach (string fileName in Directory.GetDirectories(unzipPath))
                {
                    dir = fileName;
                    break;
                }
                //copy ipag00302\ipag00302\ipag.ttf %systemroot%\fonts
                //regedit /s font.reg
                var sw = new StreamWriter(Path.Combine(dir, "temp.bat"), false, Encoding.Default);
                sw.Write("@echo off");
                sw.WriteLine();
                sw.Write("copy ipag.ttf %systemroot%\\fonts");
                sw.WriteLine();
                sw.Write("regedit /s font.reg");
                sw.Close();
                sw = new StreamWriter(Path.Combine(dir, "font.reg"), false, Encoding.Default);
                sw.WriteLine("REGEDIT4");
                sw.WriteLine();
                sw.WriteLine("[HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Fonts]");
                sw.WriteLine("\"IPAゴシック (TrueType)\" = \"ipag.ttf\"");
                sw.Close();
                var p = new System.Diagnostics.Process();
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = System.Environment.GetEnvironmentVariable("ComSpec"),
                    CreateNoWindow = true,
                    Arguments = @"/c temp.bat",
                    WorkingDirectory = dir
                };
                p.StartInfo = psi;
                p.Start();
                p.WaitForExit();
                Success = true;
            }
            catch (Exception ex)
            {
                ErrorLog = string.Format("IPAFont Install Error:\n{0}\n{1}", ex.Message, ex.StackTrace);
            }
            OnFinish();
        }
    }
}
