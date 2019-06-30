using System;
using System.Collections.Generic;

namespace PPDSingle
{
    static class LinqExtension
    {
        public static int FindIndex<T>(this IEnumerable<T> collection, Func<T, bool> func)
        {
            int iter = 0;
            foreach (T t in collection)
            {
                if (func(t))
                {
                    return iter;
                }
                iter++;
            }
            return -1;
        }
    }
}
