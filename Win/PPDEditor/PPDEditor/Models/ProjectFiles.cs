using PPDConfiguration;
using PPDPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PPDEditor.Models
{
    /// <summary>
    /// プロジェクトのファイル（*.ppdproj と、同じ名前のフォルダ）を扱います。
    /// </summary>
    public static partial class ProjectFiles
    {
        /// <summary>
        /// 今のプロジェクトの形式の番号です。
        /// </summary>
        public const int LatestVersion = 2;

        /// <summary>
        /// すべての難易度（None を含む）です。
        /// </summary>
        public static readonly AvailableDifficulty[] DifficultyArray = Enum.GetValues<AvailableDifficulty>();

        /// <summary>
        /// プロジェクトのファイルのパスから、データを置くフォルダのパスを返します。
        /// </summary>
        public static string GetProjectDir(string projectPath)
        {
            return Path.Combine(Path.GetDirectoryName(projectPath), Path.GetFileNameWithoutExtension(projectPath));
        }

        /// <summary>
        /// プロジェクトの形式の番号を返します（番号がない古いものは 1）。
        /// </summary>
        public static int GetVersion(string path)
        {
            var setting = new SettingReader(File.ReadAllText(path));
            var version = setting.ReadString("version");
            return version switch
            {
                "" => 1,
                _ => int.Parse(version),
            };
        }

        /// <summary>
        /// プロジェクトのフォルダにある難易度（レイヤーのフォルダがあるもの）を返します。
        /// </summary>
        public static AvailableDifficulty GetAvailableDifficulty(string projectDir)
        {
            AvailableDifficulty ret = AvailableDifficulty.None;
            foreach (AvailableDifficulty difficulty in DifficultyArray)
            {
                ret |= Directory.Exists(Path.Combine(projectDir, difficulty + "_" + "layers")) ? difficulty : AvailableDifficulty.None;
            }
            return ret;
        }

        public static AvailableDifficulty ParseDifficulty(string s)
        {
            return s.ToLower() switch
            {
                "base" => AvailableDifficulty.Base,
                "easy" => AvailableDifficulty.Easy,
                "normal" => AvailableDifficulty.Normal,
                "hard" => AvailableDifficulty.Hard,
                "extreme" => AvailableDifficulty.Extreme,
                _ => AvailableDifficulty.None,
            };
        }

        /// <summary>
        /// プロジェクトを「名前_backup」のフォルダに複製します（動画は除く）。
        /// </summary>
        public static void BackUp(string path)
        {
            var filename = Path.GetFileNameWithoutExtension(path);
            var parentdirectory = Path.GetDirectoryName(path);
            string directory = Path.Combine(parentdirectory, filename) + "_backup";
            EnsureDirectory(directory);
            File.Copy(path, Path.Combine(directory, Path.GetFileName(path)), true);

            var copydir = Path.Combine(directory, filename);
            EnsureDirectory(copydir);
            Utility.CopyDirectory(Path.Combine(parentdirectory, filename), copydir, [MovieFileRegex()]);
        }

        /// <summary>
        /// 古い形式のプロジェクトを今の形式にします。
        /// </summary>
        public static void Convert(string path, int version)
        {
            if (version <= 1) Convert1To2(path);
        }

        private static void Convert1To2(string path)
        {
            var setting = new SettingReader(File.ReadAllText(path));
            var sw = new SettingWriter(path, false);
            var head = AvailableDifficulty.Base.ToString();
            foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
            {
                sw.Write(head + "." + kvp.Key, kvp.Value);
            }
            // rename folder(layers->Base_layers)
            var projectdir = GetProjectDir(path);
            var dir = Path.Combine(projectdir, "layers");
            var newdir = Path.Combine(projectdir, head + "_" + "layers");
            if (Directory.Exists(dir))
            {
                Directory.Move(dir, newdir);
            }
            else
            {
                var filePath = Path.Combine(projectdir, head + ".ppd");
                EnsureDirectory(newdir);
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, Path.Combine(newdir, "layer0.ppd"), true);
                }
                sw.Write("Base.layer0", "Layer0");
                sw.Write("Base.layer0BPM", setting.ReadString("bpm"));
                sw.Write("Base.layer0BPMOffset", setting.ReadString("bpmstart"));
                sw.Write("Base.layer0DisplayWidth", 100);
                sw.Write("Base.layer0Visible", 1);
            }
            sw.Write("version", LatestVersion.ToString());
            sw.Write("selecteddifficulty", head);
            sw.Close();

            // merge ppd and evd
            var tempfilename = Path.Combine(projectdir, Path.GetRandomFileName());
            var pw = new PackWriter(tempfilename);
            foreach (PPDPackStreamWriter ppdpsw in pw.Write(["ppd", "evd"]))
            {
                string filepath = string.Empty;
                switch (ppdpsw.Filename)
                {
                    case "ppd":
                        filepath = Path.Combine(projectdir, head + ".ppd");
                        break;
                    case "evd":
                        filepath = Path.Combine(projectdir, head + ".evd");
                        break;
                }
                if (File.Exists(filepath))
                {
                    var data = File.ReadAllBytes(filepath);
                    ppdpsw.Write(data, 0, data.Length);
                    File.Delete(filepath);
                }
            }
            pw.Close();
            File.Move(tempfilename, Path.Combine(projectdir, head + ".ppd"));
        }

        /// <summary>
        /// 複数のプロジェクトの難易度を集めて、新しいプロジェクトを作ります。
        /// </summary>
        /// <param name="mergeDifficulty">作るプロジェクトの難易度。</param>
        /// <param name="getMergeInfo">難易度ごとの、元のプロジェクトとその難易度。</param>
        /// <param name="baseInfoDifficulty">歌詞や音、動画などを持ってくる難易度。</param>
        /// <param name="destProjectFilePath">作るプロジェクトのファイルのパス。</param>
        public static void Merge(AvailableDifficulty mergeDifficulty, Func<AvailableDifficulty, MergeInfo> getMergeInfo,
            AvailableDifficulty? baseInfoDifficulty, string destProjectFilePath)
        {
            foreach (AvailableDifficulty difficulty in DifficultyArray)
            {
                if (difficulty == AvailableDifficulty.None) continue;
                if ((mergeDifficulty & difficulty) == difficulty)
                {
                    var mi = getMergeInfo(difficulty);
                    if (mi.ProjectVersion != LatestVersion)
                    {
                        BackUp(mi.ProjectPath);
                        Convert(mi.ProjectPath, mi.ProjectVersion);
                    }
                }
            }
            var tempdir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
            var projectdir = Path.Combine(tempdir, Path.GetFileNameWithoutExtension(destProjectFilePath));
            EnsureDirectory(projectdir);
            var setting = new SettingReader("");
            setting.ReplaceOrAdd("version", LatestVersion.ToString());
            string moviePath = "";
            foreach (AvailableDifficulty difficulty in DifficultyArray)
            {
                if (difficulty == AvailableDifficulty.None) continue;
                if ((mergeDifficulty & difficulty) == difficulty)
                {
                    var mi = getMergeInfo(difficulty);
                    var srcDir = GetProjectDir(mi.ProjectPath);
                    Utility.CopyDirectory(Path.Combine(srcDir, mi.Difficulty + "_" + "layers"),
                        Path.Combine(projectdir, difficulty + "_" + "layers"), []);
                    File.Copy(Path.Combine(srcDir, mi.Difficulty + ".ppd"), Path.Combine(projectdir, difficulty + ".ppd"), true);
                    File.Copy(Path.Combine(srcDir, mi.Difficulty + ".scd"), Path.Combine(projectdir, difficulty + ".scd"), true);

                    var scriptDir = Path.Combine(srcDir, mi.Difficulty + "_" + "Scripts");
                    if (Directory.Exists(scriptDir))
                    {
                        Utility.CopyDirectory(scriptDir, Path.Combine(projectdir, difficulty + "_" + "Scripts"), []);
                    }

                    if (difficulty == baseInfoDifficulty)
                    {
                        var resourceDir = Path.Combine(srcDir, "Resource");
                        if (Directory.Exists(resourceDir))
                        {
                            Utility.CopyDirectory(resourceDir, Path.Combine(projectdir, "Resource"), []);
                        }

                        Utility.CopyDirectory(Path.Combine(srcDir, "sound"), Path.Combine(projectdir, "sound"), []);

                        File.Copy(Path.Combine(srcDir, "data.ini"), Path.Combine(projectdir, "data.ini"), true);
                        File.Copy(Path.Combine(srcDir, "kasi.txt"), Path.Combine(projectdir, "kasi.txt"), true);
                        File.Copy(Path.Combine(srcDir, "soundset.txt"), Path.Combine(projectdir, "soundset.txt"), true);
                        var fns = Directory.GetFiles(srcDir, "movie.*");
                        if (fns.Length > 0) moviePath = fns[0];
                    }
                    var tempSetting = new SettingReader(File.ReadAllText(mi.ProjectPath));
                    foreach (KeyValuePair<string, string> kvp in tempSetting.Dictionary)
                    {
                        if (kvp.Key.StartsWith(mi.Difficulty.ToString()))
                        {
                            setting.ReplaceOrAdd(kvp.Key.Replace(mi.Difficulty.ToString(), difficulty.ToString()), kvp.Value);
                        }
                    }
                    setting.ReplaceOrAdd("selecteddifficulty", difficulty.ToString());
                }
            }
            var sw = new SettingWriter(Path.Combine(tempdir, Path.GetFileName(destProjectFilePath)), false);
            foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
            {
                sw.Write(kvp.Key, kvp.Value);
            }
            sw.Close();

            //Copy To Local
            Utility.CopyDirectory(tempdir, Path.GetDirectoryName(destProjectFilePath), []);
            if (File.Exists(moviePath))
            {
                File.Copy(moviePath, Path.Combine(GetProjectDir(destProjectFilePath), Path.GetFileName(moviePath)), true);
            }
        }

        /// <summary>
        /// 譜面を配る形（ゲームで遊べるフォルダ）にして、コピーします。
        /// </summary>
        /// <param name="projectDir">プロジェクトのフォルダ。</param>
        /// <param name="destDir">コピー先のフォルダ（あれば消して作り直す）。</param>
        /// <param name="difficulties">含める難易度。</param>
        public static void Publish(string projectDir, string destDir, AvailableDifficulty difficulties)
        {
            if (Directory.Exists(destDir))
            {
                Directory.Delete(destDir, true);
            }
            EnsureDirectory(destDir);

            CopyFile(projectDir, destDir, "data.ini");
            CopyFile(projectDir, destDir, "kasi.txt");
            CopyFile(projectDir, destDir, "soundset.txt");
            CopyFile(projectDir, destDir, "resource.pak");
            foreach (AvailableDifficulty difficulty in DifficultyArray)
            {
                if (difficulty == AvailableDifficulty.None) continue;
                if ((difficulties & difficulty) == difficulty)
                {
                    CopyFile(projectDir, destDir, difficulty + ".ppd");
                    CopyFile(projectDir, destDir, difficulty + ".scd");
                }
            }
            Utility.CopyDirectory(Path.Combine(projectDir, "sound"), Path.Combine(destDir, "sound"), []);
        }

        private static void CopyFile(string src, string dest, string filename)
        {
            File.Copy(Path.Combine(src, filename), Path.Combine(dest, filename), true);
        }

        public static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        [GeneratedRegex("^movie\\.\\w+$")]
        private static partial Regex MovieFileRegex();
    }
}
