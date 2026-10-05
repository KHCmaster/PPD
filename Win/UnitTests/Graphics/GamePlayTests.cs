using PPDCore;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using System.Collections.Generic;
using System.Linq;
using UnitTests.PPDEditor;
using Xunit;

namespace UnitTests.Graphics
{
    /// <summary>
    /// ゲーム全体（MainGameComponent）を、動画の時間を進めて遊ぶテストです。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public sealed class GamePlayTests(GraphicsDevice graphics)
    {
        static readonly MarkData[] Marks = [ChartFiles.Mark(1, ButtonType.Circle), ChartFiles.Mark(2, ButtonType.Cross), ChartFiles.Mark(3, ButtonType.Square)];

        /// <summary>
        /// 動画の時間を 1/60 秒ずつ進めて更新します（押すボタンは時間ごとに決める）。
        /// </summary>
        static void Run(GameSession session, double from, double to, System.Func<double, ButtonType[]> pressed = null)
        {
            for (var time = from; time <= to; time += 1 / 60d)
            {
                session.Movie.Position = time;
                var buttons = pressed?.Invoke(time) ?? [];
                var count = new int[10];
                var p = new bool[10];
                foreach (var button in buttons)
                {
                    p[(int)button] = true;
                    count[(int)button] = 1;
                }
                session.Game.Update(new CustomInputInfo(count, p, new bool[10], EmptyInputInfo.Instance), MouseInfo.Empty);
            }
        }

        [Fact]
        public void Auto_PlaysToTheEndAndSavesScore()
        {
            using var session = new GameSession(graphics, Marks, [], AutoMode.All);
            var finished = 0;
            var scores = new List<int>();
            session.Game.Finished += (s, e) => finished++;
            session.Game.ScoreChanged += scores.Add;

            session.Game.Initialize();
            Assert.Contains("Play", session.Movie.Calls);
            Run(session, 0, 5.1);

            // 動画の長さ（5 秒）で終わる
            Assert.Equal(1, finished);
            Assert.False(session.Game.Mistake);
            Assert.Equal(3, session.Game.GameResultManager.CoolCount);
            Assert.True(scores.Last() > 0);
            Assert.Equal(scores.Last(), session.Game.GameResultManager.CurrentScore);
            graphics.Render(session.Game);

            session.Game.ShowResult();
            Assert.Contains("Stop", session.Movie.Calls);
        }

        [Fact]
        public void Manual_PressesAreJudgedAndRetryStartsOver()
        {
            using var session = new GameSession(graphics, Marks, []);
            session.Game.Initialize();

            // 1 秒と 3 秒のマークだけ押す
            Run(session, 0, 4, time => time is > 0.995 and < 1.01 ? [ButtonType.Circle] : time is > 2.995 and < 3.01 ? [ButtonType.Square] : []);

            var result = session.Game.GameResultManager;
            Assert.Equal((2, 1), (result.CoolCount, result.WorstCount));
            graphics.Render(session.Game);

            // やり直すと数え直す
            session.Game.Retry();
            Assert.Equal((0, 0), (result.CoolCount, result.WorstCount));
            Run(session, 0, 1.5, time => time is > 0.995 and < 1.01 ? [ButtonType.Circle] : []);
            Assert.Equal(1, result.CoolCount);

            // 今のプレイをリプレイすると、同じように押される
            Run(session, 1.5, 5.1);
            session.Game.ShowResult();
            session.Game.Replay();
            Run(session, 0, 5.1);
            Assert.Equal((1, 2), (result.CoolCount, result.WorstCount));
        }
    }
}
