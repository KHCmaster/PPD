namespace PPDPack
{
    public static class PackUtility
    {
        public static readonly byte[] FileSignature = [
            (byte)'P',
            (byte)'P',
            (byte)'D',
            (byte)'P',
            (byte)'A',
            (byte)'C',
            (byte)'K',
            (byte)'V',
            (byte)'1'
        ];

        public static int CheckHeaderStartIndex(byte[] array)
        {
            if (array == null || array.Length < FileSignature.Length)
            {
                return -1;
            }

            // 配列の最後で終わるところまで探す（以前は最後の位置を探していなかった）
            for (int i = 0; i <= array.Length - FileSignature.Length; i++)
            {
                if (CheckContainsArray(array, i))
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool CheckContainsArray(byte[] array, int startIndex)
        {

            for (int i = 0; i < FileSignature.Length; i++)
            {
                if (array[i + startIndex] != FileSignature[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
