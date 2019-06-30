using SharpDX;
using System;

namespace PPDMulti.Data
{
    class RandomColorGenerator
    {
        static byte[] table = { 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
        public static Color4 GetColor()
        {
            var rand = new Random();
            return new Color4(table[rand.Next(0, table.Length)] / 255f, table[rand.Next(0, table.Length)] / 255f, table[rand.Next(0, table.Length)] / 255f, 1);
        }
    }
}
