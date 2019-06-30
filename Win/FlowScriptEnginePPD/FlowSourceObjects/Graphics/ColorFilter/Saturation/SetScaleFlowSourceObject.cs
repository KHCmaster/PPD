using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.Saturation
{
    [ToolTipText("ColorFilter_Saturation_SetScale_Summary")]
    public partial class SetScaleFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.Saturation.SetScale"; }
        }

        [ToolTipText("ColorFilter_Saturation_SetScale_Filter")]
        public SaturationColorFilter Filter
        {
            private get;
            set;
        }

        [ToolTipText("ColorFilter_Saturation_SetScale_Scale")]
        public float Scale
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Filter));
            if (Filter != null)
            {
                SetValue(nameof(Scale));
                Filter.Scale = Scale;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
