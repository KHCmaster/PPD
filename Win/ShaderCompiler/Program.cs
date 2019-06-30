using System;
using System.IO;

namespace ShaderCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var templateFilePath = args[0];
            var filePath = Path.ChangeExtension(templateFilePath, "fx");
            if (File.Exists(filePath) && File.GetLastWriteTime(templateFilePath) == File.GetLastWriteTime(filePath))
            {
                Environment.Exit(-1);
                return;
            }
            var t1 = File.GetLastWriteTime(templateFilePath);
            var t2 = File.GetLastWriteTime(filePath);

            var parser = new Parser();
            parser.Parse(templateFilePath);
            File.WriteAllText(filePath, parser.ToStr());
            File.SetLastWriteTime(filePath, File.GetLastWriteTime(templateFilePath));
            Environment.Exit(0);
        }
    }
}
