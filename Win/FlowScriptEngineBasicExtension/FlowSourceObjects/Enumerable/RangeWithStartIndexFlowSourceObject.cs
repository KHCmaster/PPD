using System.Collections.Generic;

namespace FlowScriptEngineBasicExtension.FlowSourceObjects.Enumerable
{
    public partial class RangeWithStartIndexFlowSourceObject : RangeFlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Range_EndIndex")]
        public int EndIndex
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Range_StartIndex")]
        public int StartIndex
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
                SetValue(nameof(StartIndex));
                return GetRange(StartIndex, EndIndex, 1);
            }
        }
    }
}
