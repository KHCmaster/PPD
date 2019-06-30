using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    [ToolTipText("Hashtable_FromPair_Summary")]
    public partial class FromPairFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Hashtable.FromPair"; }
        }

        [ToolTipText("Hashtable_FromPair_IsExpanded")]
        public bool IsExpanded
        {
            private get;
            set;
        }

        [ToolTipText("Hashtable_FromPair_A")]
        public KeyValuePair<object, object> A
        {
            private get;
            set;
        }

        [ToolTipText("Hashtable_FromPair_B")]
        public KeyValuePair<object, object> B
        {
            private get;
            set;
        }

        [ToolTipText("Hashtable_FromPair_C")]
        public KeyValuePair<object, object> C
        {
            private get;
            set;
        }

        [ToolTipText("Hashtable_FromPair_D")]
        public KeyValuePair<object, object> D
        {
            private get;
            set;
        }

        [ToolTipText("Hashtable_FromPair_E")]
        public KeyValuePair<object, object> E
        {
            private get;
            set;
        }

        [ToolTipText("Hashtable_FromPair_F")]
        public KeyValuePair<object, object> F
        {
            private get;
            set;
        }

        [ToolTipText("Hashtable_FromPair_Value")]
        public Dictionary<object, object> Value
        {
            get
            {
                var ret = new Dictionary<object, object>();
                SetValue(nameof(IsExpanded));
                if (IsConnected("A", true))
                {
                    SetValue(nameof(A));
                    Add(ret, A);
                }
                if (IsConnected("B", true))
                {
                    SetValue(nameof(B));
                    Add(ret, B);
                }
                if (IsConnected("C", true))
                {
                    SetValue(nameof(C));
                    Add(ret, C);
                }
                if (IsConnected("D", true))
                {
                    SetValue(nameof(D));
                    Add(ret, D);
                }
                if (IsConnected("E", true))
                {
                    SetValue(nameof(E));
                    Add(ret, E);
                }
                if (IsConnected("F", true))
                {
                    SetValue(nameof(F));
                    Add(ret, F);
                }
                return ret;
            }
        }

        private void Add(Dictionary<object, object> hashtable, KeyValuePair<object, object> pair)
        {
            if (IsExpanded && pair.Value is Dictionary<object, object>)
            {
                var castedDict = (Dictionary<object, object>)pair.Value;
                foreach (var p in castedDict)
                {
                    Add(hashtable, p);
                }
            }
            else
            {
                hashtable[pair.Key] = pair.Value;
            }
        }
    }
}
