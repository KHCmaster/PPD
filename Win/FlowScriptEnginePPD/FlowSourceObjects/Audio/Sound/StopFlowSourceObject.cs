using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.Audio.Sound
{
    [ToolTipText("Sound_Stop_Summary")]
    public partial class StopFlowSourceObject : SoundFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Audio.Sound.Stop"; }
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
