using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ScreenFilter.Color
{
    [ToolTipText("ScreenFilter_Color_Remove_Summary")]
    public partial class RemoveFlowSourceObject : ColorScreenFilterFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.ScreenFilter.Color.Remove"; }
        }

        [ToolTipText("ScreenFilter_Color_Remove_ColorFilter")]
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
                Filter.Filters.Remove(ColorFilter);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
