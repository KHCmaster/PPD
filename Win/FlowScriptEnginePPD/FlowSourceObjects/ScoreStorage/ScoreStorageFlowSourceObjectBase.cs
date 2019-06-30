using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.ScoreStorage
{
    public abstract class ScoreStorageFlowSourceObjectBase : FlowSourceObjectBase
    {
        protected PPDFramework.ScoreStorage scoreStorage;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (Manager.Items.TryGetValue("ScoreStorage", out object val))
            {
                scoreStorage = (PPDFramework.ScoreStorage)val;
            }
        }
    }
}
