using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// 拍に合わせてボタンやキーを押して、BPM を測ります。
    /// </summary>
    public partial class BPMMeasureViewModel(Func<long> getTime) : DockViewModel
    {
        readonly List<long> times = new(10);

        /// <summary>
        /// 最後の 2 回の間の時間（ミリ秒）です。
        /// </summary>
        [ObservableProperty]
        private string interval = "0";

        [ObservableProperty]
        private string bpm = "0";

        /// <summary>
        /// 拍を 1 つ数えます。
        /// </summary>
        [RelayCommand]
        private void Tap()
        {
            times.Add(getTime());
            if (times.Count <= 1)
            {
                return;
            }
            long total = times[^1] - times[0];
            Interval = (times[^1] - times[^2]).ToString();
            // n 回押したときの拍の間は n - 1 個（以前は n 個として数えていたので、BPM が n / (n - 1) 倍に高く出ていた）
            Bpm = (60f / (total / 1000f) * (times.Count - 1)).ToString();
        }

        [RelayCommand]
        private void Clear()
        {
            times.Clear();
            Interval = "0";
            Bpm = "0";
        }
    }
}
