using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable.Lookup
{
    [ToolTipText("Enumerable_Lookup_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.Lookup.Value"; }
        }

        [ToolTipText("Enumerable_Lookup_Value_Lookup")]
        public ILookup<object, object> Lookup
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Lookup_Value_Count")]
        public int Count
        {
            get
            {
                SetValue(nameof(Lookup));
                if (Lookup != null)
                {
                    return Lookup.Count;
                }
                return 0;
            }
        }

        [ToolTipText("Enumerable_Lookup_Value_Key")]
        public object Key
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Lookup_Value_EnumerableWithKey")]
        public IEnumerable<object> EnumerableWithKey
        {
            get
            {
                SetValue(nameof(Lookup));
                SetValue(nameof(Key));
                if (Lookup == null)
                {
                    return System.Linq.Enumerable.Empty<object>();
                }
                return Lookup[Key];
            }
        }
    }
}
