using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_InsertColorFilter_Summary")]
    public partial class InsertColorFilterFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.InsertColorFilter"; }
        }

        [ToolTipText("Graphics_InsertColorFilter_ColorFilter")]
        public ColorFilterBase ColorFilter
        {
            private get;
            set;
        }

        [ToolTipText("Graphics_InsertColorFilter_Index")]
        public int Index
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
                SetValue(nameof(Index));
                Object.ColorFilters.Insert(Index, ColorFilter);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
