using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Audio.Sound
{
    [ToolTipText("Sound_Play_Summary")]
    public partial class PlayFlowSourceObject : SoundFlowSourceObjectBase
    {
        double playRatio = 1;

        public override string Name
        {
            get { return "PPD.Audio.Sound.Play"; }
        }

        protected override void OnInitialize()
        {
            if (Manager.Items.ContainsKey("PlayRatio"))
            {
                playRatio = (double)Manager.Items["PlayRatio"];
            }
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            if (Object != null)
            {
                Object.Play(playRatio);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
