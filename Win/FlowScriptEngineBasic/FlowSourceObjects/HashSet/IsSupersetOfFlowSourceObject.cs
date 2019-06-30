using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    [ToolTipText("HashSet_IsSupersetOf_Summary")]
    public partial class IsSupersetOfFlowSourceObject : HashSetFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "HashSet.IsSupersetOf"; }
        }

        [ToolTipText("HashSet_IsSupersetOf_List")]
        public IEnumerable<object> List
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_IsSupersetOf_Result")]
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
                Result = HashSet.IsSupersetOf(List.ToArray());
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
