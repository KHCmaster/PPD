using Effect2D;
using PPDCoreModel;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using UnitTests.FlowScript;
using Xunit;
using static UnitTests.Graphics.GraphicsDevice;
using DrawingColor = System.Drawing.Color;

namespace UnitTests.Graphics
{
    /// <summary>
    /// スクリプトでエフェクトを出すノード（PPD.Graphics.Effect）と EffectPool のテストです。
    /// エフェクト（.etd と画像）はゲームと同じく、リソースのパックに入れる。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public sealed class EffectNodeTests : IDisposable
    {
        readonly GraphicsDevice graphics;
        readonly string dir = Directory.CreateTempSubdirectory("PPDEffectNodeTest").FullName;
        // エフェクトは名前で全体に覚えておかれるので、毎回違う名前にする
        readonly string name = "fx" + Guid.NewGuid().ToString("N");
        readonly ScriptResourceManager resources;

        public EffectNodeTests(GraphicsDevice graphics)
        {
            this.graphics = graphics;
            var image = Path.Combine(dir, name, "red.png");
            Directory.CreateDirectory(Path.GetDirectoryName(image));
            using (var bitmap = new System.Drawing.Bitmap(20, 20))
            {
                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.Clear(DrawingColor.Red);
                }
                bitmap.Save(image, System.Drawing.Imaging.ImageFormat.Png);
            }
            // 0.2 秒、(0, 0) から (100, 0) へ動く
            var effect = new BaseEffect { Filename = image };
            var set = new EffectStateRatioSet
            {
                StartFrame = 0,
                EndFrame = 12,
                StartState = new EffectStateStructure { X = 0, Y = 0, Alpha = 1, ScaleX = 1, ScaleY = 1 },
                EndState = new EffectStateStructure { X = 100, Y = 0, Alpha = 1, ScaleX = 1, ScaleY = 1 },
            };
            foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
            {
                set[type] = new ConstantRatioMaker();
            }
            set[RatioType.X] = new LinearRatioMaker();
            effect.Sets.Add(0, set);
            effect.CheckFrameLength();
            var manager = new EffectManager { FPS = 60 };
            manager.Effects.Add(effect);
            var etd = Path.Combine(dir, name + ".etd");
            manager.Save(etd);

            var files = new Dictionary<string, byte[]>
            {
                [$@"Effect\{name}.etd"] = File.ReadAllBytes(etd),
                [$@"Effect\{name}\red.png"] = File.ReadAllBytes(image),
            };
            var pack = Path.Combine(dir, "resource.pak");
            using (var writer = new PPDPack.PackWriter(pack))
            {
                foreach (var stream in writer.Write([.. files.Keys]))
                {
                    stream.Write(files[stream.Filename], 0, files[stream.Filename].Length);
                }
            }
            resources = new ScriptResourceManager(graphics.Device, null, pack, Path.Combine(dir, "sprites"));
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

        NodeHarness Node(string node, params (string Name, object Value)[] inputs)
        {
            var type = typeof(FlowScriptEnginePPD.ToolTipText).Assembly.GetType($"FlowScriptEnginePPD.FlowSourceObjects.Graphics.Effect.{node}FlowSourceObject", true);
            var harness = new NodeHarness(type);
            harness.Items["ResourceManager"] = resources;
            harness.Initialize();
            foreach (var (n, v) in inputs)
            {
                harness.Set(n, v);
            }
            return harness;
        }

        static bool IsRed(uint[] pixels, int x, int y) => Rgb(Pixel(pixels, x, y)) == (255, 0, 0);

        [Fact]
        public void EffectNodes_CreatePlayPauseStop()
        {
            var value = Node("Value", ("Path", name + ".etd"), ("Position", new Vector2(200, 100))).Invoke();
            Assert.Equal(["Success"], value.Fired);
            using var effect = (EffectObject)value.Get("Object");
            Assert.Equal(EffectManager.PlayState.Stop, Node("Value", ("Object", effect)).Get("PlayState"));

            Assert.Equal(["Success"], Node("Play", ("Object", effect), ("PlayType", EffectManager.PlayType.Loop)).Invoke().Fired);
            Assert.Equal(EffectManager.PlayType.Loop, effect.PlayType);
            Assert.Equal(EffectManager.PlayState.Playing, Node("Value", ("Object", effect)).Get("PlayState"));
            // 位置を中心に描く
            Assert.True(IsRed(graphics.Render(effect), 200, 100));

            Assert.Equal(["Success"], Node("Pause", ("Object", effect)).Invoke().Fired);
            Assert.Equal(EffectManager.PlayState.Pause, effect.PlayState);
            Assert.Equal(["Success"], Node("Stop", ("Object", effect)).Invoke().Fired);
            Assert.Equal(EffectManager.PlayState.Stop, effect.PlayState);
        }

        [Fact]
        public void EffectNodes_FailWithoutObject()
        {
            foreach (var node in new[] { "Play", "Pause", "Stop" })
            {
                Assert.Equal(["Failed"], Node(node).Invoke().Fired);
            }
            Assert.Equal(EffectManager.PlayState.Stop, Node("Value").Get("PlayState"));
            Assert.Null(Node("Value", ("Path", "missing.etd")).Invoke().Get("Object"));
            var outside = new NodeHarness(typeof(FlowScriptEnginePPD.FlowSourceObjects.Graphics.Effect.ValueFlowSourceObject)).Initialize();
            Assert.Equal(["Failed"], outside.Invoke().Fired);
        }

        [Fact]
        public void Pool_ReusesFinishedEffects()
        {
            var poolNode = new NodeHarness(typeof(FlowScriptEnginePPD.FlowSourceObjects.Graphics.Effect.Pool.ValueFlowSourceObject));
            poolNode.Items["ResourceManager"] = resources;
            poolNode.Initialize();
            poolNode.Set("Path", name + ".etd").Set("Count", 1).Invoke();
            var pool = (EffectPool)poolNode.Get("Pool");
            Assert.Equal((1, name + ".etd"), (pool.Count, pool.FileName));

            EffectObject Use(Vector2 position)
            {
                var use = new NodeHarness(typeof(FlowScriptEnginePPD.FlowSourceObjects.Graphics.Effect.Pool.UseFlowSourceObject)).Set("Pool", pool).Set("Position", position).Invoke();
                Assert.Equal(["Success"], use.Fired);
                return (EffectObject)use.Get("Object");
            }

            var first = Use(new Vector2(10, 10));
            // 足りなければ新しく作る
            var second = Use(new Vector2(20, 20));
            Assert.NotSame(first, second);
            Assert.Equal(new Vector2(20, 20), second.Position);
            Assert.Equal(EffectManager.PlayState.Playing, second.PlayState);

            // 終わったものは使い回す（足りなくて作ったものも）
            for (var i = 0; i < 40 && (first.PlayState != EffectManager.PlayState.Stop || second.PlayState != EffectManager.PlayState.Stop); i++)
            {
                Thread.Sleep(20);
                first.Update();
                second.Update();
            }
            var reused = new[] { Use(Vector2.Zero), Use(Vector2.Zero) };
            Assert.Contains(first, reused);
            Assert.Contains(second, reused);

            var empty = new NodeHarness(typeof(FlowScriptEnginePPD.FlowSourceObjects.Graphics.Effect.Pool.UseFlowSourceObject)).Invoke();
            Assert.Equal(["Failed"], empty.Fired);
            Assert.Equal(["Failed"], new NodeHarness(typeof(FlowScriptEnginePPD.FlowSourceObjects.Graphics.Effect.Pool.ValueFlowSourceObject)).Initialize().Invoke().Fired);
        }
    }
}
