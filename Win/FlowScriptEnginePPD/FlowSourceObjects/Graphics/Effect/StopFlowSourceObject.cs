using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Effect
{
    [ToolTipText("Effect_Stop_Summary")]
    public partial class StopFlowSourceObject : EffectFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.Effect.Stop"; }
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));

            if (Object != null)
            {
                Object.Stop();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
