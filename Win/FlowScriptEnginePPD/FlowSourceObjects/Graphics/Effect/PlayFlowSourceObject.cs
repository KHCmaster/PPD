using Effect2D;
using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Effect
{
    [ToolTipText("Effect_Play_Summary")]
    public partial class PlayFlowSourceObject : EffectFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.Effect.Play"; }
        }

        [ToolTipText("Effect_Play_PlayType")]
        public EffectManager.PlayType PlayType
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(PlayType));
            SetValue(nameof(Object));

            if (Object != null)
            {
                Object.PlayType = PlayType;
                Object.Play();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
