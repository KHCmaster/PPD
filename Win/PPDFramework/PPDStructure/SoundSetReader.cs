using System;
using System.IO;

namespace PPDFramework.PPDStructure
{
    /// <summary>
    /// サウンドセット読み取りクラス
    /// </summary>
    public static class SoundSetReader
    {
        /// <summary>
        /// 読み取る
        /// </summary>
        /// <param name="path">サウンドセットのパス</param>
        /// <returns></returns>
        public static string[] Read(string path)
        {
            string[] ret = new string[0];
            if (File.Exists(path))
            {
                var sr = new StreamReader(path);
                var s = sr.ReadToEnd();
                sr.Close();
                s = s.Replace("\r\n", "\r").Replace("\r", "\n").Trim();
                ret = s.Split('\n');
                if (ret.Length == 1 && String.IsNullOrEmpty(ret[0]))
                {
                    ret = new string[0];
                }
            }
            return ret;
        }
    }
}
