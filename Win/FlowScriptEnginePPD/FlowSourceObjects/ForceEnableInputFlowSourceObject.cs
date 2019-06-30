using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects
{
    [ToolTipText("ForceEnableInput_Summary")]
    [ModifyData]
    [ModifyDataWarning]
    public partial class ForceEnableInputFlowSourceObject : ExecutableFlowSourceObject
    {
        public InputManager inputManager;

        public override string Name
        {
            get { return "PPD.ForceEnableInput"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (Manager.Items.ContainsKey("InputManager"))
            {
                inputManager = Manager.Items["InputManager"] as InputManager;
            }
        }

        public override void In(FlowEventArgs e)
        {
            if (inputManager != null)
            {
                inputManager.ForceEnable();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
