using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_AddChild_Summary")]
    public partial class AddChildFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.AddChild"; }
        }

        [ToolTipText("Graphics_AddChild_Parent")]
        public GameComponent Parent
        {
            private get;
            set;
        }

        [ToolTipText("Graphics_AddChild_Child")]
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
                Parent.AddChild(Child);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
