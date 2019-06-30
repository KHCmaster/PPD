using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_AddColorFilter_Summary")]
    public partial class AddColorFilterFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.AddColorFilter"; }
        }

        [ToolTipText("Graphics_AddColorFilter_ColorFilter")]
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
                Object.ColorFilters.Add(ColorFilter);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
