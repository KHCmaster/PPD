using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_SingleOrDefault_Summary")]
    public partial class SingleOrDefaultFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.SingleOrDefault"; }
        }

        [ToolTipText("Enumerable_SingleOrDefault_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_SingleOrDefault_Default")]
        public object Default
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_SingleOrDefault_Single")]
        public object Single
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
                    return Enumerable.Single();
                }
                catch
                {
                    return Default;
                }
            }
        }
    }
}
