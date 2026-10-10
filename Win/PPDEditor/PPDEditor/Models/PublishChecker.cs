using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFrameworkCore;
using PPDPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace PPDEditor.Models
{
    /// <summary>
    /// 発行する前に譜面を確かめます（同じ内容の譜面、近すぎる時間のマーク）。
    /// </summary>
    static class PublishChecker
    {
        /// <summary>
        /// 内容が同じ譜面がないかを確かめます。
        /// </summary>
        /// <returns>見つかった問題のメッセージ。</returns>
        public static List<string> CheckSameScore(string projectDir, AvailableDifficulty difficulties)
        {
            var errors = new List<string>();
            var hashs = new Dictionary<string, AvailableDifficulty>();
            foreach (var difficulty in GetDifficulties(difficulties))
            {
                var filePath = Path.Combine(projectDir, difficulty + ".ppd");
                var hash = CryptographyUtility.Getx2Encoding(SHA256.HashData(File.ReadAllBytes(filePath)));
                if (hashs.TryGetValue(hash, out AvailableDifficulty value))
                {
                    errors.Add(String.Format(Utility.Language["ScoreHashCollision"], value, difficulty));
                }
                else
                {
                    hashs.Add(hash, difficulty);
                }
            }
            return errors;
        }

        /// <summary>
        /// 同じボタンのマークが 1/60 秒以内に並んでいないかを確かめます。
        /// </summary>
        /// <returns>見つかった問題のメッセージ。</returns>
        public static List<string> CheckNearTime(string projectDir, AvailableDifficulty difficulties)
        {
            var errors = new List<string>();
            var checkTimeInterval = 1 / 60f;
            foreach (var difficulty in GetDifficulties(difficulties))
            {
                var filePath = Path.Combine(projectDir, difficulty + ".ppd");
                using PackReader reader = new(filePath);
                if (!reader.FileList.Contains("ppd"))
                {
                    continue;
                }
                var data = PPDReader.Read(reader.Read("ppd"));
                var timeDatas = new Dictionary<ButtonType, SortedSet<float>>();
                foreach (var d in data)
                {
                    if (!timeDatas.TryGetValue(d.ButtonType, out SortedSet<float> set))
                    {
                        set = [];
                        timeDatas.Add(d.ButtonType, set);
                    }
                    foreach (var setVal in set)
                    {
                        if (Math.Abs(setVal - d.Time) <= checkTimeInterval)
                        {
                            errors.Add(Utility.Language["NearTimeWarning"] + "\n" +
                                Utility.Language["Difficulty"] + ":" + difficulty + "\n" +
                                Utility.Language["Time"] + ":" + d.Time + "\n" +
                                Utility.Language["ButtonType"] + ":" + Utility.Language[d.ButtonType.ToString()] + "\n");
                            break;
                        }
                    }
                    set.Add(d.Time);
                }
            }
            return errors;
        }

        private static IEnumerable<AvailableDifficulty> GetDifficulties(AvailableDifficulty difficulties)
        {
            foreach (var difficulty in ProjectFiles.DifficultyArray)
            {
                if (difficulty != AvailableDifficulty.None && (difficulties & difficulty) == difficulty)
                {
                    yield return difficulty;
                }
            }
        }
    }
}
