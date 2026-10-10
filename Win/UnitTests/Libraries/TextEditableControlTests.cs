using PPDFramework;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// ゲームの中で文字を入力するのに使うコントロール（TextEditableControl）のテストです。表示はしない。
    /// </summary>
    /// <remarks>
    /// 貼り付け（Ctrl+V）とコピー（Ctrl+C）はクリップボードを使うので試さない。
    /// </remarks>
    public sealed class TextEditableControlTests : System.IDisposable
    {
        readonly TextEditableControl control = new();
        readonly List<string> log = [];

        public TextEditableControlTests()
        {
            _ = control.Handle;
            control.TextEditModeChanged += (s, e) => log.Add($"mode {control.IsTextEditMode}");
        }

        public void Dispose()
        {
            control.Dispose();
        }

        bool Key(Keys keys) => (bool)typeof(TextEditableControl).GetMethod("IsInputKey", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, [keys]);

        void Type(string text)
        {
            foreach (var c in text)
            {
                typeof(TextEditableControl).GetMethod("OnKeyPress", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, [new KeyPressEventArgs(c)]);
            }
        }

        (int Start, int End) Selection => (control.Selection.SelectionStart, control.Selection.SelectionEnd);

        [Fact]
        public void Typing_OnlyInEditMode()
        {
            Type("ab");
            Assert.Equal("", control.TextBoxText);
            Assert.False(Key(Keys.Left));

            control.IsTextEditMode = true;
            Type("ab c");

            Assert.Equal("ab c", control.TextBoxText);
            Assert.Equal(4, control.CaretIndex);
            Assert.Equal(["mode True"], log);
        }

        [Fact]
        public void EnterAndEscape_EndEditMode()
        {
            control.IsTextEditMode = true;
            Type("\r");
            Assert.False(control.IsTextEditMode);
            Assert.True(control.EnterClosed);

            control.IsTextEditMode = true;
            Assert.False(control.EnterClosed);
            Type("\u001b");
            Assert.False(control.IsTextEditMode);
            Assert.False(control.EnterClosed);
            Assert.Equal(["mode True", "mode False", "mode True", "mode False"], log);
        }

        [Fact]
        public void CaretMovesAndDeletes()
        {
            control.TextBoxText = "abcd";
            control.IsTextEditMode = true;
            Assert.Equal((4, 4), Selection);

            Assert.True(Key(Keys.Left));
            Key(Keys.Left);
            Type("X");
            Assert.Equal("abXcd", control.TextBoxText);

            Key(Keys.Back);
            Assert.Equal("abcd", control.TextBoxText);
            Key(Keys.Delete);
            Assert.Equal("abd", control.TextBoxText);
            Assert.Equal(2, control.CaretIndex);

            // 端より先には行かない
            for (var i = 0; i < 5; i++)
            {
                Key(Keys.Right);
            }
            Assert.Equal((3, 3), Selection);
            Key(Keys.Delete);
            Assert.Equal("abd", control.TextBoxText);
            for (var i = 0; i < 5; i++)
            {
                Key(Keys.Left);
            }
            Assert.Equal((0, 0), Selection);
            Key(Keys.Back);
            Assert.Equal("abd", control.TextBoxText);

            // 知らないキーは使わない
            Assert.False(Key(Keys.F1));
        }

        [Fact]
        public void ShiftSelectsAndTypingReplaces()
        {
            control.TextBoxText = "abcdef";
            control.IsTextEditMode = true;

            Key(Keys.Left | Keys.Shift);
            Key(Keys.Left | Keys.Shift);
            Assert.Equal((4, 6), Selection);
            Assert.Equal(4, control.CaretIndex);
            // 選んでいる方向と反対に動かすと、選ぶ範囲が縮む
            Key(Keys.Right | Keys.Shift);
            Assert.Equal((5, 6), Selection);

            Type("Z");
            Assert.Equal("abcdeZ", control.TextBoxText);

            Key(Keys.Left);
            Key(Keys.Right | Keys.Shift);
            Key(Keys.Right | Keys.Shift);
            Assert.Equal((5, 6), Selection);
            Key(Keys.Left | Keys.Shift);
            Assert.Equal((5, 5), Selection);

            // 選んでいるときの左右は、選ぶのをやめて端へ行く
            Key(Keys.Right | Keys.Shift);
            Key(Keys.Left);
            Assert.Equal((5, 5), Selection);
            Key(Keys.Right | Keys.Shift);
            Key(Keys.Right);
            Assert.Equal((6, 6), Selection);
        }

        [Fact]
        public void SelectAllAndDeleteSelection()
        {
            control.TextBoxText = "hello";
            control.IsTextEditMode = true;

            // Ctrl+A
            Type("\u0001");
            Assert.Equal((0, 5), Selection);
            Key(Keys.Back);
            Assert.Equal("", control.TextBoxText);

            control.TextBoxText = "hello";
            Type("\u0001");
            Key(Keys.Delete);
            Assert.Equal("", control.TextBoxText);
        }

        [Fact]
        public void SelectionAndLocationProperties()
        {
            control.TextBoxText = "abc";
            control.Selection = new TextBoxSelection(1, 3);
            control.CaretIndex = 1;
            control.TextBoxLocation = new System.Drawing.Point(10, 20);

            Assert.Equal(2, control.Selection.Length);
            Assert.Equal(1, control.CaretIndex);
            Assert.Equal(new System.Drawing.Point(10, 20), control.TextBoxLocation);
        }

        [Fact]
        public void TextBoxSelection_Equality()
        {
            var a = new TextBoxSelection(1, 3);
            var b = new TextBoxSelection(1, 3);
            var c = new TextBoxSelection(3, 1);

            Assert.True(a == b);
            Assert.False(a != b);
            Assert.True(a.Equals(b));
            Assert.False(a.Equals("x"));
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            Assert.True(a != c);
            Assert.Equal(2, a.Length);
        }
    }
}
