using FlowScriptControl.Classes;
using FlowScriptControl.Controls;
using System.Text.RegularExpressions;

namespace PPDEditor.Models
{
    /// <summary>
    /// スクリプトで使えるノード（dlls フォルダーのクラス）と、ノードの絞り込み（PPD、PPDEditor など）を用意します。
    /// </summary>
    static partial class FlowNodeFilters
    {
        static bool registered;

        /// <summary>
        /// ノードのクラスを読み込み、絞り込みを登録します（2 回目以降は何もしない）。
        /// </summary>
        public static void Register()
        {
            if (registered)
            {
                return;
            }
            registered = true;
            FlowDrawPanel.EnumerateClasses("dlls", ["FlowScriptEngineConsole"]);
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
