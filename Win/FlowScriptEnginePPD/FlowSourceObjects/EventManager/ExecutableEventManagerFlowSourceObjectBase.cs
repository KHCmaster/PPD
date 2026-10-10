using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.EventManager
{
    public abstract class ExecutableEventManagerFlowSourceObjectBase : ExecutableFlowSourceObject
    {
        protected IEventManager eventManager;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (Manager.Items.TryGetValue("EventManager", out object value))
            {
                eventManager = (IEventManager)value;
            }
        }
    }
}
