using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_ToArray_Summary")]
    public partial class ToArrayFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.ToArray"; }
        }

        [ToolTipText("Enumerable_ToArray_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_ToArray_Array")]
        public object[] Array
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return new object[0];
                }
                return Enumerable.ToArray();
            }
        }
    }
}
