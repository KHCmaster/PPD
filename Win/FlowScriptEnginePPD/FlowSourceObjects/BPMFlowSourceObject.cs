using FlowScriptEngine;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects
{
    [ToolTipText("BPM_Summary")]
    public partial class BPMFlowSourceObject : FlowSourceObjectBase
    {
        private BPMManager bpmManager;

        public override string Name
        {
            get { return "PPD.BPM"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.TryGetValue("BPMManager", out object value))
            {
                bpmManager = value as BPMManager;
            }
        }

        [ToolTipText("BPM_CurrentBPM")]
        public float CurrentBPM
        {
            get { return bpmManager != null ? bpmManager.CurrentBPM : 0; }
            set
            {
                bpmManager?.CurrentBPM = value;
            }
        }

        [ToolTipText("BPM_TargetBPM")]
        public float TargetBPM
        {
            get { return bpmManager != null ? bpmManager.TargetBPM : 0; }
            set
            {
                bpmManager?.TargetBPM = value;
            }
        }

        [ToolTipText("BPM_SetCurrentBPM")]
        public void SetCurrentBPM(FlowEventArgs e)
        {
            SetValue(nameof(CurrentBPM));
            TargetBPM = CurrentBPM;
        }

        [ToolTipText("BPM_SetTargetBPM")]
        public void SetTargetBPM(FlowEventArgs e)
        {
            SetValue(nameof(TargetBPM));
        }
    }
}
