using FlowScriptEngine;
using PPDFramework.ScreenFilters;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_RemovePostScreenFilter_Summary")]
    public partial class RemovePostScreenFilterFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.RemovePostScreenFilter"; }
        }

        [ToolTipText("Graphics_RemovePostScreenFilter_ScreenFilter")]
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
                Object.PostScreenFilters.Remove(ScreenFilter);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
