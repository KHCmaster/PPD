using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Select_Summary")]
    public partial class SelectWithIndexFlowSourceObject : FlowSourceObjectBase
    {
        private IEnumerable<object> enumerable;

        [ToolTipText("Enumerable_Select_Select")]
        public event FlowEventHandler Select;

        public override string Name
        {
            get { return "Enumerable.Select"; }
        }

        [ToolTipText("Enumerable_Select_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (enumerable == null)
                {
                    return null;
                }
                return enumerable.Select((obj, index) =>
                {
                    Value = obj;
                    Index = index;
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

        [ToolTipText("Enumerable_Select_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Select_Index")]
        public int Index
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Select_Result")]
        public object Result
        {
            private get;
            set;
        }
    }
}
