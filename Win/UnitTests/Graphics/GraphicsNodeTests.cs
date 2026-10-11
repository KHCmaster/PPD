using PPDCoreModel;
using PPDFramework;
using PPDFramework.ScreenFilters;
using PPDFramework.Shaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using UnitTests.FlowScript;
using Vortice.Mathematics;
using Xunit;
using static UnitTests.Graphics.GraphicsDevice;
using DrawingColor = System.Drawing.Color;

namespace UnitTests.Graphics
{
    /// <summary>
    /// スクリプトで絵を作って動かすノード（PPD.Graphics）を、実際のデバイスで確かめます。
    /// </summary>
    /// <remarks>
    /// 画像などはゲームと同じく、リソースのパック（Image\〜.png）から ScriptResourceManager が作る。
    /// </remarks>
    [Collection(GraphicsCollection.Name)]
    public sealed class GraphicsNodeTests : IDisposable
    {
        readonly GraphicsDevice graphics;
        readonly string dir = Directory.CreateTempSubdirectory("PPDGraphicsNodeTest").FullName;
        readonly ScriptResourceManager resources;

        public GraphicsNodeTests(GraphicsDevice graphics)
        {
            this.graphics = graphics;
            var pack = Path.Combine(dir, "resource.pak");
            // ゲームのリソースは PPDPack（V1）の形
            var files = new Dictionary<string, byte[]>
            {
                [@"Image\half.png"] = Png(100, 50, (x, y) => x < 50 ? DrawingColor.Red : DrawingColor.Blue),
                [@"Image\digits.png"] = Png(100, 10, (x, y) => DrawingColor.FromArgb(x / 10 * 25 + 5, 0, 0)),
                [@"Others\data.txt"] = "abc"u8.ToArray(),
            };
            using (var writer = new PPDPack.PackWriter(pack))
            {
                foreach (var stream in writer.Write([.. files.Keys]))
                {
                    stream.Write(files[stream.Filename], 0, files[stream.Filename].Length);
                }
            }
            // スプライトの置き場所は、テスト用のフォルダーの中（絶対パスにすると sprites の下に作らない）
            resources = new ScriptResourceManager(graphics.Device, null, pack, Path.Combine(dir, "sprites"));
        }

