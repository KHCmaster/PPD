using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_FindIndex_Summary")]
    public partial class FindIndexWithLimitFlowSourceObject : ArrayFlowSourceObjectBase
    {
        [ToolTipText("Array_FindIndex_Predicate")]
        public event FlowEventHandler Predicate;

        public override string Name
        {
            get { return "Array.FindIndex"; }
        }

        [ToolTipText("Array_FindIndex_Result")]
        public bool Result
        {
            private get;
            set;
        }

        [ToolTipText("Array_FindIndex_StartIndex")]
        public int StartIndex
        {
            private get;
            set;
        }

        [ToolTipText("Array_FindIndex_Count")]
        public int Count
        {
            private get;
            set;
        }

        [ToolTipText("Array_FindIndex_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Array_FindIndex_FoundIndex")]
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
                FoundIndex = System.Array.FindIndex(Array, StartIndex, Count, obj =>
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
