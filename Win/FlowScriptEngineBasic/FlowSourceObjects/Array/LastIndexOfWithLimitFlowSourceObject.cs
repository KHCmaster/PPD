namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_LastIndexOf_Summary")]
    public partial class LastIndexOfWithLimitFlowSourceObject : ArrayFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.LastIndexOf"; }
        }

        [ToolTipText("Array_LastIndexOf_Value")]
        public object Value
        {
            private get;
            set;
        }

        [ToolTipText("Array_LastIndexOf_StartIndex")]
        public int StartIndex
        {
            private get;
            set;
        }

        [ToolTipText("Array_LastIndexOf_Count")]
        public int Count
        {
            private get;
            set;
        }

        [ToolTipText("Array_LastIndexOf_Index")]
        public int Index
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetArray();
            if (Array != null)
            {
                SetValue(nameof(Value));
                SetValue(nameof(StartIndex));
                SetValue(nameof(Count));
                Index = System.Array.LastIndexOf(Array, Value, StartIndex, Count);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
