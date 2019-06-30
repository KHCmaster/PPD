using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Average_Summary")]
    public partial class AverageFloatFlowSourceObject : AverageFlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Average_Average")]
        public float Average
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return 0;
                }
                return Enumerable.Average(obj =>
                {
                    Value = obj;
                    FireSelectEvent();
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
            }
        }

        [ToolTipText("Enumerable_Average_Result")]
        public float Result
        {
            private get;
            set;
        }
    }
}
