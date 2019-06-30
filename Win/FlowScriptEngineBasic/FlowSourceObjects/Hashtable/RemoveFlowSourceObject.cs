using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    [ToolTipText("Hashtable_Remove_Summary")]
    public partial class RemoveFlowSourceObject : HashtableFlowSourceBase
    {
        public override string Name
        {
            get { return "Hashtable.Remove"; }
        }

        [ToolTipText("Hashtable_Remove_Key")]
        public object Key
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
                Hashtable.Remove(Key);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
