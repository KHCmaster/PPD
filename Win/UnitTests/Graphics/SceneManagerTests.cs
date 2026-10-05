using PPDFramework;
using PPDFramework.Resource;
using PPDFramework.Scene;
using PPDSound;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace UnitTests.Graphics
{
    /// <summary>
    /// シーンの切り替え（別スレッドでの読み込み、シーンのスタック、前のシーンへ渡す値）を行う SceneManager のテストです。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public class SceneManagerTests(GraphicsDevice graphics)
    {
        class FakeScene(string name, List<string> log, bool loads = true) : ISceneBase
        {
            public SceneManager SceneManager { get; set; }
            public ISound Sound { get; set; }
            public IGameHost GameHost { get; set; }
            public ResourceManager ResourceManager { get; set; }
            public Dictionary<string, object> Param { get; set; }
            public Dictionary<string, object> PreviousParam { get; set; }
            public bool IsInSceneStack { get; set; }
            public bool ShouldFinishLoad { get; set; }
            public string SpriteDir => null;
            public string Name => name;

            public bool Load()
            {
                lock (log)
                {
                    log.Add($"{name} load");
                }
                return loads;
            }

            public void Update(InputInfoBase inputInfo, MouseInfo mouseInfo) => log.Add($"{name} update");
            public void Draw() => log.Add($"{name} draw");
            public void Dispose() => log.Add($"{name} dispose");
            public void SceneStackPoped(Dictionary<string, object> param) => log.Add($"{name} poped {string.Join(",", param.Keys)}");
        }

        sealed class Title(List<string> log) : FakeScene("title", log);

        sealed class Menu(List<string> log, bool loads = true) : FakeScene("menu", log, loads);

        sealed class Game(List<string> log) : FakeScene("game", log);

        // 読み込みが終わるまで Update を呼ぶ
        static void WaitLoaded(SceneManager manager)
        {
            for (var i = 0; i < 200 && (manager.Loading || manager.CurrentScene == null); i++)
            {
                Thread.Sleep(5);
                manager.Update(null, MouseInfo.Empty, null);
            }
            Assert.False(manager.Loading);
        }

        [Fact]
        public void ChangeGame_LoadsSceneInBackground()
        {
            var log = new List<string>();
            using var manager = new SceneManager(graphics.Device);
            var events = new List<string>();
            manager.EnterLoading += (s, e) => events.Add("enter");
            manager.LeaveLoading += (s, e) => events.Add("leave");
            var title = new Title(log);

            manager.ChangeGame(title, new Dictionary<string, object> { ["a"] = 1 }, null);
            Assert.Null(manager.CurrentScene);
            WaitLoaded(manager);
            manager.Draw();

            Assert.Same(title, manager.CurrentScene);
            Assert.Same(manager, title.SceneManager);
            Assert.NotNull(title.ResourceManager);
            Assert.Equal(1, title.Param["a"]);
            Assert.True(title.IsInSceneStack);
            Assert.Equal(["enter", "leave"], events);
            Assert.Equal(["title load", "title update", "title draw"], log);
        }

        [Fact]
        public void NextScene_GetsParamsAndPreviousIsDisposed()
        {
            var log = new List<string>();
            using var manager = new SceneManager(graphics.Device);
            var title = new Title(log);
            manager.ChangeGame(title, null, null);
            WaitLoaded(manager);
            log.Clear();

            var menu = new Menu(log);
            manager.PrepareNextScene(title, menu, new Dictionary<string, object> { ["song"] = "x" }, new Dictionary<string, object> { ["cursor"] = 3 });
            WaitLoaded(manager);

            Assert.Same(menu, manager.CurrentScene);
            Assert.Equal("x", menu.Param["song"]);
            Assert.Empty(menu.PreviousParam);
            // 次のシーンは、呼んだシーンのリソースを使う
            Assert.Same(title.ResourceManager, menu.ResourceManager);
            // 前のシーンは、次のシーンを読み込んだ後の Update で破棄する
            Assert.Contains("title dispose", log);

            // 同じ種類のシーンにまた来たときは、前に残した値がもらえる
            var title2 = new Title(log);
            manager.PrepareNextScene(menu, title2, null, null);
            WaitLoaded(manager);
            Assert.Equal(3, title2.PreviousParam["cursor"]);
            Assert.Empty(title2.Param);
        }

        [Fact]
        public void SceneStack_PushAndPop()
        {
            var log = new List<string>();
            using var manager = new SceneManager(graphics.Device);
            var title = new Title(log);
            manager.ChangeGame(title, null, null);
            WaitLoaded(manager);

            var game = new Game(log);
            manager.PrepareNextScene(title, game, null, null, true);
            WaitLoaded(manager);
            Assert.Same(game, manager.CurrentScene);
            Assert.True(game.IsInSceneStack);
            log.Clear();

            manager.PopCurrentScene(new Dictionary<string, object> { ["result"] = 1 });
            manager.Update(null, MouseInfo.Empty, null);

            Assert.Same(title, manager.CurrentScene);
            Assert.Equal(["title poped result", "game dispose", "title update"], log);

            // スタックに入れていないシーンからは戻らない
            var menu = new Menu(log);
            manager.PrepareNextScene(title, menu, null, null);
            WaitLoaded(manager);
            manager.PopCurrentScene([]);
            Assert.Same(menu, manager.CurrentScene);
        }

        [Fact]
        public void FailedLoad_ReturnsToSceneOnStack()
        {
            var log = new List<string>();
            using var manager = new SceneManager(graphics.Device);
            var title = new Title(log);
            manager.ChangeGame(title, null, null);
            WaitLoaded(manager);

            manager.PrepareNextScene(title, new Menu(log, loads: false), null, null, true);
            WaitLoaded(manager);
            manager.Update(null, MouseInfo.Empty, null);

            Assert.Same(title, manager.CurrentScene);
            Assert.Contains("title poped Failed to initialize", log);
            Assert.Contains("menu dispose", log);
        }

        [Fact]
        public void PopToHome_DisposesStackedScenes()
        {
            var log = new List<string>();
            using var manager = new SceneManager(graphics.Device);
            var title = new Title(log);
            manager.ChangeGame(title, null, null);
            WaitLoaded(manager);
            var menu = new Menu(log);
            manager.PrepareNextScene(title, menu, null, null, true);
            WaitLoaded(manager);
            var game = new Game(log);
            manager.PrepareNextScene(menu, game, null, null, true);
            WaitLoaded(manager);
            log.Clear();

            manager.PopToHome();

            // ホーム（ChangeGame の前）まで戻り、シーンはなくなる
            Assert.Null(manager.CurrentScene);
            Assert.Contains("game dispose", log);
            Assert.Contains("menu dispose", log);
            Assert.Contains("title dispose", log);
        }

        [Fact]
        public void Dispose_DisposesAllScenes()
        {
            var log = new List<string>();
            var manager = new SceneManager(graphics.Device);
            var title = new Title(log);
            manager.ChangeGame(title, null, null);
            WaitLoaded(manager);
            manager.PrepareNextScene(title, new Game(log), null, null, true);
            WaitLoaded(manager);

            manager.Dispose();

            Assert.Contains("title dispose", log);
            Assert.Contains("game dispose", log);
        }
    }
}
