using PPDFramework;
using PPDShareComponent;
using System.Collections.Generic;


namespace PPDMulti
{
    /// <summary>
    /// ロード中用のシーン
    /// </summary>
    public class LoadingScene : LoadingSceneWithImage
    {
        EffectObject anim;

        public LoadingScene(PPDDevice device) : base(device)
        {

        }
        public override bool Load()
        {
            anim = new EffectObject(device, ResourceManager, Utility.Path.Combine("Loading.etd"))
            {
                Position = new SharpDX.Vector2(700, 420)
            };
            anim.PlayType = Effect2D.EffectManager.PlayType.Loop;
            anim.Play();
            AddChild(anim);

            return true;
        }

        public override void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            if (Disposed) return;
            base.Update();
        }

        public override void SendToLoading(Dictionary<string, object> parameters)
        {
        }
    }
}
