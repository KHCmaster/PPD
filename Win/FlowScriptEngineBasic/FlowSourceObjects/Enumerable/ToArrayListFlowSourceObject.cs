using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_ToArrayList_Summary")]
    public partial class ToArrayListFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.ToArrayList"; }
        }

        [ToolTipText("Enumerable_ToArrayList_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_ToArrayList_ArrayList")]
        public List<object> ArrayList
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return new List<object>();
                }
                return Enumerable.ToList();
            }
        }
    }
}
