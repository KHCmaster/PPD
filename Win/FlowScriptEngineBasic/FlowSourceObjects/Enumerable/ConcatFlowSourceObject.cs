using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Concat_Summary")]
    public partial class ConcatFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.Concat"; }
        }

        [ToolTipText("Enumerable_Concat_First")]
        public IEnumerable<object> First
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Concat_Second")]
        public IEnumerable<object> Second
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Concat_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(First));
                SetValue(nameof(Second));
                if (First == null)
                {
                    if (Second == null)
                    {
                        return System.Linq.Enumerable.Empty<object>();
                    }
                    else
                    {
                        return Second;
                    }
                }
                else if (Second == null)
                {
                    return First;
                }
                return First.Concat(Second);
            }
        }
    }
}
