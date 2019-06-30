using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Min_Summary")]
    public partial class MinInt32FlowSourceObject : MinFlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Min_Min")]
        public int Min
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return 0;
                }
                return Enumerable.Min(obj =>
                {
                    Value = obj;
                    FireSelectEvent();
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
            }
        }

        [ToolTipText("Enumerable_Min_Result")]
        public int Result
        {
            private get;
            set;
        }
    }
}
