using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Max_Summary")]
    public partial class MaxDoubleFlowSourceObject : MaxFlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Max_Max")]
        public double Max
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return 0;
                }
                return Enumerable.Max(obj =>
                {
                    Value = obj;
                    FireSelectEvent();
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
            }
        }

        [ToolTipText("Enumerable_Max_Result")]
        public double Result
        {
            private get;
            set;
        }
    }
}
