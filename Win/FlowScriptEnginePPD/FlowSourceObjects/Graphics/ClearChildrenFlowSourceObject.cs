using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_ClearChildren_Summary")]
    public partial class ClearChildrenFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ClearChildren"; }
        }

        [ToolTipText("Graphics_ClearChildren_Object")]
        public GameComponent Object
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            if (Object != null)
            {
                Object.ClearChildren();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
