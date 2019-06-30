using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Single_Summary")]
    public partial class SingleFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.Single"; }
        }

        [ToolTipText("Enumerable_Single_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Single_Single")]
        public object Single
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return null;
                }
                return Enumerable.Single();
            }
        }
    }
}
