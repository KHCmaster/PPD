using CommunityToolkit.Mvvm.ComponentModel;
using PPDFramework;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// 動画の上下左右を切り取る幅（ピクセル）を設定するダイアログです。
    /// </summary>
    public partial class MovieCutViewModel(MovieTrimmingData trimmingData) : DialogViewModel
    {
        [ObservableProperty]
        private string left = trimmingData?.Left.ToString() ?? "0";

        [ObservableProperty]
        private string right = trimmingData?.Right.ToString() ?? "0";

        [ObservableProperty]
        private string top = trimmingData?.Top.ToString() ?? "0";

        [ObservableProperty]
        private string bottom = trimmingData?.Bottom.ToString() ?? "0";

        /// <summary>
        /// 設定ファイルライターにも反映させるかです。
        /// </summary>
        [ObservableProperty]
        private bool applyToIni = PPDStaticSetting.ApplyToIniMovieTrimming;

        /// <summary>
        /// 入力した切り取りの幅です（OK を押したときに決まる）。
        /// </summary>
        public MovieTrimmingData TrimmingData
        {
            get;
            private set;
        }

        // 数字でない欄は 0 にして知らせる（以前と同じく、閉じるのはやめない）
        protected override bool Validate()
        {
            TrimmingData = new MovieTrimmingData(Parse(Top), Parse(Left), Parse(Right), Parse(Bottom));
            return true;
        }

        private float Parse(string text)
        {
            if (float.TryParse(text, out float value))
            {
                return value;
            }
            // 以前と同じく、どの欄でも「左」の名前で知らせる
            ShowMessage(Texts["CutNumberError"] + ":" + Texts["CutLabel1"]);
            return 0;
        }
    }
}
