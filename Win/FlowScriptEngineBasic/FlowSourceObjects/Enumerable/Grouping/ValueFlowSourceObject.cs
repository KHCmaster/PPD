using FlowScriptEngine;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable.Grouping
{
    [ToolTipText("Enumerable_Grouping_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.Grouping.Value"; }
        }

        [ToolTipText("Enumerable_Grouping_Value_Grouping")]
        public IGrouping<object, object> Grouping
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Grouping_Value_Key")]
        public object Key
        {
            get
            {
                SetValue(nameof(Grouping));
                if (Grouping != null)
                {
                    return Grouping.Key;
                }
                return null;
            }
        }
    }
}
