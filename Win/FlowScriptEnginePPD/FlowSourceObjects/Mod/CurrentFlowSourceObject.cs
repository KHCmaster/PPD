using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mod
{
    [ToolTipText("Mod_Current_Summary")]
    public partial class CurrentFlowSourceObject : FlowSourceObjectBase
    {
        PPDFramework.Mod.ModInfo modInfo;

        public override string Name
        {
            get { return "PPD.Mod.Current"; }
        }

        protected override void OnInitialize()
        {
            if (Manager.Items.ContainsKey("ModInfo"))
            {
                modInfo = Manager.Items["ModInfo"] as PPDFramework.Mod.ModInfo;
            }

            base.OnInitialize();
        }

        [ToolTipText("Mod_Current_Value")]
        public PPDFramework.Mod.ModInfo Value
        {
            get
            {
                return modInfo;
            }
        }
    }
}
