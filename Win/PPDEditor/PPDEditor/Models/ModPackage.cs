using PPDPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PPDEditor.Models
{
    /// <summary>
    /// MOD のファイル（スクリプトとリソースと情報をまとめたもの）を書きます。
    /// </summary>
    public static class ModPackage
    {
        /// <summary>
        /// MOD のファイルを書きます。
        /// </summary>
        /// <param name="path">書くファイル。</param>
        /// <param name="displayName">表示名。</param>
        /// <param name="authorName">作者名。</param>
        /// <param name="version">バージョン。</param>
        /// <param name="scripts">含めるスクリプト（パック内の名前）。</param>
        /// <param name="resources">含めるリソース（パック内の名前）。</param>
        /// <param name="writeScript">スクリプトを書く処理。</param>
        /// <param name="writeResource">リソースを書く処理。</param>
        public static void Write(string path, string displayName, string authorName, string version,
            IEnumerable<string> scripts, IEnumerable<string> resources,
            Action<Stream, string> writeScript, Action<Stream, string> writeResource)
        {
            var dict = new Dictionary<string, string>
            {
                {"Mod\\DisplayName", displayName},
                {"Mod\\AuthorName", authorName},
                {"Mod\\Version", version},
                {"Mod\\FlowScriptVersion", GetVersion("FlowScriptEngine.dll")},
                {"Mod\\FlowScriptBasicVersion", GetVersion("dlls\\FlowScriptEngineBasic.dll")},
                {"Mod\\FlowScriptPPDVersion", GetVersion("dlls\\FlowScriptEnginePPD.dll")},
                {"Mod\\FlowScriptSharpDXVersion", GetVersion("dlls\\FlowScriptEngineSlimDX.dll")},
                {"Mod\\FlowScriptBasicExtensionVersion", GetVersion("dlls\\FlowScriptEngineBasicExtension.dll")},
                {"Mod\\FlowScriptDataVersion", GetVersion("dlls\\FlowScriptEngineData.dll")}
            };
            var list = new List<string>();
            list.AddRange(dict.Keys);
            list.AddRange(scripts);
            list.AddRange(resources);
            using PackWriter writer = new(path);
            foreach (PPDPackStreamWriter packWriter in writer.Write([.. list]))
            {
                if (packWriter.Filename.StartsWith("Mod"))
                {
                    var bytes = Encoding.UTF8.GetBytes(dict[packWriter.Filename]);
                    packWriter.Write(bytes, 0, bytes.Length);
                }
                else if (packWriter.Filename.StartsWith("Scripts"))
                {
                    writeScript(packWriter, packWriter.Filename);
                }
                else
                {
                    writeResource(packWriter, packWriter.Filename);
                }
            }
        }

        private static string GetVersion(string path)
        {
            return FileVersionInfo.GetVersionInfo(path).FileVersion;
        }
    }
}
