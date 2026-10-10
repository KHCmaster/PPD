using System;
using System.Collections.Generic;
using System.IO;

namespace PPDUpdater.Model
{
    /// <summary>
    /// PPDUpdater の設定（PPDUpdater.ini）と、ログ（update.log）を扱います。
    /// </summary>
    static class UpdaterSetting
    {
        const string iniFileName = "PPDUpdater.ini";
        const string logFileName = "update.log";

        /// <summary>
        /// 更新リストの URL（{0} にチャンネルが入る）と、チャンネルと、言語を読み込みます。
        /// </summary>
        public static (List<string> UrlList, string Channel, string Language) Load()
        {
            if (!File.Exists(iniFileName))
            {
                return ([], null, "");
            }
            var setting = new SettingReader(File.ReadAllText(iniFileName));
            return ([.. setting["UrlList"].Split('\n')], setting["Channel"], setting["Language"]);
        }

        /// <summary>
        /// 言語を保存します（ほかの設定はそのまま残します）。
        /// </summary>
        public static void SaveLanguage(string language)
        {
            var setting = new SettingReader(File.Exists(iniFileName) ? File.ReadAllText(iniFileName) : "");
            setting.ReplaceOrAdd("Language", language);
            using SettingWriter sw = new(iniFileName, false);
            foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
            {
                sw.Write(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// ログを書き足します。大きくなりすぎたら消してから書きます。
        /// </summary>
        public static void AppendLog(string log)
        {
            if (File.Exists(logFileName) && new FileInfo(logFileName).Length >= 1000000)
            {
                File.Delete(logFileName);
            }
            using StreamWriter sw = new(logFileName, true);
            sw.WriteLine(DateTime.Now);
            sw.WriteLine(log);
        }
    }
}
