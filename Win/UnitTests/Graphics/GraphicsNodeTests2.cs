using FlowScriptEngine;
using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.Vertex;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using UnitTests.FlowScript;
using Vortice.Mathematics;
using Xunit;
using static UnitTests.Graphics.GraphicsDevice;
using DrawingColor = System.Drawing.Color;

namespace UnitTests.Graphics
{
    /// <summary>
    /// PPD.Graphics の頂点、ポリゴン、リソースのノードと、描くもののデバッガー用の書き出しのテストです。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public sealed class GraphicsNodeTests2 : IDisposable
    {
        readonly GraphicsDevice graphics;
        readonly string dir = Directory.CreateTempSubdirectory("PPDGraphicsNodeTest2").FullName;
        readonly ScriptResourceManager resources;

        public GraphicsNodeTests2(GraphicsDevice graphics)
        {
            this.graphics = graphics;
            var pack = Path.Combine(dir, "resource.pak");
            var files = new Dictionary<string, byte[]>
            {
                [@"Image\half.png"] = Png(100, 50, (x, y) => x < 50 ? DrawingColor.Red : DrawingColor.Blue),
                [@"Others\data.txt"] = "abc"u8.ToArray(),
            };
            using (var writer = new PPDPack.PackWriter(pack))
            {
                foreach (var stream in writer.Write([.. files.Keys]))
                {
                    stream.Write(files[stream.Filename], 0, files[stream.Filename].Length);
                }
            }
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
            if (Directory.Exists(PPDFramework.Sprites.SpriteManagerBase.RootDir) && !Directory.EnumerateFileSystemEntries(PPDFramework.Sprites.SpriteManagerBase.RootDir).Any())
            {
                Directory.Delete(PPDFramework.Sprites.SpriteManagerBase.RootDir);
            }
        }

        static NodeHarness Node(string name, Dictionary<string, object> items, params (string Name, object Value)[] inputs)
        {
            var type = typeof(FlowScriptEnginePPD.ToolTipText).Assembly.GetType($"FlowScriptEnginePPD.FlowSourceObjects.{name}FlowSourceObject", true);
            var harness = new NodeHarness(type);
            foreach (var (key, value) in items ?? [])
            {
                harness.Items[key] = value;
            }
            harness.Initialize();
            foreach (var (n, v) in inputs)
            {
                harness.Set(n, v);
            }
            return harness;
        }

        NodeHarness Node(string name, params (string Name, object Value)[] inputs) => Node(name, new Dictionary<string, object> { ["ResourceManager"] = resources }, inputs);

        [Fact]
        public void Vertex_FromElementAndValue()
        {
            var color = new Color4(1, 0.5f, 0.25f, 0.75f);
            var vertex = (ColoredTexturedVertex)Node("Graphics.Vertex.FromElement", ("Color", color), ("Position", new Vector3(1, 2, 3)), ("TextureCoordinates", new Vector2(0.5f, 1))).Get("Value");

            var value = Node("Graphics.Vertex.Value", ("Value", vertex));

            Assert.Equal(new Vector3(1, 2, 3), value.Get("Position"));
            Assert.Equal(new Vector2(0.5f, 1), value.Get("TextureCoordinates"));
            // 色は 0 から 1 で、作ったときの値に戻る（以前は並びが違い、0 から 255 だった）
            var read = (Color4)value.Get("Color");
            Assert.Equal(color.R, read.R, 2);
            Assert.Equal(color.G, read.G, 2);
            Assert.Equal(color.B, read.B, 2);
            Assert.Equal(color.A, read.A, 2);
        }

        [Fact]
        public void Polygon_DrawsImageOnVertices()
        {
            var buffer = Node("Graphics.VertexBuffer.Value", ("VertexCount", 4)).Invoke();
            Assert.Equal(["Success"], buffer.Fired);
            var vertices = (VertexInfo)buffer.Get("Object");
            var polygonNode = Node("Graphics.Polygon.Value", ("Path", "half.png"), ("VertexBuffer", vertices),
                ("PrimitiveType", PrimitiveType.TriangleStrip), ("PrimitiveCount", 2), ("StartIndex", 0), ("VertexCount", 4)).Invoke();
            Assert.Equal(["Success"], polygonNode.Fired);
            using var polygon = (PolygonObject)polygonNode.Get("Object");
            polygon.Position = new Vector2(10, 10);
            // 画像はまとめた大きな画像（スプライト）の一部なので、テクスチャの座標は ActualTextureCoordinates で直す
            Vector2 Actual(float u, float v) => (Vector2)Node("Graphics.Polygon.ActualTextureCoordinates", ("Object", polygon), ("TextureCoordinates", new Vector2(u, v))).Get("Value");
            IEnumerable<object> quad =
            [
                new ColoredTexturedVertex(new Vector3(0, 0, 0.5f), Actual(0, 0)),
                new ColoredTexturedVertex(new Vector3(200, 0, 0.5f), Actual(1, 0)),
                new ColoredTexturedVertex(new Vector3(0, 100, 0.5f), Actual(0, 1)),
                new ColoredTexturedVertex(new Vector3(200, 100, 0.5f), Actual(1, 1)),
            ];
            Assert.Equal(["Success"], Node("Graphics.VertexBuffer.Write", ("Object", vertices), ("Vertices", quad)).Invoke().Fired);

            var pixels = graphics.Render(polygon);

            // 画像を 2 倍に広げて描く（左半分が赤、右半分が青）
            Assert.Equal((255, 0, 0), Rgb(Pixel(pixels, 60, 60)));
            Assert.Equal((0, 0, 255), Rgb(Pixel(pixels, 160, 60)));
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 220, 60)));

            var uv = (Vector2)Node("Graphics.Polygon.ActualTextureCoordinates", ("Object", polygon), ("TextureCoordinates", new Vector2(1, 1))).Get("Value");
            Assert.True(uv.X is > 0 and <= 1 && uv.Y is > 0 and <= 1, $"{uv}");
            Assert.Equal(new Vector2(0.5f, 0.5f), Node("Graphics.Polygon.ActualTextureCoordinates", ("TextureCoordinates", new Vector2(0.5f, 0.5f))).Get("Value"));
        }

        [Fact]
        public void Polygon_SettersAndFailures()
        {
            var vertices = (VertexInfo)Node("Graphics.VertexBuffer.Value", ("VertexCount", 4)).Invoke().Get("Object");
            using var polygon = (PolygonObject)Node("Graphics.Polygon.Value", ("Path", "half.png"), ("VertexBuffer", vertices),
                ("PrimitiveType", PrimitiveType.TriangleStrip), ("PrimitiveCount", 2), ("VertexCount", 4)).Invoke().Get("Object");
            void Ok(string name, params (string, object)[] inputs) => Assert.Equal(["Success"], Node("Graphics.Polygon." + name, [("Object", polygon), .. inputs]).Invoke().Fired);

            Ok("SetPrimitiveType", ("PrimitiveType", PrimitiveType.TriangleList));
            Ok("SetPrimitiveCount", ("PrimitiveCount", 1));
            Ok("SetStartIndex", ("StartIndex", 1));
            Ok("SetVertexCount", ("VertexCount", 3));

            Assert.Equal((PrimitiveType.TriangleList, 1, 1, 3), (polygon.PrimitiveType, polygon.PrimitiveCount, polygon.StartIndex, polygon.VertexCount));
            foreach (var name in new[] { "SetPrimitiveType", "SetPrimitiveCount", "SetStartIndex", "SetVertexCount" })
            {
                Assert.Equal(["Failed"], Node("Graphics.Polygon." + name).Invoke().Fired);
            }
            Assert.Equal(["Failed"], Node("Graphics.Polygon.Value", ("Path", "half.png")).Invoke().Fired);
            Assert.Equal(["Failed"], Node("Graphics.VertexBuffer.Value", ("VertexCount", 0)).Invoke().Fired);
            Assert.Equal(["Failed"], Node("Graphics.VertexBuffer.Write").Invoke().Fired);
        }

        [Fact]
        public void Resource_ReadsOthersFromPack()
        {
            var node = Node("Resource.Value", ("Path", "data.txt")).Invoke();

            Assert.Equal(["Success"], node.Fired);
            using var reader = new StreamReader((Stream)node.Get("Value"));
            Assert.Equal("abc", reader.ReadToEnd());
            Assert.Equal(["Failed"], Node("Resource.Value", null, ("Path", "data.txt")).Invoke().Fired);
        }

        [Fact]
        public void Pause_FiresOnGameCallbacks()
        {
            var pause = Node("Pause", (Dictionary<string, object>)null);

            pause.Node.Manager.Call(PauseManager.PausedCallbackName, null);
            pause.Node.Manager.Call(PauseManager.ResumedCallbackName, null);

            Assert.Equal(["Paused", "Resumed"], pause.Fired);
        }

        [Fact]
        public void ForceEnableInput()
        {
            var input = new InputManager();
            input.Disable(MarkType.Circle, true);

            Assert.Equal(["Success"], Node("ForceEnableInput", new Dictionary<string, object> { ["InputManager"] = input }).Invoke().Fired);

            Assert.False(input.IsDisabled(MarkType.Circle));
            Assert.Equal(["Failed"], Node("ForceEnableInput", (Dictionary<string, object>)null).Invoke().Fired);
        }

        [Fact]
        public void StageObject_ReturnsLayer()
        {
            using var stage = new StageManager(graphics.Device);
            var items = new Dictionary<string, object> { ["StageManager"] = stage };

            Assert.Same(stage[LayerType.AfterMark], Node("Graphics.StageObject", items, ("Layer", LayerType.AfterMark)).Get("Value"));
            Assert.Null(Node("Graphics.StageObject", null, ("Layer", LayerType.AfterMark)).Get("Value"));

            stage[LayerType.AfterMark].AddChild(new SpriteObject(graphics.Device));
            stage.Clear();
            Assert.Equal(0, stage[LayerType.AfterMark].ChildrenCount);
        }

        static string[] Names(TypeSerializerBase serializer, object value)
        {
            var element = new XElement("Value");
            serializer.Serialize(new Serializer(), element, value);
            return [.. element.Elements("Property").Select(e => e.Attribute("Name").Value)];
        }

        [Fact]
        public void DebuggerSerializers_WriteDrawnObjects()
        {
            using var sprite = new SpriteObject(graphics.Device);
            using var rectangle = new RectangleComponent(graphics.Device, graphics.ResourceManager, new Color4(1, 0, 0, 1));
            using var text = new TextureString(graphics.Device, "a", 10, new Color4(1, 1, 1, 1));
            var vertices = (VertexInfo)Node("Graphics.VertexBuffer.Value", ("VertexCount", 4)).Invoke().Get("Object");
            using var polygon = (PolygonObject)Node("Graphics.Polygon.Value", ("Path", "half.png"), ("VertexBuffer", vertices), ("VertexCount", 4)).Invoke().Get("Object");
            string[] common = ["ChildrenCount", "Alpha", "Hidden", "Position", "Rotation", "RotationCenter", "Scale", "ScaleCenter"];

            foreach (var (serializerType, value) in new (Type, object)[]
            {
                (typeof(FlowScriptEnginePPD.TypeSerializers.GameComponentTypeSerializer), sprite),
                (typeof(FlowScriptEnginePPD.TypeSerializers.SpriteObjectTypeSerializer), sprite),
                (typeof(FlowScriptEnginePPD.TypeSerializers.RectangleComponentTypeSerializer), rectangle),
                (typeof(FlowScriptEnginePPD.TypeSerializers.TextureStringTypeSerializer), text),
                (typeof(FlowScriptEnginePPD.TypeSerializers.PolygonObjectTypeSerializer), polygon),
                (typeof(FlowScriptEnginePPD.TypeSerializers.VertexInfoSerializer), vertices),
            })
            {
                var names = Names((TypeSerializerBase)Activator.CreateInstance(serializerType), value);
                Assert.NotEmpty(names);
                if (value is GameComponent)
                {
                    Assert.Equal(common, names.Take(common.Length));
                }
            }
        }
    }
}
