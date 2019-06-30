using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Number
{
    [ToolTipText("Number_SetNumber_Summary")]
    public partial class SetNumberFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.Number.SetNumber"; }
        }

        [ToolTipText("Number_SetNumber_Object")]
        public NumberPictureObject Object
        {
            private get;
            set;
        }

        [ToolTipText("Number_SetNumber_Number")]
        public int Number
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(Number));
            if (Object != null)
            {
                Object.Value = (uint)Number;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
