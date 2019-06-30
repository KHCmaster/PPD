using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.SoundManager
{
    [ToolTipText("SoundManager_Stop_Summary")]
    public partial class StopFlowSourceObjectBase : SoundManagerFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.SoundManager.Stop"; }
        }

        [ToolTipText("SoundManager_Stop_Index")]
        public int Index
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            if (soundManager != null)
            {
                SetValue(nameof(Index));
                if (soundManager.Stop(Index))
                {
                    OnSuccess();
                }
                else
                {
                    OnFailed();
                }
            }
            else
            {
                OnFailed();
            }
        }
    }
}
