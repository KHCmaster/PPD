using System;
using System.IO;


namespace PPD
{
    public static class DebugWriter
    {
        const string filename = "debug.dat";
        public static void Write(string str)
        {
            using (StreamWriter sw = new StreamWriter(filename, true))
            {
                sw.WriteLine(DateTime.Now);
                sw.WriteLine(str);
            }
        }
    }
}
