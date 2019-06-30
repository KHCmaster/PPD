using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_At_Summary", "ArrayList_At_Remark")]
    public partial class AtFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.At"; }
        }

        [ToolTipText("ArrayList_At_Index")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_ArrayList")]
        public List<object> ArrayList
        {
            protected get;
            set;
        }

        [ToolTipText("ArrayList_At_Value")]
        public object Value
        {
            get
            {
                SetValue(nameof(ArrayList));
                if (ArrayList != null)
                {
                    SetValue(nameof(Index));
                    return ArrayList[Index];
                }
                return null;
            }
        }
    }
}
