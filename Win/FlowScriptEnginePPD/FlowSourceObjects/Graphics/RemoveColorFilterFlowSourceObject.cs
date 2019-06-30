using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_RemoveColorFilter_Summary")]
    public partial class RemoveColorFilterFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.RemoveColorFilter"; }
        }

        [ToolTipText("Graphics_RemoveColorFilter_ColorFilter")]
        public ColorFilterBase ColorFilter
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(ColorFilter));
            if (Object != null && ColorFilter != null)
            {
                Object.ColorFilters.Remove(ColorFilter);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
