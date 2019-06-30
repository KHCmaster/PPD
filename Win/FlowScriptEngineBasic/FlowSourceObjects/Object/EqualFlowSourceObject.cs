using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Object
{
    [ToolTipText("Object_Equal_Summary", "Object_Equal_Remark")]
    public partial class EqualFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Object.Equal"; }
        }

        [ToolTipText("Object_Equal_A")]
        public object A
        {
            private get;
            set;
        }

        [ToolTipText("Object_Equal_B")]
        public object B
        {
            private get;
            set;
        }

        [ToolTipText("Object_Equal_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                if (A != null)
                {
                    return A.Equals(B);
                }
                else if (B != null)
                {
                    return B.Equals(A);
                }
                return true;
            }
        }
    }
}
