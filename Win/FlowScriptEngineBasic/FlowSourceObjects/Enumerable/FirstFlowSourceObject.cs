using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_First_Summary")]
    public partial class FirstFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.First"; }
        }

        [ToolTipText("Enumerable_First_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_First_First")]
        public object First
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return null;
                }
                return Enumerable.First();
            }
        }
    }
}
