using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_SequentialEqual_Summary")]
    public partial class SequentialEqualFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.SequentialEqual"; }
        }

        [ToolTipText("Enumerable_SequentialEqual_First")]
        public IEnumerable<object> First
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_SequentialEqual_Second")]
        public IEnumerable<object> Second
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_SequentialEqual_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(First));
                SetValue(nameof(Second));
                if (First == null)
                {
                    if (Second == null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (Second == null)
                {
                    return false;
                }
                return First.SequenceEqual(Second);
            }
        }
    }
}
