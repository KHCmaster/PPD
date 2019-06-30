using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable.Lookup
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<ILookup<object, object>>
    {
        public override string Name
        {
            get { return "Enumerable.Lookup.IsType"; }
        }
    }
}
