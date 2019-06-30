using PPDFramework;
using SharpDX;
using System;
using System.Collections.Generic;

namespace PPDCore
{
    public class PPDEffectManager : GameComponent
    {
        const int defaulteffectnum = 10;
        PPDFramework.Resource.ResourceManager resourceManager;
        Dictionary<string, PPDEffectPool> pools;

        public bool SuspendAdd
        {
            get;
            set;
        }

        public PPDEffectManager(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            this.resourceManager = resourceManager;
            pools = new Dictionary<string, PPDEffectPool>();
        }
        public void CreateEffect(PathObject filename)
        {
            var ppdep = new PPDEffectPool(device, filename, defaulteffectnum, resourceManager);
            pools.Add(filename, ppdep);
        }
        public void AddEffect(string filename, Vector2 pos)
        {
            if (SuspendAdd)
            {
                return;
            }

            if (!String.IsNullOrEmpty(filename) && pools.ContainsKey(filename))
            {
                var eo = pools[filename].Use(pos);
                eo.Finish += eo_Finish;
                this.AddChild(eo);
            }
        }

        void eo_Finish(object sender, EventArgs e)
        {
            var eo = sender as EffectObject;
            this.RemoveChild(eo);
            eo.Finish -= eo_Finish;
        }

        public void Clear()
        {
            ClearChildren();
        }

        public void Retry()
        {
            SetDefault();
        }
    }
}
