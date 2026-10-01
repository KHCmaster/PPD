using PPDConfiguration;
using System;
using System.IO;
using System.Windows.Forms;

namespace PPDInstaller.Executor
{
    class InstallListExecutor : CExecutor
    {
        public static Uri directshowliburi = new("http://projectdxxx.me/installer/InstallURL.txt");
        readonly FileDownloader downloadClient = new();
        readonly string filePath = "./InstallURL.txt";
        public InstallListExecutor(Control control)
            : base(control)
        {
        }

        public override void Execute()
        {
            base.Execute();
            try
            {
                downloadClient.ProgressChanged += InstallListDownloadProgressChanged;
                downloadClient.Completed += InstallListDownloadFileCompleted;
                downloadClient.DownloadFileAsync(directshowliburi, filePath);
            }
            catch (Exception e)
            {
                ErrorLog = string.Format("InstallList Error:\n{0}\n{1}", e.Message, e.StackTrace);
                OnFinish();
            }
        }
        void InstallListDownloadProgressChanged(int progressPercentage)
        {
            Progress = progressPercentage.ToString();
            OnProgress();
        }
        void InstallListDownloadFileCompleted(Exception error)
        {
            try
            {
                downloadClient.ProgressChanged -= InstallListDownloadProgressChanged;
                downloadClient.Completed -= InstallListDownloadFileCompleted;
                using (StreamReader sr = new(filePath))
                {
                    var setting = new SettingReader(sr.ReadToEnd());
                    IPAFontInstallExecutor.ipafonturi = new Uri(setting["IPAFont"]);
                    LAVFiltersInstallExecutor.lavFiltersUri = new Uri(setting["LAVFilters"]);
                }
                Success = true;
            }
            catch (Exception ex)
            {
                ErrorLog = string.Format("InstallList Error:\n{0}\n{1}", ex.Message, ex.StackTrace);
            }
            OnFinish();
        }
    }
}
