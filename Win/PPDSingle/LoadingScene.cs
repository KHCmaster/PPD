using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System;
using System.Collections.Generic;

namespace PPDSingle
{
    /// <summary>
    /// ロード中用のシーン
    /// </summary>
    public class LoadingScene : LoadingSceneWithImage
    {
        EffectObject anim;
        TextureString progressText;
        Queue<int> progressQueue;

        public LoadingScene(PPDDevice device) : base(device)
        {
            progressQueue = new Queue<int>();
        }

        public override bool Load()
        {
            anim = new EffectObject(device, ResourceManager, Utility.Path.Combine("Loading.etd"))
            {
                Position = new Vector2(700, 420)
            };
            anim.PlayType = Effect2D.EffectManager.PlayType.Loop;
            anim.Play();
            progressText = new TextureString(device, "", 20, PPDColors.White)
            {
                Position = new Vector2(10, 420)
            };
            progressText.Border = new Border
            {
                Color = PPDColors.Black,
                Thickness = 1
            };
            AddChild(anim);
            AddChild(progressText);

            return true;
        }

        public override void EnterLoading()
        {
            lock (progressQueue)
            {
                progressQueue.Clear();
            }
            progressText.Hidden = true;
            base.EnterLoading();
        }

        public override void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            lock (progressQueue)
            {
                if (progressQueue.Count > 0)
                {
                    int val = 0;
                    while (progressQueue.Count > 0)
                    {
                        val = progressQueue.Dequeue();
                    }
                    progressText.Hidden = false;
                    progressText.Text = String.Format("{0}%", val);
                }
            }
            Update();
            base.Update(inputInfo, mouseInfo);
        }

        public override void SendToLoading(Dictionary<string, object> parameters)
        {
            if (parameters == null)
            {
                return;
            }
            if (parameters.ContainsKey("Progress"))
            {
                var val = parameters["Progress"];
                if (val is int)
                {
                    lock (progressQueue)
                    {
                        progressQueue.Enqueue((int)val);
                    }
                }
            }
        }
    }
}
