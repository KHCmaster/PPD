using PPDFramework;
using System;
using UnitTests.FlowScript;
using Xunit;

namespace UnitTests.Graphics
{
    /// <summary>
    /// 動画を操作するノード（PPD.Graphics.Movie）と、ゲームの表示の層を返すノードのテストです。動画はテストの偽物。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public sealed class MovieNodeTests(GraphicsDevice graphics)
    {
        static NodeHarness Movie(string node, GameSession.FakeMovie movie, params (string Name, object Value)[] inputs)
        {
            var type = typeof(FlowScriptEnginePPD.ToolTipText).Assembly.GetType($"FlowScriptEnginePPD.FlowSourceObjects.Graphics.Movie.{node}FlowSourceObject", true);
            var harness = new NodeHarness(type);
            if (movie != null)
            {
                harness.Items["Movie"] = movie.Movie;
            }
            harness.Initialize();
            foreach (var (name, value) in inputs)
            {
                harness.Set(name, value);
            }
            return harness;
        }

        [Fact]
        public void MovieNodes_ControlTheMovie()
        {
            var movie = GameSession.FakeMovie.Create();
            movie.Position = 1.5;
            movie.Movie.Volume = 80;

            Assert.Equal(["Success"], Movie("Play", movie).Invoke().Fired);
            Assert.Equal(["Success"], Movie("Pause", movie).Invoke().Fired);
            Assert.Equal(["Success"], Movie("Stop", movie).Invoke().Fired);
            Assert.Equal(["Play", "Pause", "Stop"], movie.Calls);

            Assert.Equal(["Success"], Movie("Seek", movie, ("Time", 3.0)).Invoke().Fired);
            Assert.Equal("Seek", movie.Calls[^1]);
            Assert.Equal(["Success"], Movie("SetVolume", movie, ("Volume", 60)).Invoke().Fired);
            Assert.Equal(60, movie.Movie.Volume);

            Assert.Equal(5f, Movie("Length", movie).Get("Value"));
            Assert.Equal(1.5f, Movie("Position", movie).Get("Value"));
            Assert.Equal(60, Movie("Volume", movie).Get("Value"));
        }

        [Fact]
        public void MovieNodes_WithoutMovie()
        {
            foreach (var node in new[] { "Play", "Pause", "Stop", "Seek", "SetVolume" })
            {
                Assert.Equal(["Failed"], Movie(node, null).Invoke().Fired);
            }
            Assert.Equal(0f, Movie("Length", null).Get("Value"));
            Assert.Equal(0f, Movie("Position", null).Get("Value"));
        }

        [Fact]
        public void SoundNodes_WithoutSound_Fail()
        {
            foreach (var node in new[] { "Play", "PlayWithVolume", "Stop" })
            {
                var type = typeof(FlowScriptEnginePPD.ToolTipText).Assembly.GetType($"FlowScriptEnginePPD.FlowSourceObjects.Audio.Sound.{node}FlowSourceObject", true);
                var harness = new NodeHarness(type);
                harness.Items["PlayRatio"] = 1.5;
                Assert.Equal(["Failed"], harness.Initialize().Invoke().Fired);
            }
        }

        [Fact]
        public void GameInterfaceNode_ReturnsLayers()
        {
            var names = new[] { "GameInterface", "HoldLayer", "MarkEffectLayer", "ConnectLayer", "MarkLayer", "ComboLayer", "SlideLayer" };
            var harness = new NodeHarness(typeof(FlowScriptEnginePPD.FlowSourceObjects.Graphics.GameInterfaceFlowSourceObject));
            foreach (var name in names)
            {
                harness.Items[name] = new SpriteObject(graphics.Device);
            }
            harness.Initialize();

            Assert.Same(harness.Items["GameInterface"], harness.Get("Value"));
            foreach (var name in names[1..])
            {
                Assert.Same(harness.Items[name], harness.Get(name));
            }
            Assert.Null(new NodeHarness(typeof(FlowScriptEnginePPD.FlowSourceObjects.Graphics.GameInterfaceFlowSourceObject)).Initialize().Get("Value"));
        }
    }
}
