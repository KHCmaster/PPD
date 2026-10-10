using PPDShared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Xml;

namespace PPDReleaser
{
    internal class Program
    {
        private static string winDir;
        private static string releaseDir;
        private static string resourceDir;
        private static string currentReleaseDir;
        private static string x64Dir;
        private static void Main()
        {
            winDir = GetUpDir(5, Assembly.GetExecutingAssembly().Location);
            releaseDir = Path.Combine(winDir, "release");
            resourceDir = Path.Combine(releaseDir, "resource");
            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            currentReleaseDir = Path.Combine(releaseDir, "releases", fvi.FileVersion);
            CreateDirectory(currentReleaseDir);
            Console.WriteLine(String.Format("{0} y/n?", currentReleaseDir));
            if (Console.ReadLine() != "y")
            {
                return;
            }
            x64Dir = Path.Combine(currentReleaseDir, "PPDInstaller(x64)");
            CreateDirectory(x64Dir);
            CopyDir(resourceDir, x64Dir);
            BeforeCopyExecutables();
            CopyExecutables(Path.Combine(releaseDir, "x64"), Path.Combine(x64Dir));
            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static void BeforeCopyExecutables()
        {
            string bitText = "x64";
            CopyFile(Path.Combine(winDir, "FlowScriptEngineBasic\\bin\\" + bitText + "\\Release\\FlowScriptEngineBasic.dll"),
                Path.Combine(releaseDir, bitText + "\\PPD\\dlls\\FlowScriptEngineBasic.dll"));
            CopyFile(Path.Combine(winDir, "FlowScriptEngineSlimDX\\bin\\" + bitText + "\\Release\\FlowScriptEngineSlimDX.dll"),
                Path.Combine(releaseDir, bitText + "\\PPD\\dlls\\FlowScriptEngineSlimDX.dll"));
            CopyFile(Path.Combine(winDir, "FlowScriptEnginePPD\\bin\\" + bitText + "\\Release\\FlowScriptEnginePPD.dll"),
                Path.Combine(releaseDir, bitText + "\\PPD\\dlls\\FlowScriptEnginePPD.dll"));
            CopyFile(Path.Combine(winDir, "FlowScriptEngineConsole\\bin\\" + bitText + "\\Release\\FlowScriptEngineConsole.dll"),
                Path.Combine(releaseDir, bitText + "\\PPD\\dlls\\FlowScriptEngineConsole.dll"));
            CopyFile(Path.Combine(winDir, "FlowScriptEnginePPDEditor\\bin\\" + bitText + "\\Release\\FlowScriptEnginePPDEditor.dll"),
                Path.Combine(releaseDir, bitText + "\\PPD\\dlls\\FlowScriptEnginePPDEditor.dll"));
            CopyFile(Path.Combine(winDir, "FlowScriptEngineBasicExtension\\bin\\" + bitText + "\\Release\\FlowScriptEngineBasicExtension.dll"),
                Path.Combine(releaseDir, bitText + "\\PPD\\dlls\\FlowScriptEngineBasicExtension.dll"));
            CopyFile(Path.Combine(winDir, "FlowScriptEngineData\\bin\\" + bitText + "\\Release\\FlowScriptEngineData.dll"),
                Path.Combine(releaseDir, bitText + "\\PPD\\dlls\\FlowScriptEngineData.dll"));
        }

        private static void CopyExecutables(string srcDir, string targetDir)
        {
            // インストーラー（PPDInstaller）は、展開したフォルダーの一番上に置く
            var copyInfo = new CopyInfo(srcDir, targetDir);
            copyInfo.CopyDir("Lang");
            copyInfo.CopyFiles([
                .. ApplicationFiles("PPDInstaller"),
                "CommunityToolkit.Mvvm.dll",
                "ICSharpCode.SharpZipLib.dll",
                "Interop.IWshRuntimeLibrary.dll",
                "ErrorHandle.dll",
                "PPDConfiguration.dll"
            ]);
            var errors = CheckDependencies(targetDir);

            copyInfo = new CopyInfo(srcDir, Path.Combine(targetDir, "Data"));

            var pdCopyInfo = copyInfo.GetNext("BMSTOPPD");
            pdCopyInfo.CopyFiles([
                .. ApplicationFiles("BMSTOPPD"),
                "CommunityToolkit.Mvvm.dll"
            ]);
            errors = [.. errors, .. CheckDependencies(pdCopyInfo.Dir)];

            var executables = ReleaseFiles.VersionedFiles;
            pdCopyInfo = copyInfo.GetNext("PPD");
            pdCopyInfo.CopyDir("Lang");
            pdCopyInfo.CopyDir("Functions");
            pdCopyInfo.CopyDir("Commands");
            pdCopyInfo.CopyDir("Review_Preset");
            pdCopyInfo.CopyFiles(
            [
                .. executables,
                .. ReleaseFiles.OtherFiles,
            ]);

            CopyFile("Items\x64\fmodex64.dll", Path.Combine(pdCopyInfo.Dir, "fmodex64.dll"));
            executables = [.. executables, "fmodex64.dll"];
            errors = [.. errors, .. CheckDependencies(pdCopyInfo.Dir)];
            if (errors.Length > 0)
            {
                // 足りないファイルがあると、そのアプリは起動しない
                throw new InvalidOperationException("Missing files:" + Environment.NewLine + String.Join(Environment.NewLine, errors));
            }

            var prefixes = new string[] { "", "beta_" };
            foreach (var prefix in prefixes)
            {
                CreateAssemblyInfo(Path.Combine(Directory.GetParent(targetDir).FullName, String.Format("{0}assembly_x64.xml", prefix)), [.. executables.Select(e => Path.Combine(targetDir, "Data", "PPD", e))]);
            }
        }

        /// <summary>
        /// .NET のアプリを動かすのに必要なファイル（起動するための exe と、本体の dll、設定）です。
        /// </summary>
        private static string[] ApplicationFiles(string name)
        {
            return [name + ".exe", name + ".dll", name + ".runtimeconfig.json", name + ".deps.json", name + ".dll.config"];
        }

        /// <summary>
        /// フォルダーのアプリの deps.json に書かれたファイル（x64 の Windows で使うもの）が、すべてあるかを確かめます。
        /// </summary>
        /// <returns>足りないファイル。</returns>
        public static string[] CheckDependencies(string dir)
        {
            var missing = new List<string>();
            foreach (var depsFile in Directory.GetFiles(dir, "*.deps.json"))
            {
                using var document = JsonDocument.Parse(File.ReadAllText(depsFile));
                foreach (var target in document.RootElement.GetProperty("targets").EnumerateObject())
                {
                    foreach (var library in target.Value.EnumerateObject())
                    {
                        // プロジェクトやパッケージの dll は、deps.json のパス（lib/net8.0/... など）にかかわらず、アプリと同じフォルダーに置かれる
                        foreach (var kind in new[] { "runtime", "native" })
                        {
                            if (library.Value.TryGetProperty(kind, out var assets))
                            {
                                foreach (var asset in assets.EnumerateObject())
                                {
                                    var path = Path.Combine(dir, Path.GetFileName(asset.Name));
                                    if (!File.Exists(path))
                                    {
                                        missing.Add(String.Format("{0}: {1}", Path.GetFileName(depsFile), asset.Name));
                                    }
                                }
                            }
                        }
                        if (library.Value.TryGetProperty("runtimeTargets", out var runtimeTargets))
                        {
                            foreach (var asset in runtimeTargets.EnumerateObject())
                            {
                                var rid = asset.Value.GetProperty("rid").GetString();
                                if ((rid == "win" || rid == "win-x64") && !File.Exists(Path.Combine(dir, asset.Name)))
                                {
                                    missing.Add(String.Format("{0}: {1}", Path.GetFileName(depsFile), asset.Name));
                                }
                            }
                        }
                    }
                }
            }
            return [.. missing];
        }

        public static string GetUpDir(int count, string path)
        {
            for (int i = 0; i < count; i++)
            {
                path = Path.GetDirectoryName(path);
            }
            return path;
        }

        public static void CopyDir(string srcDir, string destDir)
        {
            foreach (string dir in Directory.GetDirectories(srcDir))
            {
                var newDir = Path.Combine(destDir, Path.GetFileName(dir));
                CreateDirectory(newDir);
                CopyDir(dir, newDir);
            }

            foreach (string file in Directory.GetFiles(srcDir))
            {
                var newFile = Path.Combine(destDir, Path.GetFileName(file));
                CopyFile(file, newFile);
            }
        }

        public static void CreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Console.WriteLine("CreateDirectory:{0}", dir);
                Directory.CreateDirectory(dir);
            }
        }

