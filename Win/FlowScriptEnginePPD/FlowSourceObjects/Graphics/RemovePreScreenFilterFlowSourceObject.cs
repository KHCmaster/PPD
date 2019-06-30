using FlowScriptEngine;
using PPDFramework.ScreenFilters;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_RemovePreScreenFilter_Summary")]
    public partial class RemovePreScreenFilterFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.RemovePreScreenFilter"; }
        }

        [ToolTipText("Graphics_RemovePreScreenFilter_ScreenFilter")]
        public ScreenFilterBase ScreenFilter
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(ScreenFilter));
            if (Object != null && ScreenFilter != null)
            {
                Object.PreScreenFilters.Remove(ScreenFilter);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
