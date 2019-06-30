using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    [ToolTipText("HashSet_IsProperSupersetOf_Summary")]
    public partial class IsProperSupersetOfFlowSourceObject : HashSetFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "HashSet.IsProperSupersetOf"; }
        }

        [ToolTipText("HashSet_IsProperSupersetOf_List")]
        public IEnumerable<object> List
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_IsProperSupersetOf_Result")]
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
                Result = HashSet.IsProperSupersetOf(List.ToArray());
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
