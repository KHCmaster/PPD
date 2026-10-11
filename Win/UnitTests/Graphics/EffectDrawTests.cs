using Effect2D;
using PPDFramework;
using PPDFramework.RenderMask;
using PPDFramework.Shaders;
using System.Linq;
using System.Numerics;
using Vortice.Mathematics;
using Xunit;
using static UnitTests.Graphics.GraphicsDevice;

namespace UnitTests.Graphics
{
    /// <summary>
    /// マスク、合成の方法、画面のフィルター、文字の縁取りなど、描き方を変えるものを実際に描いて確かめます。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public class EffectDrawTests(GraphicsDevice graphics)
    {
        RectangleComponent Rectangle(Color4 color, float x, float y, float width, float height)
        {
            return new RectangleComponent(graphics.Device, graphics.ResourceManager, color)
            {
                Position = new Vector2(x, y),
                RectangleWidth = width,
                RectangleHeight = height,
            };
        }

        static readonly Color4 Red = new(1, 0, 0, 1);
        static readonly Color4 White = new(1, 1, 1, 1);

        [Theory]
        [InlineData(MaskType.Include, true, false)]
        [InlineData(MaskType.Exclude, false, true)]
        public void Mask_LimitsDrawing(MaskType maskType, bool insideDrawn, bool outsideDrawn)
        {
            using var red = Rectangle(Red, 0, 0, 200, 100);
            red.Mask = Rectangle(White, 50, 0, 50, 100);
            red.MaskType = maskType;

            var pixels = graphics.Render(red);

            Assert.Equal(insideDrawn, Rgb(Pixel(pixels, 75, 50)).R > 200);
            Assert.Equal(outsideDrawn, Rgb(Pixel(pixels, 150, 50)).R > 200);
            red.Mask.Dispose();
        }

        [Fact]
        public void GaussianFilter_BlursEdges()
        {
            using var sprite = new SpriteObject(graphics.Device);
            sprite.AddChild(Rectangle(White, 100, 0, 100, 100));
            var sharp = graphics.Render(sprite);
            Assert.Equal(0, Rgb(Pixel(sharp, 97, 50)).R);

            sprite.PostScreenFilters.Add(new PPDFramework.ScreenFilters.GaussianFilter { Disperson = 5 });
            var blurred = graphics.Render(sprite);

            // 端の外側に少し色が広がり、内側は少し暗くなる
            Assert.InRange(Rgb(Pixel(blurred, 97, 50)).R, 1, 254);
            Assert.InRange(Rgb(Pixel(blurred, 101, 50)).R, 1, 254);
            Assert.Equal(255, Rgb(Pixel(blurred, 150, 50)).R);
        }

        [Fact]
        public void MosaicMask_MakesBlocks()
        {
            using var sprite = new SpriteObject(graphics.Device);
            // 1 ピクセルごとに白と黒
            for (var x = 0; x < 64; x += 2)
            {
                sprite.AddChild(Rectangle(White, x, 0, 1, 64));
            }
            var stripes = graphics.Render(sprite);
            Assert.NotEqual(Pixel(stripes, 0, 10), Pixel(stripes, 1, 10));

            // モザイクは、マスクを持つものの形の範囲で、後ろに描いたものにかかる
            var cover = Rectangle(new Color4(1, 1, 1, 0.01f), 0, 0, 64, 64);
            cover.RenderMasks.Add(new MosaicMask { Size = 8 });
            sprite.InsertChild(cover, 0);
            var mosaic = graphics.Render(sprite);

            // 8 ピクセルの中は同じ色になる
            Assert.Equal(Pixel(mosaic, 0, 10), Pixel(mosaic, 1, 10));
            Assert.Equal(Pixel(mosaic, 2, 10), Pixel(mosaic, 5, 10));
            Assert.False(new MosaicMask().Enabled);
        }

        [Theory]
        [InlineData(BlendMode.Normal, 0, 0, 255)]
        [InlineData(BlendMode.Multiply, 0, 0, 0)]
        [InlineData(BlendMode.Screen, 255, 0, 255)]
        [InlineData(BlendMode.Difference, 255, 0, 255)]
        [InlineData(BlendMode.LinearDodge, 255, 0, 255)]
        public void BlendModes_CombineWithBackground(BlendMode mode, int r, int g, int b)
        {
            using var sprite = new SpriteObject(graphics.Device);
            sprite.AddChild(Rectangle(Red, 0, 0, 100, 100));
            var blue = Rectangle(new Color4(0, 0, 1, 1), 0, 0, 100, 100);
            blue.BlendMode = mode;
            sprite.InsertChild(blue, 0);

            var (actualR, actualG, actualB) = Rgb(Pixel(graphics.Render(sprite), 50, 50));

            Assert.InRange(actualR, r - 3, r + 3);
            Assert.InRange(actualG, g - 3, g + 3);
            Assert.InRange(actualB, b - 3, b + 3);
        }

        static int Lit(uint[] pixels, int left, int top, int width, int height, System.Func<(int R, int G, int B), bool> match)
        {
            var count = 0;
            for (var x = left; x < left + width; x++)
            {
                for (var y = top; y < top + height; y++)
                {
                    if (match(Rgb(Pixel(pixels, x, y))))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        [Fact]
        public void TextBorder_IsDrawnAroundCharacters()
        {
            using var text = new TextureString(graphics.Device, "■", 30, new Color4(0, 0, 0, 1)) { Position = new Vector2(50, 50) };
            Assert.Equal(0, Lit(graphics.Render(text), 40, 40, 60, 50, c => c.R > 200));

            text.Border = new Border { Color = Red, Thickness = 3, Type = BorderType.Outside };
            var pixels = graphics.Render(text);

            Assert.True(Lit(pixels, 40, 40, 60, 50, c => c.R > 200 && c.G < 50) > 20);
        }

        [Fact]
        public void Text_MultiLineAndMaxWidth()
        {
            using var text = new TextureString(graphics.Device, "abc\ndef", 20, new Color4(1, 1, 1, 1));
            graphics.Render(text);
            Assert.Equal(20f, text.Height);

            // 複数行は、幅と高さを決めて作る
            using var multi = new TextureString(graphics.Device, "abc\ndef\nghi", 20, 300, 200, true, new Color4(1, 1, 1, 1));
            var pixels = graphics.Render(multi);
            Assert.True(multi.MultiLine);
            Assert.True(multi.Height >= 55, $"高さ: {multi.Height}");
            Assert.True(Lit(pixels, 0, 45, 60, 25, c => c != (0, 0, 0)) > 0);
            Assert.Equal(0, Lit(pixels, 0, (int)multi.Height + 5, 60, 20, c => c != (0, 0, 0)));

            using var limited = new TextureString(graphics.Device, new string('a', 40), 20, 100, new Color4(1, 1, 1, 1));
            pixels = graphics.Render(limited);
            Assert.Equal(100f, limited.MaxWidth);
            // Width は文字全体の幅のままで、描くのは MaxWidth まで
            Assert.True(limited.Width > 100);
            Assert.Equal(0, Lit(pixels, 102, 0, 50, 25, c => c != (0, 0, 0)));
        }

        [Fact]
        public void Text_ScrollsWhenAllowed()
        {
            using var text = new TextureString(graphics.Device, new string('a', 40), 20, 100, new Color4(1, 1, 1, 1)) { AllowScroll = true };

            var first = graphics.Render(text);
            for (var i = 0; i < 100; i++)
            {
                text.Update();
            }
            System.Threading.Thread.Sleep(600);
            for (var i = 0; i < 100; i++)
            {
                text.Update();
            }
            var later = graphics.Render(text);

            Assert.True(text.JustWidth > 100, $"文字全体の幅: {text.JustWidth}");
            Assert.False(first.Take(Width * 20).SequenceEqual(later.Take(Width * 20)), "スクロールしていない");
        }
    }
}
