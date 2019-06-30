using PPDConfiguration;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace PPDInstaller.Executor
{
    class InstallListExecutor : CExecutor
    {
        public static Uri directshowliburi = new Uri("http://projectdxxx.me/installer/InstallURL.txt");
        System.Net.WebClient downloadClient = new System.Net.WebClient();
        string filePath = "./InstallURL.txt";
        public InstallListExecutor(Control control)
            : base(control)
        {
        }

        public override void Execute()
        {
            base.Execute();
            try
            {
                downloadClient.DownloadProgressChanged += InstallListDownloadProgressChanged;
                downloadClient.DownloadFileCompleted += InstallListDownloadFileCompleted;
                downloadClient.DownloadFileAsync(directshowliburi, filePath);
            }
            catch (Exception e)
            {
                ErrorLog = string.Format("InstallList Error:\n{0}\n{1}", e.Message, e.StackTrace);
                OnFinish();
            }
        }
        void InstallListDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage.ToString();
            OnProgress();
        }
        void InstallListDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                downloadClient.DownloadProgressChanged -= InstallListDownloadProgressChanged;
                downloadClient.DownloadFileCompleted -= InstallListDownloadFileCompleted;
                using (StreamReader sr = new StreamReader(filePath))
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
