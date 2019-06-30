using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_ToLookup_Summary")]
    public partial class ToLookupFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_ToLookup_Select")]
        public event FlowEventHandler Select;
        [ToolTipText("Enumerable_ToLookup_ElementSelect")]
        public event FlowEventHandler ElementSelect;

        public override string Name
        {
            get { return "Enumerable.ToLookup"; }
        }

        [ToolTipText("Enumerable_ToLookup_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_ToLookup_Lookup")]
        public ILookup<object, object> Lookup
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return null;
                }
                return Enumerable.ToLookup(obj =>
                {
                    Value = obj;
                    FireEvent(Select, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                },
                obj =>
                {
                    ElementSelectValue = obj;
                    FireEvent(ElementSelect, true);
                    ProcessChildEvent();
                    SetValue(nameof(ElementSelectResult));
                    return ElementSelectResult;
                });
            }
        }

        [ToolTipText("Enumerable_ToLookup_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_ToLookup_Result")]
        public object Result
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_ToLookup_ElementSelectValue")]
        public object ElementSelectValue
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_ToLookup_ElementSelectResult")]
        public object ElementSelectResult
        {
            private get;
            set;
        }
    }
}
