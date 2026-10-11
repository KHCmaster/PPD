using CommunityToolkit.Mvvm.ComponentModel;
using PPDEditorCommon;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PPDEditor.ViewModels
{
    /// <summary>
    /// パラメーターのプリセットのメニューの 1 項目（フォルダーかプリセット）です。
    /// </summary>
    public partial class PresetNode(string name, ParameterPreset preset) : ObservableObject
    {
        public string Name { get; } = name;

        /// <summary>
        /// プリセットです（フォルダーのときは null）。
        /// </summary>
        public ParameterPreset Preset { get; } = preset;

        public List<PresetNode> Children { get; } = [];

        /// <summary>
        /// 選んでいるマークに、プリセットのパラメーターがすべて当てはまるかです。
        /// </summary>
        [ObservableProperty]
        public partial bool IsChecked { get; set; }

        /// <summary>
        /// 選んでいるマークに、プリセットのパラメーターが一部だけ当てはまるかです。
        /// </summary>
        [ObservableProperty]
        public partial bool IsPartial { get; set; }

        /// <summary>
        /// フォルダーの中身からメニューの項目を作ります（フォルダーを先に、プリセットを後に並べる）。
        /// </summary>
        public static List<PresetNode> Build(ParameterPresetFolder folder)
        {
            var nodes = new List<PresetNode>();
            foreach (var childFolder in folder.Folders)
            {
                var node = new PresetNode(childFolder.Name, null);
                node.Children.AddRange(Build(childFolder));
                nodes.Add(node);
            }
            foreach (var preset in folder.Presets)
            {
                nodes.Add(new PresetNode(Path.GetFileNameWithoutExtension(preset.FilePath), preset));
            }
            return nodes;
        }

        /// <summary>
        /// プリセットの項目をすべて返します。
        /// </summary>
        public static IEnumerable<PresetNode> GetPresets(IEnumerable<PresetNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Preset != null)
                {
                    yield return node;
                }
                foreach (var descendant in GetPresets(node.Children))
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// 選んでいるマークの共通のパラメーターから、チェックと一部だけの印を付け直します。
        /// </summary>
        public static void UpdateCheckState(IEnumerable<PresetNode> nodes, Dictionary<string, string> commonParameters)
        {
            foreach (var node in GetPresets(nodes).ToArray())
            {
                var state = Utility.GetPresetCheckState(node.Preset, commonParameters);
                node.IsChecked = state == true;
                node.IsPartial = state == null;
            }
        }
    }
}
