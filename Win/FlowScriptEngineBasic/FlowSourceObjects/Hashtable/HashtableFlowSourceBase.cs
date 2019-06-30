using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    public abstract class HashtableFlowSourceBase : ExecutableFlowSourceObject
    {
        [ToolTipText("Hashtable_Hashtable")]
        public Dictionary<object, object> Hashtable
        {
            protected get;
            set;
        }

        protected void SetHashtable()
        {
            SetValue(nameof(Hashtable));
        }
    }
}
