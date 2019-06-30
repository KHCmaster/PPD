using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<Dictionary<object, object>>
    {
        public override string Name
        {
            get { return "Hashtable.IsType"; }
        }
    }
}
