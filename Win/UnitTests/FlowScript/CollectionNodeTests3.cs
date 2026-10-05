using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// ArrayList の残りのノードと、Enumerable の Ordered、Grouping、Lookup のノードのテストです。
    /// </summary>
    public class CollectionNodeTests3
    {
        static List<object> List(params object[] items) => [.. items];

        static NodeHarness Ok(string node, params (string Name, object Value)[] inputs) => OkWith(node, null, inputs);

        // event: イベント（Predicate など）が出たときの処理
        static NodeHarness OkWith(string node, (string Name, System.Action<NodeHarness> Handler)? @event, params (string Name, object Value)[] inputs)
        {
            var harness = new NodeHarness(node);
            if (@event is var (eventName, handler))
            {
                harness.On(eventName, handler);
            }
            foreach (var (name, value) in inputs)
            {
                harness.Set(name, value);
            }
            harness.Invoke();
            Assert.Equal("Success", harness.Fired.Last());
            return harness;
        }

        [Theory]
        [InlineData("ArrayList.BinarySearch")]
        [InlineData("ArrayList.BinarySearchWithLimit")]
        [InlineData("ArrayList.FindIndex")]
        [InlineData("ArrayList.FindIndexWithLimit")]
        [InlineData("ArrayList.FindLastIndex")]
        [InlineData("ArrayList.FindLastIndexWithLimit")]
        [InlineData("ArrayList.GetRange")]
        [InlineData("ArrayList.LastIndexOf")]
        [InlineData("ArrayList.LastIndexOfWithLimit")]
        [InlineData("ArrayList.SetAt")]
        [InlineData("ArrayList.SetRange")]
        public void WithoutList_Fails(string node)
        {
            Assert.Equal(["Failed"], new NodeHarness(node).Invoke().Fired);
        }

        [Fact]
        public void ArrayList_RangesAndIndexes()
        {
            var list = List("a", "b", "c", "b");

            Assert.Equal(["b", "c"], (List<object>)Ok("ArrayList.GetRange", ("ArrayList", list), ("Index", 1), ("Count", 2)).Get("Value"));
            Assert.Equal(3, Ok("ArrayList.LastIndexOf", ("ArrayList", list), ("Object", "b")).Get("Value"));
            Assert.Equal(1, Ok("ArrayList.LastIndexOfWithLimit", ("ArrayList", list), ("Object", "b"), ("StartIndex", 2), ("Count", 3)).Get("Value"));
            Ok("ArrayList.SetAt", ("ArrayList", list), ("Index", 0), ("Value", "z"));
            Assert.Equal(["z", "b", "c", "b"], list);
            Ok("ArrayList.SetRange", ("ArrayList", list), ("Index", 0), ("Value", List("x", "y")));
            Assert.Equal(["x", "y", "c", "b"], list);
            Assert.Equal([7, 7, 7], (List<object>)Ok("ArrayList.Repeat", ("Object", 7), ("Count", 3)).Get("Value"));
            Assert.Equal(true, new NodeHarness("ArrayList.IsType").Set("Target", list).Get("Value"));
            Assert.Equal(false, new NodeHarness("ArrayList.IsType").Set("Target", "list").Get("Value"));
            Assert.Equal(false, new NodeHarness("ArrayList.IsType").Get("Value"));
        }

        [Fact]
        public void ArrayList_FindWithPredicate()
        {
            var list = List(1, 4, 6, 7);
            NodeHarness Find(string node, System.Func<int, bool> predicate, params (string, object)[] inputs) =>
                OkWith(node, ("Predicate", h => h.Set("Result", predicate((int)h.Get("Value")))), [("ArrayList", list), .. inputs]);

            Assert.Equal(1, Find("ArrayList.FindIndex", v => v % 2 == 0).Get("FoundIndex"));
            Assert.Equal(2, Find("ArrayList.FindLastIndex", v => v % 2 == 0).Get("FoundIndex"));
            Assert.Equal(-1, Find("ArrayList.FindIndex", v => v > 10).Get("FoundIndex"));
            Assert.Equal(3, Find("ArrayList.FindIndexWithLimit", v => v > 6, ("StartIndex", 2), ("Count", 2)).Get("FoundIndex"));
            Assert.Equal(1, Find("ArrayList.FindLastIndexWithLimit", v => v < 5, ("StartIndex", 2), ("Count", 2)).Get("FoundIndex"));
        }

        [Fact]
        public void ArrayList_BinarySearch()
        {
            var list = List(1, 3, 5, 7, 9);
            static void Compare(NodeHarness h) => h.Set("Result", ((int)h.Get("X")).CompareTo((int)h.Get("Y")));

            Assert.Equal(3, OkWith("ArrayList.BinarySearch", ("Compare", Compare), ("ArrayList", list), ("Value", 7)).Get("FoundIndex"));
            Assert.True((int)OkWith("ArrayList.BinarySearch", ("Compare", Compare), ("ArrayList", list), ("Value", 4)).Get("FoundIndex") < 0);
            Assert.Equal(1, OkWith("ArrayList.BinarySearchWithLimit", ("Compare", Compare), ("ArrayList", list), ("Value", 3), ("StartIndex", 0), ("Count", 3)).Get("FoundIndex"));
            // 範囲の外にあるものは見つからない
            Assert.True((int)OkWith("ArrayList.BinarySearchWithLimit", ("Compare", Compare), ("ArrayList", list), ("Value", 9), ("StartIndex", 0), ("Count", 3)).Get("FoundIndex") < 0);
        }

        [Fact]
        public void ArrayList_FromElement_UsesConnectedInputs()
        {
            var s = new ScriptRunner();
            var entry = s.Entry();
            var inner = s.Node("ArrayList.FromElement");
            var outer = s.Node("ArrayList.FromElement", ("IsExpanded", "True"));
            var probe = s.Probe("list");
            s.Flow(entry, "Start", probe, "In").Flow(outer, "Value", probe, "Value")
                .Flow(s.Node("Int32.Value", ("Value", "1")), "Value", inner, "A")
                .Flow(s.Node("Int32.Value", ("Value", "1")), "Value", inner, "C")
                .Flow(inner, "Value", outer, "B")
                .Flow(s.Node("Int32.Value", ("Value", "3")), "Value", outer, "F");

            var value = (List<object>)Assert.Single(ScriptRunner.Values(s.Run(), "list"));

            Assert.Equal([1, 1, 3], value);
        }

        static IOrderedEnumerable<object> ByLength(params object[] items) => items.OrderBy(i => ((string)i).Length);

        [Fact]
        public void Ordered_ThenByAndThenByDescending()
        {
            NodeHarness Then(string node) => new NodeHarness("Enumerable.Ordered." + node).Set("OrderedEnumerable", ByLength("bb", "c", "aa", "b"))
                .On("Select", h => h.Set("SelectResult", h.Get("Value")))
                .On("Compare", h => h.Set("CompareResult", string.CompareOrdinal((string)h.Get("X"), (string)h.Get("Y"))));
            object[] Read(NodeHarness h)
            {
                object[] result = null;
                h.Run(_ => result = ((IEnumerable<object>)h.Get("OrderedEnumerable"))?.ToArray());
                return result;
            }

            Assert.Equal(new object[] { "b", "c", "aa", "bb" }, Read(Then("ThenBy")));
            Assert.Equal(new object[] { "c", "b", "bb", "aa" }, Read(Then("ThenByDescending")));
            Assert.Null(Read(new NodeHarness("Enumerable.Ordered.ThenBy")));
            Assert.Null(Read(new NodeHarness("Enumerable.Ordered.ThenByDescending")));
        }

        [Fact]
        public void GroupingAndLookup()
        {
            var grouping = new object[] { 1, 2, 3 }.GroupBy(i => (object)((int)i % 2)).First();
            Assert.Equal(1, new NodeHarness("Enumerable.Grouping.Value").Set("Grouping", grouping).Get("Key"));
            Assert.Null(new NodeHarness("Enumerable.Grouping.Value").Get("Key"));

            var lookup = new object[] { "a", "bb", "cc" }.ToLookup(s => (object)((string)s).Length);
            var value = new NodeHarness("Enumerable.Lookup.Value").Set("Lookup", lookup).Set("Key", 2);
            Assert.Equal(2, value.Get("Count"));
            Assert.Equal(new object[] { "bb", "cc" }, ((IEnumerable<object>)value.Get("EnumerableWithKey")).ToArray());
            var empty = new NodeHarness("Enumerable.Lookup.Value");
            Assert.Equal(0, empty.Get("Count"));
            Assert.Empty((IEnumerable<object>)empty.Get("EnumerableWithKey"));

            Assert.Equal(true, Ok("Enumerable.Lookup.Contains", ("Lookup", lookup), ("Key", 1)).Get("Result"));
            Assert.Equal(false, Ok("Enumerable.Lookup.Contains", ("Lookup", lookup), ("Key", 3)).Get("Result"));
            Assert.Equal(["Failed"], new NodeHarness("Enumerable.Lookup.Contains").Invoke().Fired);
        }
    }
}