        static byte[] Png(int width, int height, Func<int, int, DrawingColor> color)
        {
            using var bitmap = new System.Drawing.Bitmap(width, height);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    bitmap.SetPixel(x, y, color(x, y));
                }
            }
            using var memory = new MemoryStream();
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            return memory.ToArray();
        }

        public void Dispose()
        {
            resources.Dispose();
            Directory.Delete(dir, true);
            // スプライトのマネージャーはいつも作業フォルダーに sprites を作るので、空なら消す
            if (Directory.Exists(PPDFramework.Sprites.SpriteManagerBase.RootDir) && !Directory.EnumerateFileSystemEntries(PPDFramework.Sprites.SpriteManagerBase.RootDir).Any())
            {
                Directory.Delete(PPDFramework.Sprites.SpriteManagerBase.RootDir);
            }
        }

        NodeHarness Node(string name, params (string Name, object Value)[] inputs)
        {
            var type = typeof(FlowScriptEnginePPD.ToolTipText).Assembly.GetType($"FlowScriptEnginePPD.FlowSourceObjects.Graphics.{name}FlowSourceObject", true);
            var harness = new NodeHarness(type);
            harness.Items["ResourceManager"] = resources;
            harness.Initialize();
            foreach (var (n, v) in inputs)
            {
                harness.Set(n, v);
            }
            return harness;
        }

        T Create<T>(string name, params (string Name, object Value)[] inputs)
        {
            var harness = Node(name, inputs).Invoke();
            Assert.Equal(["Success"], harness.Fired);
            return (T)harness.Get(harness.Node.GetType().GetProperty("Object") != null ? "Object" : "Filter");
        }

        [Fact]
        public void RectangleNodes()
        {
            using var rectangle = Create<RectangleComponent>("Rectangle.Value", ("Width", 100f), ("Height", 50f), ("Color", new Color4(1, 0, 0, 1)));
            Assert.Equal((255, 0, 0), Rgb(Pixel(graphics.Render(rectangle), 90, 40)));

            Assert.Equal(["Success"], Node("Rectangle.SetWidth", ("Object", rectangle), ("Width", 200f)).Invoke().Fired);
            Assert.Equal(["Success"], Node("Rectangle.SetHeight", ("Object", rectangle), ("Height", 80f)).Invoke().Fired);
            Assert.Equal(["Success"], Node("Rectangle.SetColor", ("Object", rectangle), ("Color", new Color4(0, 0, 1, 1))).Invoke().Fired);

            Assert.Equal((0, 0, 255), Rgb(Pixel(graphics.Render(rectangle), 190, 70)));
            foreach (var name in new[] { "Rectangle.SetWidth", "Rectangle.SetHeight", "Rectangle.SetColor" })
            {
                Assert.Equal(["Failed"], Node(name).Invoke().Fired);
            }
        }

        [Fact]
        public void ImageNodes_UseResourcePack()
        {
            using var image = Create<PictureObject>("Image.Value", ("Path", "half.png"), ("Position", new Vector2(10, 10)));
            var pixels = graphics.Render(image);
            Assert.Equal((255, 0, 0), Rgb(Pixel(pixels, 30, 30)));
            Assert.Equal((0, 0, 255), Rgb(Pixel(pixels, 90, 30)));

            using var centered = Create<PictureObject>("Image.Value", ("Path", "half.png"), ("Position", new Vector2(200, 200)), ("IsCenter", true));
            Assert.Equal((255, 0, 0), Rgb(Pixel(graphics.Render(centered), 170, 200)));

            Assert.Equal(["Success"], Node("Image.SetRect", ("Object", image), ("X", 50f), ("Width", 50f), ("Height", 50f)).Invoke().Fired);
            Assert.Equal((0, 0, 255), Rgb(Pixel(graphics.Render(image), 30, 30)));
            Assert.Equal(["Failed"], Node("Image.SetRect").Invoke().Fired);
        }

        [Fact]
        public void NumberNodes()
        {
            using var number = Create<NumberPictureObject>("Number.Value", ("Path", "digits.png"), ("Position", new Vector2(100, 100)), ("MaxDigit", -1), ("Alignment", Alignment.Left));
            Assert.Equal(["Success"], Node("Number.SetNumber", ("Object", number), ("Number", 42)).Invoke().Fired);

            var pixels = graphics.Render(number);

            Assert.Equal((105, 55), (Rgb(Pixel(pixels, 105, 105)).R, Rgb(Pixel(pixels, 115, 105)).R));
            Assert.Equal(["Failed"], Node("Number.SetNumber").Invoke().Fired);
        }

        [Fact]
        public void TextAndSpriteNodes()
        {
            using var text = Create<TextureString>("Text.Value", ("Text", "abc"), ("Height", 20), ("Color", new Color4(1, 1, 1, 1)));
            Assert.Equal(["Success"], Node("Text.SetText", ("Object", text), ("Text", "abcdef")).Invoke().Fired);
            Assert.Equal("abcdef", text.Text);
            Assert.Equal(["Failed"], Node("Text.SetText").Invoke().Fired);
            // Text がなければ空の文字
            using var empty = Create<TextureString>("Text.Value", ("Height", 20));
            Assert.Equal("", empty.Text);

            using var sprite = Create<SpriteObject>("Sprite.Value");
            Assert.NotNull(sprite);
        }

        [Theory]
        [InlineData("Rectangle.Value")]
        [InlineData("Image.Value")]
        [InlineData("Number.Value")]
        [InlineData("Text.Value")]
        [InlineData("Sprite.Value")]
        public void ValueNodes_OutsideGame_Fail(string name)
        {
            var type = typeof(FlowScriptEnginePPD.ToolTipText).Assembly.GetType($"FlowScriptEnginePPD.FlowSourceObjects.Graphics.{name}FlowSourceObject", true);

            Assert.Equal(["Failed"], new NodeHarness(type).Initialize().Invoke().Fired);
        }

        public static TheoryData<string, string, object, Func<GameComponent, object>> Setters() => new()
        {
            { "SetPosition", "Position", new Vector2(3, 4), c => c.Position },
            { "SetAlpha", "Alpha", 0.25f, c => c.Alpha },
            { "SetHidden", "Hidden", true, c => c.Hidden },
            { "SetRotation", "Rotation", 1.5f, c => c.Rotation },
            { "SetRotationCenter", "RotationCenter", new Vector2(5, 6), c => c.RotationCenter },
            { "SetScale", "Scale", new Vector2(2, 3), c => c.Scale },
            { "SetScaleCenter", "ScaleCenter", new Vector2(7, 8), c => c.ScaleCenter },
            { "SetBlend", "Blend", Effect2D.BlendMode.Screen, c => c.BlendMode },
            { "SetMaskType", "MaskType", MaskType.Exclude, c => c.MaskType },
        };

        [Theory]
        [MemberData(nameof(Setters), DisableDiscoveryEnumeration = true)]
        public void SetterNodes_ChangeComponent(string name, string input, object value, Func<GameComponent, object> read)
        {
            using var sprite = new SpriteObject(graphics.Device);

            Assert.Equal(["Success"], Node(name, ("Object", sprite), (input, value)).Invoke().Fired);

            Assert.Equal(value, read(sprite));
            Assert.Equal(["Failed"], Node(name).Invoke().Fired);
        }

        [Fact]
        public void SetMask()
        {
            using var sprite = new SpriteObject(graphics.Device);
            using var mask = new SpriteObject(graphics.Device);

            Assert.Equal(["Success"], Node("SetMask", ("Object", sprite), ("Mask", mask)).Invoke().Fired);

            Assert.Same(mask, sprite.Mask);
        }

        [Fact]
        public void ChildNodes()
        {
            using var parent = new SpriteObject(graphics.Device);
            var a = new SpriteObject(graphics.Device);
            var b = new SpriteObject(graphics.Device);
            void Ok(string name, params (string, object)[] inputs) => Assert.Equal(["Success"], Node(name, inputs).Invoke().Fired);

            Ok("AddChild", ("Parent", parent), ("Child", a));
            Ok("InsertChild", ("Parent", parent), ("Child", b), ("Index", 0));
            Assert.Equal([b, a], parent.Children);
            Assert.Equal(2, Node("ChildrenCount", ("Object", parent)).Get("Value"));
            Assert.Same(a, Node("ChildAt", ("Parent", parent), ("Index", 1)).Get("Value"));
            Assert.Equal(true, Node("ContainsChild", ("Parent", parent), ("Child", a)).Invoke().Get("Value"));

            Ok("RemoveChild", ("Parent", parent), ("Child", a));
            Assert.Equal(false, Node("ContainsChild", ("Parent", parent), ("Child", a)).Invoke().Get("Value"));
            Ok("RemoveFromParent", ("Child", b));
            Assert.Equal(0, parent.ChildrenCount);
            parent.AddChild(a);
            parent.AddChild(b);
            Ok("ClearChildren", ("Object", parent));
            Assert.Equal(0, parent.ChildrenCount);

            foreach (var name in new[] { "AddChild", "InsertChild", "RemoveChild", "RemoveFromParent", "ClearChildren" })
            {
                Assert.Equal(["Failed"], Node(name).Invoke().Fired);
            }
            Assert.Equal(0, Node("ChildrenCount").Get("Value"));
            Assert.Null(Node("ChildAt").Get("Value"));
            Assert.Equal(["Failed"], Node("ContainsChild").Invoke().Fired);
        }

        public static TheoryData<string, Type, (string Name, object Value)[]> ColorFilterNodes() => new()
        {
            { "AverageGrayScale", typeof(AverageGrayScaleColorFilter), [] },
            { "Brightness", typeof(BrightnessColorFilter), [("Scale", 0.5f)] },
            { "Color", typeof(PPDFramework.Shaders.ColorFilter), [("Color", new Color4(1, 0, 0, 1))] },
            { "GreenGrayScale", typeof(GreenGrayScaleColorFilter), [] },
            { "HDTVGrayScale", typeof(HDTVGrayScaleColorFilter), [("GammaCorrection", 2.2f)] },
            { "Hue", typeof(HueColorFilter), [("Rotation", 90f)] },
            { "Invert", typeof(InvertColorFilter), [] },
            { "MaxGrayScale", typeof(MaxGrayScaleColorFilter), [] },
            { "MedianGrayScale", typeof(MedianGrayScaleColorFilter), [] },
            { "MiddleGrayScale", typeof(MiddleGrayScaleColorFilter), [] },
            { "NTSCGrayScale", typeof(NTSCGrayScaleColorFilter), [] },
            { "Saturation", typeof(SaturationColorFilter), [("Scale", 0.5f)] },
        };

        [Theory]
        [MemberData(nameof(ColorFilterNodes), DisableDiscoveryEnumeration = true)]
        public void ColorFilterValueNodes(string name, Type type, (string Name, object Value)[] inputs)
        {
            var filter = Create<ColorFilterBase>($"ColorFilter.{name}.Value", [("Weight", 0.75f), .. inputs]);

            Assert.IsType(type, filter);
            Assert.Equal(0.75f, filter.Weight);
            foreach (var (input, value) in inputs)
            {
                Assert.Equal(value, type.GetProperty(input).GetValue(filter));
            }
            _ = filter.ToFilterInfo();
        }

        [Fact]
        public void ColorFilterSetters()
        {
            var brightness = new BrightnessColorFilter();
            var color = new PPDFramework.Shaders.ColorFilter();
            var hdtv = new HDTVGrayScaleColorFilter();
            var hue = new HueColorFilter();
            var saturation = new SaturationColorFilter();
            void Ok(string name, params (string, object)[] inputs) => Assert.Equal(["Success"], Node("ColorFilter." + name, inputs).Invoke().Fired);

            Ok("Brightness.SetScale", ("Filter", brightness), ("Scale", 0.3f));
            Ok("Color.SetColor", ("Filter", color), ("Color", new Color4(0, 1, 0, 1)));
            Ok("HDTVGrayScale.SetGammaCorrection", ("Filter", hdtv), ("GammaCorrection", 1.8f));
            Ok("Hue.SetRotation", ("Filter", hue), ("Rotation", 45f));
            Ok("Saturation.SetScale", ("Filter", saturation), ("Scale", 0.2f));
            Ok("SetWeight", ("ColorFilter", hue), ("Weight", 0.5f));

            Assert.Equal(0.3f, brightness.Scale);
            Assert.Equal(new Color4(0, 1, 0, 1), color.Color);
            Assert.Equal(1.8f, hdtv.GammaCorrection);
            Assert.Equal(45f, hue.Rotation);
            Assert.Equal(0.2f, saturation.Scale);
            Assert.Equal(0.5f, hue.Weight);
            foreach (var name in new[] { "Brightness.SetScale", "Color.SetColor", "HDTVGrayScale.SetGammaCorrection", "Hue.SetRotation", "Saturation.SetScale", "SetWeight" })
            {
                Assert.Equal(["Failed"], Node("ColorFilter." + name).Invoke().Fired);
            }
        }

        [Fact]
        public void FilterListNodes()
        {
            using var sprite = new SpriteObject(graphics.Device);
            var invert = new InvertColorFilter();
            var hue = new HueColorFilter();
            var gaussian = new PPDFramework.ScreenFilters.GaussianFilter();
            var colorScreen = new ColorScreenFilter();
            void Ok(string name, params (string, object)[] inputs) => Assert.Equal(["Success"], Node(name, [("Object", sprite), .. inputs]).Invoke().Fired);

            Ok("AddColorFilter", ("ColorFilter", invert));
            Ok("InsertColorFilter", ("ColorFilter", hue), ("Index", 0));
            Assert.Equal<ColorFilterBase>([hue, invert], sprite.ColorFilters);
            Ok("RemoveColorFilter", ("ColorFilter", hue));
            Assert.Equal<ColorFilterBase>([invert], sprite.ColorFilters);

            Ok("AddPreScreenFilter", ("ScreenFilter", gaussian));
            Ok("InsertPreScreenFilter", ("ScreenFilter", colorScreen), ("Index", 0));
            Assert.Equal<ScreenFilterBase>([colorScreen, gaussian], sprite.PreScreenFilters);
            Ok("RemovePreScreenFilter", ("ScreenFilter", gaussian));
            Ok("AddPostScreenFilter", ("ScreenFilter", gaussian));
            Ok("InsertPostScreenFilter", ("ScreenFilter", colorScreen), ("Index", 0));
            Ok("RemovePostScreenFilter", ("ScreenFilter", colorScreen));
            Assert.Equal<ScreenFilterBase>([colorScreen], sprite.PreScreenFilters);
            Assert.Equal<ScreenFilterBase>([gaussian], sprite.PostScreenFilters);

            foreach (var name in new[] { "AddColorFilter", "InsertColorFilter", "RemoveColorFilter", "AddPreScreenFilter", "InsertPreScreenFilter", "RemovePreScreenFilter", "AddPostScreenFilter", "InsertPostScreenFilter", "RemovePostScreenFilter" })
            {
                Assert.Equal(["Failed"], Node(name).Invoke().Fired);
            }
        }

        [Fact]
        public void ScreenFilterNodes()
        {
            var gaussian = Create<PPDFramework.ScreenFilters.GaussianFilter>("ScreenFilter.Gaussian.Value", ("Disperson", 3f));
            Assert.Equal(3f, gaussian.Disperson);
            Assert.Equal(["Success"], Node("ScreenFilter.Gaussian.SetDisperson", ("Filter", gaussian), ("Disperson", 5f)).Invoke().Fired);
            Assert.Equal(5f, gaussian.Disperson);
            Assert.Equal(["Failed"], Node("ScreenFilter.Gaussian.SetDisperson").Invoke().Fired);

            var color = Create<ColorScreenFilter>("ScreenFilter.Color.Value");
            var invert = new InvertColorFilter();
            var hue = new HueColorFilter();
            Assert.Equal(["Success"], Node("ScreenFilter.Color.Add", ("Filter", color), ("ColorFilter", invert)).Invoke().Fired);
            Assert.Equal(["Success"], Node("ScreenFilter.Color.Insert", ("Filter", color), ("ColorFilter", hue), ("Index", 0)).Invoke().Fired);
            Assert.Equal<ColorFilterBase>([hue, invert], color.Filters);
            Assert.Equal(["Success"], Node("ScreenFilter.Color.Remove", ("Filter", color), ("ColorFilter", hue)).Invoke().Fired);
            Assert.Equal<ColorFilterBase>([invert], color.Filters);
            foreach (var name in new[] { "Add", "Insert", "Remove" })
            {
                Assert.Equal(["Failed"], Node("ScreenFilter.Color." + name).Invoke().Fired);
            }
        }

        [Fact]
        public void ScreenFilters_ChangeDrawing()
        {
            using var sprite = new SpriteObject(graphics.Device);
            var rectangle = new RectangleComponent(graphics.Device, graphics.ResourceManager, new Color4(1, 0, 0, 1)) { RectangleWidth = 100, RectangleHeight = 100 };
            sprite.AddChild(rectangle);
            var color = new ColorScreenFilter();
            color.Filters.Add(new InvertColorFilter { Weight = 1 });
            sprite.PostScreenFilters.Add(color);

            var pixels = graphics.Render(sprite);

            // 赤を反転した色
            Assert.Equal((0, 255, 255), Rgb(Pixel(pixels, 50, 50)));
        }
    }
}
