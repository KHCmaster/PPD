using PPDFramework;
using PPDMulti.Model;
using PPDMultiCommon.Model;
using System;
using System.IO;
using System.Linq;

namespace PPDMulti
{
    class Logger
    {
        private const string folderPath = "multi_log";
        private string filePath;

        public Logger()
            : this("multi_log.txt")
        {
        }

        public Logger(string filePath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            this.filePath = Path.Combine(folderPath, filePath);
        }

        public void AddLog(string text)
        {
            File.AppendAllText(filePath, text);
            File.AppendAllText(filePath, System.Environment.NewLine);
        }

        public void AddLog(string format, params object[] args)
        {
            AddLog(String.Format(format, args));
        }

        public void AddResult(GameRule gameRule, SongInformation songInfo, UserResult[] results)
        {
            AddLog("---------------------Result--------------------------");
            AddLog("DateTime:{0}", DateTime.Now);
            AddLog("Rules:{0}", gameRule.ItemAvailable ? "Item" : "");
            AddLog("SongName:{0}", songInfo.DirectoryName);
            AddLog("Players:{0}", String.Join(",", results.Select(result => result.User.Name).ToArray()));
            int iter = 1;
            foreach (var result in results)
            {
                AddLog("No.{0} Name:{1} Score:{2} C:{3} G:{4} SF:{5} SD:{6} W:{7} MC:{8}", iter, result.User.Name, result.Result.Score,
                    result.Result.CoolCount, result.Result.GoodCount, result.Result.SafeCount,
                    result.Result.SadCount, result.Result.WorstCount, result.Result.MaxCombo);
                iter++;
            }
            AddLog("-----------------------------------------------------");
        }
    }
}
