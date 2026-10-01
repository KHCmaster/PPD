using PPDFramework;
using System.Numerics;
using PPDMath;
using System;
using System.Collections.Generic;

namespace PPDCoreModel
{
    public abstract class EffectPoolBase(PPDDevice device, int count)
    {
        protected PPDDevice device = device;
        readonly Queue<EffectObject> nonUsedList = new(count);

        public int Count
        {
            get;
            private set;
        } = count;

        public void Initialize()
        {
            for (int i = 0; i < Count; i++)
            {
                nonUsedList.Enqueue(CreateEffectInner());
            }
        }

        protected abstract EffectObject CreateEffect();

        private EffectObject CreateEffectInner()
        {
            var eo = CreateEffect();
            eo.Finish += Eo_Finish;
            return eo;
        }

        void Eo_Finish(object sender, EventArgs e)
        {
            var eo = sender as EffectObject;
            nonUsedList.Enqueue(eo);
        }

        public EffectObject Use(Vector2 pos)
        {
            EffectObject eo;
            if (nonUsedList.Count == 0)
            {
                eo = CreateEffect();
            }
            else
            {
                eo = nonUsedList.Dequeue();
            }
            eo.Stop();
            eo.PlayType = Effect2D.EffectManager.PlayType.Once;
            eo.Position = pos;
            eo.Play();
            return eo;
        }
    }
}
