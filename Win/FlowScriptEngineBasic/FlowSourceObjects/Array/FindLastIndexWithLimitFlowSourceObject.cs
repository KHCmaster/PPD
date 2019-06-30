using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_FindLastIndex_Summary")]
    public partial class FindLastIndexWithLimitFlowSourceObject : ArrayFlowSourceObjectBase
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

        [ToolTipText("Array_FindLastIndex_StartIndex")]
        public int StartIndex
        {
            private get;
            set;
        }

        [ToolTipText("Array_FindLastIndex_Count")]
        public int Count
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
                SetValue(nameof(StartIndex));
                SetValue(nameof(Count));
                FoundIndex = System.Array.FindLastIndex(Array, StartIndex, Count, obj =>
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
