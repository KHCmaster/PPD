using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// すべてのノードに当てはまる、イベントの決まりを確かめます。
    /// </summary>
    public class NodeEventTests
    {
        public static IEnumerable<Type> NodeTypes()
        {
            Type[] anchors =
            [
                typeof(FlowScriptEngineBasic.FlowSourceObjects.Misc.EntryPointFlowSourceObject),
                typeof(FlowScriptEngineConsole.FlowSourceObjects.Console.WriteLineFlowSourceObject),
                typeof(FlowScriptEngineBasicExtension.ToolTipText),
                typeof(FlowScriptEngineData.ToolTipText),
                typeof(FlowScriptEngineSlimDX.ToolTipText),
                typeof(FlowScriptEnginePPD.ToolTipText),
                typeof(FlowScriptEnginePPDEditor.ToolTipText),
            ];
            return anchors.Select(t => t.Assembly).Distinct().SelectMany(a => a.GetTypes())
                .Where(t => t.IsPublic && t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(FlowSourceObjectBase)));
        }

        /// <summary>
        /// Success と Failed があるノードは、入力がなくても In でどちらかを出します（エラーにするのはかまわない）。
        /// 出さないと、後につないだ処理が動かない。
        /// </summary>
        [Fact]
        public void In_FiresSuccessOrFailed()
        {
            var silent = new List<string>();
            var count = 0;
            // メッセージボックスやダイアログを出すノードは、画面を出すので動かさない
            string[] showsWindow = ["FlowScriptEnginePPDEditor.FlowSourceObjects.MessageBox.", "FlowScriptEnginePPDEditor.FlowSourceObjects.Dialog."];
            foreach (var type in NodeTypes().Where(t => !showsWindow.Any(t.FullName.StartsWith)))
            {
                var events = type.GetEvents().Select(e => e.Name).ToHashSet();
                if (!events.Contains("Success") || !events.Contains("Failed") || type.GetMethod("In") == null)
                {
                    continue;
                }
                NodeHarness harness;
                try
                {
                    harness = new NodeHarness(type);
                    harness.Invoke();
                }
                catch
                {
                    continue;
                }
                count++;
                if (!harness.Fired.Contains("Success") && !harness.Fired.Contains("Failed"))
                {
                    silent.Add(type.FullName);
                }
            }

            Assert.True(count > 200, $"確かめたノード: {count}");
            Assert.True(silent.Count == 0, string.Join(Environment.NewLine, silent));
        }
    }
}
