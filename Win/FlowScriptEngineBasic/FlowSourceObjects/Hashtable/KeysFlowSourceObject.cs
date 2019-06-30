using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    [ToolTipText("Hashtable_Keys_Summary", "Hashtable_Keys_Remark")]
    public partial class KeysFlowSourceObject : HashtableFlowSourceBase
    {
        public override string Name
        {
            get { return "Hashtable.Keys"; }
        }

        [ToolTipText("Hashtable_Keys_Value")]
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
                Value = Hashtable.Keys;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
