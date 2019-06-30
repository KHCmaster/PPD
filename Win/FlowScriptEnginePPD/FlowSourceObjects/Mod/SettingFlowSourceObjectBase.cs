using FlowScriptEngine;
using PPDFramework.Mod;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mod
{
    public abstract class SettingFlowSourceObjectBase : FlowSourceObjectBase
    {
        protected ModSettingManager modSettingManager;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (Manager.Items.ContainsKey("ModSettingManager"))
            {
                modSettingManager = Manager.Items["ModSettingManager"] as ModSettingManager;
            }
        }
    }
}
