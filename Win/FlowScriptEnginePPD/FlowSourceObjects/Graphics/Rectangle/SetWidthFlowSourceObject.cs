using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Rectangle
{
    [ToolTipText("Graphics_Rectangle_SetWidth_Summary")]
    public partial class SetWidthFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.Rectangle.SetWidth"; }
        }

        [ToolTipText("Graphics_Rectangle_SetWidth_Width")]
        public float Width
        {
            private get;
            set;
        }

        [ToolTipText("Graphics_Rectangle_SetWidth_Object")]
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
                SetValue(nameof(Width));
                Object.RectangleWidth = Width;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
