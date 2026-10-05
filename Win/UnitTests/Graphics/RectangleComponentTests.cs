using PPDFramework;
using PPDFramework.Shaders;
using System;
using System.Collections.Generic;
using System.Numerics;
using Vortice.Mathematics;
using Xunit;
using static UnitTests.Graphics.GraphicsDevice;

namespace UnitTests.Graphics
{
    /// <summary>
    /// GameComponent（位置、親子、表示、透明度、拡大、回転、色のフィルター）を、四角形を実際に描いて確かめます。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public class RectangleComponentTests(GraphicsDevice graphics)
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
        static readonly Color4 Blue = new(0, 0, 1, 1);

        [Fact]
        public void Rectangle_DrawsItsColorInItsArea()
        {
            using var rectangle = Rectangle(Red, 100, 50, 200, 100);

            var pixels = graphics.Render(rectangle);

            Assert.Equal((255, 0, 0), Rgb(Pixel(pixels, 150, 100)));
            Assert.Equal((255, 0, 0), Rgb(Pixel(pixels, 101, 51)));
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 50, 100)));
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 350, 100)));
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 150, 200)));
            Assert.Equal(200, rectangle.Width);
            Assert.Equal(100, rectangle.Height);
        }

        [Fact]
        public void Children_AreDrawnRelativeToParentAndInOrder()
        {
            using var parent = new SpriteObject(graphics.Device) { Position = new Vector2(100, 100) };
            var red = Rectangle(Red, 0, 0, 100, 100);
            var blue = Rectangle(Blue, 50, 0, 100, 100);
            parent.AddChild(red);
            parent.AddChild(blue);

            var pixels = graphics.Render(parent);

            Assert.Equal((255, 0, 0), Rgb(Pixel(pixels, 120, 150)));
            // 先に足した子が上に描かれる
            Assert.Equal((255, 0, 0), Rgb(Pixel(pixels, 175, 150)));
            Assert.Equal((0, 0, 255), Rgb(Pixel(pixels, 230, 150)));
            Assert.Equal(new Vector2(150, 100), blue.ScreenPos);

            // 0 番目に入れたものが一番上
            parent.RemoveChild(blue);
            parent.InsertChild(blue, 0);
            pixels = graphics.Render(parent);
            Assert.Equal((0, 0, 255), Rgb(Pixel(pixels, 175, 150)));
        }

        [Fact]
        public void HiddenAndAlpha()
        {
            using var rectangle = Rectangle(Red, 0, 0, 100, 100);

            rectangle.Hidden = true;
            Assert.Equal((0, 0, 0), Rgb(Pixel(graphics.Render(rectangle), 50, 50)));

            rectangle.Hidden = false;
            rectangle.Alpha = 0.5f;
            var (r, g, b) = Rgb(Pixel(graphics.Render(rectangle), 50, 50));
            Assert.InRange(r, 120, 135);
            Assert.Equal((0, 0), (g, b));

            // 親の透明度もかかる
            using var parent = new SpriteObject(graphics.Device) { Alpha = 0.5f };
            var child = Rectangle(Blue, 0, 0, 100, 100);
            child.Alpha = 0.5f;
            parent.AddChild(child);
            Assert.InRange(Rgb(Pixel(graphics.Render(parent), 50, 50)).B, 56, 72);
        }

        [Fact]
        public void ScaleAndRotation()
        {
            using var scaled = Rectangle(Red, 0, 0, 100, 50);
            scaled.Scale = new Vector2(2, 2);
            var pixels = graphics.Render(scaled);
            Assert.Equal((255, 0, 0), Rgb(Pixel(pixels, 190, 90)));
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 210, 90)));

            // 中心で 90 度回すと、横長が縦長になる
            using var rotated = Rectangle(Red, 300, 200, 200, 20);
            rotated.RotationCenter = new Vector2(100, 10);
            rotated.Rotation = MathF.PI / 2;
            pixels = graphics.Render(rotated);
            Assert.Equal((255, 0, 0), Rgb(Pixel(pixels, 400, 140)));
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 320, 210)));
        }

        [Fact]
        public void CanDrawAndCanUpdate_CanSkipComponents()
        {
            using var parent = new SpriteObject(graphics.Device);
            var red = Rectangle(Red, 0, 0, 100, 100);
            var blue = Rectangle(Blue, 100, 0, 100, 100);
            parent.AddChild(red);
            parent.AddChild(blue);
            parent.CanDrawChild += (component, context, depth, index) => index != 1;

            var pixels = graphics.Render(parent);

            Assert.Equal((255, 0, 0), Rgb(Pixel(pixels, 50, 50)));
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 150, 50)));

            red.CanDraw += (component, context, depth, index) => false;
            Assert.Equal((0, 0, 0), Rgb(Pixel(graphics.Render(parent), 50, 50)));
        }

        public static TheoryData<Func<ColorFilterBase>, int, int, int> Filters() => new()
        {
            { () => new InvertColorFilter(), 0, 255, 255 },
            { () => new BrightnessColorFilter { Scale = 0.5f }, 128, 0, 0 },
            { () => new AverageGrayScaleColorFilter(), 85, 85, 85 },
            { () => new MaxGrayScaleColorFilter(), 255, 255, 255 },
            // 色をかける
            { () => new ColorFilter { Color = new Color4(0.5f, 1, 1, 1) }, 128, 0, 0 },
        };

        [Theory]
        [MemberData(nameof(Filters))]
        public void ColorFilters_ChangeDrawnColor(Func<ColorFilterBase> filter, int r, int g, int b)
        {
            using var rectangle = Rectangle(Red, 0, 0, 100, 100);
            var colorFilter = filter();
            // Weight が 0（既定）のフィルターは何もしない
            rectangle.ColorFilters.Add(colorFilter);
            Assert.Equal((255, 0, 0), Rgb(Pixel(graphics.Render(rectangle), 50, 50)));
            colorFilter.Weight = 1;

            var actual = Rgb(Pixel(graphics.Render(rectangle), 50, 50));

            Assert.InRange(actual.R, r - 2, r + 2);
            Assert.InRange(actual.G, g - 2, g + 2);
            Assert.InRange(actual.B, b - 2, b + 2);
        }

        [Fact]
        public void ChildManagement()
        {
            using var parent = new SpriteObject(graphics.Device);
            var a = Rectangle(Red, 0, 0, 1, 1);
            var b = Rectangle(Red, 0, 0, 1, 1);
            var c = Rectangle(Red, 0, 0, 1, 1);

            parent.AddChild(a);
            parent.AddChild(b);
            parent.InsertChild(c, 1);

            Assert.Equal(3, parent.ChildrenCount);
            Assert.Equal([a, c, b], parent.Children);
            Assert.Same(c, parent[1]);
            Assert.Same(b, parent.GetChildAt(2));
            Assert.Equal(2, parent.IndexOf(b));
            Assert.Same(parent, a.Parent);
            Assert.True(parent.ContainsChild(c));

            parent.RemoveChild(c);
            Assert.False(parent.ContainsChild(c));
            Assert.Null(c.Parent);
            c.Dispose();

            parent.ClearChildren();
            Assert.Equal(0, parent.ChildrenCount);
            Assert.Null(a.Parent);
            a.Dispose();
            b.Dispose();
        }

        [Fact]
        public void ClearDisposeChildren_DisposesThem()
        {
            using var parent = new SpriteObject(graphics.Device);
            var child = Rectangle(Red, 0, 0, 1, 1);
            parent.AddChild(child);

            parent.ClearDisposeChildren();

            Assert.True(child.Disposed);
            Assert.Equal(0, parent.ChildrenCount);
        }

        [Fact]
        public void HitTest_UsesSize()
        {
            using var rectangle = Rectangle(Red, 0, 0, 100, 50);

            Assert.True(rectangle.HitTest(new Vector2(0, 0)));
            Assert.True(rectangle.HitTest(new Vector2(100, 50)));
            Assert.False(rectangle.HitTest(new Vector2(101, 10)));
            Assert.False(rectangle.HitTest(new Vector2(10, -1)));
        }

        [Fact]
        public void SetDefaultAndHideDefault()
        {
            using var rectangle = Rectangle(Red, 0, 0, 100, 50);
            rectangle.Alpha = 0.2f;
            rectangle.Hidden = true;
            rectangle.Scale = new Vector2(3, 3);

            rectangle.SetDefault();
            Assert.Equal((1f, false, Vector2.One), (rectangle.Alpha, rectangle.Hidden, rectangle.Scale));

            // HideDefault は表示を既定（見える、不透明）に戻す
            rectangle.Hidden = true;
            rectangle.Alpha = 0.3f;
            rectangle.HideDefault();
            Assert.Equal((1f, false), (rectangle.Alpha, rectangle.Hidden));
        }
    }
}
