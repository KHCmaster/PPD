using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Effect
{
    [ToolTipText("Effect_Pause_Summary")]
    public partial class PauseFlowSourceObject : EffectFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.Effect.Pause"; }
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));

            if (Object != null)
            {
                Object.Pause();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
