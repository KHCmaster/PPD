using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Random
{
    [ToolTipText("Random_Next_Summary")]
    public partial class NextFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Random.Next"; }
        }

        [ToolTipText("Random_Next_Min")]
        public int Min
        {
            private get;
            set;
        }

        [ToolTipText("Random_Next_Max")]
        public int Max
        {
            private get;
            set;
        }

        [ToolTipText("Random_Next_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(Min));
                SetValue(nameof(Max));
                return RandomUtility.Rand.Next(Min, Max);
            }
        }
    }
}
