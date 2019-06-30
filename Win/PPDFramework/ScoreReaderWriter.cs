using PPDFrameworkCore;
using System;
using System.Text;

namespace PPDFramework
{
    /// <summary>
    /// スコアを書き込み、読み取ります
    /// </summary>
    public static class ScoreReaderWriter
    {
        const string zerostring = "0000000";
        /// <summary>
        /// 読み取ります
        /// </summary>
        /// <param name="resultIDs"></param>
        /// <returns></returns>
        public static string[] ReadScore(int[] resultIDs)
        {
            string[] ret = new string[(int)Difficulty.Extreme + 1];
            for (int i = 0; i < ret.Length; i++)
            {
                if (resultIDs[i] >= 0)
                {
                    PPDDatabase.DB.FindScore(resultIDs[i], out int score);
                    var sb = new StringBuilder();
                    sb.Append('0', Math.Max(0, 7 - score.ToString().Length));
                    sb.Append(score.ToString());
                    ret[i] = sb.ToString();
                }
                else
                {
                    ret[i] = zerostring;
                }
            }
            return ret;
        }

        /// <summary>
        /// 書き込みます
        /// </summary>
        /// <param name="scoreID"></param>
        /// <param name="difficulty"></param>
        /// <param name="counts"></param>
        /// <param name="maxcombo"></param>
        /// <param name="score"></param>
        /// <param name="resulttype"></param>
        /// <param name="finishtime"></param>
        /// <param name="songInfo"></param>
        /// <returns></returns>
        public static bool WriteScore(int scoreID, Difficulty difficulty, int[] counts, int maxcombo, int score, ResultEvaluateType resulttype, float finishtime, SongInformation songInfo)
        {
            return PPDDatabase.DB.WriteScore(scoreID, difficulty, counts, maxcombo, score, resulttype, finishtime, songInfo);
        }
    }
}

