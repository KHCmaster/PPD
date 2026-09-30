using PPDUpdater.Model;
using System;
using System.IO;
using System.Windows.Forms;

namespace PPDUpdater.Executor
{
    class DownloadUpdateExecutor : CExecutor
    {
        private FileDownloader downloadClient;

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
            downloadClient = new FileDownloader();
            downloadClient.ProgressChanged += downloadClient_DownloadProgressChanged;
            downloadClient.Completed += downloadClient_DownloadFileCompleted;
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

        private void downloadClient_DownloadProgressChanged(int progressPercentage)
        {
            Progress = progressPercentage;
            OnProgress();
        }

        private void downloadClient_DownloadFileCompleted(Exception error)
        {
            Success = error == null;
            if (!Success)
            {
                ErrorLog += error.Message;
            }
            else
            {
                File.Create(Path.Combine(Dir, Utility.Complete)).Close();
            }
            OnFinish();
        }
    }
}
