using PPDFramework.Web;
using PPDFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace PPDCore
{
    class ReviewManager
    {
        private List<Thread> threads = new List<Thread>();

        private static ReviewManager manager = new ReviewManager();

        private ReviewManager()
        {
        }

        public static ReviewManager Manager
        {
            get
            {
                return manager;
            }
        }

        public void Review(string str, int rate, byte[] scoreHash, Action<bool> endCallBack)
        {
            for (int i = threads.Count - 1; i >= 0; i--)
            {
                if (threads[i].ThreadState == ThreadState.Stopped)
                {
                    threads[i].Interrupt();
                    threads[i].Join();
                    threads.RemoveAt(i);
                }
            }
            var thread = ThreadManager.Instance.GetThread(Post);
            thread.Start(new ArrayList { str, rate, scoreHash, endCallBack });
        }

        private void Post(object obj)
        {
            var list = obj as ArrayList;
            try
            {
                var ret = WebManager.Instance.Review((string)list[0], (int)list[1], (byte[])list[2]);
                (list[3] as Action<bool>).Invoke(ret);
            }
            catch
            {
                (list[3] as Action<bool>).Invoke(false);
            }
        }
    }
}

