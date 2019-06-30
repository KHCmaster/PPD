using FlowScriptEngine;
using System.Linq;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mod
{
    [ToolTipText("Mod_UsingMods_Summary")]
    public partial class UsingModsFlowSourceObject : FlowSourceObjectBase
    {
        PPDFramework.Mod.ModInfo[] modInfos;

        public override string Name
        {
            get { return "PPD.Mod.UsingMods"; }
        }

        protected override void OnInitialize()
        {
            if (Manager.Items.ContainsKey("ModInfos"))
            {
                modInfos = Manager.Items["ModInfos"] as PPDFramework.Mod.ModInfo[];
            }

            base.OnInitialize();
        }

        [ToolTipText("Mod_UsingMods_Mods")]
        public object[] Mods
        {
            get
            {
                if (modInfos == null)
                {
                    return new object[0];
                }
                return modInfos.Cast<object>().ToArray();
            }
        }
    }
}
