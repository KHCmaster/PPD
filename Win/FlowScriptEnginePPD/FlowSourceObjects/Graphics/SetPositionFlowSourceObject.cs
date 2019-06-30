using FlowScriptEngine;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_SetPosition_Summary")]
    public partial class SetPositionFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.SetPosition"; }
        }

        [ToolTipText("Graphics_SetPosition_Position")]
        public Vector2 Position
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(Position));
            if (Object != null)
            {
                Object.Position = Position;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
