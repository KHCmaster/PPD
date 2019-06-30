using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_ElementAtOrDefault_Summary")]
    public partial class ElementAtOrDefaultFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.ElementAtOrDefault"; }
        }

        [ToolTipText("Enumerable_ElementAtOrDefault_Index")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_ElementAtOrDefault_Default")]
        public object Default
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_ElementAtOrDefault_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_ElementAtOrDefault_Value")]
        public object Value
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return null;
                }
                SetValue(nameof(Index));
                SetValue(nameof(Default));
                try
                {
                    return Enumerable.ElementAt(Index);
                }
                catch
                {
                    return Default;
                }
            }
        }
    }
}
