using System;
using System.Windows.Forms;

namespace PPDInstaller.Executor
{
    class LAVFiltersInstallExecutor : CExecutor
    {
        public static Uri lavFiltersUri = new Uri("http://lavfilters.googlecode.com/files/LAVFilters-0.58.2.exe");
        System.Net.WebClient downloadClient = new System.Net.WebClient();

        private string filePath;
        public LAVFiltersInstallExecutor(string filePath, Control control)
            : base(control)
        {
            this.filePath = filePath;
        }

        public override void Execute()
        {
            base.Execute();
            try
            {
                downloadClient.DownloadProgressChanged += downloadClient_DownloadProgressChanged;
                downloadClient.DownloadFileCompleted += downloadClient_DownloadFileCompleted;
                downloadClient.DownloadFileAsync(lavFiltersUri, filePath);
                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                ErrorLog = string.Format("LAVFilters Install Error:\n{0}\n{1}", e.Message, e.StackTrace);
                OnFinish();
            }
        }

        void downloadClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                downloadClient.DownloadProgressChanged -= downloadClient_DownloadProgressChanged;
                downloadClient.DownloadFileCompleted -= downloadClient_DownloadFileCompleted;
                var p = new System.Diagnostics.Process();
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = "/SILENT"
                };
                p.StartInfo = psi;
                p.Start();
                p.WaitForExit();
                Success = true;
            }
            catch (Exception ex)
            {
                ErrorLog = string.Format("LAVFilters Install Error:\n{0}\n{1}", ex.Message, ex.StackTrace);
            }
            OnFinish();
        }

        void downloadClient_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage.ToString();
            OnProgress();
        }
    }
}
