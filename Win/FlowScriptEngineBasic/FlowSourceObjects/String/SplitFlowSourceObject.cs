using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_Split_Summary")]
    public partial class SplitFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.Split"; }
        }

        [ToolTipText("String_Split_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_Split_Separators")]
        public IEnumerable<object> Separators
        {
            private get;
            set;
        }

        [ToolTipText("String_Split_RemoveEmpty")]
        public bool RemoveEmpty
        {
            private get;
            set;
        }

        [ToolTipText("String_Split_Value")]
        public object[] Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(Separators));
                if (A != null && Separators != null)
                {
                    SetValue(nameof(RemoveEmpty));
                    return A.Split(Separators.Select(s => s.ToString()).ToArray(),
                        RemoveEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
                }
                return null;
            }
        }
    }
}
