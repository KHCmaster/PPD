using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    [ToolTipText("HashSet_SymmetricExceptWith_Summary")]
    public partial class SymmetricExceptWithFlowSourceObject : HashSetFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "HashSet.SymmetricExceptWith"; }
        }

        [ToolTipText("HashSet_SymmetricExceptWith_List")]
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
                HashSet.SymmetricExceptWith(List.ToArray());
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
