using System.Collections.Generic;
using System.Linq;

namespace PPDCore
{
    class Permutation
    {
        public static IEnumerable<T[]> Enumerate<T>(IEnumerable<T> nums)
        {
            return _GetPermutations<T>(new List<T>(), nums.ToList());
        }

        private static IEnumerable<T[]> _GetPermutations<T>(IEnumerable<T> perm, IEnumerable<T> nums)
        {
            if (nums.Count() == 0)
            {
                yield return perm.ToArray();
            }
            else
            {
                foreach (var n in nums)
                {
                    var result = _GetPermutations<T>(perm.Concat(new T[] { n }),
                                                     nums.Where(x => x.Equals(n) == false)
                                  );
                    foreach (var xs in result)
                        yield return xs.ToArray();
                }
            }
        }
    }
}
