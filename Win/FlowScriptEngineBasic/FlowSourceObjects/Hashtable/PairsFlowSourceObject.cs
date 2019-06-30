using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    [ToolTipText("Hashtable_Pairs_Summary", "Hashtable_Pairs_Remark")]
    public partial class PairsFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Hashtable.Pairs"; }
        }

        [ToolTipText("Hashtable_Hashtable")]
        public Dictionary<object, object> Hashtable
        {
            private get;
            set;
        }

        [ToolTipText("Hashtable_Pairs_Pairs")]
        public IEnumerable<object> Pairs
        {
            get
            {
                SetValue(nameof(Hashtable));
                if (Hashtable != null)
                {
                    return Hashtable.Cast<object>();
                }
                return null;
            }
        }
    }
}
