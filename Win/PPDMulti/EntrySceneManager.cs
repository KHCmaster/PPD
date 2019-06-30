using PPDFramework;
using PPDFramework.Mod;
using System.Collections.Generic;

namespace PPDMulti
{
    public class EntrySceneManager : EntrySceneManagerBase
    {
        public override string SpriteDir
        {
            get
            {
                return @"img\PPD\multi";
            }
        }

        public override PPDFramework.Scene.ISceneBase GetSceneWithArgs(PPDDevice device, PPDExecuteArg args, out Dictionary<string, object> dic)
        {
            ModManager.Instance.BackgroundLoad();
            if (!args.ContainsKey("AsHost") || !args.ContainsKey("Port") || !args.ContainsKey("IP"))
            {
                dic = null;
                return new IPSelectScene(device);
            }

            dic = new Dictionary<string, object>
            {
                { "AsHost", args["AsHost"] != "0" },
                { "IP", args["IP"] },
                { "Port", int.Parse(args["Port"]) }
            };

            return new Menu(device);
        }
    }
}