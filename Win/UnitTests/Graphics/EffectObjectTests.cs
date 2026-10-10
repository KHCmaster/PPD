using Effect2D;
using PPDFramework;
using PPDFramework.Resource;
using System;
using System.IO;
using System.Numerics;
using System.Threading;
using Xunit;
using static UnitTests.Graphics.GraphicsDevice;
using DrawingColor = System.Drawing.Color;

namespace UnitTests.Graphics
{
    /// <summary>
    /// Effect2D のエフェクトを描く EffectObject のテストです。
    /// </summary>
    /// <remarks>
    /// ファイルから読むコンストラクターは、読めないとメッセージボックスを出すので使わない（エフェクトはコードで作る）。
    /// </remarks>
    [Collection(GraphicsCollection.Name)]
    public sealed class EffectObjectTests : IDisposable
    {
        readonly GraphicsDevice graphics;
        readonly string dir = Directory.CreateTempSubdirectory("PPDEffectTest").FullName;
        readonly ResourceManager resources = new();
        readonly string image;

        public EffectObjectTests(GraphicsDevice graphics)
        {
            this.graphics = graphics;
            image = Path.Combine(dir, "red.png");
            using (var bitmap = new System.Drawing.Bitmap(20, 20))
            {
                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.Clear(DrawingColor.Red);
                }
                bitmap.Save(image, System.Drawing.Imaging.ImageFormat.Png);
            }
            resources.Add(image, ImageResourceFactoryManager.Factory.Create(graphics.Device, image, false));
        }

        public void Dispose()
        {
            resources.Dispose();
            Directory.Delete(dir, true);
        }

        // 0 から frames フレームまで、(x, y) から (endX, y) へ動く
        EffectManager Effect(float x, float y, float endX, int frames, int fps = 60)
        {
            var effect = new BaseEffect { Filename = image };
            var set = new EffectStateRatioSet
            {
                StartFrame = 0,
                EndFrame = frames,
                StartState = new EffectStateStructure { X = x, Y = y, Alpha = 1, ScaleX = 1, ScaleY = 1 },
                EndState = new EffectStateStructure { X = endX, Y = y, Alpha = 1, ScaleX = 1, ScaleY = 1 },
            };
            foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
            {
                set[type] = new ConstantRatioMaker();
            }
            set[RatioType.X] = new LinearRatioMaker();
            effect.Sets.Add(0, set);
            effect.CheckFrameLength();
            var manager = new EffectManager { FPS = fps };
            manager.Effects.Add(effect);
            manager.CheckFrameLength();
            return manager;
        }

        static bool IsRed(uint[] pixels, int x, int y) => Rgb(Pixel(pixels, x, y)) == (255, 0, 0);

        [Fact]
        public void Effect_DrawsImageAtStatePosition()
        {
            using var effect = new EffectObject(graphics.Device, resources, Effect(100, 100, 100, 10)) { Position = new Vector2(50, 0) };
            effect.Play();

            var pixels = graphics.Render(effect);

            // 中央をそろえるので、(150, 100) を中心に 20x20
            Assert.True(IsRed(pixels, 150, 100));
            Assert.True(IsRed(pixels, 141, 91));
            Assert.False(IsRed(pixels, 162, 100));
            Assert.Equal(EffectObject.EffectAlignment.Center, effect.Alignment);
            Assert.Equal(EffectManager.PlayType.Once, effect.PlayType);
        }

        [Fact]
        public void TopLeftAlignment_PutsCornerAtPosition()
        {
            using var effect = new EffectObject(graphics.Device, resources, Effect(100, 100, 100, 10)) { Alignment = EffectObject.EffectAlignment.TopLeft };
            effect.Play();

            var pixels = graphics.Render(effect);

            Assert.True(IsRed(pixels, 101, 101));
            Assert.True(IsRed(pixels, 118, 118));
            Assert.False(IsRed(pixels, 95, 101));
        }

        [Fact]
        public void Effect_MovesOverTimeAndFinishes()
        {
            using var effect = new EffectObject(graphics.Device, resources, Effect(100, 100, 300, 30, 60));
            var finished = 0;
            effect.Finish += (s, e) => finished++;
            effect.Play();
            Assert.Equal(EffectManager.PlayState.Playing, effect.PlayState);
            var start = graphics.Render(effect);
            Assert.True(IsRed(start, 100, 100));

            // 30 フレームは 0.5 秒
            for (var i = 0; i < 40 && finished == 0; i++)
            {
                Thread.Sleep(20);
                graphics.Render(effect);
            }

            Assert.Equal(1, finished);
            Assert.Equal(EffectManager.PlayState.Stop, effect.PlayState);
        }

        [Fact]
        public void PauseSeekAndStop()
        {
            using var effect = new EffectObject(graphics.Device, resources, Effect(100, 100, 300, 600, 60));
            effect.Play();
            effect.Pause();
            Assert.Equal(EffectManager.PlayState.Pause, effect.PlayState);

            effect.Seek(EffectObject.SeekPosition.End);
            var end = graphics.Render(effect);
            Assert.True(IsRed(end, 300, 100));

            effect.Seek(EffectObject.SeekPosition.Start);
            Assert.True(IsRed(graphics.Render(effect), 100, 100));

            effect.Stop();
            Assert.Equal(EffectManager.PlayState.Stop, effect.PlayState);
        }

        [Fact]
        public void Loop_KeepsPlaying()
        {
            using var effect = new EffectObject(graphics.Device, resources, Effect(100, 100, 300, 3, 60)) { PlayType = EffectManager.PlayType.Loop };
            effect.Play();

            for (var i = 0; i < 10; i++)
            {
                Thread.Sleep(20);
                graphics.Render(effect);
            }

            Assert.Equal(EffectManager.PlayState.Playing, effect.PlayState);
        }
    }
}
