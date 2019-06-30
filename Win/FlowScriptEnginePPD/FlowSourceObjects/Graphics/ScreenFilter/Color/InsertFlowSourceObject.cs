using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ScreenFilter.Color
{
    [ToolTipText("ScreenFilter_Color_Insert_Summary")]
    public partial class InsertFlowSourceObject : ColorScreenFilterFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.ScreenFilter.Color.Insert"; }
        }

        [ToolTipText("ScreenFilter_Color_Insert_ColorFilter")]
        public PPDFramework.Shaders.ColorFilterBase ColorFilter
        {
            private get;
            set;
        }

        [ToolTipText("ScreenFilter_Color_Insert_Index")]
        public int Index
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
                SetValue(nameof(Index));
                Filter.Filters.Insert(Index, ColorFilter);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
