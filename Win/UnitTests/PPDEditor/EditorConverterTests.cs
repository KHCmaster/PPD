using PPDEditor.Converters;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// エディターの画面の値の変換（Converter）と、DataContext の入れ物（BindingProxy）のテストです。
    /// </summary>
    public class EditorConverterTests
    {
        static readonly CultureInfo culture = CultureInfo.InvariantCulture;

        [Fact]
        public void InverseBoolean_FlipsOnlyBooleans()
        {
            var converter = new InverseBooleanConverter();

            Assert.Equal(false, converter.Convert(true, typeof(bool), null, culture));
            Assert.Equal(true, converter.ConvertBack(false, typeof(bool), null, culture));
            Assert.Equal("text", converter.Convert("text", typeof(bool), null, culture));
            Assert.Null(converter.ConvertBack(null, typeof(bool), null, culture));
        }

        [Fact]
        public void EnumToBoolean_ChecksTheNamedValue()
        {
            var converter = new EnumToBooleanConverter();

            Assert.Equal(true, converter.Convert(DayOfWeek.Monday, typeof(bool), "Monday", culture));
            Assert.Equal(false, converter.Convert(DayOfWeek.Monday, typeof(bool), "Sunday", culture));
            Assert.Equal(false, converter.Convert(null, typeof(bool), "Monday", culture));
            // チェックされたら値にする。外れたときは変えない
            Assert.Equal(DayOfWeek.Friday, converter.ConvertBack(true, typeof(DayOfWeek), "Friday", culture));
            Assert.Same(Binding.DoNothing, converter.ConvertBack(false, typeof(DayOfWeek), "Friday", culture));
        }

        [Flags]
        enum Sample
        {
            None = 0,
            A = 1,
            B = 2,
        }

        [Fact]
        public void HasFlag_AndEnumIndex()
        {
            var hasFlag = new HasFlagConverter();
            Assert.Equal(true, hasFlag.Convert(Sample.A | Sample.B, typeof(bool), "B", culture));
            Assert.Equal(false, hasFlag.Convert(Sample.A, typeof(bool), "B", culture));
            Assert.Equal(false, hasFlag.Convert(3, typeof(bool), "B", culture));
            Assert.Throws<NotSupportedException>(() => hasFlag.ConvertBack(true, typeof(Sample), "B", culture));

            var index = EnumIndexConverter.Instance;
            Assert.Equal(2, index.Convert(Sample.B, typeof(int), null, culture));
            Assert.Equal(-1, index.Convert("B", typeof(int), null, culture));
            Assert.Equal(Sample.A, index.ConvertBack(1, typeof(Sample), null, culture));
            // 選んでいないとき（-1）は変えない
            Assert.Same(Binding.DoNothing, index.ConvertBack(-1, typeof(Sample), null, culture));
        }

        [Fact]
        public void DrawingColor_BecomesBrush()
        {
            var converter = new DrawingColorToBrushConverter();

            var brush = Assert.IsType<SolidColorBrush>(converter.Convert(System.Drawing.Color.FromArgb(128, 10, 20, 30), typeof(Brush), null, culture));

            Assert.Equal(Color.FromArgb(128, 10, 20, 30), brush.Color);
            Assert.Null(converter.Convert("red", typeof(Brush), null, culture));
            Assert.Throws<NotSupportedException>(() => converter.ConvertBack(brush, typeof(System.Drawing.Color), null, culture));
        }

        [Fact]
        public void BindingProxy_KeepsDataAndClones()
        {
            var proxy = new BindingProxy { Data = "context" };

            var clone = (BindingProxy)proxy.Clone();

            Assert.Equal("context", proxy.Data);
            Assert.Equal("context", clone.Data);
            Assert.NotSame(proxy, clone);
        }
    }
}
