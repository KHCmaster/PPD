using System.Linq;

namespace FlowScriptEnginePPD.FlowSourceObjects.EventManager
{
    [ToolTipText("EventManager_Value_Summary")]
    public partial class ValueFlowSourceManager : EventManagerFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.EventManager.Value"; }
        }

        [ToolTipText("EventManager_Value_VolumePercents")]
        public object[] VolumePercents
        {
            get
            {
                if (eventManager == null)
                {
                    return null;
                }
                return eventManager.VolumePercents.Cast<object>().ToArray();
            }
        }

        [ToolTipText("EventManager_Value_KeepPlayings")]
        public object[] KeepPlayings
        {
            get
            {
                if (eventManager == null)
                {
                    return null;
                }
                return eventManager.KeepPlayings.Cast<object>().ToArray();
            }
        }

        [ToolTipText("EventManager_Value_ReleaseSounds")]
        public object[] ReleaseSounds
        {
            get
            {
                if (eventManager == null)
                {
                    return null;
                }
                return eventManager.ReleaseSounds.Cast<object>().ToArray();
            }
        }
    }
}
