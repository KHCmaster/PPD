using FlowScriptEngine;
using PPDFramework.ScreenFilters;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_InsertPostScreenFilter_Summary")]
    public partial class InsertPostScreenFilterFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.InsertPostScreenFilter"; }
        }

        [ToolTipText("Graphics_InsertPostScreenFilter_ScreenFilter")]
        public ScreenFilterBase ScreenFilter
        {
            private get;
            set;
        }

        [ToolTipText("Graphics_InsertPostScreenFilter_Index")]
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
                Object.PostScreenFilters.Insert(Index, ScreenFilter);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
