using PPDCore;
using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.Graphics
{
    /// <summary>
    /// AC の長押し（ホールド）の点数（HoldManager）のテストです。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public sealed class HoldManagerTests : IDisposable
    {
        readonly GraphicsDevice graphics;
        readonly GameResources resources;
        readonly HoldManager hold;
        readonly List<int> gained = [];
        readonly List<(string Kind, int Score, int Bonus, MarkType[] Marks)> events = [];

        public HoldManagerTests(GraphicsDevice graphics)
        {
            this.graphics = graphics;
            resources = new GameResources(graphics);
            resources.WriteEffect(@"img\PPD\main_game\hold\holdback.etd");
            resources.WriteEffect(@"img\PPD\main_game\hold\maxback.etd");
            hold = new HoldManager(graphics.Device, resources.Resources);
            hold.ScoreGained += gained.Add;
            hold.HoldStart += h => events.Add(("start", h.CurrentHoldScore, h.MaxHoldBonus, h.HoldMarks));
            hold.HoldEnd += h => events.Add(("end", h.CurrentHoldScore, h.MaxHoldBonus, h.HoldMarks));
            hold.HoldMaxHold += h => events.Add(("max", h.CurrentHoldScore, h.MaxHoldBonus, h.HoldMarks));
        }

        public void Dispose()
        {
            hold.Dispose();
            resources.Dispose();
        }

        [Fact]
        public void Holding_GainsTenPointsPerFrame()
        {
            Assert.False(hold.SetPressing(ButtonType.Circle, true, 0));
            Assert.True(hold.IsHolding);
            Assert.Equal(("start", 0, 0), (events[0].Kind, events[0].Score, events[0].Bonus));
            Assert.Equal([MarkType.Circle], events[0].Marks);

            // 1 秒（60 フレーム）で 600 点
            hold.Update(1000);
            Assert.Equal(600, gained.Sum());
            graphics.Render(hold);

            // 押すボタンが増えると、数え直す（それぞれのボタンで点が入る）
            hold.SetPressing(ButtonType.Cross, true, 1000);
            Assert.Equal([MarkType.Cross, MarkType.Circle], events[^1].Marks);
            Assert.Equal(600, events[^1].Score);
            hold.Update(2000);
            Assert.Equal(600 + 1200, gained.Sum());
            // 同じボタンをもう一度押しても、始まりにはならない
            hold.SetPressing(ButtonType.Cross, true, 2000);
            Assert.Equal(2, events.Count(e => e.Kind == "start"));

            // 離すと終わる
            hold.SetPressing(ButtonType.Cross, false, 2500);
            Assert.False(hold.IsHolding);
            Assert.Equal("end", events[^1].Kind);
            for (var i = 0; i < 10; i++)
            {
                hold.Update(2500 + i * 16);
            }
            graphics.Render(hold);
        }

        [Fact]
        public void FiveSeconds_IsMaxHoldBonus()
        {
            hold.SetPressing(ButtonType.Circle, true, 0);
            hold.SetPressing(ButtonType.Cross, true, 0);

            // 5 秒（300 フレーム）で最大。押しているボタンの数 × 1500 点のボーナス
            hold.Update(6000);

            Assert.True(hold.IsMaxHolding);
            var max = Assert.Single(events, e => e.Kind == "max");
            Assert.Equal(3000, max.Bonus);
            Assert.Equal(3000 + 2 * 3000, gained.Sum());
            // それより長く押しても増えない
            hold.Update(7000);
            Assert.Equal(9000, gained.Sum());
            graphics.Render(hold);

            // 最大のあとに別のボタンを押すと、最大は終わり、新しく始まる
            Assert.True(hold.SetPressing(ButtonType.Square, true, 7000));
            Assert.False(hold.IsMaxHolding);
            Assert.Equal(["start", "start", "max", "end", "start"], events.Select(e => e.Kind));
            Assert.Equal([MarkType.Square], events[^1].Marks);
            for (var i = 0; i < 70; i++)
            {
                hold.Update(7000 + i * 16);
            }

            hold.Retry();
            Assert.False(hold.IsHolding || hold.IsMaxHolding);
            graphics.Render(hold);
        }

        [Fact]
        public void MaxHold_WithoutListener()
        {
            using var other = new HoldManager(graphics.Device, resources.Resources);
            other.SetPressing(ButtonType.Circle, true, 0);

            other.Update(6000);

            Assert.True(other.IsMaxHolding);
        }

        [Fact]
        public void ManyButtons_CountUpToSix()
        {
            foreach (var button in Enum.GetValues<ButtonType>().Take(8))
            {
                hold.SetPressing(button, true, 0);
            }

            hold.Update(6000);

            // ボーナスは 6 個までで数える
            Assert.Equal(6 * 1500, Assert.Single(events, e => e.Kind == "max").Bonus);
            graphics.Render(hold);
        }
    }
}
