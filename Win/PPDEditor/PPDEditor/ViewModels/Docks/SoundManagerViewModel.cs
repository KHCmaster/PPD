using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDEditor.Models;
using PPDEditor.ViewModels.Dialogs;
using PPDFramework.PPDStructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// ViewModel から使う、エディターと画面の機能です。
    /// </summary>
    public interface ISoundManagerHost
    {
        float CurrentTime { get; }

        /// <summary>
        /// 今の時間のボタンの音量です。
        /// </summary>
        int GetVolume(int button);

        void LoadSound(string path);

        void UnloadSound(string path);

        void PlaySound(string path, int volume);

        /// <summary>
        /// 音のファイルを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは空。</returns>
        string[] AskSoundFiles(string filter, bool multiselect);

        void ShowMessage(string text);

        bool Confirm(string text, string caption);

        /// <summary>
        /// 割り当てを編集させます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        ushort[] EditChange(SoundChangeEditViewModel viewModel);

        /// <summary>
        /// 選んでいる行を見えるようにして、ドックを前に出します。
        /// </summary>
        void ShowSelectedRow();

        void RefreshTimeline();

        /// <summary>
        /// ほかの難易度の割り当てのファイル（.scd）のパスです。プロジェクトを開いていないときは空です。
        /// </summary>
        string[] GetOtherDifficultyScdPaths();
    }

    /// <summary>
    /// ボタンを押したときに鳴らす音と、時間ごとの音の割り当てを編集します。
    /// </summary>
    public partial class SoundManagerViewModel : DockViewModel
    {
        const int PreviewVolume = -1000;

        readonly ISoundManagerHost host;
        readonly SoundChangeList data = new();
        int currentIndex;

        [ObservableProperty]
        private int selectedIndex = -1;

        /// <summary>
        /// 試しに鳴らす音・消す音・入れ替える音の番号です。
        /// </summary>
        [ObservableProperty]
        private int previewIndex;

        /// <summary>
        /// 選んでいる割り当てを、ボタンごとの音の選択に表示するかです。
        /// </summary>
        [ObservableProperty]
        private bool syncSelection;

        public SoundManagerViewModel(ISoundManagerHost host)
        {
            this.host = host;
            Slots = [.. Enumerable.Range(0, SoundChangeList.ButtonCount).Select(i => new SoundSlot())];
            RefreshSoundNames();
            data.AddChange(0, new ushort[SoundChangeList.ButtonCount]);
            RefreshRows();
            // 言語を変えたら、無音の名前も変える
            Texts.PropertyChanged += (sender, e) =>
            {
                RefreshSoundNames();
                RefreshRows();
            };
        }

        /// <summary>
        /// 音の名前です（0 番目は無音）。
        /// </summary>
        public ObservableCollection<string> SoundNames { get; } = [];

        /// <summary>
        /// 新しく割り当てるときの、ボタンごとの音です（スクエアからエルの順）。
        /// </summary>
        public SoundSlot[] Slots { get; }

        public ObservableCollection<SoundChangeRow> Rows { get; } = [];

        /// <summary>
        /// 音のファイルのパスです（0 番目は無音）。
        /// </summary>
        public List<string> SoundPaths
        {
            get { return [.. data.Paths]; }
        }

        private string GetSoundName(int index)
        {
            return index == 0 ? Texts["SMNoSound"] : Path.GetFileNameWithoutExtension(data.Paths[index]);
        }

        // 音の名前の一覧を作り直す（選んでいる番号は保つ）
        private void RefreshSoundNames()
        {
            var selections = Slots.Select(s => s.SelectedIndex).ToArray();
            var preview = PreviewIndex;
            SoundNames.Clear();
            for (int i = 0; i < data.Paths.Count; i++)
            {
                SoundNames.Add(GetSoundName(i));
            }
            for (int i = 0; i < Slots.Length; i++)
            {
                Slots[i].SelectedIndex = Math.Clamp(selections[i], 0, SoundNames.Count - 1);
            }
            PreviewIndex = Math.Clamp(preview, 0, SoundNames.Count - 1);
        }

        // 選んでいる行は、行の番号で保つ（以前の DataGridView と同じ）
        private void RefreshRows()
        {
            var index = SelectedIndex;
            Rows.Clear();
            for (int i = 0; i < data.Count; i++)
            {
                Rows.Add(new SoundChangeRow(data.Times[i], String.Join(",", data.Changes[i].Select(soundIndex => GetSoundName(soundIndex)))));
            }
            SelectedIndex = index >= 0 && index < data.Count ? index : -1;
        }

        partial void OnSelectedIndexChanged(int value)
        {
            if (SyncSelection)
            {
                SyncSlots();
            }
        }

        private void SyncSlots()
        {
            if (SelectedIndex < 0)
            {
                return;
            }
            var change = data.Changes[SelectedIndex];
            for (int i = 0; i < change.Length; i++)
            {
                Slots[i].SelectedIndex = change[i];
            }
        }

        /// <summary>
        /// タイムラインの時間が変わったときに、その時間の割り当てを選びます。
        /// </summary>
        public void OnSeeked(float time)
        {
            currentIndex = data.GetIndexAt(time);
            if (currentIndex < data.Count)
            {
                SelectedIndex = currentIndex;
            }
            if (SyncSelection)
            {
                SyncSlots();
            }
        }

        /// <summary>
        /// 押したボタンの音を、今の時間の割り当てで鳴らします。
        /// </summary>
        public void PlaySounds(bool[] pressed)
        {
            for (int i = 0; i < pressed.Length; i++)
            {
                var soundIndex = data.Changes[currentIndex][i];
                if (pressed[i] && soundIndex != 0)
                {
                    host.PlaySound(data.Paths[soundIndex], host.GetVolume(i));
                }
            }
        }

        public void ReadSounds(string[] paths)
        {
            foreach (var path in paths)
            {
                if (data.AddSound(path))
                {
                    host.LoadSound(path);
                }
                OnChanged();
            }
            RefreshSoundNames();
        }

        public void ReadChangeData(float[] times, ushort[][] changes)
        {
            for (int i = 0; i < times.Length; i++)
            {
                data.AddChange(times[i], changes[i]);
            }
            RefreshRows();
        }

        public string SaveSoundSet()
        {
            return data.SerializeSounds();
        }

        public SCDData[] GetSCDData()
        {
            return data.ToSCDData();
        }

        public float[] GetSoundChangeWithinTime(float startTime, float endTime)
        {
            return data.GetTimesWithin(startTime, endTime);
        }

        /// <summary>
        /// 割り当ての内容（ボタンごとの音の名前を改行でつないだもの）です。
        /// </summary>
        public string GetFormattedContent(float time)
        {
            var index = data.IndexOf(time);
            if (index < 0)
            {
                return "";
            }
            var sb = new StringBuilder();
            foreach (var soundIndex in data.Changes[index])
            {
                sb.Append(GetSoundName(soundIndex));
                sb.Append('\n');
            }
            return sb.ToString();
        }

        public void FocusWithTimeData(float time)
        {
            var index = data.IndexOf(time);
            if (index >= 0)
            {
                SelectedIndex = index;
                host.ShowSelectedRow();
            }
        }

        // 最初の割り当ては動かさない
        public void ChangeTime(float previousTime, float newTime)
        {
            if (previousTime > 0 && data.MoveChange(previousTime, newTime))
            {
                RefreshRows();
                OnChanged();
                host.RefreshTimeline();
            }
        }

        /// <summary>
        /// 音も割り当ても消します（読み込んだ音も消す）。
        /// </summary>
        public void ClearAll()
        {
            data.Clear();
            RefreshSoundNames();
            foreach (var slot in Slots)
            {
                slot.SelectedIndex = 0;
            }
            PreviewIndex = 0;
            RefreshRows();
        }

        /// <summary>
        /// 最初の割り当て以外を消します。
        /// </summary>
        public void ClearSetting()
        {
            data.ClearChanges();
            RefreshRows();
        }

        [RelayCommand]
        private void AddSounds()
        {
            var paths = host.AskSoundFiles(Texts["SMWaveFilter"], true);
            if (paths.Length > 0)
            {
                ReadSounds(paths);
            }
        }

        /// <summary>
        /// 今の時間に、ボタンごとの音の選択を割り当てます。
        /// </summary>
        [RelayCommand]
        private void AddChange()
        {
            if (data.AddChange(host.CurrentTime, [.. Slots.Select(s => (ushort)s.SelectedIndex)]))
            {
                RefreshRows();
                OnChanged();
            }
            else
            {
                host.ShowMessage(Texts["SMAlreadyAdded"]);
            }
        }

        [RelayCommand]
        private void DeleteChange()
        {
            if (SelectedIndex < 0)
            {
                return;
            }
            if (SelectedIndex == 0)
            {
                host.ShowMessage(Texts["SMDeleteFirst"]);
                return;
            }
            data.RemoveChangeAt(SelectedIndex);
            RefreshRows();
            OnChanged();
        }

        [RelayCommand]
        private void EditChange()
        {
            if (SelectedIndex <= 0)
            {
                return;
            }
            var paths = data.Paths.ToArray();
            paths[0] = Texts["SMNoSound"];
            var change = host.EditChange(new SoundChangeEditViewModel(data.Changes[SelectedIndex], paths));
            if (change != null)
            {
                data.SetChange(SelectedIndex, change);
                RefreshRows();
                OnChanged();
            }
        }

        [RelayCommand]
        private void Play()
        {
            if (PreviewIndex > 0)
            {
                host.PlaySound(data.Paths[PreviewIndex], PreviewVolume);
            }
        }

        /// <summary>
        /// 試聴に使う音を鳴らします（割り当ての編集のダイアログから）。
        /// </summary>
        public void PlayPreview(string path)
        {
            host.PlaySound(path, PreviewVolume);
        }

        /// <summary>
        /// 選んでいる音を消します。ほかの難易度があるときは、確かめてからそちらの割り当てからも外します。
        /// </summary>
        [RelayCommand]
        private void DeleteSound()
        {
            var index = PreviewIndex;
            if (index < 0)
            {
                return;
            }
            if (index == 0)
            {
                host.ShowMessage(Texts["SMDeleteNosound"]);
                return;
            }
            var otherScdPaths = host.GetOtherDifficultyScdPaths();
            if (otherScdPaths.Length > 0 && !host.Confirm(Texts["DeleteSoundConfirmContent"], Texts["DeleteSoundConfirm"]))
            {
                return;
            }

            var path = data.Paths[index];
            if (data.RemoveSound(index))
            {
                host.UnloadSound(path);
            }
            foreach (var slot in Slots)
            {
                slot.SelectedIndex = SoundChangeList.ShiftIndex((ushort)slot.SelectedIndex, index);
            }
            PreviewIndex = 0;
            foreach (var scdPath in otherScdPaths)
            {
                SoundChangeList.RemoveSoundFromFile(scdPath, index);
            }
            RefreshSoundNames();
            RefreshRows();
            OnChanged();
        }

        /// <summary>
        /// 選んでいる音を、別のファイル（または同じファイルを読み込み直したもの）に入れ替えます。
        /// </summary>
        [RelayCommand]
        private void ReloadSound()
        {
            var index = PreviewIndex;
            if (index == 0)
            {
                host.ShowMessage(Texts["SMDeleteNosound"]);
                return;
            }
            if (index < 0)
            {
                return;
            }
            var paths = host.AskSoundFiles(Texts["SMWaveFilter"], false);
            if (paths.Length == 0)
            {
                return;
            }
            var path = paths[0];
            var oldPath = data.Paths[index];
            if (oldPath == path)
            {
                // 同じファイルなら、読み込み直す
                host.UnloadSound(path);
                data.ReplaceSound(index, "");
            }
            if (!data.Paths.Contains(path))
            {
                host.LoadSound(path);
            }
            data.ReplaceSound(index, path);
            // 前のファイルをもう使っていなければ外す（以前は読み込んだまま残っていた）
            if (oldPath != path && !data.Paths.Contains(oldPath))
            {
                host.UnloadSound(oldPath);
            }
            RefreshSoundNames();
            RefreshRows();
            OnChanged();
        }
    }

    /// <summary>
    /// 割り当ての一覧の 1 行です。
    /// </summary>
    public record SoundChangeRow(float Time, string Sounds);
}
