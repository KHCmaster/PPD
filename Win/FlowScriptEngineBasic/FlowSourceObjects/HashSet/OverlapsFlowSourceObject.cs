using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    [ToolTipText("HashSet_Overlaps_Summary")]
    public partial class OverlapsFlowSourceObject : HashSetFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "HashSet.Overlaps"; }
        }

        [ToolTipText("HashSet_Overlaps_List")]
        public IEnumerable<object> List
        {
            private get;
            set;
        }

        [ToolTipText("HashSet_Overlaps_Result")]
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
                Result = HashSet.Overlaps(List.ToArray());
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
