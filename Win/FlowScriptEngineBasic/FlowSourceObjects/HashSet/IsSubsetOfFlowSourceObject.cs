using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    [ToolTipText("HashSet_IsSubsetOf_Summary")]
    public partial class IsSubsetOfFlowSourceObject : HashSetFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "HashSet.IsSubsetOf"; }
        }

        [ToolTipText("HashSet_IsSubsetOf_List")]
        public IEnumerable<object> List
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_IsSubsetOf_Result")]
        public bool Result
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetHashSet();
            SetValue(nameof(List));
            if (HashSet != null && List != null)
            {
                Result = HashSet.IsSubsetOf(List.ToArray());
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
