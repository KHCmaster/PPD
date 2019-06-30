using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.Hue
{
    [ToolTipText("ColorFilter_Hue_SetRotation_Summary")]
    public partial class SetRotationFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.Hue.SetRotation"; }
        }

        [ToolTipText("ColorFilter_Hue_SetRotation_Filter")]
        public HueColorFilter Filter
        {
            private get;
            set;
        }

        [ToolTipText("ColorFilter_Hue_SetRotation_Rotation")]
        public float Rotation
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Filter));
            if (Filter != null)
            {
                SetValue(nameof(Rotation));
                Filter.Rotation = Rotation;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
