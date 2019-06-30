using FlowScriptEngine;
using PPDFramework.Web;
using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace PPDFramework.Mod
{
    /// <summary>
    /// MODマネージャーのクラスです
    /// </summary>
    public class ModManager
    {
        private bool initialized;
        private bool loading;
        private HashSet<string> includeDlls;
        private static ModManager instance;

        /// <summary>
        /// シングルトンインスタンスを取得します
        /// </summary>
        public static ModManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ModManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// ルートを取得します。
        /// </summary>
        public ModInfoBase Root
        {
            get;
            private set;
        }

        /// <summary>
        /// Web上のMod一覧を取得します。
        /// </summary>
        public WebModInfo[] WebModInfos
        {
            get;
            private set;
        }

        /// <summary>
        /// 現在のFlowScriptのバージョンを取得します
        /// </summary>
        public Version FlowScriptVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// 現在のFlowScriptBasicのバージョンを取得します
        /// </summary>
        public Version FlowScriptBasicVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// 現在のFlowScriptPPDのバージョンを取得します
        /// </summary>
        public Version FlowScriptPPDVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// 現在のFlowScriptSharpDXのバージョンを取得します
        /// </summary>
        public Version FlowScriptSharpDXVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// 現在のFlowScriptBasicExtensionのバージョンを取得します。
        /// </summary>
        public Version FlowScriptBasicExtensionVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// 現在のFlowScriptDataのバージョンを取得します。
        /// </summary>
        public Version FlowScriptDataVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// 初期化されたかどうかを取得します
        /// </summary>
        public bool Initialized
        {
            get
            {
                return initialized;
            }
        }

        private ModManager()
        {
            includeDlls = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "FlowScriptEngineBasic.dll",
                "FlowScriptEnginePPD.dll",
                "FlowScriptEngineSlimDX.dll",
                "FlowScriptEngineBasicExtension.dll",
                "FlowScriptEngineData.dll"
            };
        }

        /// <summary>
        /// バックグラウンドで読み込んでいる場合に終了を待ちます
        /// </summary>
        public void WaitForLoadFinish()
        {
            if (loading)
            {
                while (true)
                {
                    if (initialized)
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }
            }
        }

        private void Load()
        {
            EnumerateClasses();
            var mods = new List<ModInfo>();
            WebModInfos = WebManager.Instance.GetMods();
            Root = new DirModInfo("mods");
            if (Directory.Exists("mods"))
            {
                Load(Root);
            }
            FlowScriptVersion = GetVersion("FlowScriptEngine.dll");
            FlowScriptBasicVersion = GetVersion("dlls\\FlowScriptEngineBasic.dll");
            FlowScriptPPDVersion = GetVersion("dlls\\FlowScriptEnginePPD.dll");
            FlowScriptSharpDXVersion = GetVersion("dlls\\FlowScriptEngineSlimDX.dll");
            FlowScriptBasicExtensionVersion = GetVersion("dlls\\FlowScriptEngineBasicExtension.dll");
            FlowScriptDataVersion = GetVersion("dlls\\FlowScriptEngineData.dll");
            initialized = true;
            loading = false;
#if DEBUG
            Console.WriteLine("Mod Load Finished");
#endif
        }

        private void Load(ModInfoBase dir)
        {
            foreach (var dirPath in Directory.GetDirectories(dir.ModPath))
            {
                var info = new DirModInfo(dirPath);
                Load(info);
                if (info.Descendants().Any(m => !m.IsDir))
                {
                    dir.AddChild(info);
                }
            }
            foreach (var filePath in Directory.GetFiles(dir.ModPath))
            {
                try
                {
                    var modInfo = new ModInfo(filePath, this, WebModInfos);
                    dir.AddChild(modInfo);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// バックグラウンドで読み込みます
        /// </summary>
        public void BackgroundLoad()
        {
            if (!initialized)
            {
                loading = true;
                ThreadManager.Instance.GetThread(Load).Start();
            }
        }

        /// <summary>
        /// 特定のフォルダ内のスクリプト情報を列挙します。
        /// </summary>
        /// <param name="directoryPath">フォルダパス。</param>
        public void EnumerateClasses(string directoryPath)
        {
            var types = new List<AssemblyAndType>();
            foreach (string dllFile in Directory.GetFiles(directoryPath, "*.dll"))
            {
                if (!includeDlls.Contains(Path.GetFileName(dllFile)))
                {
                    continue;
                }

                foreach (AssemblyAndType asm in FlowSourceEnumerator.EnumerateFromFile(dllFile, new Type[0]))
                {

                }
            }
        }

        /// <summary>
        /// 特定のフォルダ内のスクリプト情報を列挙します。
        /// </summary>
        public void EnumerateClasses()
        {
            EnumerateClasses("dlls");
        }

        private Version GetVersion(string path)
        {
            var fvi = FileVersionInfo.GetVersionInfo(path);
            return new Version(fvi.FileVersion);
        }
    }
}
