using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_LastOrDefault_Summary")]
    public partial class LastOrDefaultFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.LastOrDefault"; }
        }

        [ToolTipText("Enumerable_LastOrDefault_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_LastOrDefault_Default")]
        public object Default
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_LastOrDefault_Last")]
        public object Last
        {
            get
            {
                SetValue(nameof(Enumerable));
                SetValue(nameof(Default));
                if (Enumerable == null)
                {
                    return Default;
                }
                try
                {
                    return Enumerable.Last();
                }
                catch
                {
                    return Default;
                }
            }
        }
    }
}
