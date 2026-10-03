using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDEditorCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// ほかの譜面のイベントを、今の譜面にインポートするダイアログです。
    /// </summary>
    /// <remarks>
    /// 今のイベントはコピーして編集し、OK のときだけ <see cref="ChangeData"/> として返す。
    /// </remarks>
    public partial class ImportEventViewModel : DialogViewModel
    {
        readonly SortedList<float, EventData> currentEvent;
        readonly SortedList<float, EventData> readOnlyEvent;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private KeyValuePair<float, EventData>? selectedCurrentEvent;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
        private KeyValuePair<float, EventData>? selectedReadOnlyEvent;

        public ImportEventViewModel(SortedList<float, EventData> currentEvent, SortedList<float, EventData> readOnlyEvent)
        {
            this.currentEvent = new SortedList<float, EventData>(currentEvent);
            this.readOnlyEvent = readOnlyEvent;
            ReadOnlyEvents = [.. readOnlyEvent];
            CurrentEvents = [.. this.currentEvent];
        }

        /// <summary>
        /// イベントの中身を（読み込みだけで）表示してほしいときに起こります。
        /// </summary>
        public event Action<EventData> ShowEventRequested;

        public ObservableCollection<KeyValuePair<float, EventData>> CurrentEvents { get; }

        public ObservableCollection<KeyValuePair<float, EventData>> ReadOnlyEvents { get; }

        /// <summary>
        /// インポートした後の今のイベントです。
        /// </summary>
        public SortedList<float, EventData> ChangeData
        {
            get { return currentEvent; }
        }

        /// <summary>
        /// 時間の列の見出しです（キーに括弧があって Texts[...] で結び付けられないため）。
        /// </summary>
        public string TimeHeader
        {
            get { return Texts["Time(Sec)"]; }
        }

        public string ImportText
        {
            get { return String.Format("<={0}", Texts["Import"]); }
        }

        private bool CanImport()
        {
            return SelectedReadOnlyEvent.HasValue;
        }

        // 同じ時間のイベントがあるときはインポートしない
        [RelayCommand(CanExecute = nameof(CanImport))]
        private void Import()
        {
            var time = SelectedReadOnlyEvent.Value.Key;
            if (currentEvent.ContainsKey(time))
            {
                return;
            }
            currentEvent.Add(time, readOnlyEvent[time]);
            RefreshCurrentEvents();
        }

        private bool CanDelete()
        {
            return SelectedCurrentEvent.HasValue;
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        private void Delete()
        {
            currentEvent.Remove(SelectedCurrentEvent.Value.Key);
            RefreshCurrentEvents();
        }

        [RelayCommand]
        private void ShowEvent(KeyValuePair<float, EventData>? item)
        {
            if (item.HasValue)
            {
                ShowEventRequested?.Invoke(item.Value.Value.Clone());
            }
        }

        private void RefreshCurrentEvents()
        {
            CurrentEvents.Clear();
            foreach (var pair in currentEvent)
            {
                CurrentEvents.Add(pair);
            }
        }
    }
}
