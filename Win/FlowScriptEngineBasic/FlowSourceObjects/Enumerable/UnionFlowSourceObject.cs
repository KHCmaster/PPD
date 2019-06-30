using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Union_Summary")]
    public partial class UnionFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.Union"; }
        }

        [ToolTipText("Enumerable_Union_First")]
        public IEnumerable<object> First
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Union_Second")]
        public IEnumerable<object> Second
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Union_Enumerable")]
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
                return First.Union(Second);
            }
        }
    }
}
