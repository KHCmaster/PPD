using FlowScriptEngine;
using System.Collections.Generic;

namespace PPDCoreModel
{
    public class TemplateManager<T> : IPriorityManager where T : IPriority
    {
        protected List<T> list;
        protected Engine engine;

        public TemplateManager(Engine engine)
        {
            list = new List<T>();
            this.engine = engine;
        }

        public void Add(T source)
        {
            list.Add(source);
        }

        public void Sort()
        {
            list.Sort((val1, val2) =>
            {
                return val1.Priority - val2.Priority;
            });
        }

        public void Clear()
        {
            list.Clear();
        }
    }
}
