using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Intersect_Summary")]
    public partial class IntersectFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.Intersect"; }
        }

        [ToolTipText("Enumerable_Intersect_First")]
        public IEnumerable<object> First
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Intersect_Second")]
        public IEnumerable<object> Second
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Intersect_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(First));
                SetValue(nameof(Second));
                if (First == null || Second == null)
                {
                    return System.Linq.Enumerable.Empty<object>();
                }
                return First.Intersect(Second);
            }
        }
    }
}
