using PPDEditor;
using PPDEditor.ViewModels.Dialogs;
using PPDEditor.ViewModels.Docks;
using PPDFramework;
using PPDFramework.PPDStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// 音の割り当てのドックのテストです。音の読み込みや再生は、偽物のホストに記録するだけ。
    /// </summary>
    public class SoundManagerViewModelTests : IDisposable
    {
        sealed class FakeSoundHost : ISoundManagerHost
        {
            public float CurrentTime { get; set; }
            public List<string> Loaded { get; } = [];
            public List<string> Unloaded { get; } = [];
            public List<(string Path, int Volume)> Played { get; } = [];
            public Queue<string[]> Files { get; } = new();
            public List<string> Messages { get; } = [];
            public bool ConfirmAnswer { get; set; } = true;
            public Func<SoundChangeEditViewModel, ushort[]> Edit { get; set; } = _ => null;
            public string[] OtherScdPaths { get; set; } = [];
            public int ShownRows { get; private set; }
            public int Refreshed { get; private set; }

            public int GetVolume(int button) => -button;

            public void LoadSound(string path) => Loaded.Add(path);

            public void UnloadSound(string path) => Unloaded.Add(path);

            public void PlaySound(string path, int volume) => Played.Add((path, volume));

            public string[] AskSoundFiles(string filter, bool multiselect) => Files.Count > 0 ? Files.Dequeue() : [];

            public void ShowMessage(string text) => Messages.Add(text);

            public bool Confirm(string text, string caption) => ConfirmAnswer;

            public ushort[] EditChange(SoundChangeEditViewModel viewModel) => Edit(viewModel);

            public void ShowSelectedRow() => ShownRows++;

            public void RefreshTimeline() => Refreshed++;

            public string[] GetOtherDifficultyScdPaths() => OtherScdPaths;
        }

        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakeSoundHost host = new();
        readonly SoundManagerViewModel viewModel;

        public SoundManagerViewModelTests()
        {
            AppFiles.CopyTo("PPDEditor", folder);
            Utility.ChangeLanguage("jp");
            viewModel = new SoundManagerViewModel(host);
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        private string NoSound => Utility.Language["SMNoSound"];

        [Fact]
        public void Startup_HasSilentFirstChange()
        {
            Assert.Equal([NoSound], viewModel.SoundNames);
            var row = Assert.Single(viewModel.Rows);
            Assert.Equal(0, row.Time);
            Assert.Equal(string.Join(",", Enumerable.Repeat(NoSound, 10)), row.Sounds);
            Assert.Equal(10, viewModel.Slots.Length);
        }

        [Fact]
        public void AddSounds_LoadsOnlyNewFiles()
        {
            host.Files.Enqueue([@"C:\s\a.wav", @"C:\s\b.wav"]);
            viewModel.AddSoundsCommand.Execute(null);
            viewModel.AddSoundsCommand.Execute(null);
            viewModel.ReadSounds([@"C:\s\a.wav"]);

            Assert.Equal([@"C:\s\a.wav", @"C:\s\b.wav"], host.Loaded);
            Assert.Equal([NoSound, "a", "b", "a"], viewModel.SoundNames);
            Assert.Equal(string.Join(Environment.NewLine, "a.wav", "b.wav", "a.wav"), viewModel.SaveSoundSet());
        }

        [Fact]
        public void AddChangeAndPlaySounds()
        {
            viewModel.ReadSounds([@"C:\s\a.wav", @"C:\s\b.wav"]);
            viewModel.Slots[0].SelectedIndex = 1;
            viewModel.Slots[1].SelectedIndex = 2;
            host.CurrentTime = 2;

            viewModel.AddChangeCommand.Execute(null);
            viewModel.AddChangeCommand.Execute(null);
            Assert.Equal([Utility.Language["SMAlreadyAdded"]], host.Messages);
            Assert.Equal("a,b," + string.Join(",", Enumerable.Repeat(NoSound, 8)), viewModel.Rows[1].Sounds);
            Assert.Equal([2f], viewModel.GetSoundChangeWithinTime(1, 3));
            Assert.Equal("a\nb\n", viewModel.GetFormattedContent(2)[..4]);
            Assert.Equal("", viewModel.GetFormattedContent(5));

            viewModel.OnSeeked(1);
            viewModel.PlaySounds([true, true, false, false, false, false, false, false, false, false]);
            Assert.Empty(host.Played);
            viewModel.OnSeeked(2.5f);
            Assert.Equal(1, viewModel.SelectedIndex);
            viewModel.PlaySounds([true, true, true, false, false, false, false, false, false, false]);
            Assert.Equal([(@"C:\s\a.wav", 0), (@"C:\s\b.wav", -1)], host.Played);

            var scd = viewModel.GetSCDData();
            Assert.Equal([ButtonType.Square, ButtonType.Cross], scd.Select(d => d.ButtonType));
        }

        [Fact]
        public void SyncSelection_ShowsSelectedChangeInSlots()
        {
            viewModel.ReadSounds([@"C:\s\a.wav"]);
            viewModel.ReadChangeData([3], [[0, 1, 0, 0, 0, 0, 0, 0, 0, 0]]);

            viewModel.SelectedIndex = 1;
            Assert.Equal(0, viewModel.Slots[1].SelectedIndex);
            viewModel.SyncSelection = true;
            viewModel.SelectedIndex = 0;
            viewModel.SelectedIndex = 1;
            Assert.Equal(1, viewModel.Slots[1].SelectedIndex);
            viewModel.OnSeeked(0);
            Assert.Equal(0, viewModel.Slots[1].SelectedIndex);
        }

        [Fact]
        public void EditAndDeleteChange()
        {
            viewModel.ReadSounds([@"C:\s\a.wav"]);
            viewModel.ReadChangeData([3], [[0, 0, 0, 0, 0, 0, 0, 0, 0, 0]]);

            // 最初の割り当ては編集も削除もしない
            viewModel.SelectedIndex = 0;
            viewModel.EditChangeCommand.Execute(null);
            viewModel.DeleteChangeCommand.Execute(null);
            Assert.Equal([Utility.Language["SMDeleteFirst"]], host.Messages);

            viewModel.SelectedIndex = 1;
            host.Edit = editor =>
            {
                Assert.Equal([NoSound, "a"], editor.SoundNames);
                return [1, 1, 1, 1, 1, 1, 1, 1, 1, 1];
            };
            viewModel.EditChangeCommand.Execute(null);
            Assert.Equal(string.Join(",", Enumerable.Repeat("a", 10)), viewModel.Rows[1].Sounds);
            host.Edit = _ => null;
            viewModel.EditChangeCommand.Execute(null);

            viewModel.DeleteChangeCommand.Execute(null);
            Assert.Single(viewModel.Rows);
            viewModel.DeleteChangeCommand.Execute(null);
        }

        [Fact]
        public void MoveFocusAndClear()
        {
            viewModel.ReadSounds([@"C:\s\a.wav"]);
            viewModel.ReadChangeData([1, 2], [new ushort[10], new ushort[10]]);

            viewModel.ChangeTime(0, 5);
            viewModel.ChangeTime(1, 2);
            viewModel.ChangeTime(2, 4);
            Assert.Equal([0f, 1f, 4f], viewModel.Rows.Select(r => r.Time));
            Assert.Equal(1, host.Refreshed);

            viewModel.FocusWithTimeData(4);
            viewModel.FocusWithTimeData(9);
            Assert.Equal((2, 1), (viewModel.SelectedIndex, host.ShownRows));

            viewModel.ClearSetting();
            Assert.Single(viewModel.Rows);
            viewModel.Slots[0].SelectedIndex = 1;
            viewModel.PreviewIndex = 1;
            viewModel.ClearAll();
            Assert.Equal([NoSound], viewModel.SoundNames);
            Assert.Equal((0, 0), (viewModel.Slots[0].SelectedIndex, viewModel.PreviewIndex));
            Assert.Equal([""], viewModel.SoundPaths);
        }

        [Fact]
        public void PlayPreview()
        {
            viewModel.ReadSounds([@"C:\s\a.wav"]);

            viewModel.PlayCommand.Execute(null);
            viewModel.PreviewIndex = 1;
            viewModel.PlayCommand.Execute(null);
            viewModel.PlayPreview(@"C:\s\x.wav");

            Assert.Equal([(@"C:\s\a.wav", -1000), (@"C:\s\x.wav", -1000)], host.Played);
        }

        [Fact]
        public void DeleteSound_ShiftsAssignmentsAndOtherDifficulties()
        {
            viewModel.ReadSounds([@"C:\s\a.wav", @"C:\s\b.wav"]);
            viewModel.ReadChangeData([1], [[1, 2, 0, 0, 0, 0, 0, 0, 0, 0]]);
            viewModel.Slots[0].SelectedIndex = 2;
            var scd = folder.Combine("Hard.scd");
            SCDWriter.Write(scd, [new SCDData(1, ButtonType.Square, 2)]);
            host.OtherScdPaths = [scd];

            viewModel.DeleteSoundCommand.Execute(null);
            Assert.Equal([Utility.Language["SMDeleteNosound"]], host.Messages);

            // ほかの難易度があるときは確かめる
            viewModel.PreviewIndex = 1;
            host.ConfirmAnswer = false;
            viewModel.DeleteSoundCommand.Execute(null);
            Assert.Equal(3, viewModel.SoundNames.Count);
            host.ConfirmAnswer = true;
            viewModel.DeleteSoundCommand.Execute(null);

            Assert.Equal([NoSound, "b"], viewModel.SoundNames);
            Assert.Equal([@"C:\s\a.wav"], host.Unloaded);
            Assert.StartsWith(NoSound + ",b,", viewModel.Rows[1].Sounds);
            Assert.Equal(1, viewModel.Slots[0].SelectedIndex);
            Assert.Equal([(ushort)1], SCDReader.Read(scd).Select(d => d.SoundIndex));
        }

        [Fact]
        public void ReloadSound_ReplacesFile()
        {
            viewModel.ReadSounds([@"C:\s\a.wav"]);

            viewModel.ReloadSoundCommand.Execute(null);
            Assert.Single(host.Messages);
            viewModel.PreviewIndex = 1;
            viewModel.ReloadSoundCommand.Execute(null);

            // 同じファイルなら読み込み直す
            host.Files.Enqueue([@"C:\s\a.wav"]);
            viewModel.ReloadSoundCommand.Execute(null);
            Assert.Equal([@"C:\s\a.wav"], host.Unloaded);
            Assert.Equal([@"C:\s\a.wav", @"C:\s\a.wav"], host.Loaded);

            // 別のファイルにしたら、使わなくなったファイルは外す
            host.Files.Enqueue([@"C:\s\c.wav"]);
            viewModel.ReloadSoundCommand.Execute(null);
            Assert.Equal([NoSound, "c"], viewModel.SoundNames);
            Assert.Equal(@"C:\s\c.wav", host.Loaded.Last());
            Assert.Equal([@"C:\s\a.wav", @"C:\s\a.wav"], host.Unloaded);
        }

        [Fact]
        public void ReloadSound_KeepsFileStillUsedElsewhere()
        {
            viewModel.ReadSounds([@"C:\s\a.wav", @"C:\s\a.wav"]);
            viewModel.PreviewIndex = 2;
            host.Files.Enqueue([@"C:\s\c.wav"]);

            viewModel.ReloadSoundCommand.Execute(null);

            Assert.Empty(host.Unloaded);
            Assert.Equal([NoSound, "a", "c"], viewModel.SoundNames);
        }
    }
}
