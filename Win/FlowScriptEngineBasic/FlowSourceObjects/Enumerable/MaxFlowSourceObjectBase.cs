using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    public abstract class MaxFlowSourceObjectBase : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Max_Select")]
        public event FlowEventHandler Select;

        public override string Name
        {
            get { return "Enumerable.Max"; }
        }

        [ToolTipText("Enumerable_Max_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            protected get;
            set;
        }

        [ToolTipText("Enumerable_Max_Value")]
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
