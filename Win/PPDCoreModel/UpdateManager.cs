using FlowScriptEngine;

namespace PPDCoreModel
{
    public class UpdateManager : TemplateManager<IUpdatable>
    {
        public float MovieTime
        {
            get;
            set;
        }

        public UpdateManager(Engine engine)
            : base(engine)
        {

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
