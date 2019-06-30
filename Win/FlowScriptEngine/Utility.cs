using System;
using System.Diagnostics;

namespace FlowScriptEngine
{
    class Utility
    {
        static Stopwatch stopWatch;
        static Utility()
        {
            stopWatch = new Stopwatch();
        }

        public static void Start()
        {
            stopWatch.Reset();
            stopWatch.Start();
        }

        public static void Stop(string sign)
        {
            stopWatch.Stop();
            Console.WriteLine("{0} Tick:{1}", sign, (double)stopWatch.ElapsedTicks / Stopwatch.Frequency);
        }
    }
}
