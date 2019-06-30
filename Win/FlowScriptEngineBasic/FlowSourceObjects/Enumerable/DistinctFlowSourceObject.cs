using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Distinct_Summary")]
    public partial class DistinctFlowSourceObject : FlowSourceObjectBase
    {
        private IEnumerable<object> enumerable;

        public override string Name
        {
            get { return "Enumerable.Distinct"; }
        }

        [ToolTipText("Enumerable_Distinct_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (enumerable == null)
                {
                    return null;
                }
                return enumerable.Distinct();
            }
            set
            {
                enumerable = value;
            }
        }
    }
}
