using FlowScriptEngine;
using PPDFramework.ScreenFilters;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_AddPreScreenFilter_Summary")]
    public partial class AddPreScreenFilterFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.AddPreScreenFilter"; }
        }

        [ToolTipText("Graphics_AddPreScreenFilter_ScreenFilter")]
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
                Object.PreScreenFilters.Add(ScreenFilter);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
