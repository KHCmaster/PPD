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

        public void Dispose()
        {
            lock (threads)
            {
                foreach (var thread in threads)
                {
                    while (thread.IsAlive)
                    {
                        thread.Abort();
                        if (thread.Join(10))
                        {
                            break;
                        }
                    }
                }
            }
        }

        public Thread GetThread(ThreadStart threadStart)
        {
            lock (threads)
            {
                var thread = new Thread(threadStart);
                threads.Add(thread);
                return thread;
            }
        }

        public Thread GetThread(ParameterizedThreadStart threadStart)
        {
            lock (threads)
            {
                var thread = new Thread(threadStart);
                threads.Add(thread);
                return thread;
            }
        }
    }
}
