using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    [ToolTipText("Hashtable_Values_Summary", "Hashtable_Values_Remark")]
    public partial class ValuesFlowSourceObject : HashtableFlowSourceBase
    {
        public override string Name
        {
            get { return "Hashtable.Values"; }
        }

        [ToolTipText("Hashtable_Values_Value")]
        public IEnumerable<object> Value
        {
            get;
            private set;
        }

        public override void In(FlowEventArgs e)
        {
            SetHashtable();
            if (Hashtable != null)
            {
                Value = Hashtable.Values;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
