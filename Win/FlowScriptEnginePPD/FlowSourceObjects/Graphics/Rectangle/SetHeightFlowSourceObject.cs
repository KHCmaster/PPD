using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Rectangle
{
    [ToolTipText("Graphics_Rectangle_SetHeight_Summary")]
    public partial class SetHeightFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.Rectangle.SetHeight"; }
        }

        [ToolTipText("Graphics_Rectangle_SetHeight_Height")]
        public float Height
        {
            private get;
            set;
        }

        [ToolTipText("Graphics_Rectangle_SetHeight_Object")]
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
                SetValue(nameof(Height));
                Object.RectangleHeight = Height;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
