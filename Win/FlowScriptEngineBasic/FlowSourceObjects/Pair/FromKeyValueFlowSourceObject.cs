using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Pair
{
    [ToolTipText("Pair_FromKeyValue_Summary")]
    public partial class FromKeyValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Pair.FromKeyValue"; }
        }

        [ToolTipText("Pair_FromKeyValue_Pair")]
        public KeyValuePair<object, object> Pair
        {
            get
            {
                SetValue(nameof(Key));
                SetValue(nameof(Value));
                return new KeyValuePair<object, object>(Key, Value);
            }
        }

        [ToolTipText("Pair_FromKeyValue_Key")]
        public object Key
        {
            private get;
            set;
        }

        [ToolTipText("Pair_FromKeyValue_Value")]
        public object Value
        {
            private get;
            set;
        }
    }
}
