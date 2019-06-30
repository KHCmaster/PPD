using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_SetRotation_Summary")]
    public partial class SetRotationFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.SetRotation"; }
        }

        [ToolTipText("Graphics_SetRotation_Rotation")]
        public float Rotation
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(Rotation));
            if (Object != null)
            {
                Object.Rotation = Rotation;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
