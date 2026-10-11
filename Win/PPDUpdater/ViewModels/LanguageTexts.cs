using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace PPDUpdater.ViewModels
{
    /// <summary>
    /// 画面の文字です。<c>{Binding Texts[キー]}</c> で今の言語の文字を表示します。言語を変えると表示も変わります。
    /// </summary>
    /// <remarks>
    /// 言語ファイル（Lang\lang_PPDUpdater_*.ini）にないキーは日本語で表示する。
    /// </remarks>
    public class LanguageTexts : ObservableObject
    {
        static readonly Dictionary<string, string> defaultTexts = new()
        {
            ["Language"] = "言語",
            ["File"] = "ファイル",
            ["Finish"] = "終了",
            ["AskFinish"] = "未インストールのバージョンがありますが、終了してもよろしいですか？",
            ["AskFinishCaption"] = "終了確認",
            ["Download"] = "ダウンロード",
            ["Install"] = "インストール",
            ["AlreadyLatest"] = "既に最新版です",
            ["UpdateListFail"] = "アップデートリストの取得に失敗しました",
            ["Waiting"] = "待機中",
            ["Downloading"] = "ダウンロード中",
            ["DownloadFinished"] = "ダウンロード完了",
            ["Installing"] = "インストール中",
            ["InstallFinished"] = "インストール完了",
            ["UpdateFinished"] = "アップデートが正常に終了しました",
            ["Header1"] = "バージョン",
            ["Header2"] = "状態",
            ["Label1"] = "現在のバージョン",
            ["Label3"] = "更新を確認中…",
            ["GroupBox1"] = "ログ",
            ["Button1"] = "閉じる",
            ["Button2"] = "表示/非表示",
        };

        SettingReader setting;

        public string this[string key]
        {
            get
            {
                var text = setting?[key];
                return string.IsNullOrEmpty(text) && defaultTexts.TryGetValue(key, out var defaultText) ? defaultText : text;
            }
        }

        /// <summary>
        /// 言語ファイルを読み込んで、表示し直させます。ファイルがないときは何もしません。
        /// </summary>
        public void Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }
            setting = new SettingReader(File.ReadAllText(fileName));
            OnPropertyChanged("Item[]");
        }
    }

    /// <summary>
    /// 言語メニューの 1 項目です。
    /// </summary>
    public partial class LanguageOption(string displayName, string fileName, string iso) : ObservableObject
    {
        public string DisplayName { get; } = displayName;

        public string FileName { get; } = fileName;

        /// <summary>
        /// 設定に保存する言語の名前（lang_PPDUpdater_{ISO}.ini）です。
        /// </summary>
        public string Iso { get; } = iso;

        [ObservableProperty]
        public partial bool IsChecked { get; set; }
    }

    /// <summary>
    /// リソースの中から DataContext を使うための入れ物です（DataGrid の列などは DataContext を受け継がない）。
    /// </summary>
    public class BindingProxy : Freezable
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

        public object Data
        {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }
}
