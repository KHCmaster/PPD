using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using static UnitTests.FlowScript.ScriptRunner;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// スクリプトの作り（エディターで囲んだスコープ、ループの Break と Continue）と、Engine の Reset などのテストです。
    /// </summary>
    public class ScriptStructureTests
    {
        // 値を設定してから、記録するノードを順に動かす
        static int SetThen(ScriptRunner s, int sequential, int index, string name, string value)
        {
            var set = s.Node("Scope.SetValue", ("ValueName", name));
            s.Flow(sequential, "Out" + index, set, "In").Flow(s.Node("String.Value", ("Value", value)), "Value", set, "Value");
            return set;
        }

        // 記録するノードと IsDefined（同じスコープに入れる）
        static int[] Reader(ScriptRunner s, int sequential, int index, string label, string name)
        {
            var probe = s.Probe(label);
            var defined = s.Node("Scope.IsDefined", ("ValueName", name));
            s.Flow(sequential, "Out" + index, probe, "In").Flow(defined, "Value", probe, "Value2");
            return [probe, defined];
        }

        [Fact]
        public void ScopeValues_AreVisibleInChildScopesOnly()
        {
            var s = new ScriptRunner();
            var outer = s.Scope();
            var inner = s.Scope(outer);
            var sibling = s.Scope();
            var seq = s.Node("Logic.Sequential");
            s.Flow(s.Entry(), "Start", seq, "In");

            var setOuter = SetThen(s, seq, 0, "a", "outer");
            var setInner = SetThen(s, seq, 1, "b", "inner");
            var readInner = Reader(s, seq, 2, "inner sees a", "a");
            var readOuter = Reader(s, seq, 3, "outer sees b", "b");
            var readSibling = Reader(s, seq, 4, "sibling sees a", "a");
            s.InScope(outer, [setOuter, .. readOuter]).InScope(inner, [setInner, .. readInner]).InScope(sibling, readSibling);
            // 内側のスコープの GetValue は外側の値も読める
            var get = s.Node("Scope.GetValue", ("ValueName", "a"));
            s.InScope(inner, get).Flow(get, "Value", readInner[0], "Value");

            var records = s.Run();

            Assert.Equal(("outer", (object)true), (Single(records, "inner sees a").Value, Single(records, "inner sees a").Value2));
            Assert.Equal(false, Single(records, "outer sees b").Value2);
            Assert.Equal(false, Single(records, "sibling sees a").Value2);
        }

        static (string Label, object Value, object Value2) Single(List<(string Label, object Value, object Value2)> records, string label)
        {
            return Assert.Single(records, r => r.Label == label);
        }

        [Fact]
        public void ScopeValue_InChildShadowsParent()
        {
            var s = new ScriptRunner();
            var outer = s.Scope();
            var inner = s.Scope(outer);
            var seq = s.Node("Logic.Sequential");
            s.Flow(s.Entry(), "Start", seq, "In");
            var setOuter = SetThen(s, seq, 0, "a", "outer");
            var setInner = SetThen(s, seq, 1, "a", "inner");
            var probeOuter = s.Probe("outer");
            var probeInner = s.Probe("inner");
            var getOuter = s.Node("Scope.GetValue", ("ValueName", "a"));
            var getInner = s.Node("Scope.GetValue", ("ValueName", "a"));
            s.Flow(seq, "Out2", probeOuter, "In").Flow(getOuter, "Value", probeOuter, "Value")
                .Flow(seq, "Out3", probeInner, "In").Flow(getInner, "Value", probeInner, "Value")
                .InScope(outer, setOuter, probeOuter, getOuter).InScope(inner, setInner, probeInner, getInner);

            var records = s.Run();

            Assert.Equal(["outer"], Values(records, "outer"));
            Assert.Equal(["inner"], Values(records, "inner"));
        }

        [Fact]
        public void ScopeEvent_GoesFromInvokingScopeToItsChildren()
        {
            var s = new ScriptRunner();
            var outer = s.Scope();
            var inner = s.Scope(outer);
            var sibling = s.Scope();
            var invoke = s.Node("Scope.InvokeEvent", ("EventName", "x"));
            s.Flow(s.Entry(), "Start", invoke, "In").InScope(outer, invoke);
            foreach (var (scope, label) in new[] { (outer, "outer"), (inner, "inner"), (sibling, "sibling") })
            {
                var handle = s.Node("Scope.HandleEvent", ("EventName", "x"));
                var probe = s.Probe(label);
                s.Flow(handle, "Invoked", probe, "In").InScope(scope, handle, probe);
            }

            var records = s.Run();

            // 子のスコープ、自分の順で、ほかのスコープには届かない
            Assert.Equal(["inner", "outer"], records.Select(r => r.Label));
        }

        // ループの本体で Out0 を記録し、i が stopAt のときに Break か Continue を動かす
        static List<(string Label, object Value, object Value2)> RunLoop(string loopNode, string control, int stopAt)
        {
            var s = new ScriptRunner();
            var seq = s.Node("Logic.Sequential");
            var counter = s.Node("Scope.GetValue", ("ValueName", "i"));
            var cast = s.Node("Int32.Cast");
            var increment = s.Node("Scope.SetValue", ("ValueName", "i"));
            var add = s.Node("Int32.Add", ("B", "1"));
            var isStop = s.Node("Int32.Equal", ("B", stopAt.ToString()));
            var @if = s.Node("Logic.If");
            var controlNode = s.Node("Repeat." + control);
            var body = s.Probe("body");
            s.Flow(counter, "Value", cast, "CastValue").Flow(cast, "Value", add, "A").Flow(add, "Value", increment, "Value")
                .Flow(cast, "Value", isStop, "A").Flow(isStop, "Value", @if, "Condition")
                .Flow(seq, "Out0", increment, "In")
                .Flow(seq, "Out1", @if, "In").Flow(@if, "OnTrue", controlNode, "In")
                .Flow(seq, "Out2", body, "In").Flow(cast, "Value", body, "Value");

            var init = s.Node("Scope.SetValue", ("ValueName", "i"));
            var zero = s.Node("Int32.Value", ("Value", "0"));
            var entry = s.Entry();
            var start = s.Node("Logic.Sequential");
            s.Flow(entry, "Start", start, "In").Flow(start, "Out0", init, "In").Flow(zero, "Value", init, "Value");
            var loop = s.Node("Repeat." + loopNode);
            s.Flow(start, "Out1", loop, "In").Flow(start, "Out2", s.Probe("end"), "In");
            switch (loopNode)
            {
                case "For":
                case "While":
                case "DoWhile":
                    // 5 回まで
                    var less = s.Node("Int32.Less", ("B", "5"));
                    s.Flow(cast, "Value", less, "A").Flow(less, "Value", loop, "Condition").Flow(loop, "Loop", seq, "In");
                    break;
                case "Foreach":
                    // 5 個のリスト
                    var list = s.Node("ArrayList.Repeat", ("Count", "5"));
                    s.Flow(start, "Out0", list, "In").Flow(s.Node("Int32.Value", ("Value", "0")), "Value", list, "Object")
                        .Flow(list, "Value", loop, "Enumerable").Flow(loop, "Loop", seq, "In");
                    break;
            }
            return s.Run();
        }

        [Theory]
        [InlineData("For")]
        [InlineData("Foreach")]
        [InlineData("While")]
        [InlineData("DoWhile")]
        public void Break_LeavesLoop(string loop)
        {
            var records = RunLoop(loop, "Break", 3);

            // 値を増やしてから調べるので、i が 3 になった回で抜ける（その回の本体は動かない）
            Assert.Equal([1, 2], Values(records, "body"));
            Assert.Equal("end", records.Last().Label);
        }

        [Theory]
        [InlineData("For")]
        [InlineData("Foreach")]
        [InlineData("While")]
        [InlineData("DoWhile")]
        public void Continue_SkipsRestOfBody(string loop)
        {
            var records = RunLoop(loop, "Continue", 3);

            Assert.Equal([1, 2, 4, 5], Values(records, "body"));
            Assert.Equal("end", records.Last().Label);
        }

        [Theory]
        [InlineData("Break")]
        [InlineData("Continue")]
        public void LoopControl_OutsideLoop_IsScriptError(string control)
        {
            var s = new ScriptRunner();
            s.Flow(s.Entry(), "Start", s.Node("Repeat." + control), "In");

            var error = Assert.Throws<InvalidOperationException>(() => s.Run());

            Assert.Contains(control, error.InnerException.ToString());
        }

        [Fact]
        public void DoWhile_RunsBodyOnceEvenIfFalse()
        {
            var s = new ScriptRunner();
            var loop = s.Node("Repeat.DoWhile", ("Condition", "False"));
            s.Flow(s.Entry(), "Start", loop, "In").Flow(loop, "Loop", s.Probe("body"), "In");

            Assert.Single(s.Run());
        }

        [Fact]
        public void Foreach_EmptyOrMissing_DoesNothing()
        {
            var s = new ScriptRunner();
            var empty = s.Node("Repeat.Foreach");
            var list = s.Node("ArrayList.Value");
            var withList = s.Node("Repeat.Foreach");
            var seq = s.Node("Logic.Sequential");
            s.Flow(s.Entry(), "Start", seq, "In").Flow(seq, "Out0", empty, "In").Flow(seq, "Out1", withList, "In").Flow(list, "Value", withList, "Enumerable")
                .Flow(empty, "Loop", s.Probe("body"), "In").Flow(withList, "Loop", s.Probe("body"), "In");

            Assert.Empty(s.Run());
        }

        static FlowSourceManager Load(Engine engine, ScriptRunner script)
        {
            RegisterNodes();
            var manager = engine.Load(new MemoryStream(Encoding.UTF8.GetBytes(script.ToXml())), false);
            manager.Initialize();
            return manager;
        }

        [Fact]
        public void EngineReset_ResetsNodesAndClearsScopes()
        {
            var s = new ScriptRunner();
            var gate = s.Node("Logic.OnceGate");
            var seq = s.Node("Logic.Sequential");
            var set = s.Node("GlobalScope.SetValue", ("ValueName", "g"));
            var defined = s.Node("GlobalScope.IsDefined", ("ValueName", "g"));
            var probe = s.Probe("passed");
            s.Flow(s.Entry(), "Start", seq, "In").Flow(seq, "Out0", gate, "In").Flow(gate, "Out", probe, "In").Flow(defined, "Value", probe, "Value")
                .Flow(seq, "Out1", set, "In").Flow(s.Node("Int32.Value", ("Value", "1")), "Value", set, "Value");
            var engine = new Engine(new StringReader(""));
            Load(engine, s);
            ProbeFlowSourceObject.Records.Clear();

            engine.Start();
            engine.Start();
            // OnceGate は 1 回だけ通す。2 回目のスタートでは通らない
            Assert.Equal([false], Values(ProbeFlowSourceObject.Records, "passed"));

            engine.Reset();
            engine.Start();
            // Reset で OnceGate がまた通り、GlobalScope の値も消えている
            Assert.Equal([false, false], Values(ProbeFlowSourceObject.Records, "passed"));

            engine.Clear();
            engine.Start();
            Assert.Equal(2, ProbeFlowSourceObject.Records.Count);
        }

        [Fact]
        public void Engine_ContainsNodeAndAttribute()
        {
            var s = new ScriptRunner();
            s.Flow(s.Entry(), "Start", s.Node("Repeat.Break"), "In");
            var engine = new Engine(new StringReader(""));
            Load(engine, s);

            Assert.True(engine.ContainsNode("Repeat.Break"));
            Assert.False(engine.ContainsNode("Repeat.Continue"));
            // Break には IgnoreTest が付いている
            Assert.True(engine.ContainsAttribute(typeof(IgnoreTestAttribute)));
            Assert.False(engine.ContainsAttribute(typeof(ObsoleteAttribute)));
        }
    }
}
