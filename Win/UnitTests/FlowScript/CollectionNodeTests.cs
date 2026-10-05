using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// ArrayList と Array のノード（In で処理して Success か Failed を出すもの）のテストです。
    /// </summary>
    public class CollectionNodeTests
    {
        static List<object> List(params object[] items) => [.. items];

        // In を呼び、Success になったノードを返す
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

        [Theory]
        [InlineData("ArrayList.Add")]
        [InlineData("ArrayList.AddRange")]
        [InlineData("ArrayList.Clear")]
        [InlineData("ArrayList.Contains")]
        [InlineData("ArrayList.IndexOf")]
        [InlineData("ArrayList.IndexOfWithLimit")]
        [InlineData("ArrayList.Insert")]
        [InlineData("ArrayList.InsertRange")]
        [InlineData("ArrayList.Remove")]
        [InlineData("ArrayList.RemoveAll")]
        [InlineData("ArrayList.RemoveAt")]
        [InlineData("ArrayList.RemoveRange")]
        [InlineData("ArrayList.Reverse")]
        [InlineData("ArrayList.ReverseWithLimit")]
        [InlineData("ArrayList.Sort")]
        [InlineData("ArrayList.SortWithLimit")]
        [InlineData("Array.Clear")]
        [InlineData("Array.ClearWithLimit")]
        [InlineData("Array.Copy")]
        [InlineData("Array.CopyWithLimit")]
        [InlineData("Array.IndexOf")]
        [InlineData("Array.IndexOfWithLimit")]
        [InlineData("Array.LastIndexOf")]
        [InlineData("Array.LastIndexOfWithLimit")]
        [InlineData("Array.Reverse")]
        [InlineData("Array.ReverseWithLimit")]
        [InlineData("Array.SetAt")]
        [InlineData("Array.Sort")]
        [InlineData("Array.FindIndex")]
        [InlineData("Array.FindLastIndex")]
        [InlineData("Array.BinarySearch")]
        public void WithoutCollection_Fails(string node)
        {
            var harness = new NodeHarness(node).Invoke();

            Assert.Equal(["Failed"], harness.Fired);
        }

        [Fact]
        public void ArrayList_AddInsertAndRemove()
        {
            var list = List("a");

            Ok("ArrayList.Add", ("ArrayList", list), ("Value", "b"));
            Ok("ArrayList.AddRange", ("ArrayList", list), ("Value", new object[] { "c", "d" }));
            Ok("ArrayList.Insert", ("ArrayList", list), ("Index", 0), ("Value", "z"));
            Ok("ArrayList.InsertRange", ("ArrayList", list), ("Index", 1), ("Value", new object[] { "x", "y" }));
            Assert.Equal(["z", "x", "y", "a", "b", "c", "d"], list);

            Ok("ArrayList.Remove", ("ArrayList", list), ("Value", "x"));
            Ok("ArrayList.RemoveAt", ("ArrayList", list), ("Value", 0));
            Ok("ArrayList.RemoveRange", ("ArrayList", list), ("Index", 1), ("Count", 2));
            Assert.Equal(["y", "c", "d"], list);

            Ok("ArrayList.Reverse", ("ArrayList", list));
            Assert.Equal(["d", "c", "y"], list);
            Ok("ArrayList.ReverseWithLimit", ("ArrayList", list), ("StartIndex", 0), ("Count", 2));
            Assert.Equal(["c", "d", "y"], list);
            Ok("ArrayList.Clear", ("ArrayList", list));
            Assert.Empty(list);
        }

        [Fact]
        public void ArrayList_SearchAndCount()
        {
            var list = List("a", "b", "a");

            Assert.Equal(3, Ok("ArrayList.Count", ("ArrayList", list)).Get("Value"));
            Assert.Equal(true, Ok("ArrayList.Contains", ("ArrayList", list), ("Object", "b")).Get("Value"));
            Assert.Equal(-1, Ok("ArrayList.IndexOf", ("ArrayList", list), ("Object", "x")).Get("Value"));
            Assert.Equal(2, Ok("ArrayList.IndexOfWithLimit", ("ArrayList", list), ("Object", "a"), ("StartIndex", 1), ("Count", 2)).Get("Value"));
            Assert.Equal("b", new NodeHarness("ArrayList.At").Set("ArrayList", list).Set("Index", 1).Get("Value"));
        }

        [Fact]
        public void ArrayList_RemoveAllWithPredicate()
        {
            var list = List(1, 2, 3, 4);
            var harness = new NodeHarness("ArrayList.RemoveAll").Set("ArrayList", list)
                .On("Predicate", h => h.Set("Result", (int)h.Get("Value") % 2 == 0));

            harness.Invoke();

            Assert.Equal([1, 3], list);
            Assert.Equal("Success", harness.Fired.Last());
            Assert.Equal(4, harness.Fired.Count(f => f == "Predicate"));
        }

        [Fact]
        public void ArrayList_SortWithCompareEvent()
        {
            var list = List(3, 1, 2);
            var harness = new NodeHarness("ArrayList.Sort").Set("ArrayList", list)
                .On("Compare", h => h.Set("Result", ((int)h.Get("X")).CompareTo((int)h.Get("Y"))));

            harness.Invoke();
            Assert.Equal([1, 2, 3], list);

            var limited = List(5, 4, 3, 2, 1);
            new NodeHarness("ArrayList.SortWithLimit").Set("ArrayList", limited).Set("StartIndex", 1).Set("Count", 3)
                .On("Compare", h => h.Set("Result", ((int)h.Get("X")).CompareTo((int)h.Get("Y")))).Invoke();
            Assert.Equal([5, 2, 3, 4, 1], limited);
        }

        [Fact]
        public void ArrayList_ValueCreatesNewList()
        {
            var harness = new NodeHarness("ArrayList.Value").Invoke();
            var first = harness.Get("Value");
            harness.Invoke();

            Assert.Empty((List<object>)first);
            Assert.NotSame(first, harness.Get("Value"));
        }

        [Fact]
        public void ArrayList_OutOfRange_IsScriptError()
        {
            var harness = new NodeHarness("ArrayList.RemoveAt").Set("ArrayList", List("a")).Set("Value", 5);

            Assert.ThrowsAny<Exception>(() => harness.Invoke());
        }

        [Fact]
        public void ArrayList_CountWithoutList_IsZero()
        {
            var harness = new NodeHarness("ArrayList.Count").Invoke();

            Assert.Equal(["Success"], harness.Fired);
            Assert.Equal(0, harness.Get("Value"));
        }

        [Fact]
        public void Array_ValueCopyAndClear()
        {
            var created = (object[])new NodeHarness("Array.Value").Set("Length", 3).Invoke().Get("Value");
            Assert.Equal(3, created.Length);

            var array = new object[] { 1, 2, 3, 4 };
            Ok("Array.SetAt", ("Array", array), ("Index", 0), ("Value", 9));
            var destination = new object[4];
            Ok("Array.Copy", ("Array", array), ("DestinationArray", destination), ("Length", 2));
            Assert.Equal(new object[] { 9, 2, null, null }, destination);
            Ok("Array.CopyWithLimit", ("Array", array), ("SourceIndex", 2), ("DestinationArray", destination), ("DestinationIndex", 2), ("Length", 2));
            Assert.Equal(new object[] { 9, 2, 3, 4 }, destination);

            Ok("Array.ClearWithLimit", ("Array", destination), ("Index", 1), ("Length", 2));
            Assert.Equal(new object[] { 9, null, null, 4 }, destination);
            Ok("Array.Clear", ("Array", destination));
            Assert.All(destination, Assert.Null);
            Assert.Equal(4, new NodeHarness("Array.Length").Set("Array", array).Get("Value"));
        }

        [Fact]
        public void Array_SearchAndReverse()
        {
            var array = new object[] { "a", "b", "a", "c" };

            Assert.Equal(0, Ok("Array.IndexOf", ("Array", array), ("Value", "a")).Get("Index"));
            Assert.Equal(2, Ok("Array.LastIndexOf", ("Array", array), ("Value", "a")).Get("Index"));
            Assert.Equal(2, Ok("Array.IndexOfWithLimit", ("Array", array), ("Value", "a"), ("StartIndex", 1), ("Count", 3)).Get("Index"));
            Assert.Equal(0, Ok("Array.LastIndexOfWithLimit", ("Array", array), ("Value", "a"), ("StartIndex", 1), ("Count", 2)).Get("Index"));

            Ok("Array.ReverseWithLimit", ("Array", array), ("Index", 1), ("Length", 2));
            Assert.Equal(new object[] { "a", "a", "b", "c" }, array);
            Ok("Array.Reverse", ("Array", array));
            Assert.Equal(new object[] { "c", "b", "a", "a" }, array);
        }

        [Fact]
        public void Array_FindWithPredicate()
        {
            var array = new object[] { 1, 4, 6, 7 };
            NodeHarness Find(string node) => new NodeHarness(node).Set("Array", array)
                .On("Predicate", h => h.Set("Result", (int)h.Get("Value") % 2 == 0)).Invoke();

            Assert.Equal(1, Find("Array.FindIndex").Get("FoundIndex"));
            Assert.Equal(2, Find("Array.FindLastIndex").Get("FoundIndex"));
            var limited = new NodeHarness("Array.FindIndexWithLimit").Set("Array", array).Set("StartIndex", 2).Set("Count", 2)
                .On("Predicate", h => h.Set("Result", (int)h.Get("Value") > 6)).Invoke();
            Assert.Equal(3, limited.Get("FoundIndex"));
            var last = new NodeHarness("Array.FindLastIndexWithLimit").Set("Array", array).Set("StartIndex", 2).Set("Count", 2)
                .On("Predicate", h => h.Set("Result", (int)h.Get("Value") < 5)).Invoke();
            Assert.Equal(1, last.Get("FoundIndex"));
        }

        [Fact]
        public void Array_SortAndBinarySearch()
        {
            static void Compare(NodeHarness h) => h.Set("Result", ((int)h.Get("X")).CompareTo((int)h.Get("Y")));
            var array = new object[] { 5, 3, 9, 1 };

            new NodeHarness("Array.Sort").Set("Array", array).On("Compare", Compare).Invoke();
            Assert.Equal(new object[] { 1, 3, 5, 9 }, array);

            var items = new object[] { "e", "c", "i", "a" };
            var keys = new object[] { 5, 3, 9, 1 };
            new NodeHarness("Array.SortWithItems").Set("Array", keys).Set("Items", items).On("Compare", Compare).Invoke();
            Assert.Equal(new object[] { "a", "c", "e", "i" }, items);

            var partial = new object[] { 4, 3, 2, 1 };
            new NodeHarness("Array.SortWithLimit").Set("Array", partial).Set("StartIndex", 1).Set("Length", 2).On("Compare", Compare).Invoke();
            Assert.Equal(new object[] { 4, 2, 3, 1 }, partial);

            var search = new NodeHarness("Array.BinarySearch").Set("Array", array)
                .On("Compare", h => h.Set("Result", ((int)h.Get("X")).CompareTo(5)));
            search.Invoke();
            Assert.Equal(2, search.Get("FoundIndex"));
        }
    }
}
