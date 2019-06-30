using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    [ToolTipText("Hashtable_Clear_Summary")]
    public partial class ClearFlowSourceObject : HashtableFlowSourceBase
    {
        public override string Name
        {
            get { return "Hashtable.Clear"; }
        }

        public override void In(FlowEventArgs e)
        {
            SetHashtable();
            if (Hashtable != null)
            {
                Hashtable.Clear();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
