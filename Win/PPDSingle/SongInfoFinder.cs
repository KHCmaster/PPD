using PPDFramework;
using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PPDSingle
{
    class SongInfoFinder
    {
        public string Name
        {
            get;
            set;
        }

        public SongSelectFilter Filter
        {
            get;
            private set;
        }

        public List<SongInformation> Result
        {
            get;
            private set;
        }

        public bool Finished
        {
            get;
            private set;
        }

        Queue<SongInformation> queue;
        public SongInfoFinder(SongSelectFilter filter)
        {
            Result = new List<SongInformation>();
            queue = new Queue<SongInformation>();
            this.Filter = filter;
        }

        Thread t;
        public bool Find()
        {
            if (t != null)
            {
                if (t.ThreadState != ThreadState.Running)
                {
                    Interrupt();
                }
                else
                {
                    return false;
                }
            }

            Finished = false;
            t = ThreadManager.Instance.GetThread(InnerFind);
            t.Start();
            return true;
        }

        private void Interrupt()
        {
            if (t != null && t.ThreadState == ThreadState.Running)
            {
                t.Interrupt();
                t.Join();
                t = null;
            }
        }

        private void InnerFind()
        {
            Result.Clear();
            queue.Clear();
            var sp = Name.ToLower().Split(new char[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);
            if (Name != string.Empty)
            {
                queue.Enqueue(SongInformation.Root);
                while (queue.Count > 0)
                {
                    var info = queue.Dequeue();
                    if (info.IsPPDSong)
                    {
                        if (Filter.Filter(info))
                        {
                            var lower = info.DirectoryName.ToLower();
                            if (Array.TrueForAll(sp, lower.Contains))
                            {
                                Result.Add(info);
                            }
                        }
                    }
                    else
                    {
                        foreach (SongInformation child in info.Children)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }
            }
            Finished = true;
        }
    }
}
