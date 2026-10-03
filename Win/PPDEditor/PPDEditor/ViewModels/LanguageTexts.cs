using CommunityToolkit.Mvvm.ComponentModel;

namespace PPDEditor.ViewModels
{
    /// <summary>
    /// 画面の文字です。<c>{Binding Texts[キー]}</c> で今の言語の文字を表示します。言語を変えると表示も変わります。
    /// </summary>
    public class LanguageTexts : ObservableObject
    {
        /// <summary>
        /// すべての画面で共有する 1 つのインスタンスです（言語を変えたときにまとめて表示し直すため）。
        /// </summary>
        public static LanguageTexts Instance { get; } = new();

        private LanguageTexts()
        {
        }

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
}
