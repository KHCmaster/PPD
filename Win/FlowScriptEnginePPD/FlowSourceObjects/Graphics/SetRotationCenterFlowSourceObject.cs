using FlowScriptEngine;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_SetRotationCenter_Summary")]
    public partial class SetRotationCenterFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.SetRotationCenter"; }
        }

        [ToolTipText("Graphics_SetRotationCenter_RotationCenter")]
        public Vector2 RotationCenter
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(RotationCenter));
            if (Object != null)
            {
                Object.RotationCenter = RotationCenter;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
