namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    class BitUtility
    {
        public static int Ones(int value)
        {
            var x = (uint)value;
            x -= ((x >> 1) & 0x55555555);
            x = (((x >> 2) & 0x33333333) + (x & 0x33333333));
            x = (((x >> 4) + x) & 0x0f0f0f0f);
            x += (x >> 8);
            x += (x >> 16);
            return (int)(x & 0x0000003f);
        }

        public static int Int32BitCount()
        {
            return sizeof(int) * 8;
        }

        public static bool HasFlag(int value, int pos)
        {
            return (value & (1 << pos)) != 0;
        }
    }
}
