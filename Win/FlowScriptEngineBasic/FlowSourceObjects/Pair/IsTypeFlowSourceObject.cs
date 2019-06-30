using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Pair
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<KeyValuePair<object, object>>
    {
        public override string Name
        {
            get { return "Pair.IsType"; }
        }
    }
}
