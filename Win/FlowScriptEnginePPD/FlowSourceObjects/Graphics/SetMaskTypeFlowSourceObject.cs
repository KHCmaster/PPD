using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_SetMaskType_Summary")]
    public partial class SetMaskTypeFlowSourceObject : GraphicsFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.SetMaskType"; }
        }

        [ToolTipText("Graphics_SetMaskType_MaskType")]
        public PPDFramework.Shaders.MaskType MaskType
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            if (Object != null)
            {
                SetValue(nameof(MaskType));
                Object.MaskType = MaskType;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
