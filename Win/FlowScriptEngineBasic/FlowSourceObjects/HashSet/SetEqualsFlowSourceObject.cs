using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    [ToolTipText("HashSet_SetEquals_Summary")]
    public partial class SetEqualsFlowSourceObject : HashSetFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "HashSet.SetEquals"; }
        }

        [ToolTipText("HashSet_SetEquals_List")]
        public IEnumerable<object> List
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_SetEquals_Result")]
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
                Result = HashSet.SetEquals(List.ToArray());
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
