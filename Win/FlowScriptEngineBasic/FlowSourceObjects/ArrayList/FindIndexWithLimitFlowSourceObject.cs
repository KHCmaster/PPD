using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_FindIndex_Summary")]
    public partial class FindIndexWithLimitFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        [ToolTipText("ArrayList_FindIndex_Predicate")]
        public event FlowEventHandler Predicate;

        public override string Name
        {
            get { return "ArrayList.FindIndex"; }
        }

        [ToolTipText("ArrayList_FindIndex_Result")]
        public bool Result
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_FindIndex_StartIndex")]
        public int StartIndex
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_FindIndex_Count")]
        public int Count
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_FindIndex_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("ArrayList_FindIndex_FoundIndex")]
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
                FoundIndex = ArrayList.FindIndex(StartIndex, Count, obj =>
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
