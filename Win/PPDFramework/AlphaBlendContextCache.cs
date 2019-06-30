using PPDFramework.Shaders;
using System.Collections.Generic;

namespace PPDFramework
{
    class AlphaBlendContextCache : DisposableComponent
    {
        List<AlphaBlendContext> allContexts;
        int gotIndex;

        public AlphaBlendContextCache(int initialCount)
        {
            allContexts = new List<AlphaBlendContext>();
            for (var i = 0; i < initialCount; i++)
            {
                allContexts.Add(new AlphaBlendContext());
            }
        }

        public AlphaBlendContext Get()
        {
            AlphaBlendContext ret;
            lock (allContexts)
            {
                if (allContexts.Count <= gotIndex)
                {
                    ret = new AlphaBlendContext();
                    allContexts.Add(ret);
                }
                else
                {
                    ret = allContexts[gotIndex];
                    ret.Initialize();
                }
                gotIndex++;
            }
            return ret;
        }

        public AlphaBlendContext Clone(AlphaBlendContext context)
        {
            var newContext = Get();
            context.Clone(newContext);
            return newContext;
        }

        public void Reset()
        {
            gotIndex = 0;
        }
    }
}
