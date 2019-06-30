using Effect2D;
using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_SetBlend_Summary")]
    public partial class SetBlendFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.SetBlend"; }
        }

        [ToolTipText("Graphics_SetBlend_Blend")]
        public BlendMode Blend
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(Blend));
            if (Object != null)
            {
                Object.BlendMode = Blend;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
