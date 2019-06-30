using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.HashSet
{
    public partial class ValueFlowSourceObject : TemplateClassValueFlowSourceObject<HashSet<object>>
    {
        public override string Name
        {
            get { return "HashSet.Value"; }
        }
    }
}
