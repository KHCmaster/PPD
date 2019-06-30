using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasicExtension.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Range_Summary")]
    public abstract class RangeFlowSourceObjectBase : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Enumerable.Range"; }
        }

        protected IEnumerable<object> GetRange(int startIndex, int endIndex, int step)
        {
            if (step > 0)
            {
                for (int i = startIndex; i < endIndex; i += step)
                {
                    yield return i;
                }
            }
            else if (step < 0)
            {
                for (int i = startIndex; i > endIndex; i += step)
                {
                    yield return i;
                }
            }
        }
    }
}
