using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Min_Summary")]
    public partial class MinFloatFlowSourceObject : MinFlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Min_Min")]
        public float Min
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
        public float Result
        {
            private get;
            set;
        }
    }
}
