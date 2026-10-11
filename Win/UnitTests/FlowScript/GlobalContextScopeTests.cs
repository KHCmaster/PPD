using System;
using System.Linq;
using Xunit;
using static UnitTests.FlowScript.ScriptRunner;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// GlobalScope（エンジンのすべてのスクリプトで同じ）と ContextScope（同じコンテキストのスクリプトで同じ）のノードのテストです。
    /// </summary>
    public class GlobalContextScopeTests
    {
        static readonly Guid context1 = Guid.NewGuid();
        static readonly Guid context2 = Guid.NewGuid();

        // 始めに値を設定するスクリプト
        static ScriptRunner Setter(string scope, string name, string value)
        {
            var s = new ScriptRunner();
            var set = s.Node(scope + ".SetValue", ("ValueName", name));
            s.Flow(s.Entry(), "Start", set, "In").Flow(s.Node("String.Value", ("Value", value)), "Value", set, "Value");
            return s;
        }

        // 値（定義されていないものを読むとエラーなので、read のときだけ）と、定義されているかを記録するスクリプト
        static ScriptRunner Reader(string scope, string name, bool read = true)
        {
            var s = new ScriptRunner();
            var probe = s.Probe(scope + "." + name);
            s.Flow(s.Entry(), "Start", probe, "In")
                .Flow(s.Node(scope + ".IsDefined", ("ValueName", name)), "Value", probe, "Value2");
            if (read)
            {
                s.Flow(s.Node(scope + ".GetValue", ("ValueName", name)), "Value", probe, "Value");
            }
            return s;
        }

        static (object Value, object Defined) Read(System.Collections.Generic.List<(string Label, object Value, object Value2)> records, string label)
        {
            var (_, value, value2) = Assert.Single(records, r => r.Label == label);
            return (value, value2);
        }

        [Fact]
        public void GlobalValues_AreSharedByAllContexts()
        {
            var records = RunTogether((Setter("GlobalScope", "g", "global"), context1), (Reader("GlobalScope", "g"), context2), (Reader("GlobalScope", "none", read: false), context2));

            Assert.Equal(("global", true), Read(records, "GlobalScope.g"));
            Assert.Equal(((object)null, false), Read(records, "GlobalScope.none"));
        }

        [Fact]
        public void ContextValues_AreSharedInSameContextOnly()
        {
            var records = RunTogether((Setter("ContextScope", "c", "context"), context1), (Reader("ContextScope", "c"), context1));
            Assert.Equal(("context", true), Read(records, "ContextScope.c"));

            records = RunTogether((Setter("ContextScope", "c", "context"), context1), (Reader("ContextScope", "c", read: false), context2));
            Assert.Equal(((object)null, false), Read(records, "ContextScope.c"));
        }

        [Fact]
        public void ContextAndScriptValues_FallBackToGlobal()
        {
            var records = RunTogether((Setter("GlobalScope", "g", "global"), context1), (Reader("ContextScope", "g"), context2), (Reader("Scope", "g"), context2));

            // 親のスコープの値も読める（IsDefined も親を見る）
            Assert.Equal(("global", true), Read(records, "ContextScope.g"));
            Assert.Equal(("global", true), Read(records, "Scope.g"));
        }

        [Fact]
        public void ValueNameNotSet_IsNullAndNotDefined()
        {
            foreach (var scope in new[] { "GlobalScope", "ContextScope" })
            {
                var s = new ScriptRunner();
                var probe = s.Probe("value");
                var set = s.Node(scope + ".SetValue");
                var entry = s.Entry();
                s.Flow(entry, "Start", set, "In").Flow(entry, "Start", probe, "In")
                    .Flow(s.Node(scope + ".GetValue"), "Value", probe, "Value")
                    .Flow(s.Node(scope + ".IsDefined"), "Value", probe, "Value2");

                var (_, value, value2) = Assert.Single(s.Run());

                Assert.Null(value);
                Assert.Equal(false, value2);
            }
        }

        // 名前が x のイベントを受けて、Arg を記録するスクリプト
        static ScriptRunner Handler(string node, string label, bool handled = false)
        {
            var s = new ScriptRunner();
            var handle = s.Node(node, ("EventName", "x"), ("Handled", handled.ToString()));
            var probe = s.Probe(label);
            s.Flow(handle, "Invoked", probe, "In").Flow(handle, "Arg", probe, "Value");
            return s;
        }

        static ScriptRunner Invoker(string scope)
        {
            var s = new ScriptRunner();
            var invoke = s.Node(scope + ".InvokeEvent", ("EventName", "x"));
            s.Flow(s.Entry(), "Start", invoke, "In").Flow(s.Node("String.Value", ("Value", "arg")), "Value", invoke, "Arg");
            return s;
        }

        [Fact]
        public void GlobalEvent_ReachesEveryContextAndScript()
        {
            var records = RunTogether(
                (Invoker("GlobalScope"), context1),
                (Handler("GlobalScope.PreviewHandleEvent", "global preview"), context1),
                (Handler("GlobalScope.HandleEvent", "global"), context1),
                (Handler("ContextScope.HandleEvent", "context1"), context1),
                (Handler("ContextScope.HandleEvent", "context2"), context2),
                (Handler("Scope.HandleEvent", "script2"), context2));

            Assert.All(records, r => Assert.Equal("arg", r.Value));
            // プレビュー、子（コンテキスト、スクリプト）、自分の順
            var labels = records.Select(r => r.Label).ToList();
            Assert.Equal("global preview", labels[0]);
            Assert.Equal("global", labels[^1]);
            Assert.Equal(["context1", "context2", "global", "global preview", "script2"], labels.Order());
            Assert.True(labels.IndexOf("script2") < labels.IndexOf("context2"));
        }

        [Fact]
        public void ContextEvent_StaysInItsContext()
        {
            var records = RunTogether(
                (Invoker("ContextScope"), context1),
                (Handler("ContextScope.PreviewHandleEvent", "preview1"), context1),
                (Handler("ContextScope.HandleEvent", "context1"), context1),
                (Handler("Scope.HandleEvent", "script1"), context1),
                (Handler("ContextScope.HandleEvent", "context2"), context2),
                (Handler("GlobalScope.HandleEvent", "global"), context2));

            Assert.Equal(["preview1", "script1", "context1"], records.Select(r => r.Label));
        }

        [Theory]
        [InlineData("GlobalScope")]
        [InlineData("ContextScope")]
        public void HandledInPreview_StopsEvent(string scope)
        {
            var records = RunTogether(
                (Invoker(scope), context1),
                (Handler(scope + ".PreviewHandleEvent", "preview", handled: true), context1),
                (Handler("Scope.HandleEvent", "script"), context1),
                (Handler(scope + ".HandleEvent", "self"), context1));

            Assert.Equal(["preview"], records.Select(r => r.Label));
        }

        [Fact]
        public void OtherEventNames_AreIgnored()
        {
            var invoker = new ScriptRunner();
            var invoke = invoker.Node("GlobalScope.InvokeEvent", ("EventName", "other"));
            invoker.Flow(invoker.Entry(), "Start", invoke, "In");

            var records = RunTogether((invoker, context1), (Handler("GlobalScope.HandleEvent", "global"), context1),
                (Handler("GlobalScope.PreviewHandleEvent", "preview"), context1), (Handler("ContextScope.HandleEvent", "context"), context1),
                (Handler("ContextScope.PreviewHandleEvent", "context preview"), context1));

            Assert.Empty(records);
        }
    }
}
