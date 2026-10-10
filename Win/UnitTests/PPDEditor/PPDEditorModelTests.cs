using BezierCaliculator;
using PPDConfiguration;
using PPDEditor;
using PPDEditor.Models;
using PPDEditorCommon;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.EVDData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Xunit;
using PointF = System.Drawing.PointF;

namespace UnitTests.PPDEditor
{
    public class KasiDataTests
    {
        [Fact]
        public void ParseAndSerialize_RoundTrip()
        {
            var kasi = new KasiData();

            kasi.Parse("1.5:first\r\n\t0:zero\nbroken line\n3:\n2.25:second");

            Assert.Equal([0f, 1.5f, 2.25f, 3f], kasi.Times);
            Assert.Equal(["zero", "first", "second", ""], kasi.Texts);
            Assert.Equal(string.Join(Environment.NewLine, "0:zero", "1.5:first", "2.25:second", "3:"), kasi.Serialize());
        }

        [Fact]
        public void LyricsWithColon_RoundTrip()
        {
            var kasi = new KasiData();
            kasi.Add(1, "Re:Start: 2");

            var loaded = new KasiData();
            loaded.Parse(kasi.Serialize());

            // 歌詞の中の「:」で切らない
            Assert.Equal(["Re:Start: 2"], loaded.Texts);
        }

        [Fact]
        public void AddMoveAndRemove()
        {
            var kasi = new KasiData();
            Assert.True(kasi.Add(1, "a"));
            Assert.False(kasi.Add(1, "b"));
            Assert.True(kasi.Add(2, "b"));

            Assert.False(kasi.Move(5, 6));
            Assert.False(kasi.Move(1, 2));
            Assert.True(kasi.Move(1, 3));
            Assert.Equal([2f, 3f], kasi.Times);
            Assert.True(kasi.Contains(3));
            Assert.Equal(1, kasi.IndexOf(3));

            kasi.RemoveAt(0);
            Assert.Equal(1, kasi.Count);
            kasi.Clear();
            Assert.Empty(kasi.Items);
        }

        [Fact]
        public void CurrentAndFormattedContent()
        {
            var kasi = new KasiData();
            Assert.Equal("", kasi.GetCurrent(1));
            kasi.Add(1, "a");
            kasi.Add(2, "");
            kasi.Add(4, "c");

            Assert.Equal("a", kasi.GetCurrent(0));
            Assert.Equal("a", kasi.GetCurrent(1.5f));
            Assert.Equal("", kasi.GetCurrent(3));
            Assert.Equal("c", kasi.GetCurrent(10));
            Assert.Equal(" ", kasi.GetFormattedContent(2));
            Assert.Equal("a", kasi.GetFormattedContent(1));
            Assert.Equal("", kasi.GetFormattedContent(3));
            Assert.Equal([1f, 2f], kasi.GetTimesWithin(1, 3));
        }
    }

    public class EventListTests
    {
        private static EventList Create()
        {
            var list = new EventList();
            list.Add(0, new EventData());
            return list;
        }

        [Fact]
        public void AddMoveReplaceAndRemove()
        {
            var list = Create();
            Assert.False(list.Add(0, new EventData()));
            Assert.True(list.Add(5, new EventData { BPM = 120 }));
            Assert.False(list.Move(1, 2));
            Assert.False(list.Move(0, 5));
            Assert.True(list.Move(5, 6));
            Assert.Equal([0f, 6f], list.Times);

            list.ReplaceAt(1, new EventData { BPM = 140 });
            Assert.Equal(140, list.Events[1].BPM);
            Assert.Equal(6, list.Times[1]);
            list.SetInitialBpm(80);
            Assert.Equal(80, list.Events[0].BPM);

            var copy = list.Data;
            list.RemoveAt(1);
            Assert.Equal(1, list.Count);
            Assert.Equal(2, copy.Count);
            list.Data = copy;
            Assert.Equal(2, list.Count);
            list.Clear();
            Assert.Empty(list.Items);
        }

