using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("ContainsChild_Summary")]
    public partial class ContainsChildFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ContainsChild"; }
        }

        [ToolTipText("ContainsChild_Parent")]
        public GameComponent Parent
        {
            private get;
            set;
        }

        [ToolTipText("ContainsChild_Child")]
        public GameComponent Child
        {
            private get;
            set;
        }

        [ToolTipText("ContainsChild_Value")]
        public bool Value
        {
            get;
            private set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Parent));
            SetValue(nameof(Child));

            if (Parent != null)
            {
                Value = Parent.ContainsChild(Child);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
