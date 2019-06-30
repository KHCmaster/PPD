using PPDFramework;
using PPDFramework.Web;
using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace PPDSingle
{
    class WebSongInformationManager
    {
        private static WebSongInformationManager instance = new WebSongInformationManager();

        private Thread updateWorkerThread;
        private Thread updateScoreWorkerThread;

        private WebSongInformation[] informations;
        private Dictionary<string, WebSongInformation> webSongInfoHashDict = new Dictionary<string, WebSongInformation>();
        private Dictionary<string, WebSongInformation> webSongInfoIDDict = new Dictionary<string, WebSongInformation>();

        public static WebSongInformationManager Instance
        {
            get
            {
                return instance;
            }
        }

        public DateTime LastUpdateTime
        {
            get;
            private set;
        }

        public WebSongInformation this[string scoreHash]
        {
            get
            {
                webSongInfoHashDict.TryGetValue(scoreHash, out WebSongInformation ret);
                return ret;
            }
        }

        public WebSongInformation[] UpdatableInformations
        {
            get
            {
                if (informations == null)
                {
                    return new WebSongInformation[0];
                }
                return informations.Where(info => info.CanUpdate).ToArray();
            }
        }

        public int UpdatableCount
        {
            get
            {
                return UpdatableInformations.Length;
            }
        }

        public event Action Updated;
        public event Action<int, SongInformation> ScoreUpdated;
        public event Action<int, SongInformation> ScoreUpdateFailed;

        private WebSongInformationManager()
        {
        }

        public WebSongInformation FindById(string id)
        {
            webSongInfoIDDict.TryGetValue(id, out WebSongInformation ret);
            return ret;
        }

        public void Update(bool async)
        {
            if (DateTime.Now - LastUpdateTime < TimeSpan.FromHours(1))
            {
                if (informations != null)
                {
                    foreach (WebSongInformation info in informations)
                    {
                        info.CheckNewOrCanUpdate();
                    }
                }
                Updated?.Invoke();
                return;
            }

            if (async)
            {
                if (updateWorkerThread != null && updateWorkerThread.IsAlive)
                {
                    return;
                }
                updateWorkerThread = ThreadManager.Instance.GetThread(UpdateImpl);
                updateWorkerThread.Start();
            }
            else
            {
                if (updateWorkerThread != null && updateWorkerThread.IsAlive)
                {
                    while (updateWorkerThread != null && updateWorkerThread.IsAlive)
                    {
                        Thread.Sleep(10);
                    }
                }
                else
                {
                    UpdateImpl();
                }
            }
        }

        public void UpdateScore()
        {
            updateScoreWorkerThread = ThreadManager.Instance.GetThread(UpdateScoreImpl);
            updateScoreWorkerThread.Start();
        }

        public void Stop()
        {
            if (updateWorkerThread != null && updateWorkerThread.IsAlive)
            {
                updateWorkerThread.Abort();
                updateWorkerThread = null;
            }
            if (updateScoreWorkerThread != null && updateScoreWorkerThread.IsAlive)
            {
                updateScoreWorkerThread.Abort();
                updateScoreWorkerThread = null;
            }
        }

        private void UpdateImpl()
        {
            try
            {
                informations = WebManager.Instance.GetScores();
                webSongInfoHashDict.Clear();
                webSongInfoIDDict.Clear();
                foreach (var information in informations)
                {
                    foreach (var diff in information.Difficulties)
                    {
                        webSongInfoHashDict[diff.Hash] = information;
                    }
                    webSongInfoIDDict[information.Hash] = information;
                }
                LastUpdateTime = DateTime.Now;
                Updated?.Invoke();
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception)
            {
            }
        }

        private void UpdateLastTime(string dir)
        {
            DateTime now = DateTime.Now;
            Directory.SetLastWriteTime(dir, now);
            foreach (string childDir in Directory.GetDirectories(dir))
            {
                UpdateLastTime(childDir);
            }
            foreach (string file in Directory.GetFiles(dir))
            {
                File.SetLastWriteTime(file, now);
            }
        }

        private void UpdateScoreImpl()
        {
            try
            {
                int iter = 1;
                WebSongInformation[] list = UpdatableInformations;
                foreach (WebSongInformation webSongInfo in list)
                {
                    try
                    {
                        var tempPath = Path.GetTempFileName();
                        WebManager.Instance.DownloadFile(webSongInfo, tempPath);
                        var target = webSongInfo.GetSongInformation();

                        var unzipDir = Path.Combine(Path.GetTempPath(), "Temp");
                        if (Directory.Exists(unzipDir))
                        {
                            Directory.Delete(unzipDir, true);
                        }
                        Directory.CreateDirectory(unzipDir);
                        Utility.FastZip.ExtractZip(tempPath, unzipDir, "");
                        foreach (string dir in Directory.GetDirectories(unzipDir))
                        {
                            Utility.CopyDirectory(dir, target.DirectoryPath, new System.Text.RegularExpressions.Regex[0]);
                        }
                        UpdateLastTime(target.DirectoryPath);

                        ScoreUpdated?.Invoke(iter, target);
                    }
                    catch (Exception)
                    {
                        ScoreUpdateFailed?.Invoke(iter, null);
                    }
                    finally
                    {
                        iter++;
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception)
            {
            }
        }
    }
}
