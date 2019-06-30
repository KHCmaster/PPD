using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    [ToolTipText("HashSet_IntersectWith_Summary")]
    public partial class IntersectWithFlowSourceObject : HashSetFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "HashSet.IntersectWith"; }
        }

        [ToolTipText("HashSet_IntersectWith_List")]
        public IEnumerable<object> List
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetHashSet();
            SetValue(nameof(List));
            if (HashSet != null && List != null)
            {
                HashSet.IntersectWith(List.ToArray());
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
