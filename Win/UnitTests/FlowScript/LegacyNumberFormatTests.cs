using FlowScriptEngine;
using FlowScriptEngineSlimDX.TypeConverters;
using FlowScriptEngineSlimDX.TypeFormatters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// スクリプトの数値の書式が、以前（.NET Framework）と同じかを確かめます。
    /// </summary>
    /// <remarks>
    /// NetFrameworkNumbers.txt は、Windows の PowerShell 5.1（.NET Framework 4.x）で float / double を ToString した結果。
    /// .NET Core 3.0 から書式が変わったので（1/3 が 0.3333333 から 0.33333334 になるなど）、以前と同じ文字列になるようにしている。
    /// </remarks>
    public class LegacyNumberFormatTests
    {
        static readonly CultureInfo invariant = CultureInfo.InvariantCulture;

        static IEnumerable<(string Kind, string Bits, string Text)> Expected()
        {
            return File.ReadLines(Path.Combine(AppContext.BaseDirectory, "FlowScript", "NetFrameworkNumbers.txt"))
                .Where(l => !l.StartsWith('#'))
                .Select(l => l.Split(','))
                .Select(p => (p[0], p[1], p[2]));
        }

        static float ToFloat(string bits) => BitConverter.UInt32BitsToSingle(uint.Parse(bits, NumberStyles.HexNumber));

        static double ToDouble(string bits) => BitConverter.UInt64BitsToDouble(ulong.Parse(bits, NumberStyles.HexNumber));

        [Fact]
        public void ToString_MatchesNetFramework()
        {
            var differences = new List<string>();
            var count = 0;
            foreach (var (kind, bits, text) in Expected())
            {
                var actual = kind == "f" ? LegacyNumberFormat.ToString(ToFloat(bits), invariant) : LegacyNumberFormat.ToString(ToDouble(bits), invariant);
                if (actual != text)
                {
                    differences.Add($"{kind} {bits}: .NET Framework={text} now={actual}");
                }
                count++;
            }

            Assert.True(count > 6000);
            Assert.True(differences.Count == 0, string.Join(Environment.NewLine, differences.Take(30)));
        }

        [Fact]
        public void Format_OnlyDefaultFloatFormatsAreLegacy()
        {
            var provider = LegacyNumberFormat.Create(invariant);

            Assert.Equal("0.3333333 0.333333333333333 0.33", string.Format(provider, "{0} {1} {2:F2}", 1f / 3, 1d / 3, 1f / 3));
            Assert.Equal("0.3333333|7|text|", string.Format(provider, "{0:G}|{1}|{2}|{3}", 1f / 3, 7, "text", null));
            Assert.Equal("0", LegacyNumberFormat.ToString(-0f, invariant));
            Assert.Equal("0", LegacyNumberFormat.ToString(-0d, invariant));
            Assert.Null(LegacyNumberFormat.ToString((object)null));
            Assert.Equal("5", LegacyNumberFormat.ToString((object)5));
        }

        [Fact]
        public void TryParse_TreatsMinusZeroAsZero()
        {
            Assert.True(LegacyNumberFormat.TryParse("-0", NumberStyles.Float, invariant, out float f));
            Assert.True(LegacyNumberFormat.TryParse("-0", NumberStyles.Float, invariant, out double d));
            Assert.False(float.IsNegative(f));
            Assert.False(double.IsNegative(d));
            Assert.False(LegacyNumberFormat.TryParse("x", NumberStyles.Float, invariant, out f));
            Assert.False(LegacyNumberFormat.TryParse("x", NumberStyles.Float, invariant, out d));
            Assert.True(LegacyNumberFormat.TryParse("-1.5", NumberStyles.Float, invariant, out f));
            Assert.Equal(-1.5f, f);
        }

        /// <summary>
        /// ベクトルの文字列は、SharpDX の ToString（.NET Framework の書式）と同じにします。
        /// </summary>
        [Fact]
        public void VectorToString_MatchesSharpDXOnNetFramework()
        {
            var previous = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = invariant;
            try
            {
                var floats = Expected().Where(e => e.Kind == "f").Take(400).ToList();
                for (int i = 0; i + 3 < floats.Count; i += 4)
                {
                    var (x, y, z, w) = (floats[i], floats[i + 1], floats[i + 2], floats[i + 3]);
                    var v2 = new Vector2(ToFloat(x.Bits), ToFloat(y.Bits));
                    var v3 = new Vector3(ToFloat(x.Bits), ToFloat(y.Bits), ToFloat(z.Bits));
                    var v4 = new Vector4(ToFloat(x.Bits), ToFloat(y.Bits), ToFloat(z.Bits), ToFloat(w.Bits));
                    Assert.Equal($"X:{x.Text} Y:{y.Text}", new Vector2StringTypeConverter().Convert(v2));
                    Assert.Equal($"X:{x.Text} Y:{y.Text} Z:{z.Text}", new Vector3StringTypeConverter().Convert(v3));
                    Assert.Equal($"X:{x.Text} Y:{y.Text} Z:{z.Text} W:{w.Text}", new Vector4StringTypeConverter().Convert(v4));
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previous;
            }
        }

        [Fact]
        public void VectorFormatters_ParseScriptValues()
        {
            Assert.True(new Vector2Formatter().Format("X:200 Y:-1.5", out var v2));
            Assert.Equal(new Vector2(200, -1.5f), v2);
            Assert.True(new Vector3Formatter().Format("X:1 Y:2  Z:-0", out var v3));
            Assert.Equal(new Vector3(1, 2, 0), v3);
            Assert.False(float.IsNegative(((Vector3)v3).Z));
            Assert.True(new Vector4Formatter().Format("X:1 Y:2 Z:3 W:4", out var v4));
            Assert.Equal(new Vector4(1, 2, 3, 4), v4);

            Assert.False(new Vector2Formatter().Format("1, 2", out _));
            Assert.False(new Vector2Formatter().Format("X:1. Y:-", out _));
            Assert.False(new Vector3Formatter().Format("X:1 Y:2", out _));
            Assert.False(new Vector4Formatter().Format("X:a Y:2 Z:3 W:4", out _));
            Assert.Equal("X:2.5 Y:2.2", new Vector2Formatter().CorrentFormat);
        }

        [Theory]
        // 3 桁は 1 桁を 2 つ並べたもの（以前は #FFF が暗い灰色になっていた）
        [InlineData("#FFF", 1, 1, 1, 1)]
        [InlineData("#F80", 1, 0x88 / 255f, 0, 1)]
        [InlineData("#FF8000", 1, 128 / 255f, 0, 1)]
        [InlineData("#80FF0000", 1, 0, 0, 128 / 255f)]
        [InlineData("white", 1, 1, 1, 1)]
        [InlineData("Red", 1, 0, 0, 1)]
        [InlineData("00FF00", 0, 1, 0, 1)]
        public void Color4Formatter_ParsesHtmlAndHex(string text, float r, float g, float b, float a)
        {
            Assert.True(new Color4Formatter().Format(text, out var value));
            var color = (Vortice.Mathematics.Color4)value;
            Assert.Equal((r, g, b, a), (color.R, color.G, color.B, color.A));
        }

        [Theory]
        [InlineData("#12")]
        [InlineData("#GGG")]
        [InlineData("not a color")]
        public void Color4Formatter_RejectsInvalid(string text)
        {
            Assert.False(new Color4Formatter().Format(text, out _));
        }
    }
}
