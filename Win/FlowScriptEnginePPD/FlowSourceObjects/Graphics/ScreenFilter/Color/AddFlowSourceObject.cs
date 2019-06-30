using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ScreenFilter.Color
{
    [ToolTipText("ScreenFilter_Color_Add_Summary")]
    public partial class AddFlowSourceObject : ColorScreenFilterFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.ScreenFilter.Color.Add"; }
        }

        [ToolTipText("ScreenFilter_Color_Add_ColorFilter")]
        public PPDFramework.Shaders.ColorFilterBase ColorFilter
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Filter));
            SetValue(nameof(ColorFilter));
            if (Filter != null && ColorFilter != null)
            {
                Filter.Filters.Add(ColorFilter);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
