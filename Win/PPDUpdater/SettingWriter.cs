using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PPDUpdater
{
    /// <summary>
    /// 設定を書き込むためのクラスです
    /// </summary>
    public class SettingWriter : IDisposable
    {
        /// <summary>
        /// 破棄されたか
        /// </summary>
        public bool disposed;

        private bool iniformat;
        private string filepath;
        private Dictionary<string, Dictionary<string, string>> inipool = new Dictionary<string, Dictionary<string, string>>();
        StreamWriter sw;

        /// <summary>
        /// コンストラクタです
        /// </summary>
        /// <param name="filename">ファイルパス</param>
        /// <param name="iniformat">iniフォーマットとして書き込むか</param>
        public SettingWriter(string filename, bool iniformat)
        {
            sw = new StreamWriter(filename);
            this.iniformat = iniformat;
            if (iniformat)
            {
                filepath = Path.GetFullPath(filename);
                WriteIni("isini", "1", "formattype");
            }
        }

        /// <summary>
        /// データを書き込みます
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">バリュー</param>
        public void Write(string key, string value)
        {
            if (iniformat)
            {
                WriteIni(key, value);
            }
            else
            {
                if (sw != null)
                {
                    sw.Write("[");
                    sw.Write(key);
                    sw.Write("]");
                    sw.Write(value);
                    sw.Write("\r\n");
                }
            }
        }

        /// <summary>
        /// データを書き込みます
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">バリュー</param>
        public void Write(string key, int value)
        {
            Write(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// データを書き込みます
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">バリュー</param>
        public void Write(string key, float value)
        {
            Write(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// データを書き込みます
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">バリュー</param>
        public void Write(string key, double value)
        {
            Write(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// データを書き込みます
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">バリュー</param>
        public void Write(string key, bool value)
        {
            Write(key, value ? "1" : "0");
        }

        private void WriteIni(string key, string value)
        {
            WriteIni(key, value, "setting");
        }

        private void WriteIni(string key, string value, string section)
        {
            Dictionary<string, string> pool;
            if (inipool.ContainsKey(section))
            {
                pool = inipool[section];
            }
            else
            {
                pool = new Dictionary<string, string>();
                inipool.Add(section, pool);
            }
            if (!pool.ContainsKey(key))
            {
                pool.Add(key, value);
            }
            else
            {
                pool[key] = value;
            }
        }

        /// <summary>
        /// 閉じます
        /// </summary>
        public void Close()
        {
            if (sw == null) return;
            if (iniformat)
            {
                WriteDataAsIni();
            }
            sw.Close();
            sw = null;
        }

        private void WriteDataAsIni()
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> section in inipool)
            {
                sw.WriteLine(String.Format("[{0}]", section.Key));
                foreach (KeyValuePair<string, string> kvp in section.Value)
                {
                    sw.WriteLine(String.Format("{0}={1}", kvp.Key, kvp.Value));
                }
            }
        }

        #region IDisposable メンバ

        /// <summary>
        /// 破棄します
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (sw != null)
                    {
                        Close();
                        sw = null;
                    }
                }
            }
            disposed = true;
        }

        #endregion
    }
}
