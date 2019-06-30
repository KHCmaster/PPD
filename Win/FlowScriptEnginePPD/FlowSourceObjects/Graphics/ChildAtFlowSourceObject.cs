using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("ChildAt_Summary")]
    public partial class ChildAtFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.ChildAt"; }
        }

        [ToolTipText("ChildAt_Parent")]
        public GameComponent Parent
        {
            private get;
            set;
        }

        [ToolTipText("ChildAt_Index")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("ChildAt_Value")]
        public GameComponent Value
        {
            get
            {
                SetValue(nameof(Parent));
                SetValue(nameof(Index));
                if (Parent != null)
                {
                    return Parent[Index];
                }

                return null;
            }
        }
    }
}
