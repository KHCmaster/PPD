using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Reverse_Summary")]
    public partial class ReverseFlowSourceObject : FlowSourceObjectBase
    {
        private IEnumerable<object> enumerable;

        public override string Name
        {
            get { return "Enumerable.Reverse"; }
        }

        [ToolTipText("Enumerable_Reverse_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (enumerable == null)
                {
                    return System.Linq.Enumerable.Empty<object>();
                }
                return enumerable.Reverse();
            }
            set
            {
                enumerable = value;
            }
        }
    }
}
