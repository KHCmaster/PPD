using PPDConfiguration;
using System.Collections.Generic;
using System.IO;

namespace PPDInstaller.ViewModels
{
    /// <summary>
    /// 画面の文字です。<c>{Binding Texts[キー]}</c> で選んだ言語の文字を表示します。
    /// </summary>
    /// <remarks>
    /// 言語は起動したときに選ぶだけなので、変更は通知しない。言語ファイル（Lang\{番号}.ini）にないキーは日本語で表示する。
    /// </remarks>
    public class LanguageTexts
    {
        static readonly Dictionary<string, string> defaultTexts = new()
        {
            ["AskFinishText"] = "インストールを終了してもよろしいですか？",
            ["AskFinishCaption"] = "終了確認",
            ["NonExistDataFile"] = "Dataファイルが存在しないのでインストールを行うことができません。",
            ["Button1"] = "キャンセル",
            ["Button2"] = "次へ",
            ["Button3"] = "前へ",
            ["Finish"] = "終了",
            ["Config"] = "コンフィグ",
            ["KeyConfig"] = "キーコンフィグ",
            ["ScoreFolder"] = "譜面フォルダ",
            ["Language"] = "jp",
            ["FontName"] = "IPAGothic",
            ["Label1"] = "PPDのインストールを開始します",
            ["EasyInstall"] = "かんたんインストール",
            ["CustomInstall"] = "カスタムインストール",
            ["Label2"] = "インストールするフォルダを選択してください",
            ["Label5"] = "インストールリスト(必須)",
            ["Label6"] = "インストールリスト(任意)",
            ["CheckBox1"] = "PPD",
            ["CheckBox4"] = "IPA フォント",
            ["CheckBox6"] = "BMSTOPPD",
            ["CheckBox10"] = "Effect2DEditor",
            ["GroupBox1"] = "インストール済みならチェックされません",
            ["LinkLabel"] = "使用ライセンス(必ず確認してください)",
            ["RegisterStartMenu"] = "スタートメニューに登録する",
            ["Label7"] = "以下をダウンロードしインストールします",
            ["Necessary"] = "必須",
            ["Optional"] = "任意",
            ["Downloading"] = "ダウンロードしています...",
            ["Extracting"] = "解凍しています",
            ["Installing"] = "インストールしています...",
            ["Label8"] = "インストールが完了しました。",
            ["Label9"] = "以下の項目でインストールエラーがありました",
            ["Label3"] = "インストールは中断されました",
            ["Label4"] = "PPD、および関連ソフトのインストールは中断されました。プログラムをインストールする際には、再度このプログラムを実行してください。終了ボタンを押すことでセットアップを終了します。",
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
        /// 言語ファイルを読み込みます。ファイルがないときは何もしません。
        /// </summary>
        public void Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                setting = new SettingReader(File.ReadAllText(fileName));
            }
        }
    }
}
