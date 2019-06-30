using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_InsertChild_Summary")]
    public partial class InsertChildFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.InsertChild"; }
        }

        [ToolTipText("Graphics_InsertChild_Parent")]
        public GameComponent Parent
        {
            private get;
            set;
        }

        [ToolTipText("Graphics_InsertChild_Child")]
        public GameComponent Child
        {
            private get;
            set;
        }

        [ToolTipText("Graphics_InsertChild_Index")]
        public int Index
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Parent));
            SetValue(nameof(Child));
            SetValue(nameof(Index));
            if (Parent != null && Child != null)
            {
                Parent.InsertChild(Child, Index);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
