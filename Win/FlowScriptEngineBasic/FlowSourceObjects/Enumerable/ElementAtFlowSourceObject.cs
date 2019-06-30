using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_ElementAt_Summary")]
    public partial class ElementAtFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.ElementAt"; }
        }

        [ToolTipText("Enumerable_ElementAt_Index")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_ElementAt_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_ElementAt_Value")]
        public object Value
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return null;
                }
                SetValue(nameof(Index));
                return Enumerable.ElementAt(Index);
            }
        }
    }
}
