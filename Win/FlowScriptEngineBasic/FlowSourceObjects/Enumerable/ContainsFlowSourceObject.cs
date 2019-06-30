using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Contains_Summary")]
    public partial class ContainsFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.Contains"; }
        }

        [ToolTipText("Enumerable_Contains_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Contains_Value")]
        public object Value
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Contains_Contains")]
        public bool Contains
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return false;
                }
                SetValue(nameof(Value));
                return Enumerable.Contains(Value);
            }
        }
    }
}
