using PPDPack;
using System;
using System.IO;

namespace PPDUnpack
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("PPDUnpack.exe [packfile]");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File does not exist.");
                return;
            }

            var baseDir = Path.GetFileNameWithoutExtension(args[0]);
            using (PackReader reader = new PackReader(args[0]))
            {
                foreach (var name in reader.FileList)
                {
                    var r = reader.Read(name);
                    byte[] b = new byte[r.Length];
                    r.Read(b, 0, b.Length);
                    var fileName = Path.Combine(baseDir, name);
                    var dirName = Path.GetDirectoryName(fileName);
                    if (!String.IsNullOrEmpty(dirName))
                    {
                        Directory.CreateDirectory(dirName);
                    }
                    File.WriteAllBytes(fileName, b);
                }
            }
        }
    }
}
