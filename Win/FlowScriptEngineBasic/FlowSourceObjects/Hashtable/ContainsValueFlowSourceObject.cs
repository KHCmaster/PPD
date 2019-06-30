using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    [ToolTipText("Hashtable_ContainsValue_Summary")]
    public partial class ContainsValueFlowSourceObject : HashtableFlowSourceBase
    {
        public override string Name
        {
            get { return "Hashtable.ContainsValue"; }
        }

        [ToolTipText("Hashtable_ContainsValue_Value")]
        public object Value
        {
            private get;
            set;
        }

        [ToolTipText("Hashtable_ContainsValue_Contains")]
        public bool Contains
        {
            get;
            private set;
        }

        public override void In(FlowEventArgs e)
        {
            SetHashtable();
            if (Hashtable != null)
            {
                SetValue(nameof(Value));
                Value = Hashtable.ContainsValue(Value);
                OnSuccess();

            }
            else
            {
                OnFailed();
            }
        }
    }
}
