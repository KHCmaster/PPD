using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Sum_Summary")]
    public partial class SumDoubleFlowSourceObject : SumFlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Sum_Sum")]
        public double Sum
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return 0;
                }
                return Enumerable.Sum(obj =>
                {
                    Value = obj;
                    FireSelectEvent();
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
            }
        }

        [ToolTipText("Enumerable_Sum_Result")]
        public double Result
        {
            private get;
            set;
        }
    }
}
