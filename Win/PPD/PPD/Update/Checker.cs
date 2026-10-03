using PPDConfiguration;
using PPDFramework.Web;
using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace PPD.Update
{
    static class Checker
    {
        public static bool CheckInstallInfo()
        {
            try
            {
                return CheckInstallInfoImpl();
            }
            catch
            {
                return true;
            }
        }

        public static bool CheckUpdate()
        {
            try
            {
                return CheckUpdateImpl();
            }
            catch
            {
                return true;
            }
        }

        public static bool CheckAssembly(out string errors)
        {
            errors = "";
            try
            {
                return CheckAssemblyImpl(out errors);
            }
            catch
            {
                return true;
            }
        }

        private static bool CheckInstallInfoImpl()
        {
            if (!File.Exists("install.info"))
            {
                return false;
            }

            var analyzer = new SettingReader(File.ReadAllText("install.info"));
            if (analyzer["PPD"] != "1")
            {
                return false;
            }

            return true;
        }

        private static bool CheckUpdateImpl()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return true;
            }

            var installVersion = new Version(Settings.Setting.Install["InstallVersion"]);
            var splits = Settings.Setting.Updater["UrlList"].Split('/').Select(s => s.Replace("{0}", "")).ToArray();
            splits[^1] = Settings.Setting.Updater["Channel"] + splits[^1];
            var url = new Uri(String.Join("/", splits));
            var latestVersion = new Version("0.0.0.0");
            using (var res = SyncHttpClient.Get(url.AbsoluteUri))
            using (Stream stream = res.GetResponseStream())
            using (XmlReader reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement("Update"))
                    {
                        var version = reader.GetAttribute("Version");
                        var tempVersion = new Version(version);
                        if (tempVersion > latestVersion)
                        {
                            latestVersion = tempVersion;
                        }
                    }
                }
            }

            return installVersion >= latestVersion;
        }

        private static bool CheckAssemblyImpl(out string errors)
        {
            errors = "";
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return true;
            }

            string fileName = "assembly_x64.xml";
            fileName = Settings.Setting.Updater["Channel"] + fileName;
            var url = String.Format("{0}/update/{1}", WebManager.BaseUrl, fileName);

            var content = GetUrlContent(url);
            var asms = new List<AssemblyInfo>();
            using (MemoryStream stream = new())
            {
                var byteContent = Encoding.ASCII.GetBytes(content);
                stream.Write(byteContent, 0, byteContent.Length);
                stream.Seek(0, SeekOrigin.Begin);
                using XmlReader reader = XmlReader.Create(stream);
                while (reader.Read())
                {
                    if (reader.IsStartElement("Assembly"))
                    {
                        asms.Add(new AssemblyInfo(reader.GetAttribute("FileName"), reader.GetAttribute("Version")));
                    }
                }
            }

            var executables = new string[]{
                "Effect2DEditor.exe",
                "FlowScriptControlTest.exe",
                "KeyConfiger.exe",
                "PPD.exe",
                "PPDConfig.exe",
                "PPDEditor.exe",
                "PPDExpansion.exe",
                "PPDMultiServerService.exe",
                "BezierCaliculator.dll",
                "BezierDrawControl.dll",
                "ColorCanvas.dll",
                "DirectShow.dll",
                "Effect2D.dll",
                "ErrorHandle.dll",
                "FlowScriptControl.dll",
                "FlowScriptDrawControl.dll",
                "FlowScriptEngine.dll",
                "CommunityToolkit.Mvvm.dll",
                "Microsoft.Bcl.AsyncInterfaces.dll",
                "System.Buffers.dll",
                "System.ComponentModel.Annotations.dll",
                "System.Memory.dll",
                "System.Numerics.Vectors.dll",
                "System.Runtime.CompilerServices.Unsafe.dll",
                "Microsoft.Xaml.Behaviors.dll",
                "ICSharpCode.SharpZipLib.dll",
                "MessagePack.dll",
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
                "PPDMath.dll",
                "Vortice.Mathematics.dll",
                "Vortice.Direct2D1.dll",
                "Vortice.DirectInput.dll",
                "Vortice.DirectX.dll",
                "SharpGen.Runtime.dll",
                "SharpGen.Runtime.COM.dll",
                "Vortice.DirectSound.dll",
                "Vortice.DXGI.dll",
                "Vortice.Direct3D11.dll",
                "System.Data.SQLite.dll",
                "System.Threading.Tasks.Extensions.dll",
                "System.ValueTuple.dll",
                "DotNetProjects.DataVisualization.Toolkit.dll",
                "AvalonDock.dll",
                "YamlDotNet.dll",
                "x64\\SQLite.Interop.dll",
                "skins\\PPDModSettingUI.dll",
                "skins\\PPDMulti.dll",
                "skins\\PPDMultiCommon.dll",
                "skins\\PPDMultiServer.dll",
                "skins\\PPDSingle.dll",
                "dlls\\FlowScriptEngineBasic.dll",
                "dlls\\FlowScriptEngineBasicExtension.dll",
                "dlls\\FlowScriptEngineConsole.dll",
                "dlls\\FlowScriptEngineData.dll",
                "dlls\\FlowScriptEnginePPD.dll",
                "dlls\\FlowScriptEnginePPDEditor.dll",
                "dlls\\FlowScriptEngineSlimDX.dll",
            };

            string asmDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            var errorFileNames = new List<string>();
            foreach (string filePath in executables)
            {
                var file = Path.Combine(asmDir, filePath);
                if (!File.Exists(file))
                {
                    errorFileNames.Add(String.Format("{0}:{1}", filePath, "File Not Found"));
                    continue;
                }
                var info = asms.FirstOrDefault(i => Path.GetFileName(file).Equals(i.FileName, StringComparison.CurrentCultureIgnoreCase));
                if (info == null)
                {
                    errorFileNames.Add(String.Format("{0}:{1}", filePath, "Assembly List Item Not Found"));
                    continue;
                }
                var version = GetVersion(file);
                if (info.Version != version)
                {
                    errorFileNames.Add(String.Format("{0}:{1},{2},{3}", filePath, "Version unmatch", info.Version, version));
                    continue;
                }
            }

            if (errorFileNames.Count > 0)
            {
                errors = String.Join(Environment.NewLine, errorFileNames.ToArray());
                return false;
            }

            return true;
        }

        private static string GetVersion(string filePath)
        {
            var verionInfo = FileVersionInfo.GetVersionInfo(filePath);
            return verionInfo.FileVersion;
        }

        private static string GetUrlContent(string url)
        {
            return SyncHttpClient.GetString(url);
        }

        class AssemblyInfo(string fileName, string version)
        {
            public string FileName
            {
                get;
                private set;
            } = fileName;

            public string Version
            {
                get;
                private set;
            } = version;
        }
    }
}
