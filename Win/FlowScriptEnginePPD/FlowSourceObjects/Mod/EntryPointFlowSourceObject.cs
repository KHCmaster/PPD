using FlowScriptEngine;
using PPDFramework.Mod;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mod
{
    [ToolTipText("Mod_EntryPoint_Summary")]
    public partial class EntryPointFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Mod_EntryPoint_Start")]
        public event FlowEventHandler Start;
        protected override void OnInitialize()
        {
            Manager.RegisterCallBack(ModSettingManager.MODSTART, ModStart);
        }

        private void ModStart(object[] args)
        {
            FireEvent(Start);
        }

        public override string Name
        {
            get { return "PPD.Mod.EntryPoint"; }
        }
    }
}
