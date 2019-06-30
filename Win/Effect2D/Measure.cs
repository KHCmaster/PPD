using System.Collections.Generic;
using System.Diagnostics;

namespace Effect2D
{
    static class Measure
    {
        static Stopwatch stopwatch;
        static Stack<long> stack;
        static Measure()
        {

            stopwatch = new Stopwatch();
            stopwatch.Start();
            stack = new Stack<long>();
        }

        public static long Start()
        {
            long val = stopwatch.ElapsedMilliseconds;
            stack.Push(val);
            return val;
        }

        public static long Stop()
        {
            return stopwatch.ElapsedMilliseconds - stack.Pop();
        }
    }
}
