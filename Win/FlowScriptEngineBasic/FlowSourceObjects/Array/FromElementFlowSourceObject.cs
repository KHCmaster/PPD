using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_FromElement_Summary")]
    public partial class FromElementFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.FromElement"; }
        }

        [ToolTipText("Array_FromElement_IsExpanded")]
        public bool IsExpanded
        {
            private get;
            set;
        }

        [ToolTipText("Array_FromElement_A")]
        public object A
        {
            private get;
            set;
        }

        [ToolTipText("Array_FromElement_B")]
        public object B
        {
            private get;
            set;
        }

        [ToolTipText("Array_FromElement_C")]
        public object C
        {
            private get;
            set;
        }

        [ToolTipText("Array_FromElement_D")]
        public object D
        {
            private get;
            set;
        }

        [ToolTipText("Array_FromElement_E")]
        public object E
        {
            private get;
            set;
        }

        [ToolTipText("Array_FromElement_F")]
        public object F
        {
            private get;
            set;
        }

        [ToolTipText("Array_FromElement_Value")]
        public object[] Value
        {
            get
            {
                var ret = new List<object>();
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
                return ret.ToArray();
            }
        }

        private void Add(List<object> list, object value)
        {
            if (IsExpanded && value is object[])
            {
                var castedArray = (object[])value;
                foreach (object elem in castedArray)
                {
                    Add(list, elem);
                }
            }
            else
            {
                list.Add(value);
            }
        }
    }
}
