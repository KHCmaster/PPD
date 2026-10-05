using FlowScriptEngineBasic.FlowSourceObjects.Enumerable;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// Enumerable のノードのテストです。Select などのイベントで、テストが Result を決めます。
    /// </summary>
    public class EnumerableNodeTests
    {
        static readonly object[] numbers = [3, 1, 4, 1, 5];

        static NodeHarness Node(string node, params (string Name, object Value)[] inputs)
        {
            var harness = new NodeHarness("Enumerable." + node);
            foreach (var (name, value) in inputs)
            {
                harness.Set(name, value);
            }
            return harness;
        }

        // 遅延して評価される列は、スクリプトの中で最後まで読む（読むときにイベントが出る）
        static object[] ReadAll(NodeHarness harness, string name = "Enumerable")
        {
            object[] result = null;
            harness.Run(h => result = ((IEnumerable<object>)h.Get(name))?.ToArray());
            return result;
        }

        static int Int(NodeHarness h, string name = "Value") => (int)h.Get(name);

        [Fact]
        public void Select_UsesResultOfEachEvent()
        {
            var select = Node("Select", ("Enumerable", numbers)).On("Select", h => h.Set("Result", Int(h) * 10));

            Assert.Equal(new object[] { 30, 10, 40, 10, 50 }, ReadAll(select));
            Assert.Equal(5, select.Fired.Count(f => f == "Select"));
            Assert.Null(ReadAll(Node("Select")));
        }

        [Fact]
        public void SelectWithIndexAndSelectMany()
        {
            var withIndex = Node("SelectWithIndex", ("Enumerable", new object[] { "a", "b" }))
                .On("Select", h => h.Set("Result", (string)h.Get("Value") + h.Get("Index")));
            Assert.Equal(new object[] { "a0", "b1" }, ReadAll(withIndex));

            var many = Node("SelectMany", ("Enumerable", new object[] { 1, 2 }))
                .On("Select", h => h.Set("Result", Enumerable.Repeat<object>(h.Get("Value"), Int(h))));
            Assert.Equal(new object[] { 1, 2, 2 }, ReadAll(many));
        }

        [Fact]
        public void FilteringWithPredicate()
        {
            NodeHarness Filter(string node, Func<NodeHarness, bool> predicate) =>
                Node(node, ("Enumerable", numbers)).On("Predicate", h => h.Set("Result", predicate(h)));

            Assert.Equal(new object[] { 3, 4, 5 }, ReadAll(Filter("Where", h => Int(h) > 2)));
            Assert.Equal(new object[] { 3 }, ReadAll(Filter("TakeWhile", h => Int(h) > 2)));
            Assert.Equal(new object[] { 1, 4, 1, 5 }, ReadAll(Filter("SkipWhile", h => Int(h) > 2)));
            Assert.Equal(new object[] { 3, 1, 4 }, ReadAll(Filter("TakeWhileWithIndex", h => Int(h, "Index") < 3)));
            Assert.Equal(new object[] { 1, 5 }, ReadAll(Filter("SkipWhileWithIndex", h => Int(h, "Index") < 3)));
        }

        [Fact]
        public void QueriesWithPredicate()
        {
            object Query(string node, string output, Func<NodeHarness, bool> predicate, params (string, object)[] inputs) =>
                Node(node, [("Enumerable", numbers), .. inputs]).On("Predicate", h => h.Set("Result", predicate(h))).Read(output);

            Assert.Equal(true, Query("All", "All", h => Int(h) > 0));
            Assert.Equal(false, Query("All", "All", h => Int(h) > 1));
            Assert.Equal(true, Query("Any", "Any", h => Int(h) == 5));
            Assert.Equal(false, Query("Any", "Any", h => Int(h) == 9));
            Assert.Equal(2, Query("CountWithPredicate", "Count", h => Int(h) == 1));
            Assert.Equal(4, Query("FirstWithPredicate", "First", h => Int(h) % 2 == 0));
            Assert.Equal(5, Query("LastWithPredicate", "Last", h => Int(h) > 2));
            Assert.Equal(4, Query("SingleWithPredicate", "Single", h => Int(h) == 4));
            Assert.Equal("none", Query("FirstOrDefaultWithPredicate", "First", h => false, ("Default", "none")));
            Assert.Equal(1, Query("FirstOrDefaultWithPredicate", "First", h => Int(h) < 2, ("Default", "none")));
            Assert.Equal("none", Query("LastOrDefaultWithPredicate", "Last", h => false, ("Default", "none")));
            Assert.Equal(1, Query("LastOrDefaultWithPredicate", "Last", h => Int(h) < 2, ("Default", "none")));
            Assert.Equal("none", Query("SingleOrDefaultWithPredicate", "Single", h => Int(h) == 1, ("Default", "none")));
            Assert.Equal(3, Query("SingleOrDefaultWithPredicate", "Single", h => Int(h) == 3, ("Default", "none")));
        }

        [Fact]
        public void QueriesWithPredicate_WithoutEnumerable()
        {
            Assert.Equal(false, Node("All").Read("All"));
            Assert.Equal(false, Node("Any").Read("Any"));
            Assert.Equal(0, Node("CountWithPredicate").Read("Count"));
            Assert.Null(Node("FirstWithPredicate").Read("First"));
            Assert.Null(Node("LastWithPredicate").Read("Last"));
            Assert.Null(Node("SingleWithPredicate").Read("Single"));
        }

        [Fact]
        public void Aggregate_Variants()
        {
            static void Add(NodeHarness h) => h.Set("Result", (int)h.Get("Obj1") + (int)h.Get("Obj2"));

            var plain = new NodeHarness(typeof(AggregateFlowSourceObjectBase)).Set("Enumerable", numbers).On("Accumurate", Add);
            Assert.Equal(14, plain.Read("Aggregate"));
            Assert.Equal(4, plain.Fired.Count);

            Assert.Equal(114, Node("AggregateWithSeed", ("Enumerable", numbers), ("Seed", 100)).On("Accumurate", Add).Read("Aggregate"));

            var withResult = Node("AggregateWithSeedAndResultSelect", ("Enumerable", numbers), ("Seed", 0)).On("Accumurate", Add)
                .On("ResultSelect", h => h.Set("ResultSelectResult", "sum=" + h.Get("ResultSelectValue")));
            Assert.Equal("sum=14", withResult.Read("Aggregate"));

            Assert.Null(new NodeHarness(typeof(AggregateFlowSourceObjectBase)).Read("Aggregate"));
            Assert.Null(Node("AggregateWithSeed").Read("Aggregate"));
            Assert.Null(Node("AggregateWithSeedAndResultSelect").Read("Aggregate"));
        }

        [Theory]
        [InlineData("Sum", 14, 14f, 14.0)]
        [InlineData("Average", 2.8, 2.8f, 2.8)]
        [InlineData("Max", 5, 5f, 5.0)]
        [InlineData("Min", 1, 1f, 1.0)]
        public void NumberAggregates(string kind, object int32, float single, double @double)
        {
            object Read(string type, Func<int, object> convert) => Node(kind + type, ("Enumerable", numbers))
                .On("Select", h => h.Set("Result", convert(Int(h)))).Read(kind);

            Assert.Equal(int32, Read("Int32", v => v));
            Assert.Equal(single, (float)Read("Float", v => (float)v), 5);
            Assert.Equal(@double, (double)Read("Double", v => (double)v), 10);
        }

        [Fact]
        public void NumberAggregates_WithoutEnumerable_AreZero()
        {
            foreach (var kind in new[] { "Sum", "Average", "Max", "Min" })
            {
                foreach (var type in new[] { "Int32", "Float", "Double" })
                {
                    Assert.Equal(0.0, Convert.ToDouble(Node(kind + type).Read(kind)));
                }
            }
        }

        [Fact]
        public void OrderBy_UsesSelectAndCompare()
        {
            NodeHarness Order(string node) => Node(node, ("Enumerable", new object[] { "bb", "a", "ccc" }))
                .On("Select", h => h.Set("SelectResult", ((string)h.Get("Value")).Length))
                .On("Compare", h => h.Set("CompareResult", ((int)h.Get("X")).CompareTo((int)h.Get("Y"))));

            Assert.Equal(new object[] { "a", "bb", "ccc" }, ReadAll(Order("OrderBy"), "OrderedEnumerable"));
            Assert.Equal(new object[] { "ccc", "bb", "a" }, ReadAll(Order("OrderByDescending"), "OrderedEnumerable"));
            Assert.Null(ReadAll(Node("OrderBy"), "OrderedEnumerable"));
            Assert.Null(ReadAll(Node("OrderByDescending"), "OrderedEnumerable"));
        }

        static void Parity(NodeHarness h) => h.Set("Result", Int(h) % 2 == 0 ? "even" : "odd");

        [Fact]
        public void GroupBy_Variants()
        {
            var groups = ReadAll(Node("GroupBy", ("Enumerable", numbers)).On("Select", Parity), "Grouping").Cast<IGrouping<object, object>>().ToArray();
            Assert.Equal(new object[] { "odd", "even" }, groups.Select(g => g.Key));
            Assert.Equal(new object[] { 3, 1, 1, 5 }, groups[0]);

            var elements = ReadAll(Node("GroupByWithElementSelect", ("Enumerable", numbers)).On("Select", Parity)
                .On("ElementSelect", h => h.Set("ElementSelectResult", Int(h, "ElementSelectValue") * 10)), "Grouping")
                .Cast<IGrouping<object, object>>().ToArray();
            Assert.Equal(new object[] { 40 }, elements[1]);

            var results = ReadAll(Node("GroupByWithElementSelectAndResultSelect", ("Enumerable", numbers)).On("Select", Parity)
                .On("ElementSelect", h => h.Set("ElementSelectResult", h.Get("ElementSelectValue")))
                .On("ResultSelect", h => h.Set("ResultSelectResult", $"{h.Get("ResultSelectKey")}:{((IEnumerable<object>)h.Get("ResultSelectValues")).Count()}")), "Grouping");
            Assert.Equal(new object[] { "odd:4", "even:1" }, results);

            Assert.Null(ReadAll(Node("GroupBy"), "Grouping"));
            Assert.Null(ReadAll(Node("GroupByWithElementSelect"), "Grouping"));
            Assert.Null(ReadAll(Node("GroupByWithElementSelectAndResultSelect"), "Grouping"));
        }

        [Fact]
        public void ToLookupAndToHashtable()
        {
            ILookup<object, object> lookup = null;
            var toLookup = Node("ToLookup", ("Enumerable", numbers)).On("Select", Parity)
                .On("ElementSelect", h => h.Set("ElementSelectResult", -Int(h, "ElementSelectValue")));
            toLookup.Run(h => lookup = (ILookup<object, object>)h.Get("Lookup"));
            Assert.Equal(new object[] { -3, -1, -1, -5 }, lookup["odd"]);

            var table = (Dictionary<object, object>)Node("ToHashtable", ("Enumerable", new object[] { "a", "bb" }))
                .On("Select", h => h.Set("Result", ((string)h.Get("Value")).Length)).Read("Hashtable");
            Assert.Equal("bb", table[2]);

            var withElement = (Dictionary<object, object>)Node("ToHashtableWithElementSelect", ("Enumerable", new object[] { "a", "bb" }))
                .On("Select", h => h.Set("Result", h.Get("Value")))
                .On("ElementSelect", h => h.Set("ElementSelectResult", ((string)h.Get("ElementSelectValue")).ToUpperInvariant())).Read("Hashtable");
            Assert.Equal("BB", withElement["bb"]);

            Assert.Null(Node("ToLookup").Read("Lookup"));
            Assert.Empty((Dictionary<object, object>)Node("ToHashtable").Read("Hashtable"));
            Assert.Empty((Dictionary<object, object>)Node("ToHashtableWithElementSelect").Read("Hashtable"));
        }

        [Fact]
        public void GroupJoin_MatchesInnerByKey()
        {
            var join = Node("GroupJoin", ("Enumerable", new object[] { "odd", "even" }), ("InnerEnumerable", numbers))
                .On("OuterSelect", h => h.Set("OuterSelectResult", h.Get("OuterSelectValue")))
                .On("InnerSelect", h => h.Set("InnerSelectResult", Int(h, "InnerSelectValue") % 2 == 0 ? "even" : "odd"))
                .On("ResultSelect", h => h.Set("ResultSelectResult", $"{h.Get("ResultSelectValue")}:{string.Join(",", (IEnumerable<object>)h.Get("ResultSelectValues"))}"));

            Assert.Equal(new object[] { "odd:3,1,1,5", "even:4" }, ReadAll(join));
            Assert.Empty(ReadAll(Node("GroupJoin", ("Enumerable", numbers))));
        }

        [Fact]
        public void Zip_CombinesPairs()
        {
            var zip = Node("Zip", ("First", new object[] { 1, 2, 3 }), ("Second", new object[] { "a", "b" }))
                .On("Select", h => h.Set("Result", $"{h.Get("Obj1")}{h.Get("Obj2")}"));
            Assert.Equal(new object[] { "1a", "2b" }, ReadAll(zip));

            Assert.Equal(new object[] { "a" }, ReadAll(Node("Zip", ("Second", new object[] { "a" }))));
            Assert.Empty(ReadAll(Node("Zip")));
        }

        [Fact]
        public void SetOperations()
        {
            object[] first = [1, 2, 2, 3];
            object[] second = [3, 4];
            object[] Combine(string node) => ReadAll(Node(node, ("First", first), ("Second", second)));

            Assert.Equal(new object[] { 1, 2, 2, 3, 3, 4 }, Combine("Concat"));
            Assert.Equal(new object[] { 1, 2 }, Combine("Except"));
            Assert.Equal(new object[] { 3 }, Combine("Intersect"));
            Assert.Equal(new object[] { 1, 2, 3, 4 }, Combine("Union"));
            Assert.Equal(true, Node("SequentialEqual", ("First", first), ("Second", new object[] { 1, 2, 2, 3 })).Read("Value"));
            Assert.Equal(false, Node("SequentialEqual", ("First", first), ("Second", second)).Read("Value"));
        }

        [Fact]
        public void ElementQueries()
        {
            object Get(string node, string output, params (string, object)[] inputs) => Node(node, [("Enumerable", numbers), .. inputs]).Read(output);

            Assert.Equal(5, Get("Count", "Count"));
            Assert.Equal(true, Get("Contains", "Contains", ("Value", 4)));
            Assert.Equal(false, Get("Contains", "Contains", ("Value", 9)));
            Assert.Equal(4, Get("ElementAt", "Value", ("Index", 2)));
            Assert.Equal(5, Get("ElementAtOrDefault", "Value", ("Index", 4), ("Default", "none")));
            Assert.Equal("none", Get("ElementAtOrDefault", "Value", ("Index", 9), ("Default", "none")));
            Assert.Equal(3, Get("First", "First"));
            Assert.Equal(5, Get("Last", "Last"));
            Assert.Equal(3, Get("FirstOrDefault", "First", ("Default", "none")));
            Assert.Equal(5, Get("LastOrDefault", "Last", ("Default", "none")));
            Assert.Equal(7, Node("Single", ("Enumerable", new object[] { 7 })).Read("Single"));
            Assert.Equal("none", Node("SingleOrDefault", ("Enumerable", Array.Empty<object>()), ("Default", "none")).Read("Single"));
            Assert.Equal("none", Node("FirstOrDefault", ("Enumerable", Array.Empty<object>()), ("Default", "none")).Read("First"));
            Assert.Equal("none", Node("LastOrDefault", ("Enumerable", Array.Empty<object>()), ("Default", "none")).Read("Last"));
        }

        [Fact]
        public void Reshaping()
        {
            Assert.Equal(new object[] { 3, 1, 4, 5 }, ReadAll(Node("Distinct", ("Enumerable", numbers))));
            Assert.Equal(new object[] { 5, 1, 4, 1, 3 }, ReadAll(Node("Reverse", ("Enumerable", numbers))));
            Assert.Equal(new object[] { 3, 1 }, ReadAll(Node("Take", ("Enumerable", numbers), ("Count", 2))));
            Assert.Equal(new object[] { 1, 5 }, ReadAll(Node("Skip", ("Enumerable", numbers), ("Count", 3))));
            Assert.Equal(numbers, (object[])Node("ToArray", ("Enumerable", numbers)).Read("Array"));
            Assert.Equal(numbers, (List<object>)Node("ToArrayList", ("Enumerable", numbers)).Read("ArrayList"));
            Assert.Equal(new object[] { "x" }, ReadAll(Node("DefaultIfEmpty", ("Enumerable", Array.Empty<object>()), ("Default", "x"))));
            Assert.Equal(new object[] { "x" }, ReadAll(Node("DefaultIfEmpty", ("Default", "x"))));
            Assert.Equal(numbers, ReadAll(Node("DefaultIfEmpty", ("Enumerable", numbers), ("Default", "x"))));
        }
    }
}
