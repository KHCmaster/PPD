using System.Linq;
using Xunit;
using static UnitTests.FlowScript.ScriptRunner;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// FlowScriptEngineBasic のイベントのあるノードを、スクリプトを動かして確かめます。
    /// </summary>
    public class BasicScriptTests
    {
        [Theory]
        [InlineData("True", "true")]
        [InlineData("False", "false")]
        public void If_ChoosesBranch(string condition, string expected)
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var cond = s.Node("Boolean.Value", ("Value", condition));
            var @if = s.Node("Logic.If");
            s.Flow(entry, "Start", @if, "In").Flow(cond, "Value", @if, "Condition")
                .Flow(@if, "OnTrue", s.Probe("true"), "In").Flow(@if, "OnFalse", s.Probe("false"), "In");

            Assert.Equal([expected], s.Run().Select(r => r.Label));
        }

        [Fact]
        public void Sequential_FiresInOrder()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var seq = s.Node("Logic.Sequential");
            s.Flow(entry, "Start", seq, "In");
            foreach (var i in new[] { 2, 0, 1 })
            {
                s.Flow(seq, "Out" + i, s.Probe("out" + i), "In");
            }

            Assert.Equal(["out0", "out1", "out2"], s.Run().Select(r => r.Label));
        }

        [Fact]
        public void OnceGate_PassesOnlyFirst()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var seq = s.Node("Logic.Sequential");
            var gate = s.Node("Logic.OnceGate");
            s.Flow(entry, "Start", seq, "In").Flow(seq, "Out0", gate, "In").Flow(seq, "Out1", gate, "In").Flow(gate, "Out", s.Probe("passed"), "In");

            Assert.Single(s.Run());
        }

        [Theory]
        [InlineData(2, "out2")]
        [InlineData(9, "default")]
        public void Switch_MatchesTarget(int value, string expected)
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var sw = s.Node("Logic.Switch");
            s.Flow(entry, "Start", sw, "In")
                .Flow(s.Node("Int32.Value", ("Value", value.ToString())), "Value", sw, "Value")
                .Flow(s.Node("Int32.Value", ("Value", "1")), "Value", sw, "Target1")
                .Flow(s.Node("Int32.Value", ("Value", "2")), "Value", sw, "Target2")
                .Flow(sw, "Out1", s.Probe("out1"), "In").Flow(sw, "Out2", s.Probe("out2"), "In").Flow(sw, "Default", s.Probe("default"), "In");

            Assert.Equal([expected], s.Run().Select(r => r.Label));
        }

        // i を 0 から 4 まで増やしながら、記録する
        [Fact]
        public void For_LoopsWithScopeVariable()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var @for = s.Node("Repeat.For");
            var init = s.Node("Scope.SetValue", ("ValueName", "i"));
            var zero = s.Node("Int32.Value", ("Value", "0"));
            var get = s.Node("Scope.GetValue", ("ValueName", "i"));
            var cast = s.Node("Int32.Cast");
            var less = s.Node("Int32.Less", ("B", "5"));
            var add = s.Node("Int32.Add", ("B", "1"));
            var increment = s.Node("Scope.SetValue", ("ValueName", "i"));
            var probe = s.Probe("i");
            var end = s.Probe("end");
            s.Flow(entry, "Start", @for, "In")
                .Flow(@for, "Init", init, "In").Flow(zero, "Value", init, "Value")
                .Flow(get, "Value", cast, "CastValue").Flow(cast, "Value", less, "A").Flow(less, "Value", @for, "Condition")
                .Flow(@for, "Loop", probe, "In").Flow(cast, "Value", probe, "Value")
                .Flow(@for, "LoopEnd", increment, "In").Flow(cast, "Value", add, "A").Flow(add, "Value", increment, "Value")
                .Flow(@for, "LoopEnd", end, "In");

            var records = s.Run();

            Assert.Equal([0, 1, 2, 3, 4], Values(records, "i"));
            Assert.Equal(5, Values(records, "end").Length);
        }

        [Theory]
        // 以前の入力の名前（ArrayList）でもつなげる
        [InlineData("Enumerable")]
        [InlineData("ArrayList")]
        public void Foreach_VisitsItems(string inputName)
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var seq = s.Node("Logic.Sequential");
            var (list, next) = s.StringList(seq, "a", "b", "c");
            var each = s.Node("Repeat.Foreach");
            var probe = s.Probe("item");
            s.Flow(entry, "Start", seq, "In").Flow(seq, "Out" + next, each, "In")
                .Flow(list, "Value", each, inputName)
                .Flow(each, "Loop", probe, "In").Flow(each, "Value", probe, "Value").Flow(each, "Iter", probe, "Value2");

            var records = s.Run();

            Assert.Equal(["a", "b", "c"], records.Select(r => r.Value));
            Assert.Equal([0, 1, 2], records.Select(r => r.Value2));
        }

        [Fact]
        public void While_StopsAtBreak()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var loop = s.Node("Repeat.While", ("Condition", "True"));
            var gate = s.Node("Logic.Sequential");
            var brk = s.Node("Repeat.Break");
            s.Flow(entry, "Start", loop, "In").Flow(loop, "Loop", gate, "In")
                .Flow(gate, "Out0", s.Probe("body"), "In").Flow(gate, "Out1", brk, "In").Flow(gate, "Out2", s.Probe("after"), "In");

            var records = s.Run();

            Assert.Equal(["body"], records.Select(r => r.Label));
        }

        [Theory]
        [InlineData("Int32.Parse", "42", 42)]
        [InlineData("Float.Parse", "1.5", 1.5f)]
        [InlineData("Double.Parse", "-2.25", -2.25)]
        public void Parse_SucceedsOrFails(string node, string text, object expected)
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var seq = s.Node("Logic.Sequential");
            var good = s.Node(node, ("A", text));
            var bad = s.Node(node, ("A", "abc"));
            var ok = s.Probe("ok");
            s.Flow(entry, "Start", seq, "In").Flow(seq, "Out0", good, "In").Flow(seq, "Out1", bad, "In")
                .Flow(good, "Success", ok, "In").Flow(good, "Value", ok, "Value")
                .Flow(good, "Failed", s.Probe("good failed"), "In")
                .Flow(bad, "Failed", s.Probe("bad failed"), "In").Flow(bad, "Success", s.Probe("bad ok"), "In");

            var records = s.Run();

            Assert.Equal(["ok", "bad failed"], records.Select(r => r.Label));
            Assert.Equal(expected, records[0].Value);
        }

        [Fact]
        public void BooleanParse()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var parse = s.Node("Boolean.Parse", ("Value", "true"));
            var ok = s.Probe("ok");
            s.Flow(entry, "Start", parse, "In").Flow(parse, "Success", ok, "In").Flow(parse, "Result", ok, "Value");

            Assert.Equal([true], s.Run().Select(r => r.Value));
        }

        [Fact]
        public void ArrayList_AddCountAndContains()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var seq = s.Node("Logic.Sequential");
            var list = s.Node("ArrayList.Value");
            var add1 = s.Node("ArrayList.Add");
            var add2 = s.Node("ArrayList.Add");
            var count = s.Node("ArrayList.Count");
            var contains = s.Node("ArrayList.Contains");
            var a = s.Node("String.Value", ("Value", "a"));
            var b = s.Node("String.Value", ("Value", "b"));
            var probe = s.Probe("count");
            var found = s.Probe("contains");
            s.Flow(entry, "Start", seq, "In")
                .Flow(seq, "Out0", list, "In")
                .Flow(seq, "Out1", add1, "In").Flow(list, "Value", add1, "ArrayList").Flow(a, "Value", add1, "Value")
                .Flow(seq, "Out2", add2, "In").Flow(list, "Value", add2, "ArrayList").Flow(b, "Value", add2, "Value")
                .Flow(seq, "Out3", count, "In").Flow(list, "Value", count, "ArrayList").Flow(count, "Success", probe, "In").Flow(count, "Value", probe, "Value")
                .Flow(seq, "Out4", contains, "In").Flow(list, "Value", contains, "ArrayList").Flow(b, "Value", contains, "Object")
                .Flow(contains, "Success", found, "In").Flow(contains, "Value", found, "Value");

            var records = s.Run();

            Assert.Equal([2], Values(records, "count"));
            Assert.Equal([true], Values(records, "contains"));
        }

        [Fact]
        public void Hashtable_AddGetAndCount()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var seq = s.Node("Logic.Sequential");
            var table = s.Node("Hashtable.Value");
            var add = s.Node("Hashtable.Add");
            var get = s.Node("Hashtable.GetValue");
            var missing = s.Node("Hashtable.GetValue");
            var contains = s.Node("Hashtable.ContainsKey");
            var missingProbe = s.Probe("contains");
            var key = s.Node("String.Value", ("Value", "key"));
            var other = s.Node("String.Value", ("Value", "other"));
            var value = s.Node("Int32.Value", ("Value", "7"));
            var probe = s.Probe("value");
            s.Flow(entry, "Start", seq, "In")
                .Flow(seq, "Out0", table, "In")
                .Flow(seq, "Out1", add, "In").Flow(table, "Value", add, "Hashtable").Flow(key, "Value", add, "Key").Flow(value, "Value", add, "Value")
                .Flow(seq, "Out2", get, "In").Flow(table, "Value", get, "Hashtable").Flow(key, "Value", get, "Key")
                .Flow(get, "Success", probe, "In").Flow(get, "Value", probe, "Value")
                .Flow(seq, "Out3", missing, "In").Flow(table, "Value", missing, "Hashtable").Flow(other, "Value", missing, "Key")
                .Flow(missing, "Failed", s.Probe("missing"), "In").Flow(missing, "Success", s.Probe("missing ok"), "In")
                .Flow(seq, "Out4", contains, "In").Flow(table, "Value", contains, "Hashtable").Flow(other, "Value", contains, "Key")
                .Flow(contains, "Success", missingProbe, "In").Flow(contains, "Value", missingProbe, "Value");

            var records = s.Run();

            Assert.Equal([7], Values(records, "value"));
            // ない鍵は Failed になる（以前はいつも Success で、値を読むとエラーになっていた）
            Assert.Single(records, r => r.Label == "missing");
            Assert.DoesNotContain(records, r => r.Label == "missing ok");
            Assert.Equal([false], Values(records, "contains"));
        }

        // "a,b,a" のうち "a" だけを残して数える
        [Fact]
        public void EnumerableWhere_UsesPredicateEvent()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var seq = s.Node("Logic.Sequential");
            var (list, next) = s.StringList(seq, "a", "b", "a");
            var where = s.Node("Enumerable.Where");
            var equal = s.Node("String.Equal", ("B", "a"));
            var count = s.Node("Enumerable.Count");
            var probe = s.Probe("count");
            s.Flow(entry, "Start", seq, "In").Flow(seq, "Out" + next, probe, "In")
                .Flow(list, "Value", where, "Enumerable")
                .Flow(where, "Value", equal, "A").Flow(equal, "Value", where, "Result")
                .Flow(where, "Enumerable", count, "Enumerable").Flow(count, "Count", probe, "Value");

            Assert.Equal([2], s.Run().Select(r => r.Value));
        }

        // 別の名前のイベントを前もって受けるノードが 2 つあっても、イベントは 1 回だけ届く
        [Fact]
        public void ScopeEvent_DeliveredOnceWithSeveralPreviewHandlers()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var invoke = s.Node("Scope.InvokeEvent", ("EventName", "x"));
            var previewX = s.Node("Scope.PreviewHandleEvent", ("EventName", "x"));
            var previewY = s.Node("Scope.PreviewHandleEvent", ("EventName", "y"));
            var handle = s.Node("Scope.HandleEvent", ("EventName", "x"));
            var arg = s.Node("Int32.Value", ("Value", "5"));
            var probe = s.Probe("handled");
            s.Flow(entry, "Start", previewX, "In").Flow(entry, "Start", previewY, "In").Flow(entry, "Start", handle, "In").Flow(entry, "Start", invoke, "In")
                .Flow(arg, "Value", invoke, "Arg")
                .Flow(previewX, "Invoked", s.Probe("preview x"), "In")
                .Flow(handle, "Invoked", probe, "In").Flow(handle, "Arg", probe, "Value");

            var records = s.Run();

            Assert.Equal([5], Values(records, "handled"));
            Assert.Single(records, r => r.Label == "preview x");
        }

        [Fact]
        public void Scope_SetGetAndIsDefined()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var seq = s.Node("Logic.Sequential");
            var set = s.Node("Scope.SetValue", ("ValueName", "name"));
            var text = s.Node("String.Value", ("Value", "stored"));
            var get = s.Node("Scope.GetValue", ("ValueName", "name"));
            var defined = s.Node("Scope.IsDefined", ("ValueName", "name"));
            var undefined = s.Node("Scope.IsDefined", ("ValueName", "nothing"));
            var before = s.Probe("before");
            var after = s.Probe("after");
            s.Flow(entry, "Start", seq, "In")
                .Flow(seq, "Out0", before, "In").Flow(defined, "Value", before, "Value")
                .Flow(seq, "Out1", set, "In").Flow(text, "Value", set, "Value")
                .Flow(seq, "Out2", after, "In").Flow(get, "Value", after, "Value").Flow(undefined, "Value", after, "Value2");

            var records = s.Run();

            Assert.Equal([false], Values(records, "before"));
            Assert.Equal(("after", "stored", (object)false), records[1]);
        }
    }
}
