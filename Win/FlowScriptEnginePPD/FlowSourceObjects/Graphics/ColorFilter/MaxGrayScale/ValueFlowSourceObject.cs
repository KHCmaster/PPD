using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.MaxGrayScale
{
    [ToolTipText("ColorFilter_MaxGrayScale_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.MaxGrayScale.Value"; }
        }

        [ToolTipText("ColorFilter_MaxGrayScale_Value_Filter")]
        public MaxGrayScaleColorFilter Filter
        {
            get;
            private set;
        }

        [ToolTipText("ColorFilter_MaxGrayScale_Value_Weight")]
        public float Weight
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Weight));
            Filter = new MaxGrayScaleColorFilter
            {
                Weight = Weight
            };
            OnSuccess();
        }
    }
}
