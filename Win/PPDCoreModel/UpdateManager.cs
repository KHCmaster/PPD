using FlowScriptEngine;

namespace PPDCoreModel
{
    public class UpdateManager(Engine engine) : TemplateManager<IUpdatable>(engine)
    {
        public float MovieTime
        {
            get;
            set;
        }

        public void Update()
        {
            foreach (IUpdatable updatable in list)
            {
                updatable.Update(MovieTime);
            }
            engine.Update();
        }
    }
}
