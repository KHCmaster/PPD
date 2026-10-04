using BezierCaliculator;
using Effect2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// エフェクト（*.etd）の読み書きと再生のテストです。
    /// </summary>
    public class Effect2DTests
    {
        static BezierControlPoint Bcp(float x, float y, PointF? first = null, PointF? third = null)
        {
            var bcp = new BezierControlPoint { Second = new PointF(x, y) };
            if (first.HasValue)
            {
                bcp.First = first.Value;
                bcp.ValidFirst = true;
            }
            if (third.HasValue)
            {
                bcp.Third = third.Value;
                bcp.ValidThird = true;
            }
            return bcp;
        }

        // 0-10 フレームで、X は線形、Alpha はベジエ、ほかは一定。位置はベジエで動かす
        static BaseEffect CreateEffect(string imagePath)
        {
            var effect = new BaseEffect { Filename = imagePath };
            var set = new EffectStateRatioSet
            {
                StartFrame = 0,
                EndFrame = 10,
                StartState = new EffectStateStructure { X = 0, Y = 0, Alpha = 1 },
                EndState = new EffectStateStructure { X = 100, Y = 50, Alpha = 0, Rotation = 90, ScaleX = 2, ScaleY = 3, BlendMode = BlendMode.Screen },
            };
            foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
            {
                set[type] = new ConstantRatioMaker();
            }
            set[RatioType.X] = new LinearRatioMaker();
            // ベジエの位置は、この比で曲線の上を進む
            set[RatioType.BezierPosition] = new LinearRatioMaker();
            set[RatioType.Alpha] = new BezierRatioMaker(Bcp(0, 128, third: new PointF(64, 128)), Bcp(128, 0, first: new PointF(64, 0)));
            set.BAnalyzer = new BezierAnalyzer([Bcp(0, 0, third: new PointF(0, 50)), Bcp(100, 50, first: new PointF(100, 0))]);
            effect.Sets.Add(0, set);
            effect.CheckFrameLength();
            return effect;
        }

        [Fact]
        public void SaveThenLoad_KeepsSetsMakersAndBezier()
        {
            using var folder = new TempFolder();
            var manager = new EffectManager { FPS = 30 };
            manager.Effects.Add(CreateEffect(folder.Combine("fx", "image.png")));
            var path = folder.Combine("fx.etd");
            manager.Save(path);
            var images = new List<string>();

            var loaded = EffectLoader.Load(path, false, images.Add);

            Assert.Equal(30, loaded.FPS);
            Assert.Equal([folder.Combine("fx", "image.png")], images);
            var effect = (BaseEffect)Assert.Single(loaded.Effects);
            var set = Assert.Single(effect.Sets.Values);
            Assert.Equal((0, 10), (set.StartFrame, set.EndFrame));
            Assert.IsType<LinearRatioMaker>(set[RatioType.X]);
            Assert.IsType<BezierRatioMaker>(set[RatioType.Alpha]);
            Assert.IsType<ConstantRatioMaker>(set[RatioType.Y]);
            Assert.True(set.IsBezierPosition);
            Assert.Equal((100f, 50f, 0f, 90f, 2f, 3f), (set.EndState.X, set.EndState.Y, set.EndState.Alpha, set.EndState.Rotation, set.EndState.ScaleX, set.EndState.ScaleY));
            Assert.Equal(BlendMode.Screen, set.EndState.BlendMode);
            Assert.Equal(10, loaded.FrameLength);
            Assert.False(EffectLoader.IsCached(path));
        }

        [Fact]
        public void EffectReference_LoadsChildEffect()
        {
            using var folder = new TempFolder();
            var child = new EffectManager { FPS = 120 };
            child.Effects.Add(CreateEffect(folder.Combine("child", "image.png")));
            child.Save(folder.Combine("child.etd"));
            var parent = new EffectManager();
            var reference = CreateEffect(folder.Combine("child.etd"));
            reference.Effects = child.Effects;
            parent.Effects.Add(reference);
            parent.Save(folder.Combine("parent.etd"));
            Assert.Equal(["child.etd"], EffectLoader.GetEffectReference(folder.Combine("parent.etd")));

            var loaded = EffectLoader.Load(folder.Combine("parent.etd"), _ => { });

            var loadedReference = (BaseEffect)Assert.Single(loaded.Effects);
            Assert.Equal(folder.Combine("child.etd"), loadedReference.Filename);
            Assert.Single(loadedReference.Effects);
            Assert.Equal(120, loadedReference.FPS);
            // キャッシュしたものは複製を返し、使う画像も知らせる
            Assert.True(EffectLoader.IsCached(folder.Combine("parent.etd")));
            var images = new List<string>();
            var cached = EffectLoader.Load(folder.Combine("parent.etd"), images.Add);
            Assert.NotSame(loaded, cached);
            Assert.Single(cached.Effects);
            Assert.Equal([folder.Combine("child", "image.png")], images);
            EffectLoader.ClearCache();
            Assert.False(EffectLoader.IsCached(folder.Combine("parent.etd")));
            Assert.Null(EffectLoader.Load(folder.Combine("missing.etd"), _ => { }));
        }

        [Fact]
        public void Seek_MixesStates()
        {
            var manager = new EffectManager();
            manager.Effects.Add(CreateEffect("image.png"));
            manager.CheckFrameLength();
            var drawn = new List<(string File, EffectStateStructure State)>();

            manager.Seek(5);
            manager.Draw((file, state) => drawn.Add((file, state)));

            var (drawnFile, state) = Assert.Single(drawn);
            Assert.Equal("image.png", drawnFile);
            // 位置はベジエで動かすので、線形の真ん中とは違う
            Assert.InRange(state.X, 1, 99);
            Assert.InRange(state.Alpha, 0.01f, 0.99f);
            // 一定の値は、終わりまで始めの値
            Assert.Equal(1, state.ScaleX);
            Assert.Equal(5, manager.CurrentFrame);
        }

        [Fact]
        public void Playback_OnceFinishesAndStops()
        {
            var manager = new EffectManager { FPS = 100000 };
            manager.Effects.Add(CreateEffect("image.png"));
            manager.CheckFrameLength();
            var finished = 0;
            manager.Finish += (sender, e) => finished++;

            manager.Update();
            Assert.Equal(EffectManager.PlayState.Stop, manager.State);
            manager.Play(EffectManager.PlayType.Once);
            Assert.Equal(EffectManager.PlayState.Playing, manager.State);
            Thread.Sleep(20);
            manager.Update();

            Assert.Equal(1, finished);
            Assert.Equal(EffectManager.PlayState.Stop, manager.State);
            Assert.Equal(10, manager.CurrentFrame);
        }

        [Fact]
        public void Playback_ReverseOnceFinishesAtStart()
        {
            var manager = new EffectManager { FPS = 100000 };
            manager.Effects.Add(CreateEffect("image.png"));
            manager.CheckFrameLength();
            var finished = 0;
            manager.Finish += (sender, e) => finished++;

            manager.Play(EffectManager.PlayType.ReverseOnce);
            Thread.Sleep(20);
            manager.Update();

            Assert.Equal(1, finished);
            Assert.Equal(0, manager.CurrentFrame);
        }

        [Theory]
        [InlineData(EffectManager.PlayType.Loop)]
        [InlineData(EffectManager.PlayType.ReverseLoop)]
        public void Playback_LoopsStayInRange(EffectManager.PlayType type)
        {
            var manager = new EffectManager { FPS = 1000 };
            manager.Effects.Add(CreateEffect("image.png"));
            manager.CheckFrameLength();

            manager.Play(type);
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(7);
                manager.Update();
                Assert.InRange(manager.CurrentFrame, 0, 10);
                Assert.Equal(EffectManager.PlayState.Playing, manager.State);
            }
            manager.Pause();
            Assert.Equal(EffectManager.PlayState.Pause, manager.State);
            manager.Stop();
            Assert.Equal((EffectManager.PlayState.Stop, 0f), (manager.State, manager.CurrentFrame));
        }

        [Fact]
        public void Clone_CopiesEffects()
        {
            var manager = new EffectManager { FPS = 24 };
            manager.Effects.Add(CreateEffect("image.png"));
            manager.CheckFrameLength();

            var clone = (EffectManager)manager.Clone();

            Assert.Equal(24, clone.FPS);
            var effect = (BaseEffect)Assert.Single(clone.Effects);
            Assert.NotSame(manager.Effects[0], effect);
            Assert.Equal("image.png", effect.Filename);
            Assert.Equal(100, effect.Sets.Values[0].EndState.X);
            Assert.NotSame(manager.Effects[0].Sets.Values[0].EndState, effect.Sets.Values[0].EndState);
        }
    }

    public class EffectStateTests
    {
        [Fact]
        public void MixedState_AndIndexer()
        {
            var start = new EffectStateStructure { X = 0, Alpha = 1, BlendMode = BlendMode.Multiply };
            var end = new EffectStateStructure { X = 10, Alpha = 0 };

            var half = start.GetMixedState(0.5f, end);
            var ratios = Enumerable.Repeat(0.5f, Effect2D.Utility.RatioTypeArray.Length).ToArray();
            ratios[(int)RatioType.X] = 1;
            ratios[(int)RatioType.Alpha] = 0;
            var mixed = start.GetMixedState(ratios, end);

            Assert.Equal((5f, 0.5f, BlendMode.Multiply), (half.X, half.Alpha, half.BlendMode));
            Assert.Equal((0f, 0f), (mixed[RatioType.X], mixed[RatioType.Alpha]));
            half[RatioType.Rotation] = 45;
            Assert.Equal(45, half.Rotation);
            Assert.Equal("{X:5 Y:0 Alpha:0.5 Rotation:45 ScaleX:1 ScaleY:1}", half.ToString());
        }

        [Fact]
        public void Compose_StacksMatricesAndAlpha()
        {
            var parent = new EffectStateStructure { Alpha = 0.5f, BlendMode = BlendMode.Screen };
            parent.Compose(new EffectStateStructure());
            var child = new EffectStateStructure { X = 10, Alpha = 0.5f };

            child.Compose(parent);
            Assert.Equal(2, child.ComposedMatrices.Length);
            Assert.Equal(0.25f, child.ComposedAlpha);
            // ブレンドがないときは親のブレンド
            Assert.Equal(BlendMode.Screen, child.ComposedBlendMode);

            var own = new EffectStateStructure { BlendMode = BlendMode.Multiply };
            own.Compose(parent);
            Assert.Equal(BlendMode.Multiply, own.ComposedBlendMode);
            Assert.Equal(10, child.ComposedMatrices[^1].M41);
        }

        [Fact]
        public void BezierRatioMaker_FollowsCurve()
        {
            var set = new EffectStateRatioSet { StartFrame = 0, EndFrame = 10 };
            var p1 = new BezierControlPoint { Second = new PointF(0, 128) };
            var p2 = new BezierControlPoint { Second = new PointF(128, 0) };
            var maker = new BezierRatioMaker(p1, p2);
            set[RatioType.X] = maker;

            Assert.Equal(1, maker.GetRatio(-1));
            Assert.Equal(0, maker.GetRatio(10));
            Assert.InRange(maker.GetRatio(5), 0.45f, 0.55f);

            var clone = (BezierRatioMaker)maker.Clone();
            Assert.NotSame(maker.Analyzer, clone.Analyzer);
            Assert.Equal(maker.Analyzer.BCPS.Length, clone.Analyzer.BCPS.Length);
        }
    }
}
