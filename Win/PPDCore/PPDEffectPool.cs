using PPDCoreModel;
using PPDFramework;

namespace PPDCore
{
    class PPDEffectPool : EffectPoolBase
    {
        PathObject filename;
        PPDFramework.Resource.ResourceManager resourceManager;

        public PPDEffectPool(PPDDevice device, PathObject filename, int count, PPDFramework.Resource.ResourceManager resourceManager)
            : base(device, count)
        {
            this.filename = filename;
            this.resourceManager = resourceManager;
            Initialize();
        }

        protected override EffectObject CreateEffect()
        {
            return new EffectObject(device, resourceManager, filename);
        }
    }
}