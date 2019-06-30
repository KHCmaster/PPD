using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PPDFramework
{
#if BENCHMARK

    /// <summary>
    /// ベンチマークのクラスです。
    /// </summary>
    public class Benchmark
    {
        private static Benchmark instance = new Benchmark();

        private int currentLoopIndex;
        private Dictionary<string, double> times;
        private Dictionary<string, Dictionary<string, double>> categoryTimes;
        private Stopwatch stopwatch;

        /// <summary>
        /// インスタンスを取得します。
        /// </summary>
        public static Benchmark Instance
        {
            get { return instance; }
        }

        private Benchmark()
        {
            times = new Dictionary<string, double>();
            categoryTimes = new Dictionary<string, Dictionary<string, double>>();
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        /// <summary>
        /// 開始します。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public BenchmarkHandler Start(string key)
        {
            if (!times.ContainsKey(key))
            {
                times[key] = 0;
            }
            return new BenchmarkHandler(stopwatch.ElapsedTicks, key);
        }

        /// <summary>
        /// 開始します。
        /// </summary>
        /// <param name="category"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public BenchmarkHandler Start(string category, string key)
        {
            if (!categoryTimes.TryGetValue(category, out Dictionary<string, double> dict))
            {
                dict = new Dictionary<string, double>();
                categoryTimes[category] = dict;
            }
            if (!dict.ContainsKey(key))
            {
                dict[key] = 0;
            }
            return new BenchmarkHandler(stopwatch.ElapsedTicks, category, key);
        }

        internal void Stop(string key, long ticks)
        {
            times[key] += (stopwatch.ElapsedTicks - ticks) / (double)Stopwatch.Frequency;
        }

        internal void Stop(string category, string key, long ticks)
        {
            categoryTimes[category][key] += (stopwatch.ElapsedTicks - ticks) / (double)Stopwatch.Frequency;
        }

        /// <summary>
        /// １ループを終わります。
        /// </summary>
        public void EndLoop()
        {
            currentLoopIndex++;
            if (currentLoopIndex == 60)
            {
                foreach (var p in times)
                {
                    Console.WriteLine("{0}: {1:F2} ms", p.Key, p.Value * 1000 / 60);
                }
                var sum = times.Sum(p => p.Value);
                Console.WriteLine("Total: {0:F2} ms", sum * 1000 / 60);
                times.Clear();
                currentLoopIndex = 0;
            }
        }

        /// <summary>
        /// １ループを終わります。
        /// </summary>
        public void EndCategory()
        {
            foreach (var p in categoryTimes)
            {
                foreach (var pp in p.Value)
                {
                    Console.WriteLine("{0}: {1}: {2:F2} ms", p.Key, pp.Key, pp.Value * 1000 / 60);
                }
            }
            categoryTimes.Clear();
        }
    }

    /// <summary>
    /// ベンチマークのハンドラーです。
    /// </summary>
    public class BenchmarkHandler : ReturnableComponent
    {
        string key;
        long ticks;
        string category;

        internal BenchmarkHandler(long ticks, string key)
        {
            this.ticks = ticks;
            this.key = key;
        }

        internal BenchmarkHandler(long ticks, string category, string key)
        {
            this.ticks = ticks;
            this.key = key;
            this.category = category;
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            if (String.IsNullOrEmpty(category))
            {
                Benchmark.Instance.Stop(key, ticks);
            }
            else
            {
                Benchmark.Instance.Stop(category, key, ticks);
            }
        }
    }
#endif
}
