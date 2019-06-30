using System.Collections.Generic;

namespace Effect2D
{
    /// <summary>
    /// ２分探索器
    /// </summary>
    public static class BinaryFinder
    {
        /// <summary>
        /// 一番近く前のインデックスを取得する
        /// </summary>
        /// <param name="list"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int FindNearest(IList<int> list, ref float data)
        {
            return FindNearest(list, ref data, 0, list.Count - 1);
        }
        private static int FindNearest(IList<int> list, ref float data, int sindex, int eindex)
        {
            if (eindex - sindex <= 1)
            {
                if (data < list[eindex]) return sindex;
                else return eindex;
            }
            int mindex = (eindex + sindex) / 2;
            if (data < list[mindex]) return FindNearest(list, ref data, sindex, mindex);
            else return FindNearest(list, ref data, mindex, eindex);
        }
    }
}
