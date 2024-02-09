using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
            var copyInfo = new CopyInfo(srcDir, targetDir);
            copyInfo.CopyDir("Lang");
            copyInfo.CopyFiles(new string[]{
                "ICSharpCode.SharpZipLib.dll",
                "Interop.IWshRuntimeLibrary.dll",
                "PPDInstaller.exe",
                "PPDInstaller.exe.config",
                "ErrorHandle.dll",
                "PPDConfiguration.dll"
            });

            copyInfo = new CopyInfo(srcDir, Path.Combine(targetDir, "Data"));

            var pdCopyInfo = copyInfo.GetNext("BMSTOPPD");
            pdCopyInfo.CopyFiles(new string[]{
                "BMSTOPPD.exe",
                "BMSTOPPD.exe.config"
            });

            var executables = new string[]{
                "Effect2DEditor.exe",
                "FlowScriptControlTest.exe",
                "KeyConfiger.exe",
                "PPD.exe",
                "PPDConfig.exe",
                "PPDEditor.exe",
                "PPDExpansion.exe",
                "PPDMultiServerService.exe",
                "PPDUpdater.exe",
                "BezierCaliculator.dll",
                "BezierDrawControl.dll",
                "ColorCanvas.dll",
                "DirectShow.dll",
                "Effect2D.dll",
                "EntityFramework.dll",
                "EntityFramework.SqlServer.dll",
                "ErrorHandle.dll",
                "FlowScriptControl.dll",
                "FlowScriptDrawControl.dll",
                "FlowScriptEngine.dll",
                "GalaSoft.MvvmLight.dll",
                "GalaSoft.MvvmLight.Extras.dll",
                "ICSharpCode.SharpZipLib.dll",
                "MessagePack.dll",
                "Microsoft.Practices.ServiceLocation.dll",
                "MoreLinq.dll",
                "Newtonsoft.Json.dll",
                "PPDConfiguration.dll",
                "PPDCore.dll",
                "PPDCoreModel.dll",
                "PPDEditorCommon.dll",
                "PPDExpansionCore.dll",
                "PPDFramework.dll",
                "PPDFrameworkCore.dll",
                "PPDInput.dll",
                "PPDMovie.dll",
                "PPDPack.dll",
                "PPDShareComponent.dll",
                "PPDSound.dll",
                "SharpDX.dll",
                "SharpDX.Desktop.dll",
                "SharpDX.Mathematics.dll",
                "SharpDX.Direct2D1.dll",
                "SharpDX.Direct3D9.dll",
                "SharpDX.DirectInput.dll",
                "SharpDX.DirectSound.dll",
                "SharpDX.DXGI.dll",
                "SharpDX.Direct3D11.Effects.dll",
                "SharpDX.Direct3D11.dll",
                "SharpDX.D3DCompiler.dll",
                "System.Data.SQLite.dll",
                "System.Data.SQLite.EF6.dll",
                "System.Data.SQLite.Linq.dll",
                "System.Threading.Tasks.Extensions.dll",
                "System.ValueTuple.dll",
                "System.Windows.Interactivity.dll",
                "System.Windows.Controls.DataVisualization.Toolkit.dll",
                "WacomMTDN.dll",
                "WeifenLuo.WinFormsUI.Docking.dll",
                "WPFToolkit.dll",
                "YamlDotNet.dll",
                "BlueSky.dll",
                "x64\\SQLite.Interop.dll",
                "skins\\GalaSoft.MvvmLight.dll",
                "skins\\GalaSoft.MvvmLight.Extras.dll",
                "skins\\Microsoft.Practices.ServiceLocation.dll",
                "skins\\PPDModSettingUI.dll",
                "skins\\PPDMulti.dll",
                "skins\\PPDMultiCommon.dll",
                "skins\\PPDMultiServer.dll",
                "skins\\PPDSingle.dll",
                "skins\\System.Windows.Interactivity.dll",
                "dlls\\FlowScriptEngineBasic.dll",
                "dlls\\FlowScriptEngineBasicExtension.dll",
                "dlls\\FlowScriptEngineConsole.dll",
                "dlls\\FlowScriptEngineData.dll",
                "dlls\\FlowScriptEnginePPD.dll",
                "dlls\\FlowScriptEnginePPDEditor.dll",
                "dlls\\FlowScriptEngineSlimDX.dll",
            };
            pdCopyInfo = copyInfo.GetNext("PPD");
            pdCopyInfo.CopyDir("Lang");
            pdCopyInfo.CopyDir("Functions");
            pdCopyInfo.CopyDir("Commands");
            pdCopyInfo.CopyDir("Review_Preset");
            pdCopyInfo.CopyFiles(executables.Concat(new string[]{
                "Effect2DEditor.exe.config",
                "FlowScriptControlTest.exe.config",
                "KeyConfiger.exe.config",
                "PPD.exe.config",
                "PPDConfig.exe.config",
                "PPDEditor.exe.config",
                "PPDExpansion.exe.config",
                "PPDMultiServerService.exe.config",
                "MultiRoomInfo.xml",
                "PPDUpdater.exe.config",
            }).ToArray());

            CopyFile("Items\\x64\\fmodex64.dll", Path.Combine(pdCopyInfo.Dir, "fmodex64.dll"));
            executables = executables.Concat(new string[] { "fmodex64.dll" }).ToArray();

            var prefixes = new string[] { "", "beta_" };
            foreach (var prefix in prefixes)
            {
                CreateAssemblyInfo(Path.Combine(Directory.GetParent(targetDir).FullName, String.Format("{0}assembly_x64.xml", prefix)), executables.Select(e => Path.Combine(targetDir, "Data", "PPD", e)).ToArray());
            }
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
            using (XmlWriter writer = XmlWriter.Create(filePath, setting))
            {
                writer.WriteStartElement("Root");
                foreach (string file in filePaths)
                {
                    WriteVersionInfo(writer, file);
                }
                writer.WriteEndElement();
            }
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

    internal class CopyInfo
    {
        private string srcDir;
        private string targetDir;

        public CopyInfo(string srcDir, string targetDir)
        {
            this.srcDir = srcDir;
            this.targetDir = targetDir;
        }

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