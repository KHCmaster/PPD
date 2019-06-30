using FlowScriptEngine;
using PPDFramework.ScreenFilters;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_InsertPreScreenFilter_Summary")]
    public partial class InsertPreScreenFilterFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.InsertPreScreenFilter"; }
        }

        [ToolTipText("Graphics_InsertPreScreenFilter_ScreenFilter")]
        public ScreenFilterBase ScreenFilter
        {
            private get;
            set;
        }

        [ToolTipText("Graphics_InsertPreScreenFilter_Index")]
        public int Index
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
                SetValue(nameof(Index));
                Object.PreScreenFilters.Insert(Index, ScreenFilter);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
