using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_RemoveFromParent_Summary")]
    public partial class RemoveFromParentFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.RemoveFromParent"; }
        }

        [ToolTipText("Graphics_RemoveFromParent_Child")]
        public GameComponent Child
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Child));
            if (Child != null && Child.Parent != null)
            {
                Child.Parent.RemoveChild(Child);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
