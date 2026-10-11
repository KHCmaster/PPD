using FlowScriptEngineBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// HashSet、Hashtable、Stream、Stopwatch、Random、Logic.All と Any のノードのテストです。
    /// </summary>
    public class CollectionNodeTests2
    {
        static NodeHarness Ok(string node, params (string Name, object Value)[] inputs)
        {
            var harness = new NodeHarness(node);
            foreach (var (name, value) in inputs)
            {
                harness.Set(name, value);
            }
            harness.Invoke();
            Assert.Equal(["Success"], harness.Fired);
            return harness;
        }

        static NodeHarness Failed(string node)
        {
            var harness = new NodeHarness(node).Invoke();
            Assert.Equal(["Failed"], harness.Fired);
            return harness;
        }

        [Fact]
        public void HashSet_AddRemoveAndQueries()
        {
            var set = (HashSet<object>)new NodeHarness("HashSet.Value").Invoke().Get("Value");

            Assert.Equal(true, Ok("HashSet.Add", ("HashSet", set), ("Item", "a")).Get("IsNew"));
            Assert.Equal(false, Ok("HashSet.Add", ("HashSet", set), ("Item", "a")).Get("IsNew"));
            Ok("HashSet.Add", ("HashSet", set), ("Item", "b"));
            Assert.Equal(2, Ok("HashSet.Count", ("HashSet", set)).Get("Value"));
            Assert.Equal(true, Ok("HashSet.Contains", ("HashSet", set), ("Item", "b")).Get("Result"));
            Assert.Equal(true, Ok("HashSet.Remove", ("HashSet", set), ("Item", "b")).Get("Removed"));
            Assert.Equal(false, Ok("HashSet.Remove", ("HashSet", set), ("Item", "b")).Get("Removed"));
            Ok("HashSet.Clear", ("HashSet", set));
            Assert.Empty(set);

            foreach (var node in new[] { "HashSet.Add", "HashSet.Clear", "HashSet.Contains", "HashSet.Remove", "HashSet.UnionWith", "HashSet.IsSubsetOf", "HashSet.SetEquals" })
            {
                Failed(node);
            }
            // Count は ArrayList.Count と同じく、HashSet がなければ 0
            Assert.Equal(0, Ok("HashSet.Count").Get("Value"));
        }

        [Fact]
        public void HashSet_SetOperations()
        {
            static HashSet<object> Set(params object[] items) => [.. items];
            object[] other = [2, 3, 4];

            var union = Set(1, 2);
            Ok("HashSet.UnionWith", ("HashSet", union), ("List", other));
            Assert.Equal([1, 2, 3, 4], union.Order());
            var intersect = Set(1, 2, 3);
            Ok("HashSet.IntersectWith", ("HashSet", intersect), ("List", other));
            Assert.Equal([2, 3], intersect.Order());
            var except = Set(1, 2, 3);
            Ok("HashSet.ExceptWith", ("HashSet", except), ("List", other));
            Assert.Equal([1], except);
            var symmetric = Set(1, 2, 3);
            Ok("HashSet.SymmetricExceptWith", ("HashSet", symmetric), ("List", other));
            Assert.Equal([1, 4], symmetric.Order());

            Assert.Equal(true, Ok("HashSet.IsSubsetOf", ("HashSet", Set(2, 3)), ("List", other)).Get("Result"));
            Assert.Equal(true, Ok("HashSet.IsProperSubsetOf", ("HashSet", Set(2, 3)), ("List", other)).Get("Result"));
            Assert.Equal(false, Ok("HashSet.IsSupersetOf", ("HashSet", Set(2, 3)), ("List", other)).Get("Result"));
            Assert.Equal(true, Ok("HashSet.IsProperSupersetOf", ("HashSet", Set(1, 2, 3, 4)), ("List", other)).Get("Result"));
            Assert.Equal(true, Ok("HashSet.Overlaps", ("HashSet", Set(4, 9)), ("List", other)).Get("Result"));
            Assert.Equal(true, Ok("HashSet.SetEquals", ("HashSet", Set(4, 3, 2)), ("List", other)).Get("Result"));
        }

        // つないだ入力だけを使うノードは、スクリプトとして動かす
        [Fact]
        public void HashSet_FromElement_UsesConnectedInputs()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var set = s.Node("HashSet.FromElement");
            var probe = s.Probe("set");
            s.Flow(entry, "Start", probe, "In").Flow(set, "Value", probe, "Value")
                .Flow(s.Node("Int32.Value", ("Value", "1")), "Value", set, "A")
                .Flow(s.Node("Int32.Value", ("Value", "1")), "Value", set, "B")
                .Flow(s.Node("Int32.Value", ("Value", "2")), "Value", set, "C");

            var value = (HashSet<object>)Assert.Single(ScriptRunner.Values(s.Run(), "set"));

            Assert.Equal([1, 2], value.Order());
        }

        [Fact]
        public void HashSet_FromElement_ExpandsSets()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var inner = s.Node("HashSet.FromElement");
            var outer = s.Node("HashSet.FromElement", ("IsExpanded", "True"));
            var probe = s.Probe("set");
            s.Flow(entry, "Start", probe, "In").Flow(outer, "Value", probe, "Value")
                .Flow(s.Node("Int32.Value", ("Value", "1")), "Value", inner, "A")
                .Flow(s.Node("Int32.Value", ("Value", "2")), "Value", inner, "B")
                .Flow(inner, "Value", outer, "A")
                .Flow(s.Node("Int32.Value", ("Value", "3")), "Value", outer, "B");

            var value = (HashSet<object>)Assert.Single(ScriptRunner.Values(s.Run(), "set"));

            Assert.Equal([1, 2, 3], value.Order());
        }

        [Fact]
        public void Hashtable_RemainingOperations()
        {
            var table = new Dictionary<object, object> { ["a"] = 1, ["b"] = 2 };

            Assert.Equal(2, Ok("Hashtable.Count", ("Hashtable", table)).Get("Value"));
            Assert.Equal(true, Ok("Hashtable.ContainsValue", ("Hashtable", table), ("Value", 2)).Get("Contains"));
            Assert.Equal(new object[] { "a", "b" }, [.. ((IEnumerable<object>)Ok("Hashtable.Keys", ("Hashtable", table)).Get("Value"))]);
            Assert.Equal(new object[] { 1, 2 }, [.. ((IEnumerable<object>)Ok("Hashtable.Values", ("Hashtable", table)).Get("Value"))]);
            var pairs = ((IEnumerable<object>)new NodeHarness("Hashtable.Pairs").Set("Hashtable", table).Get("Pairs")).ToArray();
            Assert.Equal(new KeyValuePair<object, object>("a", 1), pairs[0]);
            Ok("Hashtable.Remove", ("Hashtable", table), ("Key", "a"));
            Assert.Equal(["b"], table.Keys);
            Ok("Hashtable.Clear", ("Hashtable", table));
            Assert.Empty(table);

            foreach (var node in new[] { "Hashtable.Add", "Hashtable.Clear", "Hashtable.ContainsKey", "Hashtable.ContainsValue", "Hashtable.Remove", "Hashtable.Keys", "Hashtable.Values", "Hashtable.GetValue" })
            {
                Failed(node);
            }
            Assert.Equal(0, Ok("Hashtable.Count").Get("Value"));
        }

        [Fact]
        public void Hashtable_FromPair_UsesConnectedPairs()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var table = s.Node("Hashtable.FromPair");
            var probe = s.Probe("table");
            s.Flow(entry, "Start", probe, "In").Flow(table, "Value", probe, "Value");
            foreach (var (input, key, value) in new[] { ("A", "x", "1"), ("C", "y", "2") })
            {
                var pair = s.Node("Pair.FromKeyValue");
                s.Flow(pair, "Pair", table, input)
                    .Flow(s.Node("String.Value", ("Value", key)), "Value", pair, "Key")
                    .Flow(s.Node("Int32.Value", ("Value", value)), "Value", pair, "Value");
            }

            var result = (Dictionary<object, object>)Assert.Single(ScriptRunner.Values(s.Run(), "table"));

            Assert.Equal(2, result.Count);
            Assert.Equal(1, result["x"]);
            Assert.Equal(2, result["y"]);
        }

        static MemoryStream Bytes()
        {
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write((byte)7);
                writer.Write((short)-2);
                writer.Write((ushort)65000);
                writer.Write(123456);
                writer.Write(1.5f);
                writer.Write(2.25);
            }
            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void Stream_ReadsValuesInOrder()
        {
            var stream = Bytes();

            Assert.Equal(7, Ok("Stream.ReadByte", ("Stream", stream)).Get("Value"));
            Assert.Equal(-2, Ok("Stream.ReadInt16", ("Stream", stream)).Get("Value"));
            Assert.Equal(65000, Ok("Stream.ReadUInt16", ("Stream", stream)).Get("Value"));
            Assert.Equal(123456, Ok("Stream.ReadInt32", ("Stream", stream)).Get("Value"));
            Assert.Equal(1.5f, Ok("Stream.ReadFloat", ("Stream", stream)).Get("Value"));
            Assert.Equal(2.25, Ok("Stream.ReadDouble", ("Stream", stream)).Get("Value"));
            Assert.Equal((int)stream.Length, new NodeHarness("Stream.Length").Set("Stream", stream).Get("Value"));
            Assert.Equal((int)stream.Length, new NodeHarness("Stream.Position").Set("Stream", stream).Get("Value"));

            Ok("Stream.Seek", ("Stream", stream), ("Offset", 1), ("SeekOrigin", SeekOrigin.Begin));
            Assert.Equal(-2, Ok("Stream.ReadInt16", ("Stream", stream)).Get("Value"));
        }

        [Fact]
        public void Stream_ReadsText()
        {
            var text = new MemoryStream(Encoding.UTF8.GetBytes("一行目\n二行目"));
            Assert.Equal("一行目\n二行目", Ok("Stream.ReadAllText", ("Stream", text), ("Encoding", EncodingType.UTF_8)).Get("Value"));

            var lines = new MemoryStream(Encoding.UTF8.GetBytes("a\r\nb\nc"));
            Assert.Equal(new object[] { "a", "b", "c" }, [.. ((List<object>)Ok("Stream.ReadAllLines", ("Stream", lines), ("Encoding", EncodingType.UTF_8)).Get("Value"))]);

            // 日本語の文字コード（ゲームでは起動したときにコードページを使えるようにしている）
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var sjis = new MemoryStream(Encoding.GetEncoding(932).GetBytes("譜面"));
            Assert.Equal("譜面", Ok("Stream.ReadAllText", ("Stream", sjis), ("Encoding", EncodingType.SHIFT_JIS)).Get("Value"));
            var euc = new MemoryStream(Encoding.GetEncoding(20932).GetBytes("譜面"));
            Assert.Equal("譜面", Ok("Stream.ReadAllText", ("Stream", euc), ("Encoding", EncodingType.EUC_JP)).Get("Value"));

            foreach (var node in new[] { "Stream.ReadByte", "Stream.ReadInt32", "Stream.ReadAllText", "Stream.ReadAllLines", "Stream.Seek" })
            {
                Failed(node);
            }
        }

        [Fact]
        public void Stopwatch_StartStopAndReset()
        {
            var watch = (Stopwatch)new NodeHarness("Stopwatch.Value").Invoke().Get("Value");

            Ok("Stopwatch.Start", ("Stopwatch", watch));
            Assert.True(watch.IsRunning);
            System.Threading.Thread.Sleep(5);
            Ok("Stopwatch.Stop", ("Stopwatch", watch));
            var elapsed = Ok("Stopwatch.Elapsed", ("Stopwatch", watch));
            Assert.True((double)elapsed.Get("Milliseconds") > 0);
            Assert.True((double)elapsed.Get("ExactSeconds") > 0);
            Ok("Stopwatch.Reset", ("Stopwatch", watch));
            Assert.Equal(0, watch.ElapsedTicks);
            Assert.Equal(true, new NodeHarness("Stopwatch.IsType").Set("Target", watch).Get("Value"));

            foreach (var node in new[] { "Stopwatch.Start", "Stopwatch.Stop", "Stopwatch.Reset", "Stopwatch.Elapsed" })
            {
                Failed(node);
            }
        }

        [Fact]
        public void Random_StaysInRange()
        {
            var next = new NodeHarness("Random.Next").Set("Min", 3).Set("Max", 6);
            var nextDouble = new NodeHarness("Random.NextDouble");
            for (int i = 0; i < 50; i++)
            {
                Assert.InRange((int)next.Get("Value"), 3, 5);
                Assert.InRange((double)nextDouble.Get("Value"), 0, 1);
            }

        }

        [Fact]
        public void RandomTrigger_FiresOneConnectedOutput()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var seq = s.Node("Logic.Sequential");
            var trigger = s.Node("Random.Trigger");
            s.Flow(entry, "Start", seq, "In");
            for (int i = 0; i < 6; i++)
            {
                s.Flow(seq, "Out" + i, trigger, "In");
            }
            // つないでいない出力（Out1）は選ばれない
            s.Flow(trigger, "Out0", s.Probe("out0"), "In").Flow(trigger, "Out2", s.Probe("out2"), "In");

            var records = s.Run();

            Assert.Equal(6, records.Count);
            Assert.All(records, r => Assert.True(r.Label is "out0" or "out2", r.Label));
        }

        [Fact]
        public void LogicAll_FiresWhenEveryConnectedInputArrivedAndCanReset()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var seq = s.Node("Logic.Sequential");
            var all = s.Node("Logic.All");
            s.Flow(entry, "Start", seq, "In")
                .Flow(seq, "Out0", all, "In0").Flow(seq, "Out1", s.Probe("after In0"), "In")
                .Flow(seq, "Out2", all, "In2")
                .Flow(seq, "Out3", all, "Reset").Flow(seq, "Out4", all, "In0")
                .Flow(seq, "Out5", s.Probe("after reset"), "In")
                .Flow(seq, "Out6", all, "In2")
                .Flow(all, "Out", s.Probe("all"), "In");

            var labels = s.Run().Select(r => r.Label);

            // In0 と In2 だけをつないでいるので、その 2 つがそろえば出る
            Assert.Equal(["after In0", "all", "after reset", "all"], labels);
        }

        [Fact]
        public void LogicAny_FiresForEachInput()
        {
            var any = new NodeHarness("Logic.Any");

            any.Invoke("In0").Invoke("In3");

            Assert.Equal(["Out", "Out"], any.Fired);
        }
    }
}
