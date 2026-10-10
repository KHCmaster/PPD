using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace FolderDiffChecker.Models
{
    /// <summary>
    /// 配布サーバーの接続先と、つなぐための鍵（PuTTY の秘密鍵）の場所です。テキストファイルから読みます。
    /// </summary>
    /// <remarks>
    /// 1 行に「キー=値」を書く。# で始まる行と空の行は読まない。キーは HostName、Port、HostKeyFingerprint、CredentialsPath。
    /// CredentialsPath が相対パスのときは、設定ファイルのあるフォルダーから探す。
    /// </remarks>
    sealed class UploadSettings
    {
        /// <summary>
        /// 設定ファイルの名前です（FolderDiffChecker を起動したフォルダーに置く）。
        /// </summary>
        public const string FileName = "upload_settings.txt";

        private UploadSettings(string hostName, int port, string hostKeyFingerprint, string credentialsPath)
        {
            HostName = hostName;
            Port = port;
            HostKeyFingerprint = hostKeyFingerprint;
            CredentialsPath = credentialsPath;
        }

        public string HostName { get; }

        public int Port { get; }

        /// <summary>
        /// サーバーのホスト鍵の指紋です（ほかのサーバーにつながないように確かめる）。
        /// </summary>
        public string HostKeyFingerprint { get; }

        /// <summary>
        /// 秘密鍵のファイルのフルパスです。
        /// </summary>
        public string CredentialsPath { get; }

        /// <summary>
        /// 設定ファイルを読みます。
        /// </summary>
        /// <param name="error">読めなかったときの理由。</param>
        /// <returns>読めなかったときは null。</returns>
        public static UploadSettings Load(string path, out string error)
        {
            if (!File.Exists(path))
            {
                error = String.Format("Locate {0}.", path);
                return null;
            }
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in File.ReadAllLines(path))
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                {
                    continue;
                }
                var index = trimmed.IndexOf('=');
                if (index <= 0)
                {
                    error = String.Format("{0}: invalid line \"{1}\".", path, trimmed);
                    return null;
                }
                values[trimmed[..index].Trim()] = trimmed[(index + 1)..].Trim();
            }

            foreach (var key in new[] { "HostName", "Port", "HostKeyFingerprint", "CredentialsPath" })
            {
                if (!values.TryGetValue(key, out var value) || value.Length == 0)
                {
                    error = String.Format("{0}: {1} is missing.", path, key);
                    return null;
                }
            }
            if (!int.TryParse(values["Port"], NumberStyles.None, CultureInfo.InvariantCulture, out var port) || port <= 0 || port > 65535)
            {
                error = String.Format("{0}: Port must be a number from 1 to 65535.", path);
                return null;
            }

            var directory = Path.GetDirectoryName(Path.GetFullPath(path));
            var credentialsPath = Path.GetFullPath(values["CredentialsPath"], directory);
            if (!File.Exists(credentialsPath))
            {
                error = String.Format("Locate {0}.", credentialsPath);
                return null;
            }

            error = null;
            return new UploadSettings(values["HostName"], port, values["HostKeyFingerprint"], credentialsPath);
        }
    }
}
