using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PPDUpdater
{
    /// <summary>
    /// 設定読み取りクラス
    /// </summary>
    public class SettingReader
    {
        Dictionary<string, string> dictionary;

        /// <summary>
        /// iniフォーマットかどうか
        /// </summary>
        public bool IsIniFormat
        {
            get;
            private set;
        }

        /// <summary>
        /// キーを元に文字列を取得します
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                return ReadString(key);
            }
            set
            {
                ReplaceOrAdd(key, value);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="data">解析文字列</param>
        public SettingReader(string data)
        {
            data = data.Replace("\r\n", "\r").Replace("\r", "\n");
            dictionary = new Dictionary<string, string>(10);
            if (data.Contains("[formattype]"))
            {
                IsIniFormat = true;
                ReadAsIni(data);
            }
            else
            {
                ReadAsNormal(data);
            }
        }

        private void ReadAsIni(string data)
        {
            var sectionregex = new Regex("^\\[\\w*\\]");
            var pairregex = new Regex("^(?<key>[^=]+?) *= *(?<value>.*)$");
            foreach (string line in data.Split('\n'))
            {
                if (line.StartsWith("#")) continue;
                if (sectionregex.Match(line).Success) continue;
                var m = pairregex.Match(line);
                if (m.Success)
                {
#if DEBUG
                    if (dictionary.ContainsKey(m.Groups["key"].Value))
                    {
                        Console.WriteLine("{0} is multiple defined", m.Groups["key"].Value);
                    }
#endif
                    ReplaceOrAdd(m.Groups["key"].Value, m.Groups["value"].Value);
                }
            }
        }

        private void ReadAsNormal(string data)
        {
            var regex = new Regex("^\\[(\\w+)\\](.*)");
            var key = "";
            var datas = new List<string>();
            Action addData = () =>
            {
                if (datas.Count > 0 && !String.IsNullOrEmpty(key))
                {
#if DEBUG
                    if (dictionary.ContainsKey(key))
                    {
                        Console.WriteLine("{0} is multiple defined", key);
                    }
#endif
                    ReplaceOrAdd(key, String.Join("\n", datas.ToArray()));
                }
                datas.Clear();
                key = "";
            };
            foreach (var line in data.Split('\n'))
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    addData();
                    key = match.Groups[1].Value;
                    datas.Add(match.Groups[2].Value);
                }
            }
            addData();
        }

        /// <summary>
        /// ディクショナリー
        /// </summary>
        public Dictionary<string, string> Dictionary
        {
            get
            {
                return dictionary;
            }
        }

        /// <summary>
        /// 書き込むか置き換えます
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">バリュー</param>
        public void ReplaceOrAdd(string key, string value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        /// <summary>
        /// 書き込むか置き換えます
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">バリュー</param>
        public void ReplaceOrAdd(string key, int value)
        {
            ReplaceOrAdd(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 書き込むか置き換えます
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">バリュー</param>
        public void ReplaceOrAdd(string key, float value)
        {
            ReplaceOrAdd(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 書き込むか置き換えます
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">バリュー</param>
        public void ReplaceOrAdd(string key, double value)
        {
            ReplaceOrAdd(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 書き込むか置き換えます
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">バリュー</param>
        public void ReplaceOrAdd(string key, bool value)
        {
            ReplaceOrAdd(key, value ? "1" : "0");
        }

        /// <summary>
        /// 設定を取得する
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns></returns>
        public string ReadString(string key)
        {
            if (!dictionary.TryGetValue(key, out string ret))
            {
#if DEBUG
                Console.WriteLine("No Key:{0}", key);
#endif
                ret = "";

            }
            return ret.Trim();
        }

        /// <summary>
        /// intで設定を取得する
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="defaultValue">規定値</param>
        /// <returns></returns>
        public int ReadInt(string key, int defaultValue)
        {
            if (int.TryParse(ReadString(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out int val))
            {
                return val;
            }
            return defaultValue;
        }

        /// <summary>
        /// floatで設定を取得する
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="defaultValue">規定値</param>
        /// <returns></returns>
        public float ReadFloat(string key, float defaultValue)
        {
            if (float.TryParse(ReadString(key), NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
            {
                return val;
            }
            return defaultValue;
        }

        /// <summary>
        /// doubleで設定を取得する
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="defaultValue">規定値</param>
        /// <returns></returns>
        public double ReadDouble(string key, double defaultValue)
        {
            if (double.TryParse(ReadString(key), NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
            {
                return val;
            }
            return defaultValue;
        }

        /// <summary>
        /// boolで設定を取得する
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns></returns>
        public bool ReadBoolean(string key)
        {
            return ReadString(key) == "1";
        }

        /// <summary>
        /// キーを含むかどうかを返します。
        /// </summary>
        /// <param name="key">キー。</param>
        /// <returns>キーを含むかどうか。</returns>
        public bool ContainsKey(string key)
        {
            return dictionary.ContainsKey(key);
        }
    }
}
