using System.Linq;
using System.Numerics;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// 基本のノードの残り（入力と出力のすべての口、範囲を指定する並べ替えと探索、商と余り、数の読み取り、行列の行と列）のテストです。
    /// </summary>
    public class BasicNodeTests5
    {
        [Fact]
        public void LogicAll_UsesEveryInput()
        {
            var s = new ScriptRunner();
            var seq = s.Node("Logic.Sequential");
            var all = s.Node("Logic.All");
            s.Flow(s.Entry(), "Start", seq, "In").Flow(all, "Out", s.Probe("all"), "In");
            // In1、In3、In4、In5 をつなぎ、最後の 1 つで出る
            string[] inputs = ["In1", "In3", "In4", "In5"];
            for (var i = 0; i < inputs.Length; i++)
            {
                s.Flow(seq, "Out" + i, all, inputs[i]);
            }

            Assert.Equal(["all"], s.Run().Select(r => r.Label));
        }

        [Fact]
        public void LogicAny_UsesEveryInput()
        {
            var any = new NodeHarness("Logic.Any");

            any.Invoke("In1").Invoke("In2").Invoke("In4").Invoke("In5");

            Assert.Equal(4, any.Fired.Count);
        }

        [Fact]
        public void RandomTrigger_UsesEveryOutput()
        {
            var s = new ScriptRunner();
            var seq = s.Node("Logic.Sequential");
            var trigger = s.Node("Random.Trigger");
            s.Flow(s.Entry(), "Start", seq, "In");
            for (var i = 0; i < 7; i++)
            {
                s.Flow(seq, "Out" + i, trigger, "In");
            }
            string[] outputs = ["Out1", "Out3", "Out4", "Out5", "Out6"];
            foreach (var output in outputs)
            {
                s.Flow(trigger, output, s.Probe(output), "In");
            }

            var records = s.Run();

            Assert.Equal(7, records.Count);
            Assert.All(records, r => Assert.Contains(r.Label, outputs));
        }

        [Theory]
        [InlineData(7, 2, 3, 1)]
        [InlineData(-7, 2, -3, -1)]
        [InlineData(5, 0, int.MaxValue, 0)]
        [InlineData(-5, 0, int.MinValue, 0)]
        public void DivRem_HandlesZero(int a, int b, int quotient, int remainder)
        {
            var node = new NodeHarness("Math.DivRem").Set("A", a).Set("B", b).Invoke();

            Assert.Equal((quotient, remainder), ((int)node.Get("Quotient"), (int)node.Get("Remainder")));
        }

        static void Compare(NodeHarness h) => h.Set("Result", ((int)h.Get("X")).CompareTo((int)h.Get("Y")));

        [Fact]
        public void LimitedSortAndSearch()
        {
            // 範囲の中だけ並べ替え、項目も一緒に動かす
            var keys = new object[] { 9, 3, 2, 1, 0 };
            var items = new object[] { "i", "c", "b", "a", "z" };
            var sort = new NodeHarness("Array.SortWithItemsWithLimit").Set("Array", keys).Set("Items", items).Set("StartIndex", 1).Set("Length", 3)
                .On("Compare", Compare).Invoke();
            Assert.Equal("Success", sort.Fired[^1]);
            Assert.Equal(new object[] { 9, 1, 2, 3, 0 }, keys);
            Assert.Equal(new object[] { "i", "a", "b", "c", "z" }, items);
            Assert.Equal(["Failed"], new NodeHarness("Array.SortWithItemsWithLimit").Invoke().Fired);

            // 範囲の中だけ探す
            var search = new NodeHarness("Array.BinarySearchWithLimit").Set("Array", keys).Set("Value", 3).Set("StartIndex", 1).Set("Length", 3)
                .On("Compare", Compare).Invoke();
            Assert.Equal(3, search.Get("FoundIndex"));
            Assert.Equal(["Failed"], new NodeHarness("Array.BinarySearchWithLimit").Invoke().Fired.Where(f => f == "Failed"));
        }

        [Fact]
        public void ConstrainedCopy()
        {
            var destination = new object[4];

            var copy = new NodeHarness("Array.ConstrainedCopy").Set("Array", new object[] { 1, 2, 3 }).Set("SourceIndex", 1)
                .Set("DestinationArray", destination).Set("DestinationIndex", 2).Set("Length", 2).Invoke();

            Assert.Equal(["Success"], copy.Fired);
            Assert.Equal(new object[] { null, null, 2, 3 }, destination);
            Assert.Equal(["Failed"], new NodeHarness("Array.ConstrainedCopy").Set("Array", new object[1]).Invoke().Fired);
        }

        [Fact]
        public void EnumerableConcatUnionAndEqual_WithMissingInputs()
        {
            object[] a = [1, 2], b = [2, 3];

            Assert.Equal([1, 2, 2, 3], ((System.Collections.Generic.IEnumerable<object>)new NodeHarness("Enumerable.Concat").Set("First", a).Set("Second", b).Get("Enumerable")).Cast<int>());
            Assert.Equal(b, new NodeHarness("Enumerable.Concat").Set("Second", b).Get("Enumerable"));
            Assert.Equal(a, new NodeHarness("Enumerable.Concat").Set("First", a).Get("Enumerable"));
            Assert.Empty((System.Collections.Generic.IEnumerable<object>)new NodeHarness("Enumerable.Concat").Get("Enumerable"));
        }

        [Theory]
        [InlineData("FlowScriptEngineBasic.TypeFormatters.DoubleFormatter", "1.5", 1.5d)]
        [InlineData("FlowScriptEngineBasic.TypeFormatters.FloatFormatter", "2.5", 2.5f)]
        public void NumberFormatters(string typeName, string text, object expected)
        {
            var formatter = (FlowScriptEngine.TypeFormatterBase)System.Activator.CreateInstance(typeof(FlowScriptEngineBasic.ToolTipText).Assembly.GetType(typeName, true));

            Assert.True(formatter.Format(text, out var value));
            Assert.Equal(expected, value);
            Assert.False(formatter.Format("abc", out var none));
            Assert.Null(none);
            Assert.False(string.IsNullOrEmpty(formatter.CorrentFormat));
            Assert.Equal(expected.GetType(), formatter.Type);
        }

        [Fact]
        public void MatrixRowsAndColumns()
        {
            var matrix = new Matrix4x4(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
            object Get(System.Type type, int index) => new NodeHarness(type).Set("A", matrix).Set("Index", index).Get("Value");

            var column = typeof(FlowScriptEngineSlimDX.FlowSourceObjects.Matrix.GetColumnFlowSourceObject);
            var row = typeof(FlowScriptEngineSlimDX.FlowSourceObjects.Matrix.GetRowFlowSourceObject);
            Assert.Equal(new Vector4(1, 5, 9, 13), Get(column, 0));
            Assert.Equal(new Vector4(4, 8, 12, 16), Get(column, 3));
            Assert.Equal(new Vector4(5, 6, 7, 8), Get(row, 1));
            Assert.Equal(new Vector4(13, 14, 15, 16), Get(row, 3));
            Assert.Equal(Vector4.Zero, Get(column, 4));
            Assert.Equal(Vector4.Zero, Get(row, -1));
        }
    }
}
