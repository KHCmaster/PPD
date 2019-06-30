using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<IEnumerable<object>>
    {
        public override string Name
        {
            get { return "Enumerable.IsType"; }
        }
    }
}
