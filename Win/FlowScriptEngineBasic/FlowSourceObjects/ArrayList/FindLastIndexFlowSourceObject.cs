using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_FindLastIndex_Summary")]
    public partial class FindLastIndexFlowSourceObject : ArrayListFlowSourceObjectBase
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
                FoundIndex = ArrayList.FindLastIndex(obj =>
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
