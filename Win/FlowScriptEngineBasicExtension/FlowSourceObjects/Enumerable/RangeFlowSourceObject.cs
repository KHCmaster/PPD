using System.Collections.Generic;

namespace FlowScriptEngineBasicExtension.FlowSourceObjects.Enumerable
{
    public partial class RangeFlowSourceObject : RangeFlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Range_EndIndex")]
        public int EndIndex
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Range_Range")]
        public IEnumerable<object> Range
        {
            get
            {
                SetValue(nameof(EndIndex));
                return GetRange(0, EndIndex, 1);
            }
        }
    }
}
