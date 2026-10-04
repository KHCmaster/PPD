using CommunityToolkit.Mvvm.ComponentModel;
using PPDConfiguration;
using System.IO;

namespace Effect2DEditor.ViewModels
{
    /// <summary>
    /// 画面の文字です。<c>{Binding Texts[キー]}</c> で今の言語の文字を表示します。言語を変えると表示も変わります。
    /// </summary>
    /// <remarks>
    /// 言語ファイルにないキーは、日本語のファイルの文字にする（それもなければキーをそのまま出す）。
    /// </remarks>
    public class LanguageTexts : ObservableObject
    {
        const string fallbackFileName = "lang_Effect2DEditor_jp.ini";

        /// <summary>
        /// すべての画面で共有する 1 つのインスタンスです（言語を変えたときにまとめて表示し直すため）。
        /// </summary>
        public static LanguageTexts Instance { get; } = new();

        SettingReader reader;
        SettingReader fallback;

        private LanguageTexts()
        {
        }

        public string this[string key]
        {
            get
            {
                var text = reader?[key];
                if (string.IsNullOrEmpty(text))
                {
                    fallback ??= LoadFallback();
                    text = fallback?[key];
                }
                return string.IsNullOrEmpty(text) ? key : text;
            }
        }

        /// <summary>
        /// 言語ファイルを読み込み、表示し直させます。ファイルがないときは今の言語のままにします。
        /// </summary>
        public void Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }
            reader = new SettingReader(File.ReadAllText(fileName));
            OnPropertyChanged("Item[]");
        }

        private static SettingReader LoadFallback()
        {
            var path = Path.Combine("Lang", fallbackFileName);
            return File.Exists(path) ? new SettingReader(File.ReadAllText(path)) : new SettingReader("");
        }
    }
}
