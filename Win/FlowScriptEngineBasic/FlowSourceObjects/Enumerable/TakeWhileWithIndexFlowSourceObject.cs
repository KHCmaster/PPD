using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_TakeWhile_Summary")]
    public partial class TakeWhileWithIndexFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_TakeWhile_Predicate")]
        public event FlowEventHandler Predicate;
        private IEnumerable<object> enumerable;

        public override string Name
        {
            get { return "Enumerable.TakeWhile"; }
        }

        [ToolTipText("Enumerable_TakeWhile_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (enumerable == null)
                {
                    return System.Linq.Enumerable.Empty<object>();
                }
                return enumerable.TakeWhile((obj, index) =>
                {
                    Value = obj;
                    Index = index;
                    FireEvent(Predicate, true);
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

        [ToolTipText("Enumerable_TakeWhile_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_TakeWhile_Index")]
        public int Index
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_TakeWhile_Result")]
        public bool Result
        {
            private get;
            set;
        }
    }
}
