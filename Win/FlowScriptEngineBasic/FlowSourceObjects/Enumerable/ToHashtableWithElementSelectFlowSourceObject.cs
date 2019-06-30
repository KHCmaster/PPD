using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_ToHashtable_Summary")]
    public partial class ToHashtableWithElementSelectFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_ToHashtable_Select")]
        public event FlowEventHandler Select;
        [ToolTipText("Enumerable_ToHashtable_ElementSelect")]
        public event FlowEventHandler ElementSelect;

        public override string Name
        {
            get { return "Enumerable.ToHashtable"; }
        }

        [ToolTipText("Enumerable_ToHashtable_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_ToHashtable_Hashtable")]
        public Dictionary<object, object> Hashtable
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return new Dictionary<object, object>();
                }
                return Enumerable.ToDictionary(obj =>
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

        [ToolTipText("Enumerable_ToHashtable_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_ToHashtable_Result")]
        public object Result
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_ToHashtable_ElementSelectValue")]
        public object ElementSelectValue
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_ToHashtable_ElementSelectResult")]
        public object ElementSelectResult
        {
            private get;
            set;
        }
    }
}
