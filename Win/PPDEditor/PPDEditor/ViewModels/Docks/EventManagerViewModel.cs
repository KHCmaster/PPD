using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDEditor.Models;
using PPDEditorCommon;
using PPDFramework.PPDStructure.EVDData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// ViewModel から使う、エディターと画面の機能です。
    /// </summary>
    public interface IEventManagerHost
    {
        /// <summary>
        /// タイムラインの今の時間です。
        /// </summary>
        float CurrentTime { get; }

        /// <summary>
        /// 設定ファイルの BPM です（最初のイベントの BPM）。
        /// </summary>
        float DefaultBpm { get; }

        /// <summary>
        /// イベントを編集させます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        EventData EditEvent(EventData eventData);

        void ShowMessage(string text);

        bool Confirm(string text, string caption, bool yesNo);

        /// <summary>
        /// 選んでいる行を見えるようにして、ドックを前に出します。
        /// </summary>
        void ShowSelectedRow();

        void RefreshTimeline();

        /// <summary>
        /// 動画の音量を変えます。
        /// </summary>
        void ChangeMovieVolume(int volume);
    }

    /// <summary>
    /// 時間ごとのイベントを編集します。タイムラインの今の時間のイベント（音量など）も持ちます。
    /// </summary>
    public partial class EventManagerViewModel(IEventManagerHost host) : DockViewModel
    {
        readonly EventList events = new();
        readonly int[] volumePercents = [90, 90, 90, 90, 90, 90, 90, 90, 90, 90];
        readonly bool[] releaseSounds = new bool[10];
        int currentIndex = -1;

        [ObservableProperty]
        private int selectedIndex = -1;

        [ObservableProperty]
        private decimal shiftAmount;

        public ObservableCollection<KeyValuePair<float, string>> Items { get; } = [];

        /// <summary>
        /// 今の時間のマークの表示（サドゥン、ヒドゥンなど）です。
        /// </summary>
        public DisplayState DisplayState
        {
            get;
            private set;
        } = DisplayState.Normal;

        /// <summary>
        /// イベントの一覧のコピーです。設定すると、その一覧に入れ替えます。
        /// </summary>
        public SortedList<float, EventData> ChangeData
        {
            get { return events.Data; }
            set
            {
                events.Data = value;
                Refresh();
                OnChanged();
            }
        }

        /// <summary>
        /// 今の時間のボタンの音量です（DirectSound の音量の単位）。
        /// </summary>
        public int GetVolume(int i)
        {
            return -100 * (100 - volumePercents[i]);
        }

        public bool ReleaseSound(int i)
        {
            return releaseSounds[i];
        }

        public float[] GetEventsWithinTime(float startTime, float endTime)
        {
            return events.GetTimesWithin(startTime, endTime);
        }

        public string GetFormattedContent(float time)
        {
            return events.GetFormattedContent(time);
        }

        public EventData GetEvent(float time)
        {
            return events.GetEvent(time);
        }

        public float GetCorrectTime(float currentTime, float markTime)
        {
            return events.GetCorrectTime(currentTime, markTime);
        }

        public IEVDData[] GetEvdData()
        {
            return events.ToEvdData(host.DefaultBpm);
        }

        public void FocusWithTimeData(float time)
        {
            var index = events.IndexOf(time);
            if (index >= 0)
            {
                SelectedIndex = index;
                host.ShowSelectedRow();
            }
        }

        public void ChangeTime(float previousTime, float newTime)
        {
            if (previousTime >= 0 && events.Move(previousTime, newTime))
            {
                Refresh();
                OnChanged();
                host.RefreshTimeline();
            }
        }

        public void AddChange(float time, EventData data)
        {
            if (events.Add(time, data))
            {
                Refresh();
                OnChanged();
            }
        }

        public void Clear()
        {
            events.Clear();
            Refresh();
            Array.Fill(volumePercents, 90);
            Array.Fill(releaseSounds, false);
            DisplayState = DisplayState.Normal;
        }

        /// <summary>
        /// 最初のイベントの BPM が設定ファイルの BPM と違うときに、合わせるかを聞きます。
        /// </summary>
        public void CheckInitialBpm(float bpm)
        {
            if (events.Contains(0) && events.GetEvent(0).BPM != bpm && host.Confirm(Texts["EMCheckBpmText"], Texts["EMCheckBpm"], true))
            {
                events.SetInitialBpm(bpm);
                Refresh();
                OnChanged();
            }
        }

        /// <summary>
        /// タイムラインの時間が変わったときに、その時間のイベントを選び、音量などを合わせます。
        /// </summary>
        public void OnSeeked(float time)
        {
            var index = events.GetIndexAt(time);
            if (index >= 0 && index < events.Count)
            {
                SelectedIndex = index;
                ApplyEvent(events.Events[index]);
                currentIndex = index;
            }
            else
            {
                SelectedIndex = -1;
                ApplyEvent(new EventData());
                currentIndex = -1;
            }
        }

        private void ApplyEvent(EventData data)
        {
            int[] volumes = [data.SquareVolumePercent, data.CrossVolumePercent, data.CircleVolumePercent, data.TriangleVolumePercent,
                data.LeftVolumePercent, data.DownVolumePercent, data.RightVolumePercent, data.UpVolumePercent, data.RVolumePercent, data.LVolumePercent];
            bool[] releases = [data.SquareReleaseSound, data.CrossReleaseSound, data.CircleReleaseSound, data.TriangleReleaseSound,
                data.LeftReleaseSound, data.DownReleaseSound, data.RightReleaseSound, data.UpReleaseSound, data.RReleaseSound, data.LReleaseSound];
            volumes.CopyTo(volumePercents, 0);
            releases.CopyTo(releaseSounds, 0);
            host.ChangeMovieVolume(-100 * (100 - data.MovieVolumePercent));
            DisplayState = data.DisplayState;
        }

        // 選んでいる行は、行の番号で保つ（以前の DataGridView と同じ）
        private void Refresh()
        {
            var index = SelectedIndex;
            Items.Clear();
            foreach (var pair in events.Items)
            {
                Items.Add(new KeyValuePair<float, string>(pair.Key, pair.Value.ToString()));
            }
            SelectedIndex = index >= 0 && index < events.Count ? index : -1;
        }

        /// <summary>
        /// 今の時間にイベントを足します。前のイベントがあれば、それを元にします。
        /// </summary>
        [RelayCommand]
        private void Add()
        {
            var time = host.CurrentTime;
            if (events.Contains(time))
            {
                host.ShowMessage(Texts["EMAlreadyAdded"]);
                return;
            }
            var source = events.Count > 0 && currentIndex >= 0 && currentIndex < events.Count
                ? events.Events[currentIndex].Clone()
                : new EventData { BPM = host.DefaultBpm };
            var edited = host.EditEvent(source);
            if (edited != null)
            {
                AddChange(time, edited);
            }
        }

        [RelayCommand]
        private void EditSelected()
        {
            if (SelectedIndex < 0)
            {
                return;
            }
            var edited = host.EditEvent(events.Events[SelectedIndex].Clone());
            if (edited != null)
            {
                events.ReplaceAt(SelectedIndex, edited);
                Refresh();
                OnChanged();
            }
        }

        [RelayCommand]
        private void DeleteSelected()
        {
            if (SelectedIndex >= 0 && host.Confirm(Texts["EMDeleteConfirm"], Texts["EMConfirm"], false))
            {
                events.RemoveAt(SelectedIndex);
                Refresh();
                OnChanged();
            }
        }

        [RelayCommand]
        private void ShiftSelected()
        {
            if (SelectedIndex < 0)
            {
                host.ShowMessage(Texts["EMNoSelectedLine"]);
                return;
            }
            var time = events.Times[SelectedIndex];
            if (events.Move(time, time + (float)ShiftAmount))
            {
                Refresh();
                OnChanged();
            }
        }
    }
}
