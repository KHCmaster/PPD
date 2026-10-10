using System.Collections.Generic;
using System.Linq;

namespace PPDCore
{
    class Permutation
    {
        public static IEnumerable<T[]> Enumerate<T>(IEnumerable<T> nums)
        {
            return GetPermutations<T>([], [.. nums]);
        }

        private static IEnumerable<T[]> GetPermutations<T>(IEnumerable<T> perm, IEnumerable<T> nums)
        {
            if (!nums.Any())
            {
                yield return perm.ToArray();
            }
            else
            {
                foreach (var n in nums)
                {
                    var result = GetPermutations<T>(perm.Concat([n]),
                                                     nums.Where(x => x.Equals(n) == false)
                                  );
                    foreach (var xs in result)
                        yield return xs.ToArray();
                }
            }
        }
    }
}
