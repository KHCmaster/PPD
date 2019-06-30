using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects
{
    [ToolTipText("EveryFrame_Summary")]
    public partial class EveryFrameFlowSourceObject : UpdatableFlowSourceObjectBase
    {
        [ToolTipText("EveryFrame_Out")]
        public event FlowEventHandler Out;
        public override string Name
        {
            get { return "PPD.EveryFrame"; }
        }

        public override void Update(float movieTime)
        {
            base.Update(movieTime);

            FireEvent(Out);
        }
    }
}
