using System.Numerics;
using PPDMath;
using Color4 = Vortice.Mathematics.Color4;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mod
{
    [ToolTipText("Mod_AddSetting_Summary")]
    public partial class AddColor4SettingFlowSourceObject : AddSettingFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Mod.AddSetting"; }
        }

        [ToolTipText("Mod_AddSetting_Default")]
        public Color4 Default
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            base.In(e);
            SetValue(nameof(Default));
            modSettingManager?.AddColor4Setting(Key, DisplayName, Description, Default);
        }
    }
}
