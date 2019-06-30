using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;
using PPDFramework;


namespace testgame
{
    class LoadingScene : CLoading
    {
        Dictionary<string, ImageResource> resource;
        EffectObject anim;

        public LoadingScene()
        {

        }
        public override void Load()
        {
            resource = new Dictionary<string, ImageResource>();
            anim = new EffectObject("img\\default\\Loading.etd", 700, 420, resource, Device);
            anim.PlayType = Effect2D.EffectManager.PlayType.Loop;
            anim.Play();
        }
        public override void EnterLoading()
        {

        }
        public override void Update(int[] presscount, bool[] released)
        {
            anim.Update();
        }
        public override void Draw()
        {
            anim.Draw();
        }
        protected override void DisposeResource()
        {
            foreach (ImageResource p in resource.Values)
            {
                p.Dispose();
            }
        }
    }
}
