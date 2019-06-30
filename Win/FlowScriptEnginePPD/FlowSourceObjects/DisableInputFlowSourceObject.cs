using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects
{
    [ToolTipText("DisableInput_Summary")]
    [ModifyData]
    [ModifyDataWarning]
    public partial class DisableInputFlowSourceObject : ExecutableFlowSourceObject
    {
        public InputManager inputManager;

        public override string Name
        {
            get { return "PPD.DisableInput"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (Manager.Items.ContainsKey("InputManager"))
            {
                inputManager = Manager.Items["InputManager"] as InputManager;
            }
        }

        [ToolTipText("DisableInput_Button")]
        public PPDCoreModel.Data.MarkType Button
        {
            private get;
            set;
        }

        [ToolTipText("DisableInput_IsDisabled")]
        public bool IsDisabled
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            if (inputManager != null)
            {
                SetValue(nameof(Button));
                SetValue(nameof(IsDisabled));
                inputManager.Disable(Button, IsDisabled);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
