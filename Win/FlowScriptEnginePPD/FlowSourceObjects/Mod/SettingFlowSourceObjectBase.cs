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

            if (Manager.Items.TryGetValue("ModSettingManager", out object value))
            {
                modSettingManager = value as ModSettingManager;
            }
        }
    }
}
