using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using UnitTests.PPDEditor;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// 譜面のデータベース（PPDDatabase）を一時フォルダーに作ります。終わったら、設定とデータベースを元に戻します。
    /// </summary>
    /// <remarks>
    /// 設定（PPDSetting）、データベース、譜面とロジックフォルダーの一覧は、プロセス全体で 1 つなので、入れ替えて使う。
    /// 一時フォルダーを今のフォルダーにするので、PPD.ini は読まれない（デバッグの設定で、PPD.Debug.db を作る）。
    /// </remarks>
    sealed class SongDatabase : IDisposable
    {
        static readonly (Type Type, string Name)[] statics =
        [
            (typeof(PPDSetting), "setting"),
            (typeof(PPDDatabase), "db"),
            (typeof(SongInformation), "root"),
            (typeof(LogicFolderInfomation), "root"),
        ];

        readonly object[] saved;

        public TempFolder Folder { get; } = new TempFolder(setCurrentDirectory: true);

        public string SongDir => Folder.Combine("songs");

        public SongDatabase(Action<string> prepare = null)
        {
            Directory.CreateDirectory(SongDir);
            // 設定がまだなければ、ここ（PPD.ini のない一時フォルダー）で作っておく。null に戻すと、後で別のフォルダーの PPD.ini から作られることがある
            _ = PPDSetting.Setting;
            saved = [.. statics.Select(s => Field(s).GetValue(null))];
            foreach (var s in statics)
            {
                Field(s).SetValue(null, null);
            }
            PPDSetting.Initialize(true, SongDir);
            prepare?.Invoke(Folder.Combine("PPD.Debug.db"));
        }

        static FieldInfo Field((Type Type, string Name) s) => s.Type.GetField(s.Name, BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 譜面のフォルダーを書きます（data.ini と、難易度ごとの譜面のパック）。
        /// </summary>
        public string AddSong(string path, string guid, int bpm, params (string Difficulty, IEVDData[] Events)[] difficulties)
        {
            var dir = Path.Combine(SongDir, path);
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "data.ini"), string.Join("\n",
                "[formattype]",
                "bpm=" + bpm,
                "bpmstring=" + bpm + "-" + (bpm * 2),
                "difficulty easy=★1",
                "difficulty hard=★7",
                "authorname=作者",
                "guid=" + guid,
                "moviecutleft=1",
                "moviecutright=2",
                "moviecuttop=3",
                "moviecutbottom=4",
                "thumbtimestart=5",
                "thumbtimeend=6",
                "start=7",
                "end=8"));
            foreach (var (difficulty, events) in difficulties)
            {
                // ハッシュが譜面ごとに変わるように、マークの時間を変える
                ChartFiles.WritePack(Path.Combine(dir, difficulty + ".ppd"),
                    [ChartFiles.Mark(path.Length + difficulty.Length, ButtonType.Circle)], events ?? []);
            }
            return dir;
        }

        public static SongInformation Song(string name) => SongInformation.All.Single(s => s.DirectoryName == name);

        public void Dispose()
        {
            var db = Field(statics[1]).GetValue(null) as PPDDatabase;
            db?.Close();
            SQLiteConnection.ClearAllPools();
            for (var i = 0; i < statics.Length; i++)
            {
                Field(statics[i]).SetValue(null, saved[i]);
            }
            Folder.Dispose();
        }
    }

    /// <summary>
    /// 譜面のデータベース（譜面の一覧、スコア、ロジックフォルダー、譜面ごとの保存領域）のテストです。
    /// </summary>
    public class SongDatabaseTests
    {
        static readonly int[] counts = [10, 2, 1, 0, 0];

        [Fact]
        public void Update_ReadsSongsAndFolders()
        {
            using var database = new SongDatabase();
            var a = database.AddSong("a", "guid-a", 150,
                ("Easy", [new ChangeVolumeEvent(0, 90, 0)]),
                ("Hard", [new ChangeNoteTypeEvent(0, NoteType.AC)]));
            File.WriteAllText(Path.Combine(a, "movie.mp4"), "movie");
            database.AddSong(Path.Combine("group", "b"), "guid-b", 120, ("Normal", null), ("Extreme", [new ChangeNoteTypeEvent(0, NoteType.ACFT)]));
            Directory.CreateDirectory(Path.Combine(database.SongDir, "empty"));
            var updated = 0;
            void onUpdated(object s, EventArgs e) => updated++;
            SongInformation.Updated += onUpdated;
            try
            {
                SongInformation.Update();
            }
            finally
            {
                SongInformation.Updated -= onUpdated;
            }

            Assert.Equal(1, updated);
            // 譜面のないフォルダーは出ない。フォルダーが先
            Assert.Equal(["group", "a"], SongInformation.Root.Children.Select(s => s.DirectoryName));
            var group = SongInformation.Root.Children[0];
            Assert.False(group.IsPPDSong);
            Assert.Equal(1, group.ChildrenCount);
            Assert.Same(group, group.Children[0].Parent);
            Assert.Equal(["a", "b"], SongInformation.All.Select(s => s.DirectoryName).Order());

            var song = SongDatabase.Song("a");
            Assert.True(song.IsPPDSong);
            Assert.Equal(a, song.DirectoryPath);
            Assert.Equal(database.SongDir, song.ParentDirectory);
            Assert.Equal(SongInformation.AvailableDifficulty.Easy | SongInformation.AvailableDifficulty.Hard, song.Difficulty);
            Assert.Equal((150f, "150-300", "作者", "guid-a"), (song.BPM, song.BPMString, song.AuthorName, song.GUID));
            Assert.Equal("150-300", song.GetBPMString());
            Assert.Equal(("★1", "", "★7", ""), (song.GetDifficultyString(Difficulty.Easy), song.GetDifficultyString(Difficulty.Normal),
                song.GetDifficultyString(Difficulty.Hard), song.GetDifficultyString(Difficulty.Other)));
            Assert.Equal((1f, 2f, 3f, 4f), (song.TrimmingData.Left, song.TrimmingData.Right, song.TrimmingData.Top, song.TrimmingData.Bottom));
            Assert.Equal((5f, 6f, 7f, 8f), (song.ThumbStartTime, song.ThumbEndTime, song.StartTime, song.EndTime));
            Assert.Equal(Path.Combine(a, "movie.mp4"), song.MoviePath);
            Assert.False(song.IsOld);
            Assert.Equal(100, song.UserVolume);
            // 動画の音量は、最初の音量の変更（チャンネル 0）から
            Assert.Equal(-1000, song.MovieVolume);
            Assert.Equal(NoteType.AC, song.GetNoteType(Difficulty.Hard));
            Assert.True(song.GetIsAC(Difficulty.Hard) && song.IsHardAC && song.HasAC);
            Assert.False(song.HasACFT || song.GetIsACFT(Difficulty.Hard) || song.GetIsAC(Difficulty.Other));
            Assert.Equal(NoteType.Normal, song.GetNoteType(Difficulty.Other));
            Assert.Equal("0000000", song.GetScore(Difficulty.Easy));
            Assert.Equal("0", song.GetScore(Difficulty.Other));

            // ハッシュで探す
            Assert.Equal(32, song.EasyHash.Length);
            Assert.Null(song.NormalHash);
            Assert.Same(song.EasyHash, song.GetPrimaryHash());
            Assert.Same(song.HardHash, song.GetScoreHash(Difficulty.Hard));
            Assert.Null(song.GetScoreHash(Difficulty.Other));
            Assert.Equal(Difficulty.Hard, song.GetDifficulty(CryptographyUtility.Getx2Encoding(song.HardHash)));
            Assert.Equal(Difficulty.Other, song.GetDifficulty("none"));
            Assert.Same(song, SongInformation.FindSongInformationByHash(song.HardHash));
            Assert.Same(song, SongInformation.FindSongInformationByHash(song.HardHash, Difficulty.Hard));
            Assert.Null(SongInformation.FindSongInformationByHash(song.HardHash, Difficulty.Easy));
            Assert.Same(song, SongInformation.FindSongInformationByID(song.ID));
            Assert.Null(SongInformation.FindSongInformationByID(-5));

            var b = SongDatabase.Song("b");
            Assert.Same(b.NormalHash, b.GetPrimaryHash());
            Assert.Equal(Difficulty.Extreme, b.GetDifficulty(CryptographyUtility.Getx2Encoding(b.ExtremeHash)));
            Assert.True(b.IsExtremeACFT && b.HasACFT && b.GetIsACFT(Difficulty.Extreme));
            Assert.Same(b, SongInformation.FindSongInformationByHash(b.ExtremeHash, Difficulty.Extreme));
            Assert.NotNull(b.CalculateDifficulty(Difficulty.Normal).Data);
        }

        [Fact]
        public void EachDifficulty_HashAndNoteType()
        {
            using var database = new SongDatabase();
            database.AddSong("all", "guid", 120,
                ("Easy", [new ChangeNoteTypeEvent(0, NoteType.AC)]),
                ("Normal", [new ChangeNoteTypeEvent(0, NoteType.ACFT)]),
                ("Hard", null),
                ("Extreme", [new ChangeNoteTypeEvent(0, NoteType.Normal), new ChangeNoteTypeEvent(1, NoteType.AC)]));
            database.AddSong("hard", "guid-h", 120, ("Hard", null));
            database.AddSong("extreme", "guid-x", 120, ("Extreme", [new ChangeNoteTypeEvent(0, NoteType.AC)]));
            SongInformation.Update();

            var song = SongDatabase.Song("all");
            var difficulties = new[] { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard, Difficulty.Extreme };
            var hashes = new[] { song.EasyHash, song.NormalHash, song.HardHash, song.ExtremeHash };
            for (var i = 0; i < 4; i++)
            {
                Assert.Same(hashes[i], song.GetScoreHash(difficulties[i]));
                Assert.Equal(difficulties[i], song.GetDifficulty(CryptographyUtility.Getx2Encoding(hashes[i])));
                Assert.Same(song, SongInformation.FindSongInformationByHash(hashes[i], difficulties[i]));
            }
            // 最初の AC か ACFT の変更で決める
            Assert.Equal([NoteType.AC, NoteType.ACFT, NoteType.Normal, NoteType.AC], difficulties.Select(song.GetNoteType));
            Assert.Equal([true, false, false, true], difficulties.Select(song.GetIsAC));
            Assert.Equal([false, true, false, false], difficulties.Select(song.GetIsACFT));
            // 普通の譜面（AC でも ACFT でもない）はハードだけ
            Assert.True(song.HasNormal);
            Assert.False(SongDatabase.Song("extreme").HasNormal);

            // 主なハッシュは、易しい難易度から
            Assert.Same(song.EasyHash, song.GetPrimaryHash());
            Assert.Same(SongDatabase.Song("hard").HardHash, SongDatabase.Song("hard").GetPrimaryHash());
            Assert.Same(SongDatabase.Song("extreme").ExtremeHash, SongDatabase.Song("extreme").GetPrimaryHash());
            Assert.Null(SongInformation.Root.GetPrimaryHash());

            Assert.Equal(Difficulty.Other, SongInformation.ConvertAvailable(SongInformation.AvailableDifficulty.None));
            Assert.Equal(SongInformation.AvailableDifficulty.None, SongInformation.ConvertDifficulty(Difficulty.Other));
        }

        [Fact]
        public void UserVolume_IsBetween0And200()
        {
            var song = new SongInformation { UserVolume = 250 };
            Assert.Equal(200, song.UserVolume);
            song.UserVolume = -5;
            Assert.Equal(0, song.UserVolume);
        }

        [Fact]
        public void Comparer_PutsFoldersFirstThenNames()
        {
            using var database = new SongDatabase();
            database.AddSong("b", "guid-b", 120, ("Easy", null));
            database.AddSong("a", "guid-a", 120, ("Easy", null));
            database.AddSong(Path.Combine("z", "c"), "guid-c", 120, ("Easy", null));
            database.AddSong(Path.Combine("y", "d"), "guid-d", 120, ("Easy", null));
            SongInformation.Update();
            var comparer = new SongInformationComparer();
            var folders = SongInformation.Root.Children.Where(s => !s.IsPPDSong).ToArray();
            var songs = SongInformation.Root.Children.Where(s => s.IsPPDSong).ToArray();

            Assert.Equal(-1, comparer.Compare(null, songs[0]));
            Assert.Equal(1, comparer.Compare(songs[0], null));
            Assert.Equal(-1, comparer.Compare(folders[0], songs[0]));
            Assert.Equal(1, comparer.Compare(songs[0], folders[0]));
            Assert.Equal(["y", "z", "a", "b"], SongInformation.Root.Children.Order(comparer).Select(s => s.DirectoryName));
        }

        [Fact]
        public void MoviePath_FallsBackToMovieAndMusicExtensions()
        {
            using var database = new SongDatabase();
            // 拡張子は PPD.ini から読む（テストには PPD.ini がない）
            static void Set(string name, string[] value) => typeof(PPDSetting).GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(PPDSetting.Setting, value);
            Set(nameof(PPDSetting.MovieExtensions), ["avi"]);
            Set(nameof(PPDSetting.MusicExtensions), ["ogg"]);
            var video = database.AddSong("video", "guid-v", 120, ("Easy", null));
            File.WriteAllText(Path.Combine(video, "clip.avi"), "");
            File.WriteAllText(Path.Combine(video, "song.ogg"), "");
            var music = database.AddSong("music", "guid-m", 120, ("Easy", null));
            File.WriteAllText(Path.Combine(music, "song.ogg"), "");
            // 拡張子が途中で切れている名前（.oggx）は違う
            File.WriteAllText(Path.Combine(music, "other.oggx"), "");

            // 動画の拡張子が先
            Assert.Equal(Path.Combine(video, "clip.avi"), SongInformation.ReadData(video).MoviePath);
            Assert.Equal(Path.Combine(music, "song.ogg"), SongInformation.ReadData(music).MoviePath);
        }

        [Fact]
        public void ReadData_ReadsFolderWithoutDatabase()
        {
            using var database = new SongDatabase();
            var dir = database.AddSong("only", "guid", 90, ("Normal", null));

            var song = SongInformation.ReadData(dir);

            Assert.Equal(-1, song.ID);
            Assert.Equal(SongInformation.AvailableDifficulty.Normal, song.Difficulty);
            Assert.Equal(90f, song.BPM);
            // 動画の音量の変更がなければ -1000
            Assert.Equal(-1000, song.MovieVolume);
            Assert.Equal("", song.MoviePath);
            // データベースにない譜面の音量は保存しない
            song.UserVolume = 50;
            song.SaveUserVolume();
        }

        [Fact]
        public void Update_SeesChangedAndRemovedSongs()
        {
            using var database = new SongDatabase();
            var dir = database.AddSong("a", "guid", 100, ("Easy", null));
            database.AddSong("b", "guid-b", 100, ("Easy", null));
            SongInformation.Update();
            var id = SongDatabase.Song("a").ID;

            // data.ini が新しくなったら読み直す（ID は同じ）
            File.WriteAllText(Path.Combine(dir, "data.ini"), "[formattype]\nbpm=200\nguid=guid\n");
            File.SetLastWriteTime(Path.Combine(dir, "data.ini"), DateTime.Now.AddMinutes(1));
            Directory.Delete(Path.Combine(database.SongDir, "b"), true);
            SongInformation.Update();

            Assert.Equal(200f, SongDatabase.Song("a").BPM);
            Assert.Equal(id, SongDatabase.Song("a").ID);
            Assert.Equal(["a"], SongInformation.All.Select(s => s.DirectoryName));

            // 指定した譜面は、日付が同じでも読み直す
            File.WriteAllText(Path.Combine(dir, "data.ini"), "[formattype]\nbpm=300\nguid=guid\n");
            File.SetLastWriteTime(Path.Combine(dir, "data.ini"), DateTime.Now.AddMinutes(1));
            File.SetCreationTime(Path.Combine(dir, "data.ini"), DateTime.Now.AddMinutes(1));
            SongInformation.Update([SongDatabase.Song("a")]);
            Assert.Equal(300f, SongDatabase.Song("a").BPM);
        }

        [Fact]
        public void WriteScore_KeepsTheBest()
        {
            using var database = new SongDatabase();
            database.AddSong("a", "guid", 100, ("Easy", null), ("Hard", null));
            SongInformation.Update();
            var song = SongDatabase.Song("a");

            bool Write(int score, int[] c, int combo = 5, ResultEvaluateType type = ResultEvaluateType.Great) =>
                ScoreReaderWriter.WriteScore(song.ID, Difficulty.Easy, c, combo, score, type, 60, song);

            Assert.True(Write(1000, counts));
            Assert.Equal("0001000", song.GetScore(Difficulty.Easy));
            Assert.False(Write(900, counts));
            // 同じ点数なら、良い判定の数、最大コンボの順で比べる
            Assert.True(Write(1000, [11, 1, 1, 0, 0]));
            Assert.False(Write(1000, [11, 0, 2, 0, 0]));
            Assert.False(Write(1000, [11, 1, 1, 0, 0]));
            Assert.True(Write(1000, [11, 1, 1, 0, 0], combo: 6));
            // ミステイクは自己ベストにしない
            Assert.False(Write(5000, counts, type: ResultEvaluateType.Mistake));
            Assert.Equal("0001000", song.GetScore(Difficulty.Easy));
            Assert.Equal("0000000", song.GetScore(Difficulty.Hard));

            var results = PPDDatabase.DB.GetResults(song.ID);
            Assert.Equal(7, results.Length);
            Assert.All(results, r => Assert.Equal((song.ID, Difficulty.Easy, 60f), (r.ScoreID, r.Difficulty, r.FinishTime)));
            var mistake = Assert.Single(results, r => r.Score == 5000);
            Assert.Equal(ResultEvaluateType.Mistake, mistake.ResultEvaluate);
            Assert.Equal((10, 2, 1, 0, 0, 5), (mistake.CoolCount, mistake.GoodCount, mistake.SafeCount, mistake.SadCount, mistake.WorstCount, mistake.MaxCombo));

            PPDDatabase.DB.DeleteResult(mistake.ID);
            Assert.Equal(6, PPDDatabase.DB.GetResults(song.ID).Length);
            Assert.False(PPDDatabase.DB.FindScore(mistake.ID, out var none));
            Assert.Equal(0, none);

            // 読み直しても自己ベストは残る
            SongInformation.Update();
            Assert.Equal("0001000", SongDatabase.Song("a").GetScore(Difficulty.Easy));
        }

        [Fact]
        public void MovedSong_KeepsResultsAndSettings()
        {
            using var database = new SongDatabase();
            var dir = database.AddSong("a", "guid", 100, ("Easy", null));
            var noGuid = database.AddSong("c", "", 100, ("Normal", null));
            SongInformation.Update();
            var song = SongDatabase.Song("a");
            ScoreReaderWriter.WriteScore(song.ID, Difficulty.Easy, counts, 5, 1234, ResultEvaluateType.Great, 60, song);
            song.UpdateLatency(0.25f);
            song.UserVolume = 70;
            song.SaveUserVolume();
            var storage = PPDDatabase.DB.GetScoreStorage(song.ID);
            storage["k"] = "v";
            PPDDatabase.DB.SaveScoreStorage(storage);
            var link = LogicFolderInfomation.Root.AddScore(song);
            var other = SongDatabase.Song("c");
            ScoreReaderWriter.WriteScore(other.ID, Difficulty.Normal, counts, 5, 555, ResultEvaluateType.Great, 60, other);

            // GUID が同じ譜面、GUID がなければハッシュが同じ譜面に、結果と設定を移す
            Directory.Move(dir, Path.Combine(database.SongDir, "moved"));
            Directory.Move(noGuid, Path.Combine(database.SongDir, "moved2"));
            SongInformation.Update();

            var moved = SongDatabase.Song("moved");
            Assert.NotEqual(song.ID, moved.ID);
            Assert.Equal("0001234", moved.GetScore(Difficulty.Easy));
            Assert.Equal(0.25f, moved.Latency);
            Assert.Equal(70, moved.UserVolume);
            Assert.Equal("v", PPDDatabase.DB.GetScoreStorage(moved.ID)["k"]);
            Assert.Single(PPDDatabase.DB.GetResults(moved.ID));
            Assert.Equal("0000555", SongDatabase.Song("moved2").GetScore(Difficulty.Normal));
            Assert.Equal(moved.ID, Assert.Single(LogicFolderInfomation.All).ScoreID);
            Assert.Equal(link.ID, LogicFolderInfomation.All[0].ID);

            // 消えた譜面へのリンクは消える
            Directory.Delete(Path.Combine(database.SongDir, "moved"), true);
            SongInformation.Update();
            Assert.Empty(LogicFolderInfomation.All);
        }

        [Fact]
        public void LogicFolders_AddMoveRenameRemove()
        {
            using var database = new SongDatabase();
            database.AddSong("a", "guid-a", 100, ("Easy", null));
            database.AddSong("b", "guid-b", 100, ("Easy", null));
            SongInformation.Update();
            var (a, b) = (SongDatabase.Song("a"), SongDatabase.Song("b"));
            var added = new List<LogicFolderInfomation>();
            var removed = new List<LogicFolderInfomation>();
            var moved = new List<LogicFolderInfomation>();
            void onAdd(object s, EventArgs e) => added.Add((LogicFolderInfomation)s);
            void onRemove(object s, EventArgs e) => removed.Add((LogicFolderInfomation)s);
            void onMove(object s, EventArgs e) => moved.Add((LogicFolderInfomation)s);
            LogicFolderInfomation.StaticAfterAdd += onAdd;
            LogicFolderInfomation.StaticBeforeRemove += onRemove;
            LogicFolderInfomation.StaticBeforeChangeIndex += onMove;
            try
            {
                var root = LogicFolderInfomation.Root;
                Assert.True(root.IsFolder);
                Assert.Equal(0, root.ChildrenCount);

                var favorites = root.AddFolder("お気に入り");
                var linkA = favorites.AddScore(a);
                var linkB = favorites.AddScore(b, "B の譜面");
                var inner = favorites.AddFolder("中");
                var linkInner = inner.AddScore(a);

                Assert.Equal([favorites, linkA, linkB, inner, linkInner], added);
                Assert.Equal(("お気に入り", 1, true), (favorites.Name, favorites.Depth, favorites.IsFolder));
                Assert.Equal(("a", a.ID, 2, false), (linkA.Name, linkA.ScoreID, linkA.Depth, linkA.IsFolder));
                Assert.Equal("B の譜面", linkB.Name);
                Assert.Same(favorites, linkA.Parent);
                Assert.True(root.ContainAsChildrenOrSelf(linkInner));
                Assert.True(favorites.ContainAsChildrenOrSelf(favorites));
                Assert.False(inner.ContainAsChildrenOrSelf(linkA));
                Assert.False(root.ContainAsChildrenOrSelf(null));
                // フォルダーでないものの下や、譜面でないものは足せない
                Assert.Null(linkA.AddFolder("x"));
                Assert.Null(linkA.AddScore(b));
                Assert.Null(favorites.AddScore(null, "x"));

                inner.ChangeIndex(0);
                linkA.Rename("名前を変えた");
                root.ChangeIndex(0);
                Assert.Equal([inner], moved);

                // データベースから読み直しても同じ
                SongInformation.Update();
                var reloaded = Assert.Single(LogicFolderInfomation.Root.Children);
                Assert.NotSame(favorites, reloaded);
                Assert.Equal(["中", "名前を変えた", "B の譜面"], reloaded.Children.Select(c => c.Name));
                Assert.Equal([a.ID, a.ID, b.ID], LogicFolderInfomation.All.Select(l => l.ScoreID).Order());
                Assert.True(reloaded.Children[1].DateTime > DateTime.MinValue);

                // フォルダーを消すと中身も消える（ルートは消せない）
                reloaded.Children[0].Remove();
                Assert.Equal(2, reloaded.ChildrenCount);
                reloaded.Remove();
                Assert.True(reloaded.Disposed);
                LogicFolderInfomation.Root.Remove();
                Assert.Equal(2, removed.Count);
                SongInformation.Update();
                Assert.Empty(LogicFolderInfomation.Root.Children);
                Assert.Empty(LogicFolderInfomation.All);
            }
            finally
            {
                LogicFolderInfomation.StaticAfterAdd -= onAdd;
                LogicFolderInfomation.StaticBeforeRemove -= onRemove;
                LogicFolderInfomation.StaticBeforeChangeIndex -= onMove;
            }
        }

        [Fact]
        public void ScoreStorage_IsSavedPerSong()
        {
            using var database = new SongDatabase();
            database.AddSong("a", "guid", 100, ("Easy", null));
            SongInformation.Update();
            var id = SongDatabase.Song("a").ID;

            var storage = PPDDatabase.DB.GetScoreStorage(id);
            Assert.Equal(-1, storage.ID);
            storage["best"] = "1";
            PPDDatabase.DB.SaveScoreStorage(storage);
            Assert.True(storage.ID >= 0);
            storage["best"] = "2";
            PPDDatabase.DB.SaveScoreStorage(storage);

            var loaded = PPDDatabase.DB.GetScoreStorage(id);
            Assert.Equal((storage.ID, "2"), (loaded.ID, loaded["best"]));
        }

        [Fact]
        public void OldDatabase_IsUpgraded()
        {
            // 古いデータベース（列が足りない、動画の切り取りが整数）
            using var database = new SongDatabase(path =>
            {
                using var connection = new SQLiteConnection("Data Source=" + path);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = @"create table ScoreTable(
   scoreid integer primary key autoincrement, directorypath varchar(50), thumbtimestart real, thumbtimeend real, start real, end real, bpm real,
   difficultyeasy varchar(50), difficultynormal varchar(50), difficultyhard varchar(50), difficultyextreme varchar(50),
   availableeasy int, availablenormal int, availablehard int, availableextreme int,
   bestscoreeasyid int default -1, bestscorenormalid int default -1, bestscorehardid int default -1, bestscoreextremeid int default -1,
   moviecutleft int, moviecutright int, moviecuttop int, moviecutbottom int, guid varchar(50), authorname varchar(50), updatedate varchar(50),
   isppd int, movievolume int, moviepath varchar(50), parentpath varchar(50), isold int, latency real,
   easyhash blob, normalhash blob, hardhash blob, extremehash blob);
insert into ScoreTable(directorypath, parentpath, isppd, latency) values('gone', '', 0, 0.5);";
                command.ExecuteNonQuery();
            });

            var db = PPDDatabase.DB;

            Assert.Equal("real", db.GetColumnType("ScoreTable", "moviecutleft"));
            Assert.True(db.HasColumn("ScoreTable", "uservolume"));
            Assert.True(db.ExistTable("ResultTable") && db.ExistTable("LogicFolder") && db.ExistTable("ScoreStorage"));
            Assert.Equal(PPDDatabase.LogicFolderRoot, LogicFolderInfomation.Root.Name);
        }

        sealed class TestDatabase(string path) : DataBase
        {
            public override string Name => "Test";

            public override string DBPath => path;
        }

        [Fact]
        public void DataBase_CanBeReopenedAfterClose()
        {
            using var folder = new TempFolder();
            using var db = new TestDatabase(folder.Combine("test.db"));
            Assert.Equal("Test.db", db.DBName);
            Assert.True(db.IsClosed);

            db.Open();
            Assert.False(db.IsClosed);
            db.ExecuteNonQueryCommand("create table T(id integer primary key autoincrement, name varchar(10));");
            db.AddColumn("T", "count", "int", "3");
            using (db.BeginTransaction())
            {
                Assert.Equal(1, db.ExecuteDataTable("insert into T(name) values(@name);", [new("@name", "a")]));
            }
            db.Close();
            Assert.True(db.IsClosed);

            db.Open();
            using (var reader = db.ExecuteReader("select count from T;", null))
            {
                Assert.True(reader.Reader.Read());
                Assert.Equal(3, reader.Reader.GetInt32(0));
            }
            db.Close();
            SQLiteConnection.ClearAllPools();
        }
    }
}
