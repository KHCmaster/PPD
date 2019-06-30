using FlowScriptEngine;
using PPDFramework.ScreenFilters;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ScreenFilter.Color
{
    [ToolTipText("ScreenFilter_Color_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ScreenFilter.Color.Value"; }
        }

        [ToolTipText("ScreenFilter_Color_Value_Filter")]
        public ColorScreenFilter Filter
        {
            get;
            private set;
        }

        public override void In(FlowEventArgs e)
        {
            Filter = new ColorScreenFilter();
            OnSuccess();
        }
    }
}
