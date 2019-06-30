using PPDCore;
using PPDFramework;
using PPDFramework.Mod;
using PPDFrameworkCore;
using PPDShareComponent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PPDSingle
{
    public class EntrySceneManager : EntrySceneManagerBase
    {
        public override string SpriteDir
        {
            get
            {
                return @"img\PPD\single";
            }
        }

        public override PPDFramework.Scene.ISceneBase GetSceneWithArgs(PPDDevice device, PPDExecuteArg args, out Dictionary<string, object> dic)
        {
            var gameutility = new PPDGameUtility();

            if (!args.ContainsKey("d"))
            {
                ModManager.Instance.BackgroundLoad();
                dic = null;
                return new Menu(device);
            }
            string dir = args["d"], auto = args["auto"], bpm = args["bpm"], starttime = args["st"],
                difficulty = args["dif"], connect = args["connect"], modlist = args["mod"];

            var mods = new List<string>();
            foreach (string mod in modlist.Split(','))
            {
                var filename = String.Format("{0}.mod", mod);
                var filepath = Path.Combine("mods", filename);
                if (File.Exists(filepath))
                {
                    mods.Add(filepath);
                }
            }

            var modInfos = new List<ModInfo>();
            if (mods.Count > 0)
            {
                ModManager.Instance.BackgroundLoad();
                ModManager.Instance.WaitForLoadFinish();
                foreach (var mod in mods)
                {
                    var found = ModManager.Instance.Root.Descendants().OfType<ModInfo>().FirstOrDefault(m => m.ModPath.ToLower() == mod.ToLower());
                    if (found != null)
                    {
                        modInfos.Add(found);
                    }
                }
            }

            //get songinformation
            gameutility.SongInformation = SongInformation.ReadData(dir);
            //end
            gameutility.Difficulty = ParseDifficulty(difficulty);
            gameutility.DifficultString = gameutility.SongInformation.GetDifficultyString(gameutility.Difficulty);
            gameutility.Profile = ProfileManager.Instance.Next();
            gameutility.AutoMode = auto == "1" ? AutoMode.All : AutoMode.None;
            gameutility.SpeedScale = 1;
            gameutility.Random = false;
            gameutility.Connect = connect == "1";
            gameutility.AppliedMods = modInfos.Count > 0 ? modInfos.ToArray() : null;
            float.TryParse(starttime, out float starttimevalue);
            dic = new Dictionary<string, object>
            {
                { "PPDGameUtility", gameutility },
                { "ExitOnReturn", true },
                { "StartTimeEx", starttimevalue },
                { "GameInterface", new GameInterface(device) },
                { "GameResult", new GameResult(device) },
                { "PauseMenu", new PauseMenu(device, Utility.Path) },
                { "MarkImagePath", new MarkImagePaths() }
            };
            return new MainGame(device);
        }
        private Difficulty ParseDifficulty(string difficuly)
        {
            switch (difficuly.ToLower())
            {
                case "base":
                    return Difficulty.Other;
                case "easy":
                    return Difficulty.Easy;
                case "normal":
                    return Difficulty.Normal;
                case "hard":
                    return Difficulty.Hard;
                case "extreme":
                    return Difficulty.Extreme;
                default:
                    return Difficulty.Other;
            }
        }
    }
}