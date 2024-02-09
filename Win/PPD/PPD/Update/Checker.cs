using PPDConfiguration;
using PPDFramework.Web;
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
    class Checker
    {
        public bool CheckInstallInfo()
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

        public bool CheckUpdate()
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

        public bool CheckAssembly(out string errors)
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

        private bool CheckInstallInfoImpl()
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

        private bool CheckUpdateImpl()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return true;
            }

            var installVersion = new Version(Settings.Setting.Install["InstallVersion"]);
            var splits = Settings.Setting.Updater["UrlList"].Split('/').Select(s => s.Replace("{0}", "")).ToArray();
            splits[splits.Length - 1] = Settings.Setting.Updater["Channel"] + splits[splits.Length - 1];
            var url = new Uri(String.Join("/", splits));
            var latestVersion = new Version("0.0.0.0");
            var req = (HttpWebRequest)HttpWebRequest.Create(url);
            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
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

        private bool CheckAssemblyImpl(out string errors)
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
            using (MemoryStream stream = new MemoryStream())
            {
                var byteContent = Encoding.ASCII.GetBytes(content);
                stream.Write(byteContent, 0, byteContent.Length);
                stream.Seek(0, SeekOrigin.Begin);
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement("Assembly"))
                        {
                            asms.Add(new AssemblyInfo(reader.GetAttribute("FileName"), reader.GetAttribute("Version")));
                        }
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
                var info = asms.FirstOrDefault(i => Path.GetFileName(file).ToLower() == i.FileName.ToLower());
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

        private string GetUrlContent(string url)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        class AssemblyInfo
        {
            public string FileName
            {
                get;
                private set;
            }

            public string Version
            {
                get;
                private set;
            }

            public AssemblyInfo(string fileName, string version)
            {
                FileName = fileName;
                Version = version;
            }
        }
    }
}
