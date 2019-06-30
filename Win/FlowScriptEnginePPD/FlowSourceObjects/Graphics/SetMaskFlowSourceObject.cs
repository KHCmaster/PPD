using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_SetMask_Summary")]
    public partial class SetMaskFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.SetMask"; }
        }

        [ToolTipText("Graphics_SetMask_Mask")]
        public GameComponent Mask
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            if (Object != null)
            {
                SetValue(nameof(Mask));
                Object.Mask = Mask;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
