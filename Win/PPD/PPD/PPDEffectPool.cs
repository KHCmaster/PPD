using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using PPDFramework;

namespace PPD
{
    class PPDEffectPool
    {
        Queue<EffectObject> NonUsedList;
        string filename;
        Device device;
        PPDFramework.Resource.ResourceManager resourceManager;
        public PPDEffectPool(string filename, int count, PPDFramework.Resource.ResourceManager resourceManager, Device device)
        {
            this.filename = filename;
            this.device = device;
            this.resourceManager = resourceManager;
            NonUsedList = new Queue<EffectObject>(count);
            for (int i = 0; i < count; i++)
            {
                NonUsedList.Enqueue(CreateEffect());
            }
        }
        private EffectObject CreateEffect()
        {
            EffectObject eo = new EffectObject(filename, 0, 0, resourceManager, device);
            eo.Finish += new EventHandler(eo_Finish);
            return eo;
        }
        void eo_Finish(object sender, EventArgs e)
        {
            EffectObject eo = sender as EffectObject;
            NonUsedList.Enqueue(eo);
        }
        public EffectObject Use(Vector2 pos)
        {
            EffectObject eo;
            if (NonUsedList.Count == 0)
            {
                eo = CreateEffect();
            }
            else
            {
                eo = NonUsedList.Dequeue();
            }
            eo.Stop();
            eo.PlayType = Effect2D.EffectManager.PlayType.Once;
            eo.Position = pos;
            eo.Play();
            return eo;
        }
    }
}
