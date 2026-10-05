using PPDEditorCommon;
using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// エディターが Items で渡すもの（Storage、SongInfo、MovieManager、Movie、Events）を使う PPDEditor のノードと、EventData のテストです。
    /// </summary>
    public class EditorServiceNodeTests
    {
        static NodeHarness Node(string name, Dictionary<string, object> items = null, params (string Name, object Value)[] inputs)
        {
            var type = typeof(FlowScriptEnginePPDEditor.ToolTipText).Assembly.GetType($"FlowScriptEnginePPDEditor.FlowSourceObjects.{name}FlowSourceObject", true);
            var harness = new NodeHarness(type);
            foreach (var (key, value) in items ?? [])
            {
                harness.Items[key] = value;
            }
            harness.Initialize();
            foreach (var (n, v) in inputs)
            {
                harness.Set(n, v);
            }
            return harness;
        }

        sealed class FakeStorage : IStorage
        {
            public Dictionary<(string, string), string> Values { get; } = [];
            public string GetValue(string storageKey, string key) => Values.GetValueOrDefault((storageKey, key));
            public void SetValue(string storageKey, string key, string value) => Values[(storageKey, key)] = value;
        }

        [Fact]
        public void Storage_SetAndGet()
        {
            var storage = new FakeStorage();
            var items = new Dictionary<string, object> { ["Storage"] = storage };

            Assert.Equal(["Success"], Node("Storage.SetValue", items, ("StorageKey", "mod"), ("Key", "k"), ("Value", "v")).Invoke().Fired);

            Assert.Equal("v", storage.Values[("mod", "k")]);
            Assert.Equal("v", Node("Storage.GetValue", items, ("StorageKey", "mod"), ("Key", "k")).Get("Value"));
            Assert.Null(Node("Storage.GetValue", items, ("StorageKey", "other"), ("Key", "k")).Get("Value"));
            Assert.Equal(["Failed"], Node("Storage.SetValue").Invoke().Fired);
            Assert.Null(Node("Storage.GetValue", null, ("Key", "k")).Get("Value"));
        }

        sealed class FakeSongInfo : ISongInfo
        {
            public float BPM => 150;
            public float BPMOffset => 0.25f;
            public string CurrentProjectName => "song";
            public string CurrentProjectFilePath => @"C:\songs\song.ppdproj";
            public string CurrentProjectDir => @"C:\songs";
        }

        [Fact]
        public void SongInfo()
        {
            var info = Node("Song.Info", new() { ["SongInfo"] = new FakeSongInfo() });
            Assert.Equal(150f, info.Get("BPM"));
            Assert.Equal(0.25f, info.Get("BPMOffset"));
            Assert.Equal(@"C:\songs\song.ppdproj", info.Get("ProjectPath"));
            Assert.Equal("song", info.Get("ProjectName"));

            var empty = Node("Song.Info");
            Assert.Equal(0f, empty.Get("BPM"));
            Assert.Equal(0f, empty.Get("BPMOffset"));
            Assert.Null(empty.Get("ProjectPath"));
            Assert.Null(empty.Get("ProjectName"));
        }

        sealed class FakeMovieManager : IMovieManager
        {
            public List<string> Calls { get; } = [];
            public void Play() => Calls.Add("play");
            public void Stop() => Calls.Add("stop");
            public void Pause() => Calls.Add("pause");
            public void Seek(double time) => Calls.Add($"seek {time}");
            public void SetTrimming(MovieTrimmingData trimming, bool applyToIniFileWriter)
            {
                MovieTrimmingData = trimming;
                Calls.Add($"trim {trimming.Top} {trimming.Left} {trimming.Right} {trimming.Bottom} {applyToIniFileWriter}");
            }
            public MovieTrimmingData MovieTrimmingData { get; private set; } = new();
        }

        [Fact]
        public void MovieManager_Controls()
        {
            var movie = new FakeMovieManager();
            var items = new Dictionary<string, object> { ["MovieManager"] = movie };
            void Ok(string name, params (string, object)[] inputs) => Assert.Equal(["Success"], Node("Movie." + name, items, inputs).Invoke().Fired);

            Ok("Play");
            Ok("Pause");
            Ok("Seek", ("Time", 12.5));
            Ok("Stop");
            Ok("SetTrimming", ("Top", 1f), ("Left", 2f), ("Right", 3f), ("Bottom", 4f), ("ApplyToIniFileWriter", true));

            Assert.Equal(["play", "pause", "seek 12.5", "stop", "trim 1 2 3 4 True"], movie.Calls);
            var trimming = Node("Movie.Trimming", items);
            Assert.Equal((1f, 2f, 3f, 4f), ((float)trimming.Get("Top"), (float)trimming.Get("Left"), (float)trimming.Get("Right"), (float)trimming.Get("Bottom")));
        }

        [Fact]
        public void MovieManager_OutsideEditor()
        {
            foreach (var name in new[] { "Play", "Pause", "Seek", "Stop", "SetTrimming" })
            {
                Assert.Equal(["Failed"], Node("Movie." + name).Invoke().Fired);
            }
            var trimming = Node("Movie.Trimming");
            Assert.Equal(0f, trimming.Get("Top"));
            Assert.Equal(0f, trimming.Get("Left"));
            Assert.Equal(0f, trimming.Get("Right"));
            Assert.Equal(0f, trimming.Get("Bottom"));
        }

        // IMovie はメンバーが多いので、読むプロパティだけ値を返すものを作る
        public class MovieProxy : DispatchProxy
        {
            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                return targetMethod.Name switch
                {
                    "get_Length" => (object)90.5,
                    "get_MoviePosition" => 12.25,
                    "get_MovieWidth" => 640,
                    "get_MovieHeight" => 360,
                    _ => throw new NotSupportedException(targetMethod.Name),
                };
            }
        }

        [Fact]
        public void Movie_Values()
        {
            var items = new Dictionary<string, object> { ["Movie"] = DispatchProxy.Create<IMovie, MovieProxy>() };

            Assert.Equal(90.5f, Node("Movie.Length", items).Get("Value"));
            Assert.Equal(12.25f, Node("Movie.Position", items).Get("Value"));
            Assert.Equal(640, Node("Movie.Size", items).Get("Width"));
            Assert.Equal(360, Node("Movie.Size", items).Get("Height"));

            Assert.Equal(0f, Node("Movie.Length").Get("Value"));
            Assert.Equal(0f, Node("Movie.Position").Get("Value"));
            Assert.Equal(0, Node("Movie.Size").Get("Width"));
            Assert.Equal(0, Node("Movie.Size").Get("Height"));
        }

        static EventData Sample()
        {
            var data = new EventData
            {
                BPM = 180,
                BPMRapidChange = true,
                DisplayState = DisplayState.Sudden,
                MoveState = MoveState.Stop,
                NoteType = NoteType.AC,
                SlideScale = 2,
            };
            data.SetVolume(0, 50);
            data.SetVolume(3, 150);
            data.SetVolume(4, -5);
            data.SetKeepPlaying(2, true);
            data.SetReleaseSound(10, true);
            data.InitializeOrder[0] = ButtonType.L;
            data.InitializeOrder[9] = ButtonType.Square;
            return data;
        }

        [Fact]
        public void EventData_StoresChannelsAndClones()
        {
            var data = Sample();

            // 音量は 0 から 100
            Assert.Equal((50, 100, 0), (data.MovieVolumePercent, data.CircleVolumePercent, data.TriangleVolumePercent));
            Assert.True(data.CrossKeepPlaying);
            Assert.True(data.LReleaseSound);

            var clone = data.Clone();
            Assert.Equal(data.GetFormattedContent(), clone.GetFormattedContent());
            clone.SetVolume(0, 10);
            Assert.Equal(50, data.MovieVolumePercent);

            data.SetDefault();
            Assert.Equal(90, data.CrossVolumePercent);
            Assert.False(data.CrossKeepPlaying);
            Assert.Equal(100f, data.BPM);
            Assert.Equal(Enumerable.Range(0, 10).Select(i => (ButtonType)i), data.InitializeOrder);
        }

        [Fact]
        public void EventData_DrawingOrderIsInverseOfInitializeOrder()
        {
            var data = new EventData();
            data.InitializeOrder = [.. data.InitializeOrder.Reverse()];

            Assert.Equal(Enumerable.Range(0, 10).Reverse(), data.DrawingOrder);
        }

        [Fact]
        public void EventData_Text()
        {
            var data = Sample();

            var lines = data.ToString().Split('\n');
            Assert.StartsWith("50,90,90,100,0,90,", lines[0]);
            Assert.EndsWith(",Sudden,Stop,AC,2", lines[0]);
            // 初期化の順番はカンマで区切る
            Assert.Equal("L,Cross,Circle,Triangle,Left,Down,Right,Up,R,Square", lines[1]);

            var formatted = data.GetFormattedContent().Split('\n');
            Assert.Equal("50", formatted[0]);
            Assert.Equal("90,True,False", formatted[2]);
            Assert.Equal("100,False,False", formatted[3]);
            Assert.Equal("180,True", formatted[11]);
            Assert.Equal("L,Cross,Circle,Triangle,Left,Down,Right,Up,R,Square", formatted[16]);
        }

        [Fact]
        public void EventValue_ReadsEventData()
        {
            var value = Node("Event.Value", null, ("Event", Sample()));

            Assert.Equal(DisplayState.Sudden, value.Get("DisplayState"));
            Assert.Equal(MoveState.Stop, value.Get("MoveState"));
            Assert.Equal(NoteType.AC, value.Get("NoteType"));
            Assert.Equal(2f, value.Get("SlideScale"));
            Assert.Equal(180f, value.Get("BPM"));
            Assert.Equal(true, value.Get("BPMRapidChange"));
            Assert.Equal(new object[] { 50, 90, 90, 100, 0, 90, 90, 90, 90, 90, 90 }, (object[])value.Get("VolumePercents"));
            Assert.Equal(true, ((object[])value.Get("KeepPlayings"))[1]);
            Assert.Equal(true, ((object[])value.Get("ReleaseSounds"))[9]);
            Assert.Equal(ButtonType.L, ((object[])value.Get("InitializeOrder"))[0]);

            var empty = Node("Event.Value");
            Assert.Equal((DisplayState)0, empty.Get("DisplayState"));
            Assert.Equal(1f, empty.Get("SlideScale"));
            Assert.Equal(false, empty.Get("BPMRapidChange"));
            Assert.Null(empty.Get("VolumePercents"));
            Assert.Null(empty.Get("KeepPlayings"));
            Assert.Null(empty.Get("ReleaseSounds"));
            Assert.Null(empty.Get("InitializeOrder"));
        }

        [Fact]
        public void EventValues_ListsEventsByTime()
        {
            var events = new[] { new KeyValuePair<float, EventData>(0, new EventData()), new KeyValuePair<float, EventData>(5, Sample()) };

            var values = (object[])Node("Event.Values", new() { ["Events"] = events }).Get("Values");

            Assert.Equal(2, values.Length);
            Assert.Equal(5f, ((KeyValuePair<object, object>)values[1]).Key);
            Assert.Same(events[1].Value, ((KeyValuePair<object, object>)values[1]).Value);
            Assert.Null(Node("Event.Values").Get("Values"));
        }
    }
}
