using PPDFramework;
using SharpDX;
using System.Collections.Generic;

namespace PPDCoreModel
{
    public class EffectPool : EffectPoolBase
    {
        ScriptResourceManager scriptResourceManager;

        public string FileName
        {
            get;
            private set;
        }

        public EffectPool(PPDDevice device, string filename, int count, ScriptResourceManager scriptResourceManager)
            : base(device, count)
        {
            FileName = filename;
            this.scriptResourceManager = scriptResourceManager;
            Initialize();
        }

        protected override EffectObject CreateEffect()
        {
            return (EffectObject)scriptResourceManager.GetResource(FileName, PPDCoreModel.Data.ResourceKind.Effect, new Dictionary<string, object>
            {
                {"Position",Vector2.Zero}
            });
        }
    }
}