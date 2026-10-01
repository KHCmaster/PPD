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
        private static readonly WebSongInformationManager instance = new();

        private Thread updateWorkerThread;
        private volatile bool stopRequested;
        private Thread updateScoreWorkerThread;

        private WebSongInformation[] informations;
        private readonly Dictionary<string, WebSongInformation> webSongInfoHashDict = [];
        private readonly Dictionary<string, WebSongInformation> webSongInfoIDDict = [];

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
                    return [];
                }
                return [.. informations.Where(info => info.CanUpdate)];
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
                stopRequested = false;
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
            stopRequested = false;
            updateScoreWorkerThread = ThreadManager.Instance.GetThread(UpdateScoreImpl);
            updateScoreWorkerThread.Start();
        }

        public void Stop()
        {
            // .NET では Thread.Abort が使えないので、止めるよう要求する。
            // スレッドは次の区切り（1 曲ごと）で止まり、それ以降の結果は反映しない
            stopRequested = true;
            updateWorkerThread = null;
            updateScoreWorkerThread = null;
        }

        private void UpdateImpl()
        {
            try
            {
                var newInformations = WebManager.Instance.GetScores();
                if (stopRequested)
                {
                    return;
                }
                informations = newInformations;
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
                    if (stopRequested)
                    {
                        return;
                    }
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
                            Utility.CopyDirectory(dir, target.DirectoryPath, []);
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
