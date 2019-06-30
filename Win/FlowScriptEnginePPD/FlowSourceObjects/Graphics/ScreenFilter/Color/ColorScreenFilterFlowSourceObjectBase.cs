using PPDFramework.ScreenFilters;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ScreenFilter.Color
{
    public abstract class ColorScreenFilterFlowSourceObjectBase : ExecutableFlowSourceObject
    {
        [ToolTipText("ScreenFilter_Color_Remove_Filter")]
        public ColorScreenFilter Filter
        {
            protected get;
            set;
        }
    }
}
