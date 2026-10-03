using FlowScriptControl.Classes;
using FlowScriptControl.Controls;
using System.Text.RegularExpressions;

namespace FlowScriptControlTest.Models
{
    /// <summary>
    /// スクリプトで使えるノード（dlls フォルダーのクラス）と、ノードの絞り込みを用意します。
    /// </summary>
    static partial class FlowEnvironment
    {
        /// <summary>
        /// ノードのクラスを読み込みます。
        /// </summary>
        public static void LoadClasses()
        {
            FlowDrawPanel.EnumerateClasses("dlls", []);
        }

        /// <summary>
        /// ノードのクラスを読み込み、編集のときのノードの絞り込み（Console、PPD など）を登録します。
        /// </summary>
        public static void InitializeForEditing()
        {
            LoadClasses();
            FlowDrawPanel.AddFilter(new NodeFilterInfo("Console", [
                NamespaceFilter("FlowScriptEngineBasic"),
                NamespaceFilter("FlowScriptEngineBasicExtension"),
                NamespaceFilter("FlowScriptEngineConsole"),
                NamespaceFilter("FlowScriptEngineData")],
                null));
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPD", [
                NamespaceFilter("FlowScriptEngineBasic"),
                NamespaceFilter("FlowScriptEngineBasicExtension"),
                NamespaceFilter("FlowScriptEngineSlimDX"),
                NamespaceFilter("FlowScriptEnginePPD"),
                NamespaceFilter("FlowScriptEngineData")],
                null));
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPD(Script)", [
                NamespaceFilter("FlowScriptEngineBasic"),
                NamespaceFilter("FlowScriptEngineBasicExtension"),
                NamespaceFilter("FlowScriptEngineSlimDX"),
                NamespaceFilter("FlowScriptEnginePPD"),
                NamespaceFilter("FlowScriptEngineData")],
                null, null, [PPDModRegex()]));
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPD(Mod)", [
                NamespaceFilter("FlowScriptEngineBasic"),
                NamespaceFilter("FlowScriptEngineBasicExtension"),
                NamespaceFilter("FlowScriptEngineSlimDX"),
                NamespaceFilter("FlowScriptEnginePPD"),
                NamespaceFilter("FlowScriptEngineData")],
                null));
            FlowDrawPanel.AddFilter(new NodeFilterInfo("PPDEditor", [
                NamespaceFilter("FlowScriptEngineBasic"),
                NamespaceFilter("FlowScriptEngineBasicExtension"),
                NamespaceFilter("FlowScriptEngineSlimDX"),
                NamespaceFilter("FlowScriptEnginePPDEditor"),
                NamespaceFilter("FlowScriptEngineData")],
                null));
        }

        private static Regex NamespaceFilter(string ns)
        {
            return new Regex("^" + Regex.Escape(ns) + "\\.");
        }

        [GeneratedRegex("^PPD\\.Mod")]
        private static partial Regex PPDModRegex();
    }
}
