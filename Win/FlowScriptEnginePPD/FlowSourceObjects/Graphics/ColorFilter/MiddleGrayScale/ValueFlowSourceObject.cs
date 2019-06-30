using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.MiddleGrayScale
{
    [ToolTipText("ColorFilter_MiddleGrayScale_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.MiddleGrayScale.Value"; }
        }

        [ToolTipText("ColorFilter_MiddleGrayScale_Value_Filter")]
        public MiddleGrayScaleColorFilter Filter
        {
            get;
            private set;
        }

        [ToolTipText("ColorFilter_MiddleGrayScale_Value_Weight")]
        public float Weight
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Weight));
            Filter = new MiddleGrayScaleColorFilter
            {
                Weight = Weight
            };
            OnSuccess();
        }
    }
}
