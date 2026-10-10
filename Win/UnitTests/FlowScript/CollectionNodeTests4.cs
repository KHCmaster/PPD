using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// つないだ入力だけを使うノード（要素からの配列、組からの辞書、Switch）の、6 つの入力すべてのテストです。
    /// </summary>
    public class CollectionNodeTests4
    {
        static readonly string[] Inputs = ["A", "B", "C", "D", "E", "F"];

        // 要素から作ったものを、スクリプトを動かして受け取る
        static object Build(string node, bool expand, System.Func<ScriptRunner, int, (int Node, string Output)> input)
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var target = expand ? s.Node(node, ("IsExpanded", "True")) : s.Node(node);
            var probe = s.Probe("value");
            s.Flow(entry, "Start", probe, "In").Flow(target, "Value", probe, "Value");
            for (var i = 0; i < Inputs.Length; i++)
            {
                var (source, output) = input(s, i);
                s.Flow(source, output, target, Inputs[i]);
            }
            return Assert.Single(ScriptRunner.Values(s.Run(), "value"));
        }

        static (int, string) Int(ScriptRunner s, int value) => (s.Node("Int32.Value", ("Value", value.ToString())), "Value");

        [Theory]
        [InlineData("Array.FromElement")]
        [InlineData("ArrayList.FromElement")]
        [InlineData("HashSet.FromElement")]
        public void FromElement_UsesAllSixInputs(string node)
        {
            var value = (IEnumerable<object>)Build(node, false, Int);

            Assert.Equal([0, 1, 2, 3, 4, 5], value.Cast<int>().Order());
        }

        [Theory]
        [InlineData("Array.FromElement")]
        [InlineData("ArrayList.FromElement")]
        [InlineData("HashSet.FromElement")]
        public void FromElement_ExpandsInnerCollections(string node)
        {
            // どの入力にも、2 つの要素の集まりをつなぐ
            var value = (IEnumerable<object>)Build(node, true, (s, i) =>
            {
                var inner = s.Node(node);
                s.Flow(Int(s, i * 10).Item1, "Value", inner, "A").Flow(Int(s, i * 10 + 1).Item1, "Value", inner, "B");
                return (inner, "Value");
            });

            Assert.Equal(12, value.Count());
            Assert.Contains(51, value.Cast<int>());
        }

        static (int, string) Pair(ScriptRunner s, int i)
        {
            var pair = s.Node("Pair.FromKeyValue");
            s.Flow(Int(s, i).Item1, "Value", pair, "Key").Flow(Int(s, i * 100).Item1, "Value", pair, "Value");
            return (pair, "Pair");
        }

        [Fact]
        public void FromPair_UsesAllSixInputs()
        {
            var value = (Dictionary<object, object>)Build("Hashtable.FromPair", false, Pair);

            Assert.Equal(6, value.Count);
            Assert.Equal(500, value[5]);
        }

        [Fact]
        public void FromPair_ExpandsInnerDictionaries()
        {
            // 値が辞書の組は、その中身を入れる
            var value = (Dictionary<object, object>)Build("Hashtable.FromPair", true, (s, i) =>
            {
                var inner = s.Node("Hashtable.FromPair");
                var (p, output) = Pair(s, i + 10);
                s.Flow(p, output, inner, "A");
                var outer = s.Node("Pair.FromKeyValue");
                s.Flow(Int(s, i).Item1, "Value", outer, "Key").Flow(inner, "Value", outer, "Value");
                return (outer, "Pair");
            });

            Assert.Equal([10, 11, 12, 13, 14, 15], value.Keys.Cast<int>().Order());
        }

        [Theory]
        [InlineData(3, "out3")]
        [InlineData(4, "out4")]
        [InlineData(5, "out5")]
        [InlineData(6, "out6")]
        [InlineData(9, "default")]
        public void Switch_ChecksSixTargets(int value, string expected)
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var sw = s.Node("Logic.Switch");
            s.Flow(entry, "Start", sw, "In").Flow(Int(s, value).Item1, "Value", sw, "Value").Flow(sw, "Default", s.Probe("default"), "In");
            for (var i = 1; i <= 6; i++)
            {
                s.Flow(Int(s, i).Item1, "Value", sw, "Target" + i).Flow(sw, "Out" + i, s.Probe("out" + i), "In");
            }

            Assert.Equal([expected], s.Run().Select(r => r.Label));
        }

        [Fact]
        public void Switch_NullMatchesNull()
        {
            // 値をつながないと null。null の目標と同じとみなす
            var s = new ScriptRunner();
            var entry = s.Entry();
            var sw = s.Node("Logic.Switch");
            s.Flow(entry, "Start", sw, "In").Flow(s.Node("Object.Null"), "Value", sw, "Target1")
                .Flow(sw, "Out1", s.Probe("out1"), "In").Flow(sw, "Default", s.Probe("default"), "In");

            Assert.Equal(["out1"], s.Run().Select(r => r.Label));
        }
    }
}
