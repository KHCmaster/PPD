using System;
using System.Collections.Generic;
using System.Text;
using PPDFramework;
using SlimDX;
using SlimDX.Direct3D9;

namespace PPD
{
    public class PPDEffectManager : GameComponent
    {
        const int defaulteffectnum = 1;
        List<EffectObject> EffectList;
        Device device;
        PPDFramework.Resource.ResourceManager resourceManager;
        Dictionary<string, PPDEffectPool> pools;
        public PPDEffectManager(Device device, PPDFramework.Resource.ResourceManager resourceManager)
        {
            this.device = device;
            this.resourceManager = resourceManager;
            EffectList = new List<EffectObject>();
            pools = new Dictionary<string, PPDEffectPool>();
        }
        public void CreateEffect(string filename)
        {
            PPDEffectPool ppdep = new PPDEffectPool(filename, defaulteffectnum, resourceManager, device);
            pools.Add(filename, ppdep);
        }
        public void AddEffect(string filename, Vector2 pos)
        {
            if (pools.ContainsKey(filename))
            {
                EffectObject eo = pools[filename].Use(pos);
                eo.Finish += new EventHandler(eo_Finish);
                EffectList.Add(eo);
            }
        }

        void eo_Finish(object sender, EventArgs e)
        {
            EffectObject eo = sender as EffectObject;
            EffectList.Remove(eo);
            eo.Finish -= new EventHandler(eo_Finish);
        }
        public void Clear()
        {
            EffectList.Clear();
        }
        public override void Update()
        {
            for (int i = EffectList.Count - 1; i >= 0; i--) EffectList[i].Update();
        }
        public override void Draw()
        {
            for (int i = EffectList.Count - 1; i >= 0; i--) EffectList[i].Draw();
        }
    }
}
