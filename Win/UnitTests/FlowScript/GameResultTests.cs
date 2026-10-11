using PPDCoreModel;
using PPDFramework;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// ゲームの結果（スコア、ライフ、コンボ、判定の数）を持つ GameResultManager と、それを使う PPD.GameResult のノードのテストです。
    /// </summary>
    public class GameResultTests
    {
        [Fact]
        public void Life_StaysInRange()
        {
            var result = new GameResultManager();
            Assert.Equal(GameResultManager.DEFAULTLIFE, result.CurrentLife);
            Assert.Equal(511f / 1023 * 100, result.CurrentLifeAsFloat, 4);

            result.CurrentLife += 5000;
            Assert.Equal(GameResultManager.MAXLIFE, result.CurrentLife);
            Assert.False(result.IfDeath);
            result.CurrentLife = -10;
            Assert.Equal(GameResultManager.MINLIFE, result.CurrentLife);
            Assert.True(result.IfDeath);
        }

        [Fact]
        public void Combo_KeepsMaxAndNeverNegative()
        {
            var result = new GameResultManager
            {
                CurrentCombo = 12
            };
            result.CurrentCombo = 3;
            Assert.Equal(12, result.MaxCombo);
            result.CurrentCombo = -1;
            Assert.Equal(0, result.CurrentCombo);
        }

        [Theory]
        [InlineData(MarkEvaluateType.Cool, false, 500)]
        [InlineData(MarkEvaluateType.Cool, true, 250)]
        [InlineData(MarkEvaluateType.Fine, false, 300)]
        [InlineData(MarkEvaluateType.Fine, true, 150)]
        [InlineData(MarkEvaluateType.Safe, false, 100)]
        [InlineData(MarkEvaluateType.Safe, true, 50)]
        [InlineData(MarkEvaluateType.Sad, false, 50)]
        [InlineData(MarkEvaluateType.Sad, true, 30)]
        [InlineData(MarkEvaluateType.Worst, false, 0)]
        public void GainScore_ByEvaluate(MarkEvaluateType type, bool missPress, int expected)
        {
            var result = new GameResultManager();

            result.GainScore(type, missPress, 1);
            Assert.Equal(expected, result.CurrentScore);
            // 倍率は基本の点にだけかける（端数は切り捨て）
            result.GainScore(type, missPress, 0.5f);
            Assert.Equal(expected + expected / 2, result.CurrentScore);
        }

        [Theory]
        [InlineData(9, 0)]
        [InlineData(10, 50)]
        [InlineData(20, 100)]
        [InlineData(30, 150)]
        [InlineData(40, 200)]
        [InlineData(50, 250)]
        [InlineData(500, 250)]
        public void GainScore_AddsComboBonus(int combo, int bonus)
        {
            var result = new GameResultManager { CurrentCombo = combo };

            result.GainScore(MarkEvaluateType.Worst, false, 1);

            Assert.Equal(bonus, result.CurrentScore);
        }

        [Fact]
        public void Bonuses_AddToScore()
        {
            var result = new GameResultManager();

            result.AddHoldBonus(30);
            result.AddSlideBonus(20);
            result.GainScore(5);

            Assert.Equal(55, result.CurrentScore);
            Assert.Equal(30, result.HoldBonus);
            Assert.Equal(20, result.SlideBonus);
        }

        [Fact]
        public void EvaluateCounts()
        {
            var result = new GameResultManager();

            result.GainEvaluate(MarkEvaluateType.Cool, 3);
            result.GainEvaluate(MarkEvaluateType.Fine, 2);
            result.GainEvaluate(MarkEvaluateType.Safe, 1);
            result.GainEvaluate(MarkEvaluateType.Sad, 4);
            result.GainEvaluate(MarkEvaluateType.Sad, -1);
            result.GainEvaluate(MarkEvaluateType.Worst, -5);

            Assert.Equal([3, 2, 1, 3, 0], result.Evalutes);
            Assert.Equal(9, result.EvaluateSum);
        }

        [Theory]
        [InlineData(0, 0, 0, ResultEvaluateType.Perfect)]
        [InlineData(97, 3, 0, ResultEvaluateType.Excellent)]
        [InlineData(95, 5, 0, ResultEvaluateType.Great)]
        [InlineData(85, 15, 0, ResultEvaluateType.Standard)]
        [InlineData(84, 0, 16, ResultEvaluateType.Cheap)]
        public void ResultEvaluateType_ByRatioOfCoolAndFine(int coolAndFine, int safe, int worst, ResultEvaluateType expected)
        {
            var result = new GameResultManager();
            result.GainEvaluate(MarkEvaluateType.Cool, coolAndFine);
            result.GainEvaluate(MarkEvaluateType.Safe, safe);
            result.GainEvaluate(MarkEvaluateType.Worst, worst);

            Assert.Equal(expected, result.ResultEvaluateType);
        }

        [Fact]
        public void Retry_ResetsEverything()
        {
            var result = new GameResultManager { CurrentCombo = 5, CurrentLife = 10, SuspendFinish = true };
            result.GainScore(100);
            result.AddHoldBonus(5);
            result.AddSlideBonus(5);
            result.GainEvaluate(MarkEvaluateType.Cool, 1);

            result.Retry(isReplaying: true);

            Assert.Equal(0, result.MaxCombo);
            Assert.Equal(0, result.CurrentCombo);
            Assert.Equal(GameResultManager.DEFAULTLIFE, result.CurrentLife);
            Assert.Equal(0, result.CurrentScore);
            Assert.Equal(0, result.EvaluateSum);
            Assert.Equal(0, result.HoldBonus);
            Assert.Equal(0, result.SlideBonus);
            Assert.False(result.SuspendFinish);
            Assert.True(result.IsRetrying);
            Assert.True(result.IsReplaying);
        }

        static NodeHarness Node(string name, GameResultManager result, params (string Name, object Value)[] inputs)
        {
            var type = typeof(FlowScriptEnginePPD.ToolTipText).Assembly.GetType($"FlowScriptEnginePPD.FlowSourceObjects.GameResult.{name}FlowSourceObject", true);
            var harness = new NodeHarness(type);
            if (result != null)
            {
                harness.Items["GameResultManager"] = result;
            }
            foreach (var (n, v) in inputs)
            {
                harness.Set(n, v);
            }
            return harness.Initialize();
        }

        [Fact]
        public void Nodes_ChangeAndReadResult()
        {
            var result = new GameResultManager();
            void Ok(string name, params (string, object)[] inputs) => Assert.Equal(["Success"], Node(name, result, inputs).Invoke().Fired);

            Ok("GainCurrentCombo", ("Gain", 7));
            Ok("GainCurrentLife", ("Gain", -11));
            Ok("GainCurrentScore", ("Gain", 1234));
            Ok("GainEvaluateCount", ("Evaluate", MarkEvaluateType.Safe), ("Gain", 2));
            Ok("SetSuspendFinish", ("Value", true));
            result.AddHoldBonus(6);
            result.AddSlideBonus(4);
            result.ExpectedTotalSlideBonus = 40;

            var value = Node("Value", result);
            var expected = new Dictionary<string, object>
            {
                ["CurrentScore"] = 1244,
                ["CurrentLife"] = GameResultManager.DEFAULTLIFE - 11,
                ["MaxLife"] = GameResultManager.MAXLIFE,
                ["MinLife"] = GameResultManager.MINLIFE,
                ["CurrentCombo"] = 7,
                ["MaxCombo"] = 7,
                ["CoolCount"] = 0,
                ["GoodCount"] = 0,
                ["SafeCount"] = 2,
                ["SadCount"] = 0,
                ["WorstCount"] = 0,
                ["HoldBonus"] = 6,
                ["SlideBonus"] = 4,
                ["ExpectedTotalSlideBonus"] = 40,
                ["IsRetrying"] = false,
                ["IsReplaying"] = false,
            };
            foreach (var (name, v) in expected)
            {
                Assert.True(Equals(v, value.Get(name)), $"{name}: {value.Get(name)}");
            }
            Assert.True(result.SuspendFinish);
        }

        [Fact]
        public void Nodes_OutsideGame()
        {
            foreach (var name in new[] { "GainCurrentCombo", "GainCurrentLife", "GainCurrentScore", "GainEvaluateCount", "SetSuspendFinish" })
            {
                Assert.Equal(["Failed"], Node(name, null).Invoke().Fired);
            }
            var value = Node("Value", null);
            foreach (var name in new[] { "CurrentScore", "CurrentLife", "CurrentCombo", "MaxCombo", "CoolCount", "GoodCount", "SafeCount", "SadCount", "WorstCount", "HoldBonus", "SlideBonus", "ExpectedTotalSlideBonus" })
            {
                Assert.Equal(0, value.Get(name));
            }
            Assert.Equal(false, value.Get("IsRetrying"));
            Assert.Equal(false, value.Get("IsReplaying"));
            Assert.Equal(GameResultManager.MAXLIFE, value.Get("MaxLife"));
        }

        [Fact]
        public void Value_FiresDiedWhenGameCalls()
        {
            var value = Node("Value", new GameResultManager());

            // ゲームはライフがなくなると "Died" を呼ぶ
            value.Node.Manager.Call("Died", null);

            Assert.Equal(["Died"], value.Fired);
        }
    }
}
