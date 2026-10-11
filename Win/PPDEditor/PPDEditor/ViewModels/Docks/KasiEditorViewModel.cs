using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDEditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// ViewModel から使う、歌詞の編集の画面の機能です。
    /// </summary>
    public interface IKasiEditorView
    {
        void ShowMessage(string text);

        /// <summary>
        /// 歌詞を直させます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string AskNewText(string text);

        /// <summary>
        /// 選んでいる行を見えるようにして、ドックを前に出します。
        /// </summary>
        void ShowSelectedRow();

        /// <summary>
        /// 歌詞の入力欄にフォーカスを移します。
        /// </summary>
        void FocusInput();

        /// <summary>
        /// タイムラインを描き直します。
        /// </summary>
        void RefreshTimeline();
    }

    /// <summary>
    /// 歌詞（時間ごとのテキスト）を編集します。
    /// </summary>
    public partial class KasiEditorViewModel(IKasiEditorView view, Func<float> getCurrentTime) : DockViewModel
    {
        readonly KasiData data = new();

        /// <summary>
        /// 一覧で選んでいる行の番号です（選んでいないときは -1）。
        /// </summary>
        [ObservableProperty]
        public partial int SelectedIndex { get; set; } = -1;

        /// <summary>
        /// 入力している歌詞です。
        /// </summary>
        [ObservableProperty]
        public partial string InputText { get; set; } = "";

        /// <summary>
        /// 時間をずらす量（秒）です。
        /// </summary>
        [ObservableProperty]
        public partial decimal ShiftAmount { get; set; }
        public ObservableCollection<KeyValuePair<float, string>> Items { get; } = [];

        public float[] GetKasiChangeWithinTime(float startTime, float endTime)
        {
            return data.GetTimesWithin(startTime, endTime);
        }

        public string GetFormattedContent(float time)
        {
            return data.GetFormattedContent(time);
        }

        /// <summary>
        /// その時間の歌詞の行を選んで表示します。
        /// </summary>
        public void FocusWithTimeData(float time)
        {
            var index = data.IndexOf(time);
            if (index >= 0)
            {
                SelectedIndex = index;
                view.ShowSelectedRow();
            }
        }

        public void ChangeTime(float previousTime, float newTime)
        {
            if (previousTime >= 0 && data.Move(previousTime, newTime))
            {
                Refresh();
                OnChanged();
                view.RefreshTimeline();
            }
        }

        public void SetData(string text)
        {
            data.Parse(text);
            Refresh();
        }

        public string GetCurrentKasi()
        {
            return data.GetCurrent(getCurrentTime());
        }

        public string SaveData()
        {
            return data.Serialize();
        }

        public void Clear()
        {
            data.Clear();
            Refresh();
        }

        // 選んでいる行は、行の番号で保つ（以前の DataGridView と同じ）
        private void Refresh()
        {
            var index = SelectedIndex;
            Items.Clear();
            foreach (var pair in data.Items)
            {
                Items.Add(pair);
            }
            SelectedIndex = index >= 0 && index < data.Count ? index : -1;
        }

        private bool CheckSelected()
        {
            if (SelectedIndex < 0)
            {
                view.ShowMessage(Texts["KENoSelectedLine"]);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 入力した歌詞を、今の時間に足します。
        /// </summary>
        [RelayCommand]
        private void AddAtCurrentTime()
        {
            if (data.Add(getCurrentTime(), InputText.Replace("\n", "")))
            {
                Refresh();
                OnChanged();
            }
            else
            {
                view.ShowMessage(Texts["KEAlreadyAdded"]);
            }
        }

        [RelayCommand]
        private void DeleteSelected()
        {
            if (CheckSelected())
            {
                data.RemoveAt(SelectedIndex);
                Refresh();
                OnChanged();
            }
        }

        [RelayCommand]
        private void ShiftSelected()
        {
            if (!CheckSelected())
            {
                return;
            }
            var time = data.Times[SelectedIndex];
            if (data.Move(time, time + (float)ShiftAmount))
            {
                Refresh();
                OnChanged();
            }
        }

        [RelayCommand]
        private void ChangeSelectedText()
        {
            if (!CheckSelected())
            {
                return;
            }
            var newText = view.AskNewText(data.Texts[SelectedIndex]);
            if (newText == null)
            {
                return;
            }
            var time = data.Times[SelectedIndex];
            data.RemoveAt(SelectedIndex);
            data.Add(time, newText.Replace("\n", ""));
            Refresh();
            OnChanged();
        }

        /// <summary>
        /// 選んでいる行の歌詞を、今の時間に足します。
        /// </summary>
        [RelayCommand]
        private void CopySelectedToCurrentTime()
        {
            if (CheckSelected())
            {
                InputText = data.Texts[SelectedIndex];
                AddAtCurrentTime();
                OnChanged();
            }
        }

        [RelayCommand]
        private void ClearInput()
        {
            InputText = "";
            view.FocusInput();
        }
    }
}
