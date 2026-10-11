using CommunityToolkit.Mvvm.ComponentModel;

namespace PPDConfig.ViewModels
{
    /// <summary>
    /// 画面の文字です。<c>{Binding Texts[キー]}</c> で今の言語の文字を表示します。言語を変えると表示も変わります。
    /// </summary>
    public class LanguageTexts : ObservableObject
    {
        public string this[string key]
        {
            get { return Utility.Language[key]; }
        }

        /// <summary>
        /// 言語を変えたので、表示し直させます。
        /// </summary>
        public void Refresh()
        {
            OnPropertyChanged("Item[]");
        }
    }

    /// <summary>
    /// 言語のメニューの 1 項目です。
    /// </summary>
    public partial class LanguageOption(string displayName, string iso) : ObservableObject
    {
        public string DisplayName
        {
            get;
        } = displayName;

        public string Iso
        {
            get;
        } = iso;

        [ObservableProperty]
        public partial bool IsChecked { get; set; }
    }
}
