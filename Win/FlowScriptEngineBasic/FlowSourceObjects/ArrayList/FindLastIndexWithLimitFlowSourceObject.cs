using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_FindLastIndex_Summary")]
    public partial class FindLastIndexWithLimitFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        [ToolTipText("ArrayList_FindLastIndex_Predicate")]
        public event FlowEventHandler Predicate;

        public override string Name
        {
            get { return "ArrayList.FindLastIndex"; }
        }

        [ToolTipText("ArrayList_FindLastIndex_Result")]
        public bool Result
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_FindLastIndex_StartIndex")]
        public int StartIndex
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_FindLastIndex_Count")]
        public int Count
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_FindLastIndex_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("ArrayList_FindLastIndex_FoundIndex")]
        public int FoundIndex
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetArrayList();
            if (ArrayList != null)
            {
                SetValue(nameof(StartIndex));
                SetValue(nameof(Count));
                FoundIndex = ArrayList.FindLastIndex(StartIndex, Count, obj =>
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
