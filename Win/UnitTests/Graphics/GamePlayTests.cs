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
                session.Game.Update(new TestInput(pressed?.Invoke(time) ?? []), MouseInfo.Empty);
            }
        }

        /// <summary>
        /// このフレームで押したボタンだけを返す入力です。
        /// </summary>
        sealed class TestInput(ButtonType[] pressed) : InputInfoBase(false)
        {
            public override bool IsPressed(ButtonType buttonType) => pressed.Contains(buttonType);
            public override bool IsReleased(ButtonType buttonType) => false;
            public override int GetPressingFrame(ButtonType buttonType) => pressed.Contains(buttonType) ? 1 : 0;
        }

        /// <summary>
        /// ボタンの代わりに、テストからメニューの操作を起こすポーズメニューです。
        /// </summary>
        sealed class TestPauseMenu(PPDDevice device) : PauseMenuBase(device)
        {
            public override event System.EventHandler Resumed;
            public override event System.EventHandler Retryed;
            public override event System.EventHandler Replayed;
            public override event System.EventHandler Returned;
            public override event ChangeLatencyEventHandler LatencyChanged;
            public int Updates { get; private set; }
            public List<bool> Retries { get; } = [];
            public override void Load() { }
            public override void Update(InputInfoBase inputInfo) => Updates++;
            public override void Retry(bool canReplay) => Retries.Add(canReplay);
            public void Resume() => Resumed?.Invoke(this, System.EventArgs.Empty);
            public void DoRetry() => Retryed?.Invoke(this, System.EventArgs.Empty);
            public void DoReplay() => Replayed?.Invoke(this, System.EventArgs.Empty);
            public void Return() => Returned?.Invoke(this, System.EventArgs.Empty);
            public float ChangeLatency(int sign) => LatencyChanged(sign);
        }

        sealed class TestResult(PPDDevice device) : GameResultBase(device)
        {
            public override event System.EventHandler Returned;
            public override event System.EventHandler Retryed;
            public override event System.EventHandler Replayed;
            public int Updates { get; private set; }
            public int[] Evaluations => MarkEvals;
            public bool IsHighScore => HighScore;
            public int ResultScore => Score;
            public override void Load() { }
            public override void Update(InputInfoBase inputInfo) => Updates++;
            public override void Retry() { }
            public void DoRetry() => Retryed?.Invoke(this, System.EventArgs.Empty);
            public void DoReplay() => Replayed?.Invoke(this, System.EventArgs.Empty);
            public void Return() => Returned?.Invoke(this, System.EventArgs.Empty);
        }

        static void Press(GameSession session, double time, params ButtonType[] buttons)
        {
            Run(session, time, time, _ => buttons);
        }

        [Fact]
        public void Interface_PauseAndResult()
        {
            var pause = new TestPauseMenu(graphics.Device);
            var result = new TestResult(graphics.Device);
            using var session = new GameSession(graphics, Marks, [], withInterface: true, result: result, pauseMenu: pause);
            // ゲームの Maingame と同じく、結果の画面にもゲームの情報を渡す
            result.PPDGameUtility = session.Utility;
            var paused = 0;
            session.Game.Paused += (s, e) => paused++;
            session.Game.Initialize();
            Assert.True(result.CanReplay);

            Run(session, 0, 1.5, time => time is > 0.995 and < 1.01 ? [ButtonType.Circle] : []);
            graphics.Render(session.Game);

            // スタートでポーズ。ポーズ中は時間が進まず、メニューを動かす
            Press(session, 1.5, ButtonType.Start);
            Assert.Equal(1, paused);
            Assert.False(pause.Hidden);
            Assert.Contains("Pause", session.Movie.Calls);
            Run(session, 1.5, 1.6);
            Assert.True(pause.Updates > 0);
            graphics.Render(session.Game);
            // 遅延の調整はデータベースに残す
            var latency = pause.ChangeLatency(1);
            Assert.NotEqual(0, latency);
            pause.ChangeLatency(-1);
            // 再開（メニューを隠すのはメニューの側）
            var plays = session.Movie.Calls.Count(c => c == "Play");
            pause.Resume();
            Assert.Equal(plays + 1, session.Movie.Calls.Count(c => c == "Play"));

            Run(session, 1.6, 5.1, time => time is > 2.995 and < 3.01 ? [ButtonType.Square] : []);
            session.Game.ShowResult();

            // 結果を出して、データベースに自己ベストを残す
            Assert.Equal(ResultEvaluateType.Cheap, result.Result);
            Assert.Equal((2, 1), (result.Evaluations[0], result.Evaluations[4]));
            Assert.True(result.IsHighScore);
            Assert.Equal(result.ResultScore.ToString("D7"), session.Utility.SongInformation.GetScore(PPDFrameworkCore.Difficulty.Easy));
            Run(session, 5.1, 5.2);
            Assert.True(result.Updates > 0);
            graphics.Render(session.Game);

            // 結果の画面からリトライ
            result.DoRetry();
            Assert.Equal(0, session.Game.GameResultManager.CoolCount);
            pause.Return();
            pause.DoRetry();
            pause.DoReplay();
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
