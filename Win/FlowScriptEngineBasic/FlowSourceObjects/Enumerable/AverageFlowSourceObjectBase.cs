using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    public abstract class AverageFlowSourceObjectBase : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Average_Select")]
        public event FlowEventHandler Select;

        public override string Name
        {
            get { return "Enumerable.Average"; }
        }

        [ToolTipText("Enumerable_Average_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            protected get;
            set;
        }

        [ToolTipText("Enumerable_Average_Value")]
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
