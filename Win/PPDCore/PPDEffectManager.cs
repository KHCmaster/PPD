using PPDFramework;
using System.Numerics;
using PPDMath;
using System;
using System.Collections.Generic;

namespace PPDCore
{
    public class PPDEffectManager(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : GameComponent(device)
    {
        const int defaulteffectnum = 10;
        readonly PPDFramework.Resource.ResourceManager resourceManager = resourceManager;
        readonly Dictionary<string, PPDEffectPool> pools = [];

        public bool SuspendAdd
        {
            get;
            set;
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

            if (!String.IsNullOrEmpty(filename) && pools.TryGetValue(filename, out PPDEffectPool value))
            {
                var eo = value.Use(pos);
                eo.Finish += Eo_Finish;
                this.AddChild(eo);
            }
        }

        void Eo_Finish(object sender, EventArgs e)
        {
            var eo = sender as EffectObject;
            this.RemoveChild(eo);
            eo.Finish -= Eo_Finish;
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