        [Fact]
        public void EventAtTime()
        {
            var list = new EventList();
            Assert.Null(list.GetEvent(1));
            Assert.Equal(-1, list.GetIndexAt(1));
            var first = new EventData();
            var second = new EventData { BPM = 150 };
            list.Add(1, first);
            list.Add(3, second);

            Assert.Null(list.GetEvent(0.5f));
            Assert.Same(first, list.GetEvent(2));
            Assert.Same(second, list.GetEvent(3));
            Assert.Equal(0, list.GetIndexAt(2));
            Assert.Equal(1, list.GetIndexAt(10));
            Assert.Equal([3f], list.GetTimesWithin(2, 4));
            Assert.Equal("", list.GetFormattedContent(2));
            Assert.NotEqual("", list.GetFormattedContent(3));
            Assert.True(list.Contains(3));
            Assert.Equal(1, list.IndexOf(3));
        }

        [Fact]
        public void CorrectTime_SkipsStoppedRange()
        {
            var list = Create();
            list.Add(2, new EventData { MoveState = MoveState.Stop });
            list.Add(5, new EventData { MoveState = MoveState.Normal });

            // 止まっている 2 秒から 5 秒の後のマーク
            Assert.Equal(4, list.GetCorrectTime(1, 10));
            Assert.Equal(5, list.GetCorrectTime(3, 10));
            Assert.Equal(6, list.GetCorrectTime(6, 10));
            // 止まる前のマークには関係ない
            Assert.Equal(1, list.GetCorrectTime(1, 2));
        }

        [Fact]
        public void ToEvdData_WritesOnlyChanges()
        {
            var list = Create();
            list.Add(1, new EventData { BPM = 120, BPMRapidChange = true });
            var changed = new EventData { BPM = 120, MoveState = MoveState.Stop, DisplayState = DisplayState.Hidden, NoteType = NoteType.AC, SlideScale = 2 };
            changed.MovieVolumePercent = 50;
            changed.SquareKeepPlaying = true;
            changed.CrossReleaseSound = true;
            changed.InitializeOrder = [.. Enumerable.Range(0, 10).Reverse().Select(i => (ButtonType)i)];
            list.Add(2, changed);

            var evd = list.ToEvdData(100);

            Assert.DoesNotContain(evd, e => e.Time == 0);
            Assert.IsType<RapidChangeBPMEvent>(Assert.Single(evd, e => e.Time == 1));
            var at2 = evd.Where(e => e.Time == 2).Select(e => e.GetType()).ToList();
            Assert.Contains(typeof(ChangeVolumeEvent), at2);
            Assert.Contains(typeof(ChangeSoundPlayModeEvent), at2);
            Assert.Contains(typeof(ChangeDisplayStateEvent), at2);
            Assert.Contains(typeof(ChangeMoveStateEvent), at2);
            Assert.Contains(typeof(ChangeReleaseSoundEvent), at2);
            Assert.Contains(typeof(ChangeNoteTypeEvent), at2);
            Assert.Contains(typeof(ChangeSlideScaleEvent), at2);
            Assert.Contains(typeof(ChangeInitializeOrderEvent), at2);
            Assert.DoesNotContain(typeof(ChangeBPMEvent), at2);
        }

        [Fact]
        public void ToEvdData_InitialBpmDifferent_WritesBpm()
        {
            var list = Create();

            var evd = list.ToEvdData(150);

            var bpm = Assert.IsType<ChangeBPMEvent>(Assert.Single(evd));
            Assert.Equal(100, bpm.BPM);
        }
    }

    public class SoundChangeListTests
    {
        private static SoundChangeList Create()
        {
            var list = new SoundChangeList();
            list.AddChange(0, new ushort[SoundChangeList.ButtonCount]);
            return list;
        }

        [Fact]
        public void AddAndRemoveSounds_ShiftsAssignments()
        {
            var list = Create();
            Assert.True(list.AddSound(@"C:\s\a.wav"));
            Assert.True(list.AddSound(@"C:\s\b.wav"));
            Assert.False(list.AddSound(@"C:\s\a.wav"));
            list.AddChange(1, [1, 2, 3, 0, 0, 0, 0, 0, 0, 0]);

            // 2 番目（b.wav）を消すと、それを使っていた割り当ては無音、後ろは詰める
            Assert.True(list.RemoveSound(2));
            Assert.Equal([1, 0, 2, 0, 0, 0, 0, 0, 0, 0], list.Changes[1]);
            // 同じファイルがまだあるときは、読み込んだ音を消してはいけない
            Assert.False(list.RemoveSound(1));
            Assert.Equal("a.wav", list.SerializeSounds());

            list.ReplaceSound(1, @"C:\s\c.wav");
            Assert.Equal("c.wav", list.SerializeSounds());
        }

