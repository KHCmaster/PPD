using FlowScriptEngine;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_SetScaleCenter_Summary")]
    public partial class SetScaleCenterFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.SetScaleCenter"; }
        }

        [ToolTipText("Graphics_SetScaleCenter_ScaleCenter")]
        public Vector2 ScaleCenter
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(ScaleCenter));
            if (Object != null)
            {
                Object.ScaleCenter = ScaleCenter;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
