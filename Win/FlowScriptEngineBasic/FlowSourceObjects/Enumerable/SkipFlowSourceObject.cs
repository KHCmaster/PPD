using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Skip_Summary")]
    public partial class SkipFlowSourceObject : FlowSourceObjectBase
    {
        private IEnumerable<object> enumerable;

        public override string Name
        {
            get { return "Enumerable.Skip"; }
        }

        [ToolTipText("Enumerable_Skip_Count")]
        public int Count
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Skip_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (enumerable == null)
                {
                    return System.Linq.Enumerable.Empty<object>();
                }
                SetValue(nameof(Count));
                return enumerable.Skip(Count);
            }
            set
            {
                enumerable = value;
            }
        }
    }
}
