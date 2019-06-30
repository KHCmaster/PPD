using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PPDFramework.Logger
{
    /// <summary>
    /// ロガークラスです。
    /// </summary>
    public class Logger
    {
        const string logFileName = "log.txt";

        private static Logger instance = new Logger();
        Queue<LogInfo> logs;

        /// <summary>
        /// インスタンスを取得します。
        /// </summary>
        public static Logger Instance
        {
            get
            {
                return instance;
            }
        }

        private Logger()
        {
            logs = new Queue<LogInfo>();
            var thread = ThreadManager.Instance.GetThread(WorkImpl);
            thread.IsBackground = true;
            thread.Start();
        }

        private void WorkImpl()
        {
            while (true)
            {
                LogInfo log = null;
                lock (logs)
                {
                    if (logs.Count > 0)
                    {
                        log = logs.Dequeue();
                    }
                }

                if (log == null)
                {
                    Thread.Sleep(1);
                    continue;
                }
                File.AppendAllText(logFileName, String.Format("[{0} {1}]{2}{3}", log.ThreadId, log.DateTime, log.Log, Environment.NewLine));
            }
        }

        /// <summary>
        /// ログを追加します。
        /// </summary>
        /// <param name="log">ログ。</param>
        public void AddLog(string log)
        {
            if (!PPDSetting.Setting.CollectLog)
            {
                return;
            }

            lock (logs)
            {
                logs.Enqueue(new LogInfo(log, Thread.CurrentThread.ManagedThreadId, DateTime.Now));
            }
        }

        /// <summary>
        /// ログを追加します。
        /// </summary>
        /// <param name="format">フォーマット。</param>
        /// <param name="args">ログ。</param>
        public void AddLog(string format, params object[] args)
        {
            if (!PPDSetting.Setting.CollectLog)
            {
                return;
            }

            AddLog(String.Format(format, args));
        }

        private class LogInfo
        {
            public string Log
            {
                get;
                private set;
            }

            public int ThreadId
            {
                get;
                private set;
            }

            public DateTime DateTime
            {
                get;
                private set;
            }

            public LogInfo(string log, int threadId, DateTime dateTime)
            {
                Log = log;
                ThreadId = threadId;
                DateTime = dateTime;
            }
        }
    }
}
