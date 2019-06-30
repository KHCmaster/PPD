using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable.Grouping
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<IGrouping<object, object>>
    {
        public override string Name
        {
            get { return "Enumerable.Grouping.IsType"; }
        }
    }
}
