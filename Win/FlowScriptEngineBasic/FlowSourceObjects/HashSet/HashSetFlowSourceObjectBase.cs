using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    public abstract class HashSetFlowSourceObjectBase : ExecutableFlowSourceObject
    {
        [ToolTipText("HashSet_HashSet")]
        public HashSet<object> HashSet
        {
            protected get;
            set;
        }

        protected void SetHashSet()
        {
            SetValue(nameof(HashSet));
        }
    }
}
