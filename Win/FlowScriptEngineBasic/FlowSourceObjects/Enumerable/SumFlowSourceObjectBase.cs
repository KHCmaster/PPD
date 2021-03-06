﻿using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    public abstract class SumFlowSourceObjectBase : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Sum_Select")]
        public event FlowEventHandler Select;

        public override string Name
        {
            get { return "Enumerable.Sum"; }
        }

        [ToolTipText("Enumerable_Sum_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            protected get;
            set;
        }

        [ToolTipText("Enumerable_Sum_Value")]
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
