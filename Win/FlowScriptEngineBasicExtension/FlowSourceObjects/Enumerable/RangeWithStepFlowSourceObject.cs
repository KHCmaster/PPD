using System.Collections.Generic;

namespace FlowScriptEngineBasicExtension.FlowSourceObjects.Enumerable
{
    public partial class RangeWithStepFlowSourceObject : RangeFlowSourceObjectBase
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

        [ToolTipText("Enumerable_Range_Step")]
        public int Step
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
                SetValue(nameof(Step));
                return GetRange(StartIndex, EndIndex, Step);
            }
        }
    }
}
