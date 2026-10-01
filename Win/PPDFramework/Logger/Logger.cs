using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace PPDFramework.Logger
{
    /// <summary>
    /// ロガークラスです。
    /// </summary>
    public class Logger
    {
        const string logFileName = "log.txt";

        private static readonly Logger instance = new();
        readonly Queue<LogInfo> logs;

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
            // ログが来るまで寝て待ち（以前は 1ms ごとに起きて調べていた）、たまった分をまとめて 1 回で書き込む
            var batch = new List<LogInfo>();
            var builder = new StringBuilder();
            while (true)
            {
                lock (logs)
                {
                    while (logs.Count == 0)
                    {
                        Monitor.Wait(logs);
                    }
                    while (logs.Count > 0)
                    {
                        batch.Add(logs.Dequeue());
                    }
                }

                foreach (var log in batch)
                {
                    builder.AppendFormat("[{0} {1}]{2}", log.ThreadId, log.DateTime, log.Log).Append(Environment.NewLine);
                }
                File.AppendAllText(logFileName, builder.ToString());
                builder.Clear();
                batch.Clear();
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
                logs.Enqueue(new LogInfo(log, Environment.CurrentManagedThreadId, DateTime.Now));
                Monitor.Pulse(logs);
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

        private class LogInfo(string log, int threadId, DateTime dateTime)
        {
            public string Log
            {
                get;
                private set;
            } = log;

            public int ThreadId
            {
                get;
                private set;
            } = threadId;

            public DateTime DateTime
            {
                get;
                private set;
            } = dateTime;
        }
    }
}
