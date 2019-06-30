using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.Invert
{
    [ToolTipText("ColorFilter_Invert_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.Invert.Value"; }
        }

        [ToolTipText("ColorFilter_Invert_Value_Filter")]
        public InvertColorFilter Filter
        {
            get;
            private set;
        }

        [ToolTipText("ColorFilter_Invert_Value_Weight")]
        public float Weight
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Weight));
            Filter = new InvertColorFilter
            {
                Weight = Weight
            };
            OnSuccess();
        }
    }
}