        [Theory]
        [InlineData(3, 3, 0)]
        [InlineData(5, 3, 4)]
        [InlineData(2, 3, 2)]
        public void ShiftIndex(ushort index, int removed, ushort expected)
        {
            Assert.Equal(expected, SoundChangeList.ShiftIndex(index, removed));
        }

        [Fact]
        public void ChangesAtTime()
        {
            var list = Create();
            Assert.False(list.AddChange(0, new ushort[10]));
            list.AddChange(2, new ushort[10]);
            list.AddChange(4, new ushort[10]);

            Assert.Equal(0, list.GetIndexAt(-1));
            Assert.Equal(1, list.GetIndexAt(3));
            Assert.Equal(2, list.GetIndexAt(4));
            Assert.Equal([2f, 4f], list.GetTimesWithin(1, 5));
            Assert.False(list.MoveChange(1, 3));
            Assert.True(list.MoveChange(4, 5));
            Assert.Equal(2, list.IndexOf(5));
            Assert.True(list.Contains(5));

            list.SetChange(1, [7, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
            Assert.Equal(7, list.Changes[1][0]);
            list.RemoveChangeAt(2);
            Assert.Equal(2, list.Count);
            list.AddSound("a.wav");
            list.Clear();
            Assert.Single(list.Paths);
            Assert.Equal([0f], list.Times);
        }

        [Fact]
        public void ToSCDData_WritesChangedButtons()
        {
            var list = Create();
            list.AddChange(1, [1, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
            list.AddChange(2, [1, 2, 0, 0, 0, 0, 0, 0, 0, 0]);

            var scd = list.ToSCDData();

            Assert.Equal([(1f, ButtonType.Square, (ushort)1), (2f, ButtonType.Cross, (ushort)2)], scd.Select(d => (d.Time, d.ButtonType, d.SoundIndex)));
        }

        [Fact]
        public void RemoveSoundFromFile_ShiftsOtherDifficulty()
        {
            using var folder = new TempFolder();
            var path = folder.Combine("Hard.scd");
            SoundChangeList.RemoveSoundFromFile(path, 1);
            Assert.False(File.Exists(path));
            SCDWriter.Write(path, [new SCDData(1, ButtonType.Square, 1), new SCDData(2, ButtonType.Cross, 3)]);

            SoundChangeList.RemoveSoundFromFile(path, 1);

            Assert.Equal([(ushort)0, (ushort)2], SCDReader.Read(path).Select(d => d.SoundIndex));
        }
    }

    public class ProjectFilesTests
    {
        [Fact]
        public void PathsVersionAndDifficulty()
        {
            using var folder = new TempFolder();
            folder.Write("old.ppdproj", "[bpm]120\n");
            folder.Write("new.ppdproj", "[version]2\n");
            Directory.CreateDirectory(folder.Combine("new", "Base_layers"));
            Directory.CreateDirectory(folder.Combine("new", "Hard_layers"));

            Assert.Equal(folder.Combine("new"), ProjectFiles.GetProjectDir(folder.Combine("new.ppdproj")));
            Assert.Equal(1, ProjectFiles.GetVersion(folder.Combine("old.ppdproj")));
            Assert.Equal(2, ProjectFiles.GetVersion(folder.Combine("new.ppdproj")));
            Assert.Equal(AvailableDifficulty.Base | AvailableDifficulty.Hard, ProjectFiles.GetAvailableDifficulty(folder.Combine("new")));
        }

        [Theory]
        [InlineData("Base", AvailableDifficulty.Base)]
        [InlineData("EASY", AvailableDifficulty.Easy)]
        [InlineData("normal", AvailableDifficulty.Normal)]
        [InlineData("Hard", AvailableDifficulty.Hard)]
        [InlineData("extreme", AvailableDifficulty.Extreme)]
        [InlineData("other", AvailableDifficulty.None)]
        public void ParseDifficulty(string text, AvailableDifficulty expected)
        {
            Assert.Equal(expected, ProjectFiles.ParseDifficulty(text));
        }

        [Fact]
        public void BackUp_CopiesProjectWithoutMovie()
        {
            using var folder = new TempFolder();
            folder.Write("song.ppdproj", "[version]2\n");
            folder.Write(Path.Combine("song", "data.ini"), "data");
            folder.Write(Path.Combine("song", "movie.mp4"), "movie");

            ProjectFiles.BackUp(folder.Combine("song.ppdproj"));

            Assert.True(File.Exists(folder.Combine("song_backup", "song.ppdproj")));
            Assert.True(File.Exists(folder.Combine("song_backup", "song", "data.ini")));
            Assert.False(File.Exists(folder.Combine("song_backup", "song", "movie.mp4")));
        }

        [Fact]
        public void Convert_Version1WithoutLayers_MakesBaseLayer()
        {
            using var folder = new TempFolder();
            var path = folder.Combine("song.ppdproj");
            folder.Write("song.ppdproj", "[bpm]120\n[bpmstart]0.5\n");
            folder.Write(Path.Combine("song", "Base.ppd"), "ppd");
            folder.Write(Path.Combine("song", "Base.evd"), "evd");

            ProjectFiles.Convert(path, 1);

            var setting = new SettingReader(File.ReadAllText(path));
            Assert.Equal("120", setting["Base.bpm"]);
            Assert.Equal("120", setting["Base.layer0BPM"]);
            Assert.Equal("0.5", setting["Base.layer0BPMOffset"]);
            Assert.Equal("2", setting["version"]);
            Assert.Equal("Base", setting["selecteddifficulty"]);
            Assert.Equal("ppd", File.ReadAllText(folder.Combine("song", "Base_layers", "layer0.ppd")));
            // ppd と evd は 1 つのパックにまとめる
            Assert.False(File.Exists(folder.Combine("song", "Base.evd")));
            Assert.Equal(1, ProjectFiles.GetVersion(path) - 1);
            Assert.Equal(["evd", "ppd"], ReadPack(folder.Combine("song", "Base.ppd")).OrderBy(n => n));
        }

        [Fact]
        public void Convert_Version1WithLayers_RenamesFolder()
        {
            using var folder = new TempFolder();
            var path = folder.Combine("song.ppdproj");
            folder.Write("song.ppdproj", "[bpm]120\n");
            folder.Write(Path.Combine("song", "layers", "layer0.ppd"), "layer");

            ProjectFiles.Convert(path, 1);
            ProjectFiles.Convert(path, 2);

            Assert.Equal("layer", File.ReadAllText(folder.Combine("song", "Base_layers", "layer0.ppd")));
            Assert.False(Directory.Exists(folder.Combine("song", "layers")));
            Assert.Equal("", new SettingReader(File.ReadAllText(path))["Base.layer0"]);
        }

        private static List<string> ReadPack(string path)
        {
            using var reader = new PPDPack.PackReader(path);
            return [.. reader.FileList];
        }

        // マージに使うプロジェクトを作る
        private static string CreateProject(TempFolder folder, string name, string difficulty, bool withMovie)
        {
            folder.Write(name + ".ppdproj", $"[version]2\n[{difficulty}.layer0]Layer0\n[{difficulty}.layer0BPM]{name.Length * 10}\n[other]x\n");
            var dir = Path.Combine(name, "");
            folder.Write(Path.Combine(name, difficulty + "_layers", "layer0.ppd"), name);
            folder.Write(Path.Combine(name, difficulty + ".ppd"), name + " ppd");
            folder.Write(Path.Combine(name, difficulty + ".scd"), name + " scd");
            folder.Write(Path.Combine(name, difficulty + "_Scripts", "a.fsml"), "script");
            folder.Write(Path.Combine(name, "Resource", "r.png"), "r");
            folder.Write(Path.Combine(name, "sound", "s.wav"), "s");
            folder.Write(Path.Combine(name, "data.ini"), name + " data");
            folder.Write(Path.Combine(name, "kasi.txt"), "");
            folder.Write(Path.Combine(name, "soundset.txt"), "");
            if (withMovie)
            {
                folder.Write(Path.Combine(name, "movie.mp4"), "movie");
            }
            return folder.Combine(name + ".ppdproj");
        }

        [Fact]
        public void Merge_CombinesDifficultiesAndCleansUpTemp()
        {
            using var folder = new TempFolder();
            using var temp = new TempFolder();
            var first = CreateProject(folder, "first", "Base", withMovie: true);
            var second = CreateProject(folder, "second", "Hard", withMovie: false);
            var dest = folder.Combine("out", "merged.ppdproj");
            Directory.CreateDirectory(folder.Combine("out"));
            var infos = new Dictionary<AvailableDifficulty, MergeInfo>
            {
                [AvailableDifficulty.Easy] = new(first, 2, AvailableDifficulty.Base),
                [AvailableDifficulty.Extreme] = new(second, 2, AvailableDifficulty.Hard),
            };
            var previousTemp = Environment.GetEnvironmentVariable("TMP");
            Environment.SetEnvironmentVariable("TMP", temp.Path);
            try
            {
                ProjectFiles.Merge(AvailableDifficulty.Easy | AvailableDifficulty.Extreme, d => infos[d], AvailableDifficulty.Easy, dest);
            }
            finally
            {
                Environment.SetEnvironmentVariable("TMP", previousTemp);
            }

            var setting = new SettingReader(File.ReadAllText(dest));
            Assert.Equal(("Layer0", "50"), (setting["Easy.layer0"], setting["Easy.layer0BPM"]));
            Assert.Equal("60", setting["Extreme.layer0BPM"]);
            Assert.Equal("", setting["other"]);
            Assert.Equal("first ppd", File.ReadAllText(folder.Combine("out", "merged", "Easy.ppd")));
            Assert.Equal("second", File.ReadAllText(folder.Combine("out", "merged", "Extreme_layers", "layer0.ppd")));
            Assert.True(File.Exists(folder.Combine("out", "merged", "Extreme_Scripts", "a.fsml")));
            // 歌詞や音、動画は基にする難易度のプロジェクトから持ってくる
            Assert.Equal("first data", File.ReadAllText(folder.Combine("out", "merged", "data.ini")));
            Assert.True(File.Exists(folder.Combine("out", "merged", "movie.mp4")));
            Assert.True(File.Exists(folder.Combine("out", "merged", "Resource", "r.png")));
            // 作業用の一時ファイルとフォルダーは残さない
            Assert.Empty(Directory.GetFileSystemEntries(temp.Path));
        }

        [Fact]
        public void Merge_OldProject_IsBackedUpAndConverted()
        {
            using var folder = new TempFolder();
            folder.Write("old.ppdproj", "[bpm]120\n");
            folder.Write(Path.Combine("old", "layers", "layer0.ppd"), "old layer");
            folder.Write(Path.Combine("old", "Base.ppd"), "ppd");
            folder.Write(Path.Combine("old", "Base.scd"), "scd");
            folder.Write(Path.Combine("old", "sound", "s.wav"), "s");
            folder.Write(Path.Combine("old", "data.ini"), "data");
            folder.Write(Path.Combine("old", "kasi.txt"), "");
            folder.Write(Path.Combine("old", "soundset.txt"), "");
            var dest = folder.Combine("out", "merged.ppdproj");
            Directory.CreateDirectory(folder.Combine("out"));

            ProjectFiles.Merge(AvailableDifficulty.Normal, d => new MergeInfo(folder.Combine("old.ppdproj"), 1, AvailableDifficulty.Base), AvailableDifficulty.Normal, dest);

            Assert.True(File.Exists(folder.Combine("old_backup", "old.ppdproj")));
            Assert.Equal(2, ProjectFiles.GetVersion(folder.Combine("old.ppdproj")));
            Assert.Equal("old layer", File.ReadAllText(folder.Combine("out", "merged", "Normal_layers", "layer0.ppd")));
            Assert.Equal("120", new SettingReader(File.ReadAllText(dest))["Normal.bpm"]);
        }

        [Fact]
        public void Publish_CopiesPlayableFiles()
        {
            using var folder = new TempFolder();
            foreach (var name in new[] { "data.ini", "kasi.txt", "soundset.txt", "resource.pak", "Base.ppd", "Base.scd", "Hard.ppd", "Hard.scd", "project-only.txt" })
            {
                folder.Write(Path.Combine("project", name), name);
            }
            folder.Write(Path.Combine("project", "sound", "s.wav"), "s");
            folder.Write(Path.Combine("dest", "stale.txt"), "old");

            ProjectFiles.Publish(folder.Combine("project"), folder.Combine("dest"), AvailableDifficulty.Hard);

            var published = Directory.GetFiles(folder.Combine("dest"), "*", SearchOption.AllDirectories).Select(p => Path.GetRelativePath(folder.Combine("dest"), p)).OrderBy(p => p);
            Assert.Equal(["data.ini", "Hard.ppd", "Hard.scd", "kasi.txt", "resource.pak", Path.Combine("sound", "s.wav"), "soundset.txt"], published.OrderBy(p => p, StringComparer.Ordinal).ToArray().OrderBy(p => p));
        }

        [Fact]
        public void EnsureDirectoryAndDeleteIfExists()
        {
            using var folder = new TempFolder();
            ProjectFiles.EnsureDirectory(folder.Combine("a"));
            ProjectFiles.EnsureDirectory(folder.Combine("a"));
            folder.Write("f.txt", "");

            ProjectFiles.DeleteIfExists(folder.Combine("f.txt"));
            ProjectFiles.DeleteIfExists(folder.Combine("f.txt"));

            Assert.True(Directory.Exists(folder.Combine("a")));
            Assert.False(File.Exists(folder.Combine("f.txt")));
        }
    }

    public class ParameterPresetStoreTests
    {
        [Fact]
        public void SaveThenLoad_KeepsFoldersAndParameters()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            Directory.CreateDirectory(Path.Combine(ParameterPresetStore.PresetDir, "sub"));
            ParameterPresetStore.Save(Path.Combine(ParameterPresetStore.PresetDir, "top.xml"), [new("a", "1"), new("b", "2")]);
            ParameterPresetStore.Save(Path.Combine(ParameterPresetStore.PresetDir, "sub", "inner.xml"), [new("c", "3")]);
            File.WriteAllText(Path.Combine(ParameterPresetStore.PresetDir, "broken.xml"), "not xml");
            var errors = 0;

            var root = ParameterPresetStore.Load(() => errors++);

            Assert.Equal(1, errors);
            var top = Assert.Single(root.Presets);
            Assert.Equal("top", top.PresetName);
            Assert.Equal([new("a", "1"), new("b", "2")], top.Parameters);
            Assert.Equal("inner", Assert.Single(Assert.Single(root.Folders).Presets).PresetName);
            Assert.Equal(2, root.GetAllPresets().Count());
        }

        [Fact]
        public void Load_CreatesFolder()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);

            var root = ParameterPresetStore.Load(() => { });

            Assert.True(Directory.Exists(ParameterPresetStore.PresetDir));
            Assert.Empty(root.GetAllPresets());
        }
    }

