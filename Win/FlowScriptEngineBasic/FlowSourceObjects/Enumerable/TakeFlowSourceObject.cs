using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Take_Summary")]
    public partial class TakeFlowSourceObject : FlowSourceObjectBase
    {
        private IEnumerable<object> enumerable;

        public override string Name
        {
            get { return "Enumerable.Take"; }
        }

        [ToolTipText("Enumerable_Take_Count")]
        public int Count
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Take_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (enumerable == null)
                {
                    return System.Linq.Enumerable.Empty<object>();
                }
                SetValue(nameof(Count));
                return enumerable.Take(Count);
            }
            set
            {
                enumerable = value;
            }
        }
    }
}
