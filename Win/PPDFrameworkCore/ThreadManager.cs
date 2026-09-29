using System.Collections.Generic;
using System.Threading;

namespace PPDFrameworkCore
{
    public class ThreadManager
    {
        private static ThreadManager instance = new ThreadManager();
        public static ThreadManager Instance
        {
            get
            {
                return instance;
            }
        }

        List<Thread> threads;

        private ThreadManager()
        {
            threads = new List<Thread>();
        }

        public void Update()
        {
            lock (threads)
            {
                for (int i = threads.Count - 1; i >= 0; i--)
                {
                    if (!threads[i].IsAlive)
                    {
                        threads.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// アプリケーションの終了時に呼びます。
        /// スレッドはすべてバックグラウンドスレッドなので、プロセスの終了とともに止まります
        /// （.NET では Thread.Abort が使えないため、以前のように強制終了はしません）。
        /// </summary>
        public void Dispose()
        {
            lock (threads)
            {
                threads.Clear();
            }
        }

        public Thread GetThread(ThreadStart threadStart)
        {
            lock (threads)
            {
                var thread = new Thread(threadStart) { IsBackground = true };
                threads.Add(thread);
                return thread;
            }
        }

        public Thread GetThread(ParameterizedThreadStart threadStart)
        {
            lock (threads)
            {
                var thread = new Thread(threadStart) { IsBackground = true };
                threads.Add(thread);
                return thread;
            }
        }
    }
}
