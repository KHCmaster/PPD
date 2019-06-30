using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_SetHidden_Summary")]
    public partial class SetHiddenFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.SetHidden"; }
        }

        [ToolTipText("Graphics_SetHidden_Hidden")]
        public bool Hidden
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(Hidden));
            if (Object != null)
            {
                Object.Hidden = Hidden;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
