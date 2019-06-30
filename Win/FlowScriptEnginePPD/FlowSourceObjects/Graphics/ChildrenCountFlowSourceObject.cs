using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_ChildrenCount_Summary")]
    public partial class ChildrenCountFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.ChildrenCount"; }
        }

        [ToolTipText("Graphics_ChildrenCount_Object")]
        public GameComponent Object
        {
            private get;
            set;
        }

        [ToolTipText("Graphics_ChildrenCount_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(Object));
                if (Object != null)
                {
                    return Object.ChildrenCount;
                }
                return 0;
            }
        }
    }
}
