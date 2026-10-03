using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// 遠さ（0～1）を設定するダイアログです。画面では 0～100 で表示します。
    /// </summary>
    public partial class FarnessSettingViewModel(float farness) : DialogViewModel
    {
        [ObservableProperty]
        private int percent = (int)Math.Min(farness * 100, 100);

        public float Farness
        {
            get { return Percent / 100f; }
        }
    }
}
