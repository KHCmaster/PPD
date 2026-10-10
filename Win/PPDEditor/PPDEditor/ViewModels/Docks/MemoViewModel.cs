using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// プロジェクトのメモです。
    /// </summary>
    public partial class MemoViewModel : DockViewModel
    {
        const float MinFontSize = 5;
        const float MaxFontSize = 100;

        [ObservableProperty]
        private string memoText = "";

        /// <summary>
        /// 文字の大きさ（ポイント）です。設定ファイルには以前と同じくポイントで保存します。
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FontSizePixels))]
        private float memoFontSize = 9;

        /// <summary>
        /// 画面に使う文字の大きさ（ピクセル）です。
        /// </summary>
        public double FontSizePixels
        {
            get { return MemoFontSize * 96 / 72; }
        }

        partial void OnMemoTextChanged(string value)
        {
            OnChanged();
        }

        /// <summary>
        /// 文字の大きさを変えます。範囲外のときは変えません。
        /// </summary>
        public void SetFontSize(float size)
        {
            if (size >= MinFontSize && size <= MaxFontSize)
            {
                MemoFontSize = size;
                PPDStaticSetting.MemoFontSize = size;
            }
        }

        [RelayCommand]
        private void IncreaseFontSize()
        {
            SetFontSize(MemoFontSize + 1);
        }

        [RelayCommand]
        private void DecreaseFontSize()
        {
            SetFontSize(MemoFontSize - 1);
        }
    }
}
