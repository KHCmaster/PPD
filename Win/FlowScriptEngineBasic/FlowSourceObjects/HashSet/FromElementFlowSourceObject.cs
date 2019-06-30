using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    [ToolTipText("HashSet_FromElement_Summary")]
    public partial class FromElementFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "HashSet.FromElement"; }
        }

        [ToolTipText("HashSet_FromElement_IsExpanded")]
        public bool IsExpanded
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_FromElement_A")]
        public object A
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_FromElement_B")]
        public object B
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_FromElement_C")]
        public object C
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_FromElement_D")]
        public object D
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_FromElement_E")]
        public object E
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_FromElement_F")]
        public object F
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_FromElement_Value")]
        public System.Collections.Generic.HashSet<object> Value
        {
            get
            {
                var ret = new System.Collections.Generic.HashSet<object>();
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

        private void Add(System.Collections.Generic.HashSet<object> hashSet, object value)
        {
            if (IsExpanded && value is System.Collections.Generic.HashSet<object>)
            {
                foreach (object elem in (System.Collections.Generic.HashSet<object>)value)
                {
                    Add(hashSet, elem);
                }
            }
            else
            {
                hashSet.Add(value);
            }
        }
    }
}
