using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Sum_Summary")]
    public partial class SumInt32FlowSourceObject : SumFlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Sum_Sum")]
        public int Sum
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
        public int Result
        {
            private get;
            set;
        }
    }
}
