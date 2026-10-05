using PPDFramework;
using PPDFramework.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// PPDFramework の小さなクラス（スコアの保存領域、遊び方の情報、空の入力、音量）のテストです。
    /// </summary>
    public class PPDFrameworkSmallTests
    {
        [Fact]
        public void ScoreStorage_TextRoundTrip()
        {
            var storage = new ScoreStorage(3)
            {
                ["best"] = "100",
                [null] = "no key",
                ["removed"] = null,
            };

            Assert.Equal("100", storage["best"]);
            // キーが null のときは空のキー
            Assert.Equal("no key", storage[""]);
            Assert.True(storage.ContainsKey(null));
            Assert.Null(storage["missing"]);
            Assert.Equal(-1, storage.ID);
            Assert.Equal(3, storage.SongInformationID);

            // データベースに入れる文字列（null の値は書かない）から作り直す
            var text = (string)typeof(ScoreStorage).GetMethod("GetString", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(storage, null);
            var loaded = new ScoreStorage(7, 3, text);

            Assert.Equal(7, loaded.ID);
            Assert.Equal("100", loaded["best"]);
            Assert.Equal("no key", loaded[""]);
            Assert.False(loaded.ContainsKey("removed"));
        }

        [Fact]
        public void GameUtility_ModsItemsAndCallbacks()
        {
            var utility = new PPDGameUtility { AppliedMods = [] };

            Assert.True(utility.NotModifyMod);
            Assert.True(utility.CanApplyMod(null));
            utility.CanApplyModCallback = m => false;
            Assert.False(utility.CanApplyMod(null));

            utility.AddItem(null);
            Assert.Single(utility.Items);
            utility.PerfectTrial = true;
            utility.ReplayResultId = 5;
            utility.LatencyType = LatencyType.Db;
            Assert.Equal((true, 5, LatencyType.Db), (utility.PerfectTrial, utility.ReplayResultId, utility.LatencyType));
            Func<PPDFramework.Web.Ranking> update = () => null;
            utility.RankingUpdateFunc = update;
            utility.RivalRankingUpdateFunc = update;
            Assert.Same(update, utility.RankingUpdateFunc);
            Assert.Same(update, utility.RivalRankingUpdateFunc);
        }

        [Fact]
        public void EmptyInput_HasNothing()
        {
            var input = EmptyInputInfo.Instance;

            Assert.False(input.Accurate);
            Assert.All(Enum.GetValues<ButtonType>(), b =>
            {
                Assert.False(input.IsPressed(b));
                Assert.False(input.IsReleased(b));
                Assert.Equal(0, input.GetPressingFrame(b));
            });
        }

        sealed class Updatable : UpdatableGameComponent
        {
            public int Disposed { get; private set; }

            protected override void DisposeResource()
            {
                Disposed++;
                base.DisposeResource();
            }
        }

        [Fact]
        public void UpdatableGameComponent_DisposesOnce()
        {
            var component = new Updatable();
            component.Update();

            component.Dispose();
            component.Dispose();

            Assert.Equal(1, component.Disposed);
            Assert.True(component.disposed);
        }

        static ProfileManager NewProfileManager()
        {
            var constructor = typeof(ProfileManager).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes);
            return (ProfileManager)constructor.Invoke(null);
        }

        [Fact]
        public void ProfileManager_ReadsProfilesAndCycles()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            folder.Write(System.IO.Path.Combine("profiles", $"profile_{PPDSetting.Setting.LangISO}.xml"), """
                <Profiles>
                  <Profile DisplayText="最初" CoolPoint="1" GoodPoint="1" SafePoint="1" SadPoint="1" WorstPoint="1" GodMode="1" />
                  <Profile DisplayText="無敵" CoolPoint="9" GoodPoint="8" SafePoint="7" SadPoint="6" WorstPoint="5" GodMode="1" MuteSE="1" Connect="1" />
                  <Profile DisplayText="壊れた" CoolPoint="x" />
                  <Profile DisplayText="普通" CoolPoint="3" GoodPoint="2" SafePoint="1" SadPoint="0" WorstPoint="-1" />
                </Profiles>
                """);

            var manager = NewProfileManager();

            // 読めないものは飛ばす。最初のものは、いつもの点数（神モードなし）になる
            Assert.Equal(["", "無敵", "普通"], manager.Profiles.Select(p => p.DisplayText));
            Assert.Same(manager.Profiles[0], manager.Default);
            Assert.Equal((5, 2, 0, -20, -51, false), (manager.Default.CoolPoint, manager.Default.GoodPoint, manager.Default.SafePoint,
                manager.Default.SadPoint, manager.Default.WorstPoint, manager.Default.GodMode));
            var god = manager.Profiles[1];
            Assert.Equal((9, 8, 7, 6, 5, true, true, true, 1), (god.CoolPoint, god.GoodPoint, god.SafePoint, god.SadPoint, god.WorstPoint,
                god.GodMode, god.MuteSE, god.Connect, god.Index));
            Assert.False(manager.Profiles[2].GodMode);

            // 順に回る
            Assert.Same(manager.Default, manager.Current);
            Assert.Same(god, manager.Next());
            Assert.Same(manager.Profiles[2], manager.Next());
            Assert.Same(manager.Default, manager.Next());
            Assert.Same(manager.Profiles[2], manager.Previous());
            Assert.Same(manager.Profiles[2], manager.Current);
        }

        [Fact]
        public void ProfileManager_WithoutFile_HasOnlyDefault()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);

            var manager = NewProfileManager();

            Assert.Same(manager.Default, Assert.Single(manager.Profiles));
            Assert.Same(manager.Default, manager.Next());
            Assert.Same(manager.Default, manager.Previous());
            Assert.NotNull(ProfileManager.Instance.Default);
        }

        [Fact]
        public void SoundMaster_CombinesVolumes()
        {
            var master = SoundMasterControl.Instance;
            var saved = (master.GetVolume(SoundType.Master), master.GetVolume(SoundType.Movie), master.GetVolume(SoundType.Se));
            var changed = new List<string>();
            Action onMaster = () => changed.Add("master"), onMovie = () => changed.Add("movie"), onSe = () => changed.Add("se");
            master.MasterVolumeChanged += onMaster;
            master.MovieVolumeChanged += onMovie;
            master.SeVolumeChanged += onSe;
            try
            {
                master.ChangeVolume(0, SoundType.Master);
                master.ChangeVolume(0, SoundType.Movie);
                master.ChangeVolume(0, SoundType.Se);
                changed.Clear();

                // マスターを変えると、ほかも変わったことにする
                master.ChangeVolume(-5000, SoundType.Master);
                Assert.Equal(["master", "movie", "se"], changed);
                master.ChangeVolume(-5000, SoundType.Master);
                Assert.Equal(3, changed.Count);
                // 範囲の外は端に合わせる
                master.ChangeVolume(-20000, SoundType.Movie);
                Assert.Equal(SoundMasterControl.Min, master.GetVolume(SoundType.Movie));
                master.ChangeVolume(100, SoundType.Se);
                Assert.Equal(SoundMasterControl.Max, master.GetVolume(SoundType.Se));
                Assert.Equal(0, master.GetVolume((SoundType)99));

                // 割合をかけ合わせる（マスター 0.5 × 映像 0 × 音 0.5）
                Assert.Equal(SoundMasterControl.Min, master.GetVolume(0, SoundType.Movie));
                Assert.Equal(-7500, master.GetVolume(-5000, SoundType.Se));
                Assert.Equal(-5000, master.GetVolume(0, SoundType.Master));
            }
            finally
            {
                master.MasterVolumeChanged -= onMaster;
                master.MovieVolumeChanged -= onMovie;
                master.SeVolumeChanged -= onSe;
                master.ChangeVolume(saved.Item1, SoundType.Master);
                master.ChangeVolume(saved.Item2, SoundType.Movie);
                master.ChangeVolume(saved.Item3, SoundType.Se);
            }
        }
    }
}
