using FlowScriptEngine;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.Color
{
    [ToolTipText("ColorFilter_Color_SetColor_Summary")]
    public partial class SetColorFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.Color.SetScale"; }
        }

        [ToolTipText("ColorFilter_Color_SetColor_Filter")]
        public PPDFramework.Shaders.ColorFilter Filter
        {
            private get;
            set;
        }

        [ToolTipText("ColorFilter_Color_SetColor_Color")]
        public Color4 Color
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Filter));
            if (Filter != null)
            {
                SetValue(nameof(Color));
                Filter.Color = Color;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