    public class PosAndAngleTransformTests
    {
        static readonly PosAndAngleInfo[] infos = [new PosAndAngleInfo(new Vector2(100, 100), 0.5f), new PosAndAngleInfo(1.5f)];

        [Fact]
        public void None_KeepsPositions()
        {
            var (positions, angles) = PosAndAngleTransform.Apply(infos, PosAndAngleApplyMode.None, default, default, new Vector2(1, 0), false, true);

            Assert.Equal([new Vector2(100, 100), Vector2.Zero], positions);
            Assert.Equal(0.5f, angles[0], 4);
        }

        [Fact]
        public void Point_MirrorsAroundCenter()
        {
            var (positions, angles) = PosAndAngleTransform.Apply(infos, PosAndAngleApplyMode.Point, new Vector2(400, 225), default, new Vector2(1, 0), true, true);

            Assert.Equal(new Vector2(700, 350), positions[0]);
            // 角度も反対向きにする
            Assert.Equal(0.5f + MathF.PI, angles[0], 4);
            // 画面の外は画面の中に収める
            Assert.Equal(new Vector2(800, 450), positions[1]);
        }

        [Fact]
        public void Line_MirrorsAcrossVerticalLine()
        {
            var (positions, _) = PosAndAngleTransform.Apply(infos, PosAndAngleApplyMode.Line, default, new Vector2(400, 0), new Vector2(400, 450), false, false);

            Assert.Equal(700, positions[0].X, 3);
            Assert.Equal(100, positions[0].Y, 3);
        }

