using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.NTSCGrayScale
{
    [ToolTipText("ColorFilter_NTSCGrayScale_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.NTSCGrayScale.Value"; }
        }

        [ToolTipText("ColorFilter_NTSCGrayScale_Value_Filter")]
        public NTSCGrayScaleColorFilter Filter
        {
            get;
            private set;
        }

        [ToolTipText("ColorFilter_NTSCGrayScale_Value_Weight")]
        public float Weight
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Weight));
            Filter = new NTSCGrayScaleColorFilter
            {
                Weight = Weight
            };
            OnSuccess();
        }
    }
}
