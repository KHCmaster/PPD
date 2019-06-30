using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.EvaluateType
{
    [ToolTipText("EvaluateType_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        PPDFramework.MarkEvaluateType evaluateType;

        public override string Name
        {
            get { return "PPD.EvaluateType.Value"; }
        }

        [ToolTipText("EvaluateType_Value_Value")]
        public PPDFramework.MarkEvaluateType Value
        {
            get
            {
                SetValue(nameof(Value));
                return evaluateType;
            }
            set { evaluateType = value; }
        }
    }
}
