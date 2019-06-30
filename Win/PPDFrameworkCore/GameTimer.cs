using System.Diagnostics;

namespace PPDFrameworkCore
{
    /// <summary>
    /// ゲームタイマーのクラスです。
    /// </summary>
    public class GameTimer
    {
        private Stopwatch stopwatch;

        /// <summary>
        /// 経過した時間を取得します。
        /// </summary>
        public long ElapsedTime
        {
            get
            {
                return stopwatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// 経過した正確な時間を取得します。
        /// </summary>
        public double ElapsedTickTime
        {
            get
            {
                return stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000;
            }
        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        public GameTimer()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }
    }
}
