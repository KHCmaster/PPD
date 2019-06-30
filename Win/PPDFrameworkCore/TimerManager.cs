using System;
using System.Collections.Generic;

namespace PPDFrameworkCore
{
    public class TimerManager : ITimerManager
    {
        int iter = 1;
        SortedList<int, TimerInfo> list;
        SortedList<int, TimerInfo> onceList;
        Dictionary<int, TimerInfo> afterAddList;
        Dictionary<int, TimerInfo> afterOnceAddList;
        GameTimer gameTimer;
        private readonly object lockObject = new object();

        public TimerManager(GameTimer gameTimer)
        {
            this.gameTimer = gameTimer;
            list = new SortedList<int, TimerInfo>();
            onceList = new SortedList<int, TimerInfo>();
            afterAddList = new Dictionary<int, TimerInfo>();
            afterOnceAddList = new Dictionary<int, TimerInfo>();
        }

        public void Update()
        {
            long time = gameTimer.ElapsedTime;
            lock (lockObject)
            {
                foreach (TimerInfo timerInfo in list.Values)
                {
                    long count = (time - timerInfo.StartTime) / timerInfo.Interval;
                    for (int i = 0; i < count - timerInfo.CallCount; i++)
                    {
                        timerInfo.CallBack((int)(i + timerInfo.CallCount));
                    }
                    timerInfo.CallCount = count;
                }
                var removeList = new List<int>();
                foreach (var pair in onceList)
                {
                    if ((time - pair.Value.StartTime) / pair.Value.Interval >= 1)
                    {
                        pair.Value.CallBack((int)pair.Value.CallCount);
                        removeList.Add(pair.Key);
                    }
                }
                foreach (var remove in removeList)
                {
                    onceList.Remove(remove);
                }
                foreach (KeyValuePair<int, TimerInfo> kvp in afterAddList)
                {
                    list.Add(kvp.Key, kvp.Value);
                }
                foreach (var pair in afterOnceAddList)
                {
                    onceList.Add(pair.Key, pair.Value);
                }
                afterAddList.Clear();
                afterOnceAddList.Clear();
            }
        }

        public int AddTimerCallBack(Action<int> callback, int milliSec, bool onceExecute, bool immediate)
        {
            lock (lockObject)
            {
                if (onceExecute)
                {
                    afterOnceAddList.Add(iter, new TimerInfo(immediate ? 0 : gameTimer.ElapsedTime, milliSec, callback));
                }
                else
                {
                    afterAddList.Add(iter, new TimerInfo(immediate ? 0 : gameTimer.ElapsedTime, milliSec, callback));
                }
            }
            return iter++;
        }

        public void RemoveTimerCallBack(int id)
        {
            lock (lockObject)
            {
                if (list.ContainsKey(id))
                {
                    list.Remove(id);
                }
                if (onceList.ContainsKey(id))
                {
                    onceList.Remove(id);
                }
            }
        }

        class TimerInfo
        {
            public TimerInfo(long startTime, int interval, Action<int> callback)
            {
                StartTime = startTime;
                Interval = interval;
                CallBack = callback;
            }

            public long StartTime
            {
                get;
                private set;
            }

            public int Interval
            {
                get;
                private set;
            }

            public Action<int> CallBack
            {
                get;
                private set;
            }

            public long CallCount
            {
                get;
                set;
            }
        }
    }
}
