using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Random
{
    [ToolTipText("Random_NextDouble_Summary")]
    public partial class NextDoubleFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Random.NextDouble"; }
        }

        [ToolTipText("Random_NextDouble_Value")]
        public double Value
        {
            get
            {
                return RandomUtility.Rand.NextDouble();
            }
        }
    }
}
