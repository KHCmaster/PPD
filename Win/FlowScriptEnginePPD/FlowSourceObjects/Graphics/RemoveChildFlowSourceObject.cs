using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_RemoveChild_Summary")]
    public partial class RemoveChildFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.RemoveChild"; }
        }

        [ToolTipText("Graphics_RemoveChild_Parent")]
        public GameComponent Parent
        {
            private get;
            set;
        }

        [ToolTipText("Graphics_RemoveChild_Child")]
        public GameComponent Child
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Parent));
            SetValue(nameof(Child));
            if (Parent != null && Child != null)
            {
                Parent.RemoveChild(Child);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
