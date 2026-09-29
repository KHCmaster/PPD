using System;
using System.IO;

namespace ShaderCompiler
{
    class Program
    {
        /// <summary>
        /// 使い方:
        ///   ShaderCompiler.exe <テンプレート.fxt>                 テンプレートを展開して .fx を作る
        ///   ShaderCompiler.exe <テンプレート.fxt> <出力.bin>      さらに .fx をコンパイルしてバイナリを作る
        /// </summary>
        static int Main(string[] args)
        {
            var templateFilePath = args[0];
            var filePath = Path.ChangeExtension(templateFilePath, "fx");
            var generated = false;
            if (!File.Exists(filePath) || File.GetLastWriteTime(templateFilePath) != File.GetLastWriteTime(filePath))
            {
                var parser = new Parser();
                parser.Parse(templateFilePath);
                File.WriteAllText(filePath, parser.ToStr());
                File.SetLastWriteTime(filePath, File.GetLastWriteTime(templateFilePath));
                generated = true;
            }

            if (args.Length < 2)
            {
                return generated ? 0 : -1;
            }

            try
            {
                new EffectCompiler().Compile(filePath, args[1]);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
        }
    }
}
