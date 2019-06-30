using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_RemoveAll_Summary")]
    public partial class RemoveAllFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        [ToolTipText("ArrayList_RemoveAll_Predicate")]
        public event FlowEventHandler Predicate;

        public override string Name
        {
            get { return "ArrayList.RemoveAll"; }
        }

        [ToolTipText("ArrayList_RemoveAll_Result")]
        public bool Result
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_RemoveAll_Value")]
        public object Value
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetArrayList();
            if (ArrayList != null)
            {
                ArrayList.RemoveAll(obj =>
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
