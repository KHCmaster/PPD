using PPDConfiguration;
using System;
using System.Collections.Generic;
using System.IO;

namespace PPDFramework
{
    /// <summary>
    /// 例外の内容プロバイダクラス
    /// </summary>
    public class PPDExceptionContentProvider
    {
        private static PPDExceptionContentProvider provider = new PPDExceptionContentProvider();
        Dictionary<PPDExceptionType, string> dictionary;
        private PPDExceptionContentProvider()
        {
            dictionary = new Dictionary<PPDExceptionType, string>();
            var types = (PPDExceptionType[])Enum.GetValues(typeof(PPDExceptionType));
            var langFilePath = Path.Combine("Lang", String.Format("lang_{0}_{1}.ini", "PPD", PPDSetting.Setting.LangISO));
            if (File.Exists(langFilePath))
            {
                var sr = new StreamReader(langFilePath);
                var st = sr.ReadToEnd();
                sr.Close();
                var setting = new SettingReader(st);
                foreach (PPDExceptionType type in types)
                {
                    var data = setting.ReadString(type.ToString());
                    if (!dictionary.ContainsKey(type)) dictionary.Add(type, data);
                }
            }
            else
            {
                foreach (PPDExceptionType type in types)
                {
                    string data = "";
                    switch (type)
                    {
                        case PPDExceptionType.CannotOpenMovie:
                            data = "動画を開けませんでした";
                            break;
                        case PPDExceptionType.FatalError:
                            data = "致命的なエラーが発生しました。error.logを製作者にお送りください";
                            break;
                        case PPDExceptionType.NoMovieFileInDirectory:
                            data = "フォルダ内にmovie.*の動画ファイルがありませんでした";
                            break;
                        case PPDExceptionType.NoSong:
                            data = "songsフォルダ内には少なくとも一つ以上のデータが必要です";
                            break;
                        case PPDExceptionType.SkinIsNotCorrectlyImplemented:
                            data = "スキンが正しく実装されていません。スキン作成者に問い合わせてください";
                            break;
                        case PPDExceptionType.SoundReadError:
                            data = "サウンドデータの読み込みに失敗しました。";
                            break;
                        case PPDExceptionType.InvalidString:
                            data = "無効な文字列です";
                            break;
                        case PPDExceptionType.ImageReadError:
                            data = "画像ファイルの読み込みに失敗しました";
                            break;
                    }
                    if (!dictionary.ContainsKey(type)) dictionary.Add(type, data);
                }
            }
        }

        /// <summary>
        /// 内容取得
        /// </summary>
        /// <param name="type">タイプ</param>
        /// <returns></returns>
        public string GetContent(PPDExceptionType type)
        {
            if (dictionary.ContainsKey(type))
            {
                return dictionary[type];
            }
            else return "";
        }

        /// <summary>
        /// プロバイダ
        /// </summary>
        public static PPDExceptionContentProvider Provider
        {
            get
            {
                return provider;
            }
        }
    }
}
