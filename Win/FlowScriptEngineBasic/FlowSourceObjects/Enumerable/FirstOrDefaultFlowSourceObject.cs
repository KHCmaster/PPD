using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_FirstOrDefault_Summary")]
    public partial class FirstOrDefaultFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.FirstOrDefault"; }
        }

        [ToolTipText("Enumerable_FirstOrDefault_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_FirstOrDefault_Default")]
        public object Default
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_FirstOrDefault_First")]
        public object First
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
                    return Enumerable.First();
                }
                catch
                {
                    return Default;
                }
            }
        }
    }
}
