using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable.Ordered
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<IOrderedEnumerable<object>>
    {
        public override string Name
        {
            get { return "Enumerable.Ordered.IsType"; }
        }
    }
}
