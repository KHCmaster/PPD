using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    public partial class ValueFlowSourceObject : TemplateClassValueFlowSourceObject<List<object>>
    {
        public override string Name
        {
            get { return "ArrayList.Value"; }
        }
    }
}
