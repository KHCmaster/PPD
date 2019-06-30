using System;

namespace PPDFrameworkCore
{
    public interface ITimerManager
    {
        /// <summary>
        /// コールバックを追加します。
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="milliSec"></param>
        /// <param name="onceExecute"></param>
        /// <param name="immediate"></param>
        /// <returns></returns>
        int AddTimerCallBack(Action<int> callback, int milliSec, bool onceExecute, bool immediate);

        /// <summary>
        /// コールバックを除きます
        /// </summary>
        /// <param name="id"></param>
        void RemoveTimerCallBack(int id);
    }
}
