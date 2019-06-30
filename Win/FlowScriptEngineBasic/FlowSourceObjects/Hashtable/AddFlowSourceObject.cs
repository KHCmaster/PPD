using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    [ToolTipText("Hashtable_Add_Summary")]
    public partial class AddFlowSourceObject : HashtableFlowSourceBase
    {
        public override string Name
        {
            get { return "Hashtable.Add"; }
        }

        [ToolTipText("Hashtable_Add_Key")]
        public object Key
        {
            private get;
            set;
        }

        [ToolTipText("Hashtable_Add_Value")]
        public object Value
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetHashtable();
            if (Hashtable != null)
            {
                SetValue(nameof(Key));
                SetValue(nameof(Value));
                Hashtable.Add(Key, Value);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
