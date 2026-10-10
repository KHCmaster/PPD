using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FolderDiffChecker.Models
{
    /// <summary>
    /// release\releases\{バージョン}\PPDInstaller(x64) のフォルダーを扱います（差分のコピー、zip、更新リスト）。
    /// </summary>
    static partial class ReleaseFolder
    {
        static readonly string[] assemblies = ["x64"];

        /// <summary>
        /// 新しい順に 2 つのバージョンのインストーラーフォルダーを探します。
        /// </summary>
        /// <returns>見つからないときは null。</returns>
        public static (string OldFolder, string NewFolder)? FindLatestPair(string releasesDir)
        {
            if (!Directory.Exists(releasesDir))
            {
                return null;
            }

            var regex = VersionRegex();
            var dirs = Directory.GetDirectories(releasesDir).Where(dir => regex.IsMatch(Path.GetFileName(dir))).ToList();
            dirs.Sort((s1, s2) => new Version(Path.GetFileName(s1)).CompareTo(new Version(Path.GetFileName(s2))));
            if (dirs.Count < 2)
            {
                return null;
            }
            return (Path.Combine(releasesDir, dirs[^2], "PPDInstaller(x64)"), Path.Combine(releasesDir, dirs[^1], "PPDInstaller(x64)"));
        }

        /// <summary>
        /// 差分のファイルとフォルダーを、新しいフォルダーからの相対パスのまま outDir にコピーします。
        /// </summary>
        public static void CopyDiff(string newFolder, IEnumerable<DiffModel> diffs, string outDir)
        {
            string srcDir = newFolder + (newFolder.EndsWith(Path.DirectorySeparatorChar) ? "" : Path.DirectorySeparatorChar.ToString());
            foreach (var model in diffs)
            {
                var path = Path.Combine(outDir, model.FilePath[srcDir.Length..]);
                if (model.IsFile)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.Copy(model.FilePath, path, true);
                }
                else
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        /// <summary>
        /// 差分を outPath\PPDDiff{バージョン}_x{ビット} にコピーします。
        /// </summary>
        /// <returns>新しいフォルダーの名前がバージョンの形でないときは false。</returns>
        public static bool CopyDiffToVersionFolder(string newFolder, IEnumerable<DiffModel> diffs, string outPath)
        {
            var m = InstallerWithArchitectureRegex().Match(newFolder);
            if (!m.Success)
            {
                return false;
            }
            var outDir = Path.Combine(outPath, String.Format("PPDDiff{0}_x{1}", JoinVersion(m, ""), m.Groups[5].Value));
            Directory.CreateDirectory(outDir);
            CopyDiff(newFolder, diffs, outDir);
            return true;
        }

        /// <summary>
        /// インストーラーと差分の zip を作ります。
        /// </summary>
        public static bool ZipAll(string newFolder)
        {
            var m = InstallerRegex().Match(newFolder);
            if (!m.Success)
            {
                return false;
            }

            var version = JoinVersion(m, "");

            // installer zip
            var dir = Path.GetDirectoryName(newFolder);
            foreach (var assemble in assemblies)
            {
                var installerDir = Path.Combine(dir, String.Format("PPDInstaller({0})", assemble));
                Utility.Zip(Path.Combine(installerDir, "Data.pak"), Path.Combine(installerDir, "Data"));
                var installerName = String.Format("PPDInstaller{0}({1}).zip", version, assemble);
                Utility.Zip(Path.Combine(dir, installerName), installerDir, [DataFolderRegex()]);
            }

            // diff zip
            dir = Path.GetDirectoryName(Path.GetDirectoryName(newFolder));
            foreach (var assemble in assemblies)
            {
                var childDirName = String.Format("PPDDiff{0}_{1}", version, assemble);
                var zipName = String.Format("PPDDiff{0}_{1}.zip", version, assemble);
                Utility.Zip(Path.Combine(dir, childDirName, zipName), Path.Combine(dir, childDirName, "Data"));
            }
            return true;
        }

        /// <summary>
        /// 古いバージョンの更新リストに新しいバージョンを足して、新しいバージョンのフォルダーに保存します。
        /// </summary>
        public static bool UpdateList(string oldFolder, string newFolder)
        {
            var m = InstallerRegex().Match(newFolder);
            if (!m.Success)
            {
                return false;
            }

            var dir = Path.GetDirectoryName(oldFolder);
            var targetDir = Path.GetDirectoryName(newFolder);
            var fileNames = new string[] { "updatelist.xml", "beta_updatelist.xml" };
            foreach (var fileName in fileNames)
            {
                var document = XDocument.Load(Path.Combine(dir, fileName));
                foreach (var assemble in assemblies)
                {
                    document.Root.Add(new XElement("Update",
                        new XAttribute("Version", JoinVersion(m, ".")),
                        new XAttribute("Path", String.Format("PPDDiff{0}_{1}.zip", JoinVersion(m, ""), assemble)),
                        new XAttribute("AssemblyType", assemble)));
                }
                document.Save(Path.Combine(targetDir, fileName));
            }
            return true;
        }

        /// <summary>
        /// 新しいフォルダーの名前からバージョンを取ります。
        /// </summary>
        /// <param name="separator">数字の間に入れる文字。</param>
        /// <returns>バージョンの形でないときは null。</returns>
        public static string GetVersion(string newFolder, string separator)
        {
            var m = InstallerWithArchitectureRegex().Match(newFolder);
            return m.Success ? JoinVersion(m, separator) : null;
        }

        private static string JoinVersion(Match m, string separator)
        {
            return String.Join(separator, m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, m.Groups[4].Value);
        }

        [GeneratedRegex("\\d+\\.\\d+\\.\\d+\\.\\d+")]
        private static partial Regex VersionRegex();
        [GeneratedRegex("(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)\\\\PPDInstaller\\(x(\\d{2})\\)")]
        private static partial Regex InstallerWithArchitectureRegex();
        [GeneratedRegex("(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)\\\\PPDInstaller")]
        private static partial Regex InstallerRegex();
        [GeneratedRegex("^Data$")]
        private static partial Regex DataFolderRegex();
    }
}
