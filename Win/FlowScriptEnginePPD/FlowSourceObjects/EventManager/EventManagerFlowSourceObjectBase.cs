using FlowScriptEngine;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.EventManager
{
    public abstract class EventManagerFlowSourceObjectBase : FlowSourceObjectBase
    {
        protected IEventManager eventManager;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (Manager.Items.ContainsKey("EventManager"))
            {
                eventManager = (IEventManager)Manager.Items["EventManager"];
            }
        }
    }
}
