using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_SetAlpha_Summary")]
    public partial class SetAlphaFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.SetAlpha"; }
        }

        [ToolTipText("Graphics_SetAlpha_Alpha")]
        public float Alpha
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(Alpha));
            if (Object != null)
            {
                Object.Alpha = Alpha;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
