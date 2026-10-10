using System;
using System.Collections.Generic;
using System.Threading;

namespace PPDFrameworkCore
{
    public class TimerManager(GameTimer gameTimer) : ITimerManager
    {
        int iter = 1;
        readonly SortedList<int, TimerInfo> list = [];
        readonly SortedList<int, TimerInfo> onceList = [];
        readonly Dictionary<int, TimerInfo> afterAddList = [];
        readonly Dictionary<int, TimerInfo> afterOnceAddList = [];
        readonly GameTimer gameTimer = gameTimer;
        private readonly Lock lockObject = new();

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
                list.Remove(id);
                onceList.Remove(id);
            }
        }

        class TimerInfo(long startTime, int interval, Action<int> callback)
        {
            public long StartTime
            {
                get;
                private set;
            } = startTime;

            public int Interval
            {
                get;
                private set;
            } = interval;

            public Action<int> CallBack
            {
                get;
                private set;
            } = callback;

            public long CallCount
            {
                get;
                set;
            }
        }
    }
}
