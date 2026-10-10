using PPDFramework;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Vortice.Mathematics;
using Xunit;
using static UnitTests.Graphics.GraphicsDevice;
using DrawingColor = System.Drawing.Color;

namespace UnitTests.Graphics
{
    /// <summary>
    /// 画像（PictureObject、NumberPictureObject）と文字（TextureString）を実際に描いて確かめます。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public sealed class ImageComponentTests(GraphicsDevice graphics) : IDisposable
    {
        readonly string dir = Directory.CreateTempSubdirectory("PPDGraphicsTest").FullName;

        public void Dispose()
        {
            Directory.Delete(dir, true);
        }

        // 列ごとの色を決めた画像を作る
        string Image(string name, int width, int height, Func<int, int, DrawingColor> color)
        {
            var path = Path.Combine(dir, name);
            using (var bitmap = new System.Drawing.Bitmap(width, height))
            {
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        bitmap.SetPixel(x, y, color(x, y));
                    }
                }
                bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }
            return path;
        }

        // 左半分が赤、右半分が青の 100x50
        string HalfRedHalfBlue() => Image("half.png", 100, 50, (x, y) => x < 50 ? DrawingColor.Red : DrawingColor.Blue);

        [Fact]
        public void Picture_DrawsImage()
        {
            using var resources = new PPDFramework.Resource.ResourceManager();
            using var picture = new PictureObject(graphics.Device, resources, PathObject.Absolute(HalfRedHalfBlue())) { Position = new Vector2(10, 20) };

            var pixels = graphics.Render(picture);

            Assert.Equal((100f, 50f), (picture.Width, picture.Height));
            Assert.Equal((255, 0, 0), Rgb(Pixel(pixels, 30, 40)));
            Assert.Equal((0, 0, 255), Rgb(Pixel(pixels, 90, 40)));
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 120, 40)));
            Assert.True(picture.HitTest(new Vector2(99, 49)));
            Assert.False(picture.HitTest(new Vector2(101, 10)));
        }

        [Fact]
        public void Picture_Centered_IsDrawnAroundPosition()
        {
            using var resources = new PPDFramework.Resource.ResourceManager();
            using var picture = new PictureObject(graphics.Device, resources, PathObject.Absolute(HalfRedHalfBlue()), true) { Position = new Vector2(200, 200) };

            var pixels = graphics.Render(picture);

            Assert.Equal(new Vector2(-50, -25), picture.Offset);
            Assert.Equal((255, 0, 0), Rgb(Pixel(pixels, 170, 200)));
            Assert.Equal((0, 0, 255), Rgb(Pixel(pixels, 230, 200)));
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 260, 200)));
        }

        [Fact]
        public void Picture_Rectangle_DrawsPartOfImage()
        {
            using var resources = new PPDFramework.Resource.ResourceManager();
            using var picture = new PictureObject(graphics.Device, resources, PathObject.Absolute(HalfRedHalfBlue()))
            {
                // 右半分だけ
                Rectangle = new Vortice.Mathematics.Rect(50, 0, 50, 50),
            };

            var pixels = graphics.Render(picture);

            Assert.Equal((0, 0, 255), Rgb(Pixel(pixels, 25, 25)));
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 75, 25)));
        }

        [Fact]
        public void SameImage_IsSharedByResourceManager()
        {
            using var resources = new PPDFramework.Resource.ResourceManager();
            var path = PathObject.Absolute(HalfRedHalfBlue());

            using var first = new PictureObject(graphics.Device, resources, path);
            using var second = new PictureObject(graphics.Device, resources, path);

            Assert.Same(first.ImageResource, second.ImageResource);
        }

        // 数字 d の列は赤が d * 25 + 5 の、100x10 の画像（1 桁が 10 ピクセル）
        string Digits() => Image("digits.png", 100, 10, (x, y) => DrawingColor.FromArgb(x / 10 * 25 + 5, 0, 0));

        static int Red(uint[] pixels, int x, int y) => Rgb(Pixel(pixels, x, y)).R;

        [Fact]
        public void Number_DrawsEachDigit()
        {
            using var resources = new PPDFramework.Resource.ResourceManager();
            using var number = new NumberPictureObject(graphics.Device, resources, PathObject.Absolute(Digits())) { Position = new Vector2(100, 100), Value = 305, MaxDigit = -1 };

            var pixels = graphics.Render(number);

            Assert.Equal(10f, number.DigitWidth);
            Assert.Equal([80, 5, 130], new[] { 105, 115, 125 }.Select(x => Red(pixels, x, 105)));
            Assert.Equal(0, Red(pixels, 135, 105));

            // MaxDigit が 0（既定）なら何も描かない（使う側がいつも決める）
            number.MaxDigit = 0;
            Assert.Equal(0, Red(graphics.Render(number), 105, 105));
        }

        [Theory]
        [InlineData(Alignment.Left, 100)]
        [InlineData(Alignment.Center, 85)]
        [InlineData(Alignment.Right, 70)]
        public void Number_Alignment(Alignment alignment, int left)
        {
            using var resources = new PPDFramework.Resource.ResourceManager();
            using var number = new NumberPictureObject(graphics.Device, resources, PathObject.Absolute(Digits()))
            {
                Position = new Vector2(100, 100),
                Value = 999,
                MaxDigit = -1,
                Alignment = alignment,
            };

            var pixels = graphics.Render(number);

            Assert.Equal(0, Red(pixels, left - 3, 105));
            Assert.Equal(230, Red(pixels, left + 5, 105));
            Assert.Equal(230, Red(pixels, left + 25, 105));
            Assert.Equal(0, Red(pixels, left + 33, 105));
        }

        [Fact]
        public void Number_MaxDigit_PadsOrCuts()
        {
            using var resources = new PPDFramework.Resource.ResourceManager();
            using var number = new NumberPictureObject(graphics.Device, resources, PathObject.Absolute(Digits())) { Value = 7, MaxDigit = 3 };

            var pixels = graphics.Render(number);
            // 007
            Assert.Equal([5, 5, 180], new[] { 5, 15, 25 }.Select(x => Red(pixels, x, 5)));

            number.Value = 12345;
            pixels = graphics.Render(number);
            // 下の 3 桁
            Assert.Equal([80, 105, 130], new[] { 5, 15, 25 }.Select(x => Red(pixels, x, 5)));
            Assert.Equal(0, Red(pixels, 35, 5));
        }

        static int Lit(uint[] pixels, int left, int top, int width, int height)
        {
            var count = 0;
            for (var x = left; x < left + width; x++)
            {
                for (var y = top; y < top + height; y++)
                {
                    if (Rgb(Pixel(pixels, x, y)) != (0, 0, 0))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        [Fact]
        public void Text_DrawsCharacters()
        {
            using var text = new TextureString(graphics.Device, "PPD テスト", 24, new Color4(1, 1, 1, 1)) { Position = new Vector2(50, 50) };

            var pixels = graphics.Render(text);

            Assert.True(text.Width > 50, $"幅: {text.Width}");
            Assert.True(Lit(pixels, 50, 50, (int)text.Width, 30) > 50);
            // 文字の外には何もない
            Assert.Equal(0, Lit(pixels, 50 + (int)text.Width + 5, 50, 50, 30));
            Assert.Equal(0, Lit(pixels, 50, 100, 200, 30));
        }

        [Fact]
        public void Text_ChangingTextChangesWidth()
        {
            using var text = new TextureString(graphics.Device, "a", 24, new Color4(1, 1, 1, 1));
            graphics.Render(text);
            var narrow = text.Width;

            text.Text = "aaaaaaaa";
            graphics.Render(text);

            Assert.True(text.Width > narrow * 4);
            Assert.Equal("aaaaaaaa", text.Text);
            Assert.True(text.GetWidthToIndex(4) < text.Width);
        }

        [Fact]
        public void Text_ColorAndHidden()
        {
            using var text = new TextureString(graphics.Device, "■■■", 30, new Color4(0, 1, 0, 1));

            var pixels = graphics.Render(text);
            var brightest = Enumerable.Range(0, (int)text.Width).Select(x => Rgb(Pixel(pixels, x, 15))).MaxBy(c => c.G);
            Assert.True(brightest.G > 200 && brightest.R < 50 && brightest.B < 50, $"{brightest}");

            text.Hidden = true;
            Assert.Equal(0, Lit(graphics.Render(text), 0, 0, 100, 40));
        }

        [Fact]
        public void Text_Centered()
        {
            using var text = new TextureString(graphics.Device, "■■■■", 20, true, new Color4(1, 1, 1, 1)) { Position = new Vector2(400, 100) };

            var pixels = graphics.Render(text);

            // 位置を中心に左右へ広がる
            Assert.True(Lit(pixels, 400 - (int)text.Width / 2, 100, (int)text.Width / 2, 25) > 0);
            Assert.True(Lit(pixels, 400, 100, (int)text.Width / 2, 25) > 0);
            Assert.Equal(0, Lit(pixels, 400 + (int)text.Width / 2 + 5, 100, 50, 25));
        }
    }
}
