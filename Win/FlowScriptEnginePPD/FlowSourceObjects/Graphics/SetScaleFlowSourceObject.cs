using FlowScriptEngine;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_SetScale_Summary")]
    public partial class SetScaleFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.SetScale"; }
        }

        [ToolTipText("Graphics_SetScale_Scale")]
        public Vector2 Scale
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(Scale));
            if (Object != null)
            {
                Object.Scale = Scale;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
