using PPDCore;
using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using PPDFrameworkCore;
using System;
using System.IO;
using System.Reflection;
using UnitTests.Libraries;
using UnitTests.PPDEditor;
using Xunit;

namespace UnitTests.Graphics
{
    /// <summary>
    /// ゲーム全体（MainGameComponent）を、一時フォルダーの譜面とテストの画像で作ります。
    /// </summary>
    /// <remarks>
    /// 動画は時間を動かせる偽物、音とゲームのホストは何もしない偽物。Mod とネットワークは使わない。
    /// </remarks>
    sealed class GameSession : IDisposable
    {
        readonly SongDatabase database = new();
        readonly GameResources resources;

        public FakeMovie Movie { get; } = FakeMovie.Create();

        public PPDGameUtility Utility { get; }

        public MainGameComponent Game { get; }

        public GameSession(GraphicsDevice graphics, MarkData[] marks, IEVDData[] events, AutoMode auto = AutoMode.None)
        {
            Assert.False(TcpDebugControllerBase.IsListening());
            Directory.CreateDirectory("dlls");
            resources = new GameResources(graphics, setCurrentDirectory: false);
            foreach (var effect in new[] { @"hold\holdback.etd", @"hold\maxback.etd", @"slide\max_slide_effect.etd", @"slide\hatch_right.etd", @"slide\hatch_left.etd" })
            {
                resources.WriteEffect(Path.Combine(@"img\PPD\main_game", effect));
            }
            var dir = database.AddSong("song", "guid", 130, ("Easy", null));
            ChartFiles.WritePack(Path.Combine(dir, "Easy.ppd"), marks, events);
            File.WriteAllText(Path.Combine(dir, "movie.mp4"), "");
            SongInformation.Update();
            Utility = new PPDGameUtility
            {
                SongInformation = SongDatabase.Song("song"),
                Difficulty = Difficulty.Easy,
                Profile = ProfileManager.Instance.Default,
                SpeedScale = 1,
                AutoMode = auto,
                AppliedMods = [],
            };
            var host = Nothing.Create<IGameHost>();
            ((Nothing)(object)host).Results[nameof(IGameHost.GetMovie)] = Movie;
            Game = new MainGameComponent(graphics.Device, host, resources.Resources, Nothing.Create<ISound>(), null,
                Utility, null, resources.MarkImages, null, null, MainGameConfigBase.Default, 0, 0);
        }

        public void Dispose()
        {
            Game.Dispose();
            resources.Dispose();
            database.Dispose();
        }

        /// <summary>
        /// どのメソッドも何もせず、既定の値（決めたものはその値）を返すインターフェースの実装です。
        /// </summary>
        public class Nothing : DispatchProxy
        {
            public System.Collections.Generic.Dictionary<string, object> Results { get; } = [];

            public static T Create<T>() => DispatchProxy.Create<T, Nothing>();

            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                if (Results.TryGetValue(targetMethod.Name, out var result))
                {
                    return result;
                }
                var type = targetMethod.ReturnType;
                return type == typeof(void) || !type.IsValueType ? null : Activator.CreateInstance(type);
            }
        }

        /// <summary>
        /// 時間をテストから動かす動画です。プロパティは設定した値を返します。
        /// </summary>
        public class FakeMovie : DispatchProxy
        {
            readonly System.Collections.Generic.Dictionary<string, object> properties = new()
            {
                ["Initialized"] = true,
                ["Length"] = 5d,
                ["MoviePosition"] = 0d,
            };

            public static FakeMovie Create() => (FakeMovie)(object)DispatchProxy.Create<IMovie, FakeMovie>();

            public IMovie Movie => (IMovie)(object)this;

            public double Position
            {
                get => (double)properties["MoviePosition"];
                set => properties["MoviePosition"] = value;
            }

            public System.Collections.Generic.List<string> Calls { get; } = [];

            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                var name = targetMethod.Name;
                if (name.StartsWith("get_") && properties.TryGetValue(name[4..], out var value))
                {
                    return value;
                }
                if (name.StartsWith("set_"))
                {
                    properties[name[4..]] = args[0];
                    return null;
                }
                Calls.Add(name);
                var type = targetMethod.ReturnType;
                return type == typeof(void) || !type.IsValueType ? null : Activator.CreateInstance(type);
            }
        }
    }
}
