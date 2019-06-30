using System;
using System.IO;
using System.Linq;

namespace PPDConfiguration
{
    /// <summary>
    /// 言語ファイルを読み取るクラスです
    /// PPD.iniのLanguageを参照します
    /// </summary>
    public class LanguageReader
    {
        /// <summary>
        /// デフォルトの言語ISOです。
        /// </summary>
        public const string mainLangIso = "en";

        SettingReader mainAnalyzer;
        SettingReader analyzer;
        SettingReader[] appendedAnalyzers;
        SettingReader[] appendedMainAnalyzers;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="componentName">コンポーネント名(ex. PPD)</param>
        /// <param name="langISO">言語ファイルISO</param>
        public LanguageReader(string componentName, string langISO)
        {
            var path = GetLangPath(componentName, langISO);
            if (File.Exists(path))
            {
                analyzer = new SettingReader(File.ReadAllText(path));
                appendedAnalyzers = GetAppendedLangPaths(componentName, langISO).Select(s => new SettingReader(File.ReadAllText(s))).ToArray();
            }
            path = GetLangPath(componentName, mainLangIso);
            if (File.Exists(path))
            {
                mainAnalyzer = new SettingReader(File.ReadAllText(path));
                appendedMainAnalyzers = GetAppendedLangPaths(componentName, mainLangIso).Select(s => new SettingReader(File.ReadAllText(s))).ToArray();
            }
        }

        /// <summary>
        /// 言語ファイルのパスを取得します。
        /// </summary>
        /// <param name="componentName">コンポーネント名。</param>
        /// <param name="langISO">言語ファイルISO。</param>
        /// <returns></returns>
        private string GetLangPath(string componentName, string langISO)
        {
            return String.Format("Lang\\lang_{0}_{1}.ini", componentName, langISO);
        }

        private string[] GetAppendedLangPaths(string componentName, string langIso)
        {
            return Directory.GetFiles("Lang", String.Format("lang_{0}_{1}_*.ini", componentName, langIso));
        }

        /// <summary>
        /// キーに対応する文字列を取得します
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                return ReadValue(key);
            }
        }

        /// <summary>
        /// キーに対応する文字列を取得します
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string ReadValue(string key)
        {
            if (appendedAnalyzers != null)
            {
                foreach (var a in appendedAnalyzers)
                {
                    if (a.ContainsKey(key))
                    {
                        return a[key];
                    }
                }
            }
            if (analyzer != null && analyzer.ContainsKey(key))
            {
                return analyzer.ReadString(key);
            }
            if (appendedMainAnalyzers != null)
            {
                foreach (var a in appendedMainAnalyzers)
                {
                    if (a.ContainsKey(key))
                    {
                        return a[key];
                    }
                }
            }
            if (mainAnalyzer != null && mainAnalyzer.ContainsKey(key))
            {
                return mainAnalyzer.ReadString(key);
            }
#if DEBUG
            Console.WriteLine("No key:{0}", key);
#endif
            return "";
        }
    }
}
