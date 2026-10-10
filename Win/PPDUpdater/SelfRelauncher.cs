using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PPDUpdater
{
    /// <summary>
    /// PPDUpdater を一時フォルダーにコピーして、そこから動き直します。
    /// </summary>
    /// <remarks>
    /// 動いているプログラムと、それが読み込んだ dll は上書きできないので、PPD のフォルダーから動いたままだと、
    /// PPDUpdater 自身や、PPDUpdater の使う dll（SharpZipLib など）を更新できない。
    /// 動き直すときも今のフォルダー（PPD のフォルダー。更新ファイルの置き場や設定はここからの相対パス）は変えず、
    /// 元のフォルダーを引数で渡して、インストール先をそこから決める。
    /// </remarks>
    static class SelfRelauncher
    {
        /// <summary>
        /// 元のフォルダーを渡す引数です（この次の引数がフォルダー）。
        /// </summary>
        public const string FromArgument = "--from";

        /// <summary>
        /// この環境変数があるときは、動き直さない（UI の自動テストや、開発のとき）。
        /// </summary>
        public const string DisableVariable = "PPDUPDATER_NO_RELAUNCH";

        /// <summary>
        /// 一時フォルダーの中の、PPDUpdater のコピーを置くフォルダーです。
        /// </summary>
        public static string TempRoot
        {
            get { return Path.Combine(Path.GetTempPath(), "PPDUpdater"); }
        }

        /// <summary>
        /// 一時フォルダーにコピーして動き直します。動き直したときは、このプロセスはすぐに終わる。
        /// </summary>
        /// <returns>動き直したかどうか。動き直せなかったときは false（その場で動く）。</returns>
        public static bool TryRelaunch(string[] args)
        {
            var from = GetFromDirectory(args);
            if (from != null)
            {
                // 動き直した側。インストール先は元のフォルダーから決め、前に使った一時フォルダーを消す
                Utility.SetApplicationDirectory(from);
                DeleteOldCopies(TempRoot, AppContext.BaseDirectory);
                return false;
            }
            if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable(DisableVariable)))
            {
                return false;
            }
            try
            {
                var sourceDir = AppContext.BaseDirectory;
                var targetDir = Path.Combine(TempRoot, Guid.NewGuid().ToString("N"));
                foreach (var file in GetFilesToCopy(sourceDir, "PPDUpdater"))
                {
                    var target = Path.Combine(targetDir, file);
                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    File.Copy(Path.Combine(sourceDir, file), target, true);
                }
                var startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(targetDir, "PPDUpdater.exe"),
                    WorkingDirectory = Environment.CurrentDirectory,
                    // .NET Framework の既定値と同じ。管理者権限が必要な場合も起動できるようにシェル経由で起動する
                    UseShellExecute = true,
                };
                foreach (var arg in args)
                {
                    startInfo.ArgumentList.Add(arg);
                }
                startInfo.ArgumentList.Add(FromArgument);
                startInfo.ArgumentList.Add(sourceDir);
                return Process.Start(startInfo) != null;
            }
            catch (Exception)
            {
                // コピーや起動ができないときは、その場で動く（自分の使う dll は更新できない）
                return false;
            }
        }

        /// <summary>
        /// 引数から、元のフォルダーを取り出します。
        /// </summary>
        /// <returns>動き直したのでないときは null。</returns>
        public static string GetFromDirectory(string[] args)
        {
            var index = Array.IndexOf(args, FromArgument);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }

        /// <summary>
        /// アプリを動かすのに要るファイル（アプリの exe、dll、設定と、deps.json に書かれた dll）を、フォルダーからの相対パスで返します。
        /// </summary>
        public static string[] GetFilesToCopy(string dir, string applicationName)
        {
            var files = new List<string>();
            foreach (var extension in new[] { ".exe", ".dll", ".runtimeconfig.json", ".deps.json", ".dll.config" })
            {
                var file = applicationName + extension;
                if (File.Exists(Path.Combine(dir, file)))
                {
                    files.Add(file);
                }
            }
            var depsPath = Path.Combine(dir, applicationName + ".deps.json");
            if (File.Exists(depsPath))
            {
                using var document = JsonDocument.Parse(File.ReadAllText(depsPath));
                foreach (var target in document.RootElement.GetProperty("targets").EnumerateObject())
                {
                    foreach (var library in target.Value.EnumerateObject())
                    {
                        // プロジェクトやパッケージの dll は、deps.json のパスにかかわらず、アプリと同じフォルダーにある
                        foreach (var kind in new[] { "runtime", "native" })
                        {
                            if (library.Value.TryGetProperty(kind, out var assets))
                            {
                                files.AddRange(assets.EnumerateObject().Select(asset => Path.GetFileName(asset.Name)));
                            }
                        }
                        if (library.Value.TryGetProperty("runtimeTargets", out var runtimeTargets))
                        {
                            foreach (var asset in runtimeTargets.EnumerateObject())
                            {
                                var rid = asset.Value.GetProperty("rid").GetString();
                                if (rid == "win" || rid == "win-x64")
                                {
                                    files.Add(asset.Name.Replace('/', Path.DirectorySeparatorChar));
                                }
                            }
                        }
                    }
                }
            }
            return [.. files.Distinct(StringComparer.OrdinalIgnoreCase).Where(file => File.Exists(Path.Combine(dir, file)))];
        }

        /// <summary>
        /// 前に動き直したときの一時フォルダーを消します（使っているものは消せないので、そのままにする）。
        /// </summary>
        /// <param name="root">コピーを置くフォルダー（<see cref="TempRoot"/>）。</param>
        /// <param name="currentDir">今動いているコピーのフォルダー（消さない）。</param>
        public static void DeleteOldCopies(string root, string currentDir)
        {
            if (!Directory.Exists(root))
            {
                return;
            }
            var current = Path.GetFullPath(currentDir).TrimEnd(Path.DirectorySeparatorChar);
            foreach (var dir in Directory.GetDirectories(root))
            {
                if (String.Equals(Path.GetFullPath(dir).TrimEnd(Path.DirectorySeparatorChar), current, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
    }
}
