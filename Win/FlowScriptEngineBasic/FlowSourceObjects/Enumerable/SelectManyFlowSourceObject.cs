using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_SelectMany_Summary")]
    public partial class SelectManyFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_SelectMany_Select")]
        public event FlowEventHandler Select;
        private IEnumerable<object> enumerable;

        public override string Name
        {
            get { return "Enumerable.SelectMany"; }
        }

        [ToolTipText("Enumerable_SelectMany_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (enumerable == null)
                {
                    return null;
                }
                return enumerable.SelectMany(obj =>
                {
                    Value = obj;
                    FireEvent(Select, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
            }
            set
            {
                enumerable = value;
            }
        }

        [ToolTipText("Enumerable_SelectMany_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_SelectMany_Result")]
        public IEnumerable<object> Result
        {
            private get;
            set;
        }
    }
}
