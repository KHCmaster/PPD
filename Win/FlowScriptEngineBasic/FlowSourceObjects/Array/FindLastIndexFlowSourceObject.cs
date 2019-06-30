using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_FindLastIndex_Summary")]
    public partial class FindLastIndexFlowSourceObject : ArrayFlowSourceObjectBase
    {
        [ToolTipText("Array_FindLastIndex_Predicate")]
        public event FlowEventHandler Predicate;

        public override string Name
        {
            get { return "Array.FindLastIndex"; }
        }

        [ToolTipText("Array_FindLastIndex_Result")]
        public bool Result
        {
            private get;
            set;
        }

        [ToolTipText("Array_FindLastIndex_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Array_FindLastIndex_FoundIndex")]
        public int FoundIndex
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetArray();
            if (Array != null)
            {
                FoundIndex = System.Array.FindLastIndex(Array, obj =>
                {
                    Value = obj;
                    FireEvent(Predicate, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
