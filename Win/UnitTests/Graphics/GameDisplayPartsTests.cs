using PPDCore;
using PPDFramework;
using System;
using System.Numerics;
using System.Reflection;
using Xunit;

namespace UnitTests.Graphics
{
    /// <summary>
    /// ゲーム中の表示の部品（点数、コンボ、ライフ、スライドの点、評価のゲージ）のテストです。
    /// </summary>
    /// <remarks>
    /// 表示の状態は外から見えないので、フィールドを読んで確かめる。
    /// </remarks>
    [Collection(GraphicsCollection.Name)]
    public sealed class GameDisplayPartsTests : IDisposable
    {
        readonly GraphicsDevice graphics;
        readonly GameResources resources;

        public GameDisplayPartsTests(GraphicsDevice graphics)
        {
            this.graphics = graphics;
            resources = new GameResources(graphics);
        }

        public void Dispose()
        {
            resources.Dispose();
        }

        static T Field<T>(object target, string name) =>
            (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);

        [Fact]
        public void Score_CountsUpOverFrames()
        {
            using var score = new Score(graphics.Device, resources.Resources) { CurrentScore = 6000 };
            Assert.Equal(6000, score.ScorePoint);

            // 1 フレームに、差の 1/60（少なくとも 99）ずつ増える
            score.Update();
            Assert.Equal(100u, Field<uint>(score, "score"));
            score.Update();
            Assert.Equal(199u, Field<uint>(score, "score"));
            for (var i = 0; i < 100; i++)
            {
                score.Update();
            }
            Assert.Equal(6000u, Field<uint>(score, "score"));
            graphics.Render(score);

            // 差が小さければ、すぐに合わせる
            score.CurrentScore = 6050;
            score.Update();
            Assert.Equal(6050u, Field<uint>(score, "score"));

            score.Retry();
            Assert.Equal(0, score.ScorePoint);
        }

        [Fact]
        public void Combo_FollowsTheMarkAndShowsEvaluation()
        {
            using var combo = new Combo(graphics.Device, resources.Resources);
            var inner = Field<GameComponent>(combo, "comboInternal");

            // マークの少し上に出す。画面の端には寄せすぎない
            combo.ChangeCombo(10, new Vector2(300, 200));
            Assert.Equal(new Vector2(300, 150), inner.Position);
            combo.ChangeCombo(11, new Vector2(10, 60));
            Assert.Equal(new Vector2(100, 50), inner.Position);
            combo.ChangeCombo(12, new Vector2(790, 300));
            Assert.Equal(720, inner.Position.X);

            combo.ChangeState(MarkEvaluateType.Cool, false);
            Assert.Equal("Cool", Field<object>(inner, "state").ToString());
            // 押し間違いは別の画像（ワーストは同じ）
            combo.ChangeState(MarkEvaluateType.Fine, true);
            Assert.Equal("MissGood", Field<object>(inner, "state").ToString());
            combo.ChangeState(MarkEvaluateType.Worst, true);
            Assert.Equal("Worst", Field<object>(inner, "state").ToString());
            graphics.Render(combo);

            // しばらくすると消える
            for (var i = 0; i < 62; i++)
            {
                combo.Update();
            }
            Assert.Equal("Nothing", Field<object>(inner, "state").ToString());
            combo.ChangeCombo(0, Vector2.Zero);
            combo.Retry();
        }

        [Theory]
        [InlineData(10, "Danger")]
        [InlineData(50, "Normal")]
        [InlineData(100, "Full")]
        public void LifeGage_StateFollowsLife(float life, string state)
        {
            resources.WriteEffect(@"img\PPD\main_game\lifegage\dangerflare.etd");
            resources.WriteEffect(@"img\PPD\main_game\lifegage\normalflare.etd");
            resources.WriteEffect(@"img\PPD\main_game\lifegage\fullflare.etd");
            using var gage = new LifeGage(graphics.Device, resources.Resources);

            gage.CurrentLife = life;

            Assert.Equal(state, Field<object>(gage, "state").ToString());
            // 音符は右へ流れて消える
            gage.CreateOnpu(3);
            for (var i = 0; i < 300; i++)
            {
                gage.Update();
            }
            graphics.Render(gage);
            gage.Retry();
            Assert.Equal("Normal", Field<object>(gage, "state").ToString());
        }

        [Fact]
        public void SlideManager_ShowsLatestPointPerMark()
        {
            resources.WriteEffect(@"img\PPD\main_game\slide\max_slide_effect.etd");
            resources.WriteEffect(@"img\PPD\main_game\slide\hatch_right.etd");
            resources.WriteEffect(@"img\PPD\main_game\slide\hatch_left.etd");
            using var slides = new SlideManager(graphics.Device, resources.Resources);
            var points = Field<SpriteObject>(slides, "pointsSprite");
            object mark = new(), other = new();

            // 同じマークの点は、新しいものだけを出す
            slides.AddSlidePoint(mark, new Vector2(100, 100), 10, true);
            slides.AddSlidePoint(mark, new Vector2(110, 100), 20, true);
            slides.AddSlidePoint(other, new Vector2(300, 100), 10, false);
            Assert.Equal(2, points.ChildrenCount);
            slides.AddMaxSlideEffect(mark, new Vector2(120, 100), 1100);
            Assert.Equal(2, points.ChildrenCount);
            for (var i = 0; i < 120; i++)
            {
                slides.Update();
            }
            graphics.Render(slides);

            slides.Retry();
            Assert.Equal(0, points.ChildrenCount);
        }

        [Fact]
        public void TemporaryEvaluate_ShowsGageOfCurrentRank()
        {
            using var evaluate = new TemporaryEvaluate(graphics.Device, resources.Resources);
            var gages = Field<PictureObject[]>(evaluate, "gages");
            Assert.False(gages[4].Hidden);

            evaluate.Change(ResultEvaluateType.Great, 0.5f);

            Assert.True(gages[4].Hidden);
            Assert.False(gages[(int)ResultEvaluateType.Great - 1].Hidden);
            Assert.Equal(400, gages[(int)ResultEvaluateType.Great - 1].Rectangle.Width);
            graphics.Render(evaluate);

            evaluate.Retry();
            Assert.False(gages[4].Hidden);
            Assert.True(gages[(int)ResultEvaluateType.Great - 1].Hidden);
        }
    }
}
