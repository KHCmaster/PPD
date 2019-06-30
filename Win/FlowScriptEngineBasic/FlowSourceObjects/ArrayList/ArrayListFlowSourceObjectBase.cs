using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    public abstract class ArrayListFlowSourceObjectBase : ExecutableFlowSourceObject
    {
        [ToolTipText("ArrayList_ArrayList")]
        public List<object> ArrayList
        {
            protected get;
            set;
        }

        protected void SetArrayList()
        {
            SetValue(nameof(ArrayList));
        }
    }
}