        public static void CopyFile(string src, string dest)
        {
            CreateDirectory(Path.GetDirectoryName(dest));
            File.Copy(src, dest, true);
            Console.WriteLine("CopyFile:{0}->{1}", Path.GetFileName(src), Path.GetFileName(dest));
        }

        public static void CreateAssemblyInfo(string filePath, string[] filePaths)
        {
            var setting = new XmlWriterSettings
            {
                Indent = true,
                NewLineChars = Environment.NewLine
            };
            using XmlWriter writer = XmlWriter.Create(filePath, setting);
            writer.WriteStartElement("Root");
            foreach (string file in filePaths)
            {
                WriteVersionInfo(writer, file);
            }
            writer.WriteEndElement();
        }

        public static void WriteVersionInfo(XmlWriter writer, string filePath)
        {
            var verionInfo = FileVersionInfo.GetVersionInfo(filePath);
            writer.WriteStartElement("Assembly");
            writer.WriteAttributeString("FileName", Path.GetFileName(filePath));
            writer.WriteAttributeString("Version", verionInfo.FileVersion);
            writer.WriteEndElement();
        }
    }

    internal class CopyInfo(string srcDir, string targetDir)
    {
        private readonly string srcDir = srcDir;
        private readonly string targetDir = targetDir;

        public void CopyFile(string fileName)
        {
            Program.CopyFile(Path.Combine(srcDir, fileName), Path.Combine(targetDir, fileName));
        }

        public void CopyFiles(string[] files)
        {
            foreach (string file in files)
            {
                CopyFile(file);
            }
        }

        public void CopyDir(string dir)
        {
            Program.CopyDir(Path.Combine(srcDir, dir), Path.Combine(targetDir, dir));
        }

        public CopyInfo GetNext(string dir)
        {
            return new CopyInfo(Path.Combine(srcDir, dir), Path.Combine(targetDir, dir));
        }

        public string Dir
        {
            get
            {
                return targetDir;
            }
        }
    }
}