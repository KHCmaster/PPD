using PPDUpdater.Model;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace PPDUpdater.Executor
{
    class DownloadUpdateExecutor : CExecutor
    {
        private WebClient downloadClient;

        public UpdateInfo UpdateInfo
        {
            get;
            private set;
        }

        public Uri URL
        {
            get;
            private set;
        }

        public string FilePath
        {
            get;
            private set;
        }

        public int Index
        {
            get;
            private set;
        }

        public string Dir
        {
            get;
            private set;
        }

        public DownloadUpdateExecutor(UpdateInfo updateInfo, Control control, int index)
            : base(control)
        {
            this.UpdateInfo = updateInfo;
            this.Index = index;
            URL = new Uri(UpdateInfo.UrlPath);
            Dir = Path.Combine(Utility.DownloadDirectory, updateInfo.VersionInfo.ToString());
            if (!Directory.Exists(Dir))
            {
                Directory.CreateDirectory(Dir);
            }
            FilePath = Path.Combine(Dir, URL.Segments[URL.Segments.Length - 1]);
            updateInfo.FilePath = FilePath;
            downloadClient = new WebClient();
            downloadClient.DownloadProgressChanged += downloadClient_DownloadProgressChanged;
            downloadClient.DownloadFileCompleted += downloadClient_DownloadFileCompleted;
        }

        public override void Execute()
        {
            base.Execute();
            try
            {
                downloadClient.DownloadFileAsync(URL, FilePath);
            }
            catch (Exception e)
            {
                ErrorLog = e.Message + "\r\n" + e.StackTrace;
                Success = false;
            }
        }

        private void downloadClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
            OnProgress();
        }

        private void downloadClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null) Success = false;
            else if (e.Cancelled) Success = false;
            else Success = true;
            if (!Success)
            {
                ErrorLog += e.Error.Message;
            }
            else
            {
                File.Create(Path.Combine(Dir, Utility.Complete)).Close();
            }
            OnFinish();
        }
    }
}
