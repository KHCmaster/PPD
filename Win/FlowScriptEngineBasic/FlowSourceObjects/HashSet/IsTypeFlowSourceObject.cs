using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<HashSet<object>>
    {
        public override string Name
        {
            get { return "HashSet.IsType"; }
        }
    }
}