        [Fact]
        public void WithoutAngle_AnglesAreZero()
        {
            var (_, angles) = PosAndAngleTransform.Apply(infos, PosAndAngleApplyMode.Line, default, new Vector2(0, 225), new Vector2(800, 225), true, false);

            Assert.All(angles, a => Assert.Equal(0, a));
        }

        [Fact]
        public void Line_SymmetricAngle_IsNormalized()
        {
            var (positions, angles) = PosAndAngleTransform.Apply(infos, PosAndAngleApplyMode.Line, default, new Vector2(0, 225), new Vector2(800, 225), true, true);

            Assert.Equal(350, positions[0].Y, 3);
            Assert.InRange(angles[0], 0, MathF.PI * 2);
        }
    }

    public class GeometryPlacementTests
    {
        static readonly BezierControlPoint[] line = [new BezierControlPoint { Second = new PointF(0, 0) }, new BezierControlPoint { Second = new PointF(300, 0) }];

        [Fact]
        public void TooFewPointsOrMarks_ReturnsZeros()
        {
            var (positions, _) = GeometryPlacement.Compute([line[0]], [0, 1, 2], GeometryIntervalMode.Even1, GeometryAngleMode.Tangent1, 120, 50);
            Assert.All(positions, p => Assert.Equal(Vector2.Zero, p));

            Assert.Single(GeometryPlacement.Compute(line, [1], GeometryIntervalMode.Even1, GeometryAngleMode.Tangent1, 120, 50).Positions);
            Assert.Empty(GeometryPlacement.Compute(line, [1, 1], GeometryIntervalMode.Time, GeometryAngleMode.Tangent1, 120, 50).Positions);
        }

