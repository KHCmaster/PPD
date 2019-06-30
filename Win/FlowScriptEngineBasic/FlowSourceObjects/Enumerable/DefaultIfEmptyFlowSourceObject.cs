using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_DefaultIfEmpty_Summary")]
    public partial class DefaultIfEmptyFlowSourceObject : FlowSourceObjectBase
    {
        private IEnumerable<object> enumerable;

        public override string Name
        {
            get { return "Enumerable.DefaultIfEmpty"; }
        }

        [ToolTipText("Enumerable_DefaultIfEmpty_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(Enumerable));
                SetValue(nameof(Default));
                if (enumerable == null)
                {
                    return new object[] { Default };
                }
                return enumerable.DefaultIfEmpty(Default);
            }
            set { enumerable = value; }
        }

        [ToolTipText("Enumerable_DefaultIfEmpty_Default")]
        public object Default
        {
            private get;
            set;
        }
    }
}
