using PPDFramework;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Rectangle
{
    [ToolTipText("Graphics_Rectangle_SetColor_Summary")]
    public partial class SetColorFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.Rectangle.SetColor"; }
        }

        [ToolTipText("Graphics_Rectangle_SetColor_Color")]
        public Color4 Color
        {
            private get;
            set;
        }

        [ToolTipText("Graphics_Rectangle_SetColor_Object")]
        public RectangleComponent Object
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Object));
            if (Object != null)
            {
                SetValue(nameof(Color));
                Object.Color = Color;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
