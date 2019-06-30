using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Pair
{
    [ToolTipText("Pair_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Pair.Value"; }
        }

        [ToolTipText("Pair_Value_Pair")]
        public KeyValuePair<object, object> Pair
        {
            private get;
            set;
        }

        [ToolTipText("Pair_Value_Key")]
        public object Key
        {
            get
            {
                SetValue(nameof(Pair));
                return Pair.Key;
            }
        }

        [ToolTipText("Pair_Value_Value")]
        public object Value
        {
            get
            {
                SetValue(nameof(Pair));
                return Pair.Value;
            }
        }
    }
}
