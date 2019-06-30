using System;
using System.Collections.Generic;
using System.Globalization;

namespace PPDFramework
{
    /// <summary>
    /// PPD実行時の引数のクラスです。
    /// </summary>
    public class PPDExecuteArg
    {
        private static PPDExecuteArg empty = new PPDExecuteArg(new string[0]);
        private Dictionary<string, string> dict;

        /// <summary>
        /// 空の引数を取得します。
        /// </summary>
        public static PPDExecuteArg Empty
        {
            get { return empty; }
        }

        /// <summary>
        /// 生の引数文字列を取得します。
        /// </summary>
        public string[] Args
        {
            get;
            private set;
        }

        /// <summary>
        /// コレクションの数を取得します。
        /// </summary>
        public int Count
        {
            get
            {
                return dict.Count;
            }
        }

        /// <summary>
        /// キーに対応する値を取得します。
        /// </summary>
        /// <param name="key">キー。</param>
        /// <returns>値。</returns>
        public string this[string key]
        {
            get
            {
                return GetValue(key);
            }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="args">引数。</param>
        public PPDExecuteArg(string[] args)
        {
            Args = args;
            dict = new Dictionary<string, string>();
            Initialize();
        }

        /// <summary>
        /// キーを含むかどうかを取得します。
        /// </summary>
        /// <param name="key">キー。</param>
        /// <returns>キーを含むかどうか。</returns>
        public bool ContainsKey(string key)
        {
            return dict.ContainsKey(key);
        }

        /// <summary>
        /// キーに対応する値を取得します。
        /// </summary>
        /// <param name="key">キー。</param>
        /// <returns>値。</returns>
        public string GetValue(string key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            else
            {
                throw new Exception(String.Format("Key:{0} is not found", key));
            }
        }

        private bool IsKey(string str)
        {
            return str.StartsWith("-") && !Double.TryParse(str, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out double val);
        }

        private void Initialize()
        {
            var iter = Args.GetEnumerator();
            if (!iter.MoveNext())
            {
                return;
            }
            while (true)
            {
                var key = (string)iter.Current;
                if (key.StartsWith("-"))
                {
                    key = key.Substring(1);
                    var hasNext = iter.MoveNext();
                    if (!hasNext || IsKey((string)iter.Current))
                    {
                        Add(key, "");
                        if (hasNext)
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    var val = (string)iter.Current;
                    Add(key, val);
                    if (!iter.MoveNext())
                    {
                        break;
                    }
                }
                else if (!iter.MoveNext())
                {
                    break;
                }
            }
        }

        private void Add(string key, string value)
        {
            if (dict.ContainsKey(key))
            {
                Console.WriteLine("key:{0} is duplicated.", key);
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }
    }
}
