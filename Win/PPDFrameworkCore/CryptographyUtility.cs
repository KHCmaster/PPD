using System;
using System.Security.Cryptography;
using System.Text;

namespace PPDFrameworkCore
{
    /// <summary>
    /// 暗号周りのクラスです。
    /// </summary>
    public static class CryptographyUtility
    {
        /// <summary>
        /// 文字列をパースしてバイトにします。
        /// </summary>
        /// <param name="str">文字列。</param>
        /// <returns>バイトデータ。</returns>
        public static byte[] Parsex2String(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return new byte[0];
            }

            byte[] ret = new byte[str.Length / 2];
            for (int i = 0; i < str.Length; i += 2)
            {
                ret[i / 2] = (byte)int.Parse(str.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return ret;
        }

        /// <summary>
        /// MD5ハッシュを計算します。
        /// </summary>
        /// <param name="str">文字列です。</param>
        /// <returns></returns>
        public static byte[] CalcMd5Hash(string str)
        {
            return CalcMd5Hash(str, Encoding.ASCII);
        }

        /// <summary>
        /// MD5ハッシュを計算します。
        /// </summary>
        /// <param name="str">文字列です。</param>
        /// <param name="encoding">エンコーディングです。</param>
        /// <returns></returns>
        public static byte[] CalcMd5Hash(string str, Encoding encoding)
        {
            return CalcMd5Hash(encoding.GetBytes(str));
        }

        /// <summary>
        /// MD5ハッシュを計算します。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] CalcMd5Hash(byte[] bytes)
        {
            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(bytes);
            }
        }

        /// <summary>
        /// MD5ハッシュを計算します。
        /// </summary>
        /// <param name="str">文字列です。</param>
        /// <returns></returns>
        public static string CalcMd5HashString(string str)
        {
            return CalcMd5HashString(str, Encoding.ASCII);
        }

        /// <summary>
        /// MD5ハッシュを計算します。
        /// </summary>
        /// <param name="str">文字列です。</param>
        /// <param name="encoding">エンコーディングです。</param>
        /// <returns></returns>
        public static string CalcMd5HashString(string str, Encoding encoding)
        {
            return CalcMd5HashString(encoding.GetBytes(str));
        }

        /// <summary>
        /// MD5ハッシュを計算します。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string CalcMd5HashString(byte[] bytes)
        {
            using (MD5 md5 = MD5.Create())
            {
                return Getx2Encoding(md5.ComputeHash(bytes));
            }
        }

        /// <summary>
        /// SHA256ハッシュを計算します。
        /// </summary>
        /// <param name="str">文字列です。</param>
        /// <returns></returns>
        public static byte[] CalcSha256Hash(string str)
        {
            return CalcSha256Hash(str, Encoding.ASCII);
        }

        /// <summary>
        /// SHA256ハッシュを計算します。
        /// </summary>
        /// <param name="str">文字列です。</param>
        /// <param name="encoding">エンコーディングです。</param>
        /// <returns></returns>
        public static byte[] CalcSha256Hash(string str, Encoding encoding)
        {
            return CalcSha256Hash(encoding.GetBytes(str));
        }

        /// <summary>
        /// SHA256ハッシュを計算します。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] CalcSha256Hash(byte[] bytes)
        {
            using (SHA256Managed sha = new SHA256Managed())
            {
                return sha.ComputeHash(bytes);
            }
        }

        /// <summary>
        /// SHA256ハッシュを計算します。
        /// </summary>
        /// <param name="str">文字列です。</param>
        /// <returns></returns>
        public static string CalcSha256HashString(string str)
        {
            return CalcSha256HashString(str, Encoding.ASCII);
        }

        /// <summary>
        /// SHA256ハッシュを計算します。
        /// </summary>
        /// <param name="str">文字列です。</param>
        /// <param name="encoding">エンコーディングです。</param>
        /// <returns></returns>
        public static string CalcSha256HashString(string str, Encoding encoding)
        {
            return CalcSha256HashString(encoding.GetBytes(str));
        }

        /// <summary>
        /// SHA256ハッシュを計算します。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string CalcSha256HashString(byte[] bytes)
        {
            using (SHA256Managed sha = new SHA256Managed())
            {
                return Getx2Encoding(sha.ComputeHash(bytes));
            }
        }

        /// <summary>
        /// １６進数表示を取得します。
        /// </summary>
        /// <param name="bytes">バイト。</param>
        /// <returns></returns>
        public static string Getx2Encoding(byte[] bytes)
        {
            var ret = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                ret.AppendFormat("{0:x2}", bytes[i]);
            }

            return ret.ToString();
        }

        /// <summary>
        /// Base64文字列を取得します。
        /// </summary>
        /// <param name="bytes">バイト。</param>
        /// <returns></returns>
        public static string GetBase64String(byte[] bytes)
        {
            if (bytes == null)
            {
                return "";
            }
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Base64文字列からバイトを取得します。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] GetBytesFromBase64String(string str)
        {
            return Convert.FromBase64String(str);
        }
    }
}
