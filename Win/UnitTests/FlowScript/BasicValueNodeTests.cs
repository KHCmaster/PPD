using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// FlowScriptEngineBasic の値を返すノード（イベントのないもの）を、入力を直接設定して確かめます。
    /// </summary>
    /// <remarks>
    /// ほかのノードとつないでいない入力は、設定した値のまま使われるので、スクリプトを動かさなくても値を確かめられる。
    /// </remarks>
    public class BasicValueNodeTests
    {
        static readonly Assembly basic = typeof(FlowScriptEngineBasic.FlowSourceObjects.Misc.EntryPointFlowSourceObject).Assembly;

        /// <summary>
        /// ノードを作り、入力を設定して、出力を返します。
        /// </summary>
        /// <param name="node">FlowSourceObjects より後の名前（"Math.Pow" など）。</param>
        public static object Eval(string node, string output, params (string Name, object Value)[] inputs)
        {
            var type = FindType(node);
            var instance = (FlowSourceObjectBase)Activator.CreateInstance(type);
            foreach (var (name, value) in inputs)
            {
                type.GetProperty(name).SetValue(instance, value);
            }
            return type.GetProperty(output).GetValue(instance);
        }

        public static Type FindType(string node)
        {
            var prefix = "FlowScriptEngineBasic.FlowSourceObjects." + node;
            return basic.GetType(prefix + "FlowSourceObject") ?? basic.GetType(prefix + "FlowSouceObject") ?? basic.GetType(prefix + "FlowSoruceObject")
                ?? throw new ArgumentException("no node " + node);
        }

        public static TheoryData<string, string, object, (string, object)[]> Cases()
        {
            var data = new TheoryData<string, string, object, (string, object)[]>();
            void Add(string node, object expected, params (string, object)[] inputs) => data.Add(node, "Value", expected, inputs);
            void AddOut(string node, string output, object expected, params (string, object)[] inputs) => data.Add(node, output, expected, inputs);

            // Math
            Add("Math.Abs", 2.5, ("A", -2.5));
            Add("Math.AbsFloat", 2.5f, ("A", -2.5f));
            Add("Math.AbsInt32", 3, ("A", -3));
            Add("Math.Acos", Math.Acos(0.5), ("A", 0.5));
            Add("Math.Asin", Math.Asin(0.5), ("A", 0.5));
            Add("Math.Atan", Math.Atan(2.0), ("A", 2.0));
            Add("Math.Atan2", Math.Atan2(1.0, 2.0), ("Y", 1.0), ("X", 2.0));
            Add("Math.Ceiling", 3.0, ("A", 2.1));
            Add("Math.Floor", -3.0, ("A", -2.1));
            Add("Math.Truncate", -2.0, ("A", -2.9));
            Add("Math.Cos", Math.Cos(1.0), ("A", 1.0));
            Add("Math.Cosh", Math.Cosh(1.0), ("A", 1.0));
            Add("Math.Sin", Math.Sin(1.0), ("A", 1.0));
            Add("Math.Sinh", Math.Sinh(1.0), ("A", 1.0));
            Add("Math.Tan", Math.Tan(1.0), ("A", 1.0));
            Add("Math.Tanh", Math.Tanh(1.0), ("A", 1.0));
            Add("Math.E", Math.E);
            Add("Math.Pi", Math.PI);
            Add("Math.Exp", Math.Exp(2.0), ("A", 2.0));
            Add("Math.Log", Math.Log(10.0), ("A", 10.0));
            Add("Math.Log10", 3.0, ("A", 1000.0));
            Add("Math.Pow", 8.0, ("A", 2.0), ("B", 3.0));
            Add("Math.Sqrt", 3.0, ("A", 9.0));
            Add("Math.IEEERemainder", -1.0, ("A", 5.0), ("B", 3.0));
            Add("Math.Max", 5.0, ("A", 5.0), ("B", 3.0));
            Add("Math.Min", 3.0, ("A", 5.0), ("B", 3.0));
            Add("Math.MaxFloat", 5f, ("A", 5f), ("B", 3f));
            Add("Math.MinFloat", 3f, ("A", 5f), ("B", 3f));
            Add("Math.MaxInt32", 5, ("A", 5), ("B", 3));
            Add("Math.MinInt32", 3, ("A", 5), ("B", 3));
            Add("Math.Round", 2.35, ("A", 2.345), ("B", 2));
            Add("Math.Round", 2.0, ("A", 2.5), ("B", 0));
            Add("Math.Sign", -1, ("A", -0.1));
            Add("Math.SignFloat", 1, ("A", 0.1f));
            Add("Math.SignInt32", 0, ("A", 0));

            // Int32
            Add("Int32.Add", 7, ("A", 3), ("B", 4));
            Add("Int32.Subtract", -1, ("A", 3), ("B", 4));
            Add("Int32.Multiply", 12, ("A", 3), ("B", 4));
            Add("Int32.Devide", 2, ("A", 9), ("B", 4));
            Add("Int32.Remainder", 1, ("A", 9), ("B", 4));
            Add("Int32.Equal", true, ("A", 3), ("B", 3));
            Add("Int32.Greater", false, ("A", 3), ("B", 3));
            Add("Int32.GreaterOrEqual", true, ("A", 3), ("B", 3));
            Add("Int32.Less", true, ("A", 2), ("B", 3));
            Add("Int32.LessOrEqual", false, ("A", 4), ("B", 3));
            Add("Int32.Max", int.MaxValue);
            AddOut("Int32.Min", "Min", int.MinValue);
            Add("Int32.Value", 5, ("Value", 5));
            Add("Int32.Cast", 7, ("CastValue", 7));
            Add("Int32.IsType", true, ("Target", 1));
            Add("Int32.IsType", false, ("Target", "1"));

            // Float
            Add("Float.Add", 3.5f, ("A", 1.5f), ("B", 2f));
            Add("Float.Subtract", -0.5f, ("A", 1.5f), ("B", 2f));
            Add("Float.Multiply", 3f, ("A", 1.5f), ("B", 2f));
            Add("Float.Devide", 0.75f, ("A", 1.5f), ("B", 2f));
            Add("Float.Remainder", 1.5f, ("A", 5.5f), ("B", 2f));
            Add("Float.Equal", true, ("A", 1.5f), ("B", 1.5f));
            Add("Float.Greater", true, ("A", 2f), ("B", 1.5f));
            Add("Float.GreaterOrEqual", false, ("A", 1f), ("B", 1.5f));
            Add("Float.Less", false, ("A", 2f), ("B", 1.5f));
            Add("Float.LessOrEqual", true, ("A", 1.5f), ("B", 1.5f));
            Add("Float.IsNaN", true, ("A", float.NaN));
            Add("Float.IsInfinity", true, ("A", float.NegativeInfinity));
            Add("Float.IsNegativeInfinity", false, ("A", float.PositiveInfinity));
            Add("Float.IsPositiveInfinity", true, ("A", float.PositiveInfinity));
            Add("Float.Epsilon", float.Epsilon);
            Add("Float.Max", float.MaxValue);
            Add("Float.Min", float.MinValue);
            Add("Float.NaN", float.NaN);
            Add("Float.NegativeInfinity", float.NegativeInfinity);
            Add("Float.PositiveInfinity", float.PositiveInfinity);
            Add("Float.Value", 2.5f, ("Value", 2.5f));
            Add("Float.Cast", 2.5f, ("CastValue", 2.5f));
            Add("Float.IsTypeFlowSoruceObject".Replace("FlowSoruceObject", ""), true, ("Target", 1f));

            // Double
            Add("Double.Add", 3.5, ("A", 1.5), ("B", 2.0));
            Add("Double.Subtract", -0.5, ("A", 1.5), ("B", 2.0));
            Add("Double.Multiply", 3.0, ("A", 1.5), ("B", 2.0));
            Add("Double.Devide", 0.75, ("A", 1.5), ("B", 2.0));
            Add("Double.Remainder", 1.5, ("A", 5.5), ("B", 2.0));
            Add("Double.Equal", false, ("A", 1.5), ("B", 2.0));
            Add("Double.Greater", false, ("A", 1.5), ("B", 2.0));
            Add("Double.GreaterOrEqual", true, ("A", 2.0), ("B", 2.0));
            Add("Double.Less", true, ("A", 1.5), ("B", 2.0));
            Add("Double.LessOrEqual", true, ("A", 2.0), ("B", 2.0));
            Add("Double.IsNaN", false, ("A", 1.0));
            Add("Double.IsInfinity", true, ("A", double.PositiveInfinity));
            Add("Double.IsNegativeInfinity", true, ("A", double.NegativeInfinity));
            Add("Double.IsPositiveInfinity", false, ("A", double.NegativeInfinity));
            Add("Double.Epsilon", double.Epsilon);
            Add("Double.Max", double.MaxValue);
            Add("Double.Min", double.MinValue);
            Add("Double.NaN", double.NaN);
            Add("Double.NegativeInfinity", double.NegativeInfinity);
            Add("Double.PositiveInfinity", double.PositiveInfinity);
            Add("Double.Value", 2.5, ("Value", 2.5));
            Add("Double.IsType", true, ("Target", 2.5));

            // Bit
            Add("Bit.And", 0b1000, ("A", 0b1100), ("B", 0b1010));
            Add("Bit.Or", 0b1110, ("A", 0b1100), ("B", 0b1010));
            Add("Bit.Xor", 0b0110, ("A", 0b1100), ("B", 0b1010));
            Add("Bit.LeftShift", 40, ("A", 5), ("B", 3));
            Add("Bit.RightShift", -2, ("A", -8), ("B", 2));
            AddOut("Bit.Complement", "Result", ~5, ("Value", 5));
            AddOut("Bit.IsPowerOfTwo", "Result", true, ("Value", 64));
            AddOut("Bit.IsPowerOfTwo", "Result", false, ("Value", 65));
            AddOut("Bit.OnesCount", "Count", 3, ("Value", 0b1011));
            AddOut("Bit.ZerosCount", "Count", 29, ("Value", 0b1011));
            AddOut("Bit.LeadingZerosCount", "Count", 28, ("Value", 0b1011));
            AddOut("Bit.TrailingZerosCount", "Count", 3, ("Value", 0b1000));
            // 以前から、名前は Index だが、位置ではなくその桁だけを残した値を返す（スクリプトの結果を変えないため、そのままにする）
            AddOut("Bit.MostSignificant", "Index", 0b1000, ("Value", 0b1011));
            AddOut("Bit.LeastSignificant", "Index", 0b0010, ("Value", 0b1010));
            AddOut("Bit.NextLargestPowerOfTwo", "Result", 16, ("Value", 9));
            AddOut("Bit.PreviousLargestPowerOfTwo", "Result", 8, ("Value", 9));
            AddOut("Bit.Reverse", "Result", unchecked((int)0x80000000), ("Value", 1));
            AddOut("Bit.ReadableString", "String", "101", ("Value", 5), ("WithZeroPadding", false));
            AddOut("Bit.ReadableString", "String", "00000000000000000000000000000101", ("Value", 5), ("WithZeroPadding", true));

            // Logic, Boolean, Object
            Add("Logic.And", false, ("A", true), ("B", false));
            Add("Logic.Or", true, ("A", true), ("B", false));
            Add("Logic.Xor", true, ("A", true), ("B", false));
            Add("Logic.Not", false, ("Value", true));
            Add("Logic.IfTernary", "a", ("Condition", true), ("A", "a"), ("B", "b"));
            Add("Logic.IfTernary", "b", ("Condition", false), ("A", "a"), ("B", "b"));
            Add("Boolean.Value", true, ("Value", true));
            Add("Boolean.TrueString", "True");
            Add("Boolean.FalseString", "False");
            Add("Boolean.IsType", true, ("Target", false));
            Add("Object.Equal", true, ("A", 3), ("B", 3));
            Add("Object.Equal", false, ("A", 3), ("B", 3.0));
            Add("Object.Null", null);
            Add("Object.IsType", false, ("Target", null));

            // String
            Add("String.Add", "ab", ("A", "a"), ("B", "b"));
            Add("String.At", "c", ("A", "abc"), ("Index", 2));
            Add("String.CompareTo", -1, ("A", "a"), ("B", "b"));
            Add("String.Contains", true, ("A", "abc"), ("B", "bc"));
            Add("String.Empty", "");
            Add("String.EndsWith", true, ("A", "abc"), ("B", "bc"));
            Add("String.StartsWith", false, ("A", "abc"), ("B", "bc"));
            Add("String.Equal", true, ("A", "abc"), ("B", "abc"));
            Add("String.IndexOf", 1, ("A", "abcb"), ("B", "b"));
            Add("String.LastIndexOf", 3, ("A", "abcb"), ("B", "b"));
            Add("String.IndexOfWithLimit", 3, ("A", "abcb"), ("B", "b"), ("StartIndex", 2), ("Count", 2));
            Add("String.LastIndexOfWithLimit", 1, ("A", "abcb"), ("B", "b"), ("StartIndex", 2), ("Count", 3));
            Add("String.Insert", "aXbc", ("A", "abc"), ("B", "X"), ("Index", 1));
            Add("String.IsNullOrEmpty", true, ("A", ""));
            Add("String.IsNullOrEmpty", true, ("A", null));
            AddOut("String.Length", "Length", 3, ("A", "abc"));
            Add("String.NewLine", Environment.NewLine);
            Add("String.PadLeft", "  ab", ("A", "ab"), ("TotalWidth", 4));
            Add("String.PadRight", "ab  ", ("A", "ab"), ("TotalWidth", 4));
            Add("String.Remove", "a", ("A", "abc"), ("Index", 1));
            Add("String.RemoveCount", "ac", ("A", "abc"), ("Index", 1), ("Count", 1));
            Add("String.Replace", "aXc", ("A", "abc"), ("Old", "b"), ("New", "X"));
            Add("String.Substring", "bc", ("A", "abc"), ("Index", 1));
            Add("String.Substring", null, ("A", null), ("Index", 1));
            Add("String.ToLower", "abc", ("A", "AbC"));
            Add("String.ToUpper", "ABC", ("A", "AbC"));
            Add("String.Trim", "a b", ("A", " a b "));
            Add("String.TrimStart", "a b ", ("A", " a b "));
            Add("String.TrimEnd", " a b", ("A", " a b "));
            Add("String.Value", "x", ("Value", "x"));
            Add("String.IsType", true, ("Target", "x"));
            Add("String.Format", "1-b", ("A", "{0}-{1}"), ("Args", new object[] { 1, "b" }));

            // TimeSpan, DateTime
            var t1 = TimeSpan.FromMinutes(90);
            var t2 = TimeSpan.FromMinutes(30);
            Add("TimeSpan.Add", TimeSpan.FromHours(2), ("A", t1), ("B", t2));
            Add("TimeSpan.Subtract", TimeSpan.FromHours(1), ("A", t1), ("B", t2));
            AddOut("TimeSpan.Compare", "Result", 1, ("TimeSpan1", t1), ("TimeSpan2", t2));
            Add("TimeSpan.Duration", t1, ("Value", -t1));
            Add("TimeSpan.Negate", -t1, ("Value", t1));
            Add("TimeSpan.Equal", false, ("A", t1), ("B", t2));
            Add("TimeSpan.Greater", true, ("A", t1), ("B", t2));
            Add("TimeSpan.GreaterOrEqual", true, ("A", t1), ("B", t1));
            Add("TimeSpan.Less", false, ("A", t1), ("B", t2));
            Add("TimeSpan.LessOrEqual", true, ("A", t2), ("B", t1));
            Add("TimeSpan.FromDays", TimeSpan.FromDays(1.5), ("Days", 1.5));
            Add("TimeSpan.FromHours", TimeSpan.FromHours(1.5), ("Hours", 1.5));
            Add("TimeSpan.FromMinutes", TimeSpan.FromMinutes(1.5), ("Minutes", 1.5));
            Add("TimeSpan.FromSeconds", TimeSpan.FromSeconds(1.5), ("Seconds", 1.5));
            Add("TimeSpan.FromMilliseconds", TimeSpan.FromMilliseconds(1.5), ("Milliseconds", 1.5));
            Add("TimeSpan.Max", TimeSpan.MaxValue);
            Add("TimeSpan.Min", TimeSpan.MinValue);
            Add("TimeSpan.Zero", TimeSpan.Zero);
            Add("TimeSpan.ToString", "01:30:00", ("TimeSpan", t1), ("Format", "c"));
            AddOut("TimeSpan.Value", "Hours", 1, ("Value", t1));
            AddOut("TimeSpan.Value", "Minutes", 30, ("Value", t1));
            AddOut("TimeSpan.Value", "TotalMinutes", 90.0, ("Value", t1));
            var day = new DateTime(2024, 2, 28, 10, 20, 30, 400);
            Add("DateTime.Add", day.AddHours(1.5), ("DateTime", day), ("Time", t1));
            Add("DateTime.AddDays", day.AddDays(1.5), ("DateTime", day), ("Days", 1.5));
            Add("DateTime.AddHours", day.AddHours(-2), ("DateTime", day), ("Hours", -2.0));
            Add("DateTime.AddMinutes", day.AddMinutes(5), ("DateTime", day), ("Minutes", 5.0));
            Add("DateTime.AddSeconds", day.AddSeconds(5), ("DateTime", day), ("Seconds", 5.0));
            Add("DateTime.AddMilliseconds", day.AddMilliseconds(5), ("DateTime", day), ("Milliseconds", 5.0));
            Add("DateTime.AddMonths", new DateTime(2024, 3, 28, 10, 20, 30, 400), ("DateTime", day), ("Months", 1));
            Add("DateTime.AddYears", new DateTime(2025, 2, 28, 10, 20, 30, 400), ("DateTime", day), ("Years", 1));
            AddOut("DateTime.Compare", "Result", -1, ("DateTime1", day), ("DateTime2", day.AddTicks(1)));
            Add("DateTime.Equal", true, ("A", day), ("B", day));
            Add("DateTime.From", new DateTime(2024, 2, 29), ("Year", 2024), ("Month", 2), ("Day", 29));
            Add("DateTime.FromWithSecond", new DateTime(2024, 2, 29, 1, 2, 3), ("Year", 2024), ("Month", 2), ("Day", 29), ("Hour", 1), ("Minute", 2), ("Second", 3));
            Add("DateTime.FromWithMillisecond", new DateTime(2024, 2, 29, 1, 2, 3, 4), ("Year", 2024), ("Month", 2), ("Day", 29), ("Hour", 1), ("Minute", 2), ("Second", 3), ("Millisecond", 4));
            Add("DateTime.DayOfWeek.Value", DayOfWeek.Friday, ("Value", DayOfWeek.Friday));

            // Pair
            var pair = new KeyValuePair<object, object>("k", 1);
            AddOut("Pair.FromKeyValue", "Pair", pair, ("Key", "k"), ("Value", 1));
            AddOut("Pair.Value", "Key", "k", ("Pair", pair));
            AddOut("Pair.Value", "Value", 1, ("Pair", pair));
            Add("Pair.IsType", true, ("Target", pair));
            return data;
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void ValueNode(string node, string output, object expected, (string, object)[] inputs)
        {
            var actual = Eval(node, output, inputs);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void StringSplit_RemovesEmptyWhenAsked()
        {
            Assert.Equal(new object[] { "a", "", "b" }, Eval("String.Split", "Value", ("A", "a,,b"), ("Separators", new object[] { "," }), ("RemoveEmpty", false)));
            Assert.Equal(new object[] { "a", "b" }, Eval("String.Split", "Value", ("A", "a,;b"), ("Separators", new object[] { ",", ";" }), ("RemoveEmpty", true)));
        }
    }
}
