using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<List<object>>
    {
        public override string Name
        {
            get { return "ArrayList.IsType"; }
        }
    }
}
