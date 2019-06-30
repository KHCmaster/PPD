using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    public abstract class MinFlowSourceObjectBase : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Min_Select")]
        public event FlowEventHandler Select;

        public override string Name
        {
            get { return "Enumerable.Min"; }
        }

        [ToolTipText("Enumerable_Min_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            protected get;
            set;
        }

        [ToolTipText("Enumerable_Min_Value")]
        public object Value
        {
            get;
            protected set;
        }

        protected void FireSelectEvent()
        {
            FireEvent(Select, true);
        }
    }
}
