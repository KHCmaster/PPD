using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using PPDFramework;

namespace PPDSingle
{
    /// <summary>
    /// マークの評価表示UI
    /// </summary>
    class HitEvaluate : GameComponent
    {
        enum State
        {
            Cool = 0,
            Good = 1,
            Safe = 2,
            Sad = 3,
            Worst = 4,
            MissCool = 5,
            MissGood = 6,
            Nothing = 10
        }
        State state = State.Nothing;
        int count = 0;
        PictureObject[] evals;
        public HitEvaluate(Device device, PPDFramework.Resource.ResourceManager resourceManager)
        {
            string[] filenames = new string[] { "img\\default\\eva\\cool.png", "img\\default\\eva\\good.png", "img\\default\\eva\\safe.png", "img\\default\\eva\\sad.png", "img\\default\\eva\\worst.png", "img\\default\\eva\\misscool.png", "img\\default\\eva\\missgood.png" };
            evals = new PictureObject[filenames.Length];
            for (int i = 0; i < filenames.Length; i++)
            {
                evals[i] = new PictureObject(filenames[i], 750, 28, true, resourceManager, device);
                evals[i].Hidden = true;
                this.AddChild(evals[i]);
            }
        }
        public void Retry()
        {
            count = 0;
            state = State.Nothing;
            for (int i = 0; i < evals.Length; i++)
            {
                evals[i].Hidden = true;
            }
        }
        public void ChangeState(MarkEvaluateType type, bool isMissPress)
        {
            count = 0;
            if (state != State.Nothing)
            {
                evals[(int)state].Hidden = true;
            }
            state = State.Cool + (int)type;
            if (isMissPress && (type == MarkEvaluateType.Cool || type == MarkEvaluateType.Fine))
            {
                state += 5;
            }
            if (state != State.Nothing)
            {
                evals[(int)state].Hidden = false;
            }
        }
        public override void Update()
        {
            if (disposed) return;
            if (state != State.Nothing)
            {
                count++;
                if (count < 20)
                {
                    evals[(int)state].Scale = new Vector2(0.75f + 0.1f * (20 - count) / 20, 0.75f + 0.1f * (20 - count) / 20);
                }
                else
                {
                    evals[(int)state].Scale = new Vector2(0.75f, 0.75f);
                }
                if (count > 120)
                {
                    evals[(int)state].Hidden = true;
                    state = State.Nothing;
                }
            }
            base.Update();
        }
        public override void Draw()
        {
            if (Hidden) return;
            base.Draw();
        }
    }
}
