using PPDFramework;
using Vortice.Mathematics;
using Xunit;

namespace UnitTests.Graphics
{
    /// <summary>
    /// 文字列の表示（TextureString）の幅のテストです。特に、いろいろな空白の幅を確かめます。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public sealed class TextureStringTests(GraphicsDevice graphics)
    {
        static readonly Color4 White = new(1, 1, 1, 1);

        float Width(string text)
        {
            using var texture = new TextureString(graphics.Device, text, 20, White);
            return texture.JustWidth;
        }

        // 2 つの文字の間に入れた文字の幅
        float Between(string c) => Width("a" + c + "b") - Width("ab");

        [Theory]
        [InlineData(" ", "x", 1f)]
        [InlineData(" ", "x", 1f)]
        [InlineData(" ", "n", 1f)]
        [InlineData(" ", "M", 1f)]
        [InlineData(" ", "M", 1 / 3f)]
        [InlineData(" ", "M", 1 / 4f)]
        [InlineData(" ", "M", 1 / 5f)]
        [InlineData(" ", "M", 1 / 6f)]
        [InlineData(" ", "あ", 1 / 2f)]
        [InlineData("　", "あ", 1f)]
        public void Spaces_TakeTheWidthOfAReferenceCharacter(string space, string reference, float scale)
        {
            Assert.Equal(Between(reference) * scale, Between(space), 0.6f);
        }

        [Theory]
        [InlineData("​")]
        [InlineData("﻿")]
        public void ZeroWidthSpaces_HaveNoWidth(string space)
        {
            Assert.Equal(0, Between(space), 0.6f);
        }

        [Fact]
        public void WidthToIndex_AndCenteredMultiLine()
        {
            using var text = new TextureString(graphics.Device, "abc", 20, White);
            Assert.Equal(0, text.GetWidthToIndex(0));
            Assert.Equal(text.JustWidth, text.GetWidthToIndex(3), 0.01f);
            Assert.True(text.GetWidthToIndex(1) < text.GetWidthToIndex(2));

            // 複数行で中央に寄せる
            using var multi = new TextureString(graphics.Device, "abc\nde", 20, 200, 100, true, true, White);
            Assert.InRange(multi.MaxWidth, 1, 200);
            Assert.Contains(graphics.Render(multi), p => GraphicsDevice.Rgb(p) != (0, 0, 0));
        }
    }
}