        [Theory]
        [InlineData(GeometryIntervalMode.Time, new float[] { 0, 75, 300 })]
        [InlineData(GeometryIntervalMode.Even1, new float[] { 0, 150, 300 })]
        [InlineData(GeometryIntervalMode.Even2, new float[] { 0, 100, 200 })]
        [InlineData(GeometryIntervalMode.FixedDistance, new float[] { 0, 50, 200 })]
        public void IntervalModes(GeometryIntervalMode mode, float[] expectedX)
        {
            // BPM 60 なので 4 分音符は 1 秒、1 秒で 50 進む
            var (positions, _) = GeometryPlacement.Compute(line, [0, 1, 4], mode, GeometryAngleMode.Tangent1, 60, 50);

            Assert.Equal(expectedX.Length, positions.Length);
            for (int i = 0; i < expectedX.Length; i++)
            {
                Assert.Equal(expectedX[i], positions[i].X, 0);
                Assert.Equal(0, positions[i].Y, 3);
            }
        }

        [Theory]
        [InlineData(GeometryAngleMode.Tangent1, 1, 0)]
        [InlineData(GeometryAngleMode.Tangent2, -1, 0)]
        [InlineData(GeometryAngleMode.Normal1, 0, 1)]
        [InlineData(GeometryAngleMode.Normal2, 0, -1)]
        [InlineData(GeometryAngleMode.None, 1, 0)]
        public void AngleModes(GeometryAngleMode mode, float x, float y)
        {
            var (_, directions) = GeometryPlacement.Compute(line, [0, 1], GeometryIntervalMode.Even1, mode, 120, 50);

            var dir = Vector2.Normalize(directions[0]);
            Assert.Equal(x, dir.X, 3);
            Assert.Equal(y, dir.Y, 3);
        }
    }
}
