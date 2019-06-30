using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter
{
    [ToolTipText("ColorFilter_SetWeight_Summary")]
    public partial class SetWeightFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.SetWeight"; }
        }

        [ToolTipText("ColorFilter_SetWeight_ColorFilter")]
        public ColorFilterBase ColorFilter
        {
            private get;
            set;
        }

        [ToolTipText("ColorFilter_SetWeight_Weight")]
        public float Weight
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(ColorFilter));
            if (ColorFilter != null)
            {
                SetValue(nameof(Weight));
                ColorFilter.Weight = Weight;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
