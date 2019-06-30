using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    public partial class ValueFlowSourceObject : TemplateClassValueFlowSourceObject<Dictionary<object, object>>
    {
        public override string Name
        {
            get { return "Hashtable.Value"; }
        }
    }
}
