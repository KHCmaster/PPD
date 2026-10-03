using PPDEditorCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace PPDEditor.Models
{
    /// <summary>
    /// パラメーターのプリセット（preset_parameters フォルダーの XML）を読み書きします。
    /// </summary>
    static class ParameterPresetStore
    {
        public const string PresetDir = "preset_parameters";

        /// <summary>
        /// フォルダーの中のプリセットを、フォルダーの階層のまま読み込みます。
        /// </summary>
        /// <param name="onError">読み込めないプリセットがあったときに呼ばれます。</param>
        public static ParameterPresetFolder Load(Action onError)
        {
            Directory.CreateDirectory(PresetDir);
            return Load(PresetDir, onError);
        }

        private static ParameterPresetFolder Load(string dir, Action onError)
        {
            var folder = new ParameterPresetFolder(Path.GetFileName(dir));
            foreach (var childDir in Directory.GetDirectories(dir))
            {
                folder.Folders.Add(Load(childDir, onError));
            }

            foreach (var childFile in Directory.GetFiles(dir))
            {
                try
                {
                    var name = Path.Combine(Path.GetDirectoryName(childFile), Path.GetFileNameWithoutExtension(childFile))[dir.Length..].TrimStart(Path.DirectorySeparatorChar);
                    folder.Presets.Add(new ParameterPreset(childFile, name));
                }
                catch
                {
                    onError();
                }
            }
            return folder;
        }

        /// <summary>
        /// パラメーターをプリセットとして保存します。
        /// </summary>
        public static void Save(string filePath, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var document = new XDocument(new XElement("Root"));
            foreach (var pair in parameters)
            {
                document.Root.Add(new XElement("Parameter", new XAttribute("Key", pair.Key), new XAttribute("Value", pair.Value)));
            }
            document.Save(filePath);
        }
    }
}
