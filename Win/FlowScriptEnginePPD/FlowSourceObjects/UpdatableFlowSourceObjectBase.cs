using FlowScriptEngine;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects
{
    public abstract class UpdatableFlowSourceObjectBase : FlowSourceObjectBase, IUpdatable
    {
        protected UpdateManager updateManager;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.TryGetValue("UpdateManager", out object obj))
            {
                updateManager = obj as UpdateManager;
                updateManager.Add(this);
            }
        }

        [Ignore]
        public virtual int Priority
        {
            get;
            private set;
        }

        public virtual void Update(float movieTime)
        {

        }
    }
}
