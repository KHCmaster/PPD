using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Except_Summary")]
    public partial class ExceptFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.Except"; }
        }

        [ToolTipText("Enumerable_Except_First")]
        public IEnumerable<object> First
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Except_Second")]
        public IEnumerable<object> Second
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Except_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(First));
                SetValue(nameof(Second));
                if (First == null)
                {
                    return System.Linq.Enumerable.Empty<object>();
                }
                else if (Second == null)
                {
                    return First;
                }
                return First.Except(Second);
            }
        }
    }
}
