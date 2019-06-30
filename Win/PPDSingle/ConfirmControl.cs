using PPDFramework;
using PPDFramework.Shaders;
using SharpDX;
using System;

namespace PPDSingle
{
    /// <summary>
    /// 確認画面
    /// </summary>
    class ConfirmControl : FocusableGameComponent
    {
        enum State
        {
            notappeared = 0,
            appeared = 1,
            vanishing = 2,
            next = 3
        }
        TextureString songname;
        TextureString difficulty;
        EffectObject confirm;
        RectangleComponent black;

        SpriteObject sprite;

        State state;

        public ConfirmControl(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            state = State.notappeared;
            confirm = new EffectObject(device, resourceManager, Utility.Path.Combine("confirm.etd"))
            {
                Position = new Vector2(400, 225)
            };
            confirm.Finish += confirm_Finish;
            black = new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0
            };
            sprite = new SpriteObject(device);
            songname = new TextureString(device, "", 20, 210, PPDColors.White)
            {
                Position = new Vector2(400, 150)
            };
            difficulty = new TextureString(device, "", 20, true, PPDColors.White)
            {
                Position = new Vector2(400, 190)
            };

            this.AddChild(sprite);
            sprite.AddChild(songname);
            sprite.AddChild(difficulty);
            sprite.AddChild(new TextureString(device, Utility.Language["IsGoingToBePlayed"], 20, true, PPDColors.White)
            {
                Position = new Vector2(400, 230)
            });
            sprite.AddChild(new TextureString(device, Utility.Language["Yes"], 20, true, PPDColors.White)
            {
                Position = new Vector2(352, 262)
            });
            sprite.AddChild(new TextureString(device, Utility.Language["No"], 20, true, PPDColors.White)
            {
                Position = new Vector2(466, 262)
            });
            this.AddChild(confirm);
            this.AddChild(black);
        }

        /// <summary>
        /// 再生終了をハンドルする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void confirm_Finish(object sender, EventArgs e)
        {
            switch (state)
            {
                case State.vanishing:
                    state = State.notappeared;
                    confirm.Alpha = 0;
                    break;
                case State.notappeared:
                    ForceShow();
                    break;
                case State.next:
                    confirm.Alpha = 0;
                    break;
            }
        }

        /// <summary>
        /// 情報(譜面名と難易度）設定
        /// </summary>
        /// <param name="name"></param>
        /// <param name="diff"></param>
        public void SetInfo(string name, string diff)
        {
            songname.Text = name;
            if (songname.Width > 200)
            {
                songname.Position = new Vector2(290, songname.Position.Y);
            }
            else
            {
                songname.Position = new Vector2(400 - songname.Width / 2, songname.Position.Y);
            }
            songname.AllowScroll = true;
            difficulty.Text = diff;
            black.Alpha = 0;
            sprite.Alpha = 0;
        }

        protected override void UpdateImpl()
        {
            switch (state)
            {
                case State.notappeared:
                    black.Alpha += 0.02f;
                    if (black.Alpha >= 0.5f)
                    {
                        black.Alpha = 0.5f;
                    }

                    break;
                case State.vanishing:
                    black.Alpha -= 0.1f;
                    if (black.Alpha < 0)
                    {
                        black.Alpha = 0;
                    }

                    break;
                case State.next:
                    black.Alpha += 0.1f;
                    if (black.Alpha >= 1)
                    {
                        black.Alpha = 1;
                    }

                    break;
            }
        }

        protected override bool OnCanUpdate()
        {
            return !Hidden && Focused;
        }

        protected override bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return Focused;
        }

        /// <summary>
        /// 消える
        /// </summary>
        public void Vanish()
        {
            state = State.vanishing;
            sprite.Alpha = 0;
            confirm.Stop();
            confirm.PlayType = Effect2D.EffectManager.PlayType.ReverseOnce;
            confirm.Play();
        }

        /// <summary>
        /// 強制的に可視状態にする
        /// </summary>
        public void ForceShow()
        {
            black.Alpha = 0.5f;
            state = State.appeared;
            sprite.Alpha = 1;
            confirm.Alpha = 1;
            confirm.Seek(EffectObject.SeekPosition.End);
        }

        public void Show(object sender, EventArgs args)
        {
            if (state != State.appeared)
            {
                state = State.notappeared;
                confirm.Alpha = 1;
                sprite.Alpha = 0;
                confirm.Stop();
                confirm.PlayType = Effect2D.EffectManager.PlayType.Once;
                confirm.Play();
            }
        }

        /// <summary>
        /// 可視状態かどうか
        /// </summary>
        public bool Appeared
        {
            get
            {
                return state == State.appeared;
            }
        }

        /// <summary>
        /// 次のシーンに行く
        /// </summary>
        public void Next()
        {
            state = State.next;
            sprite.Alpha = 0;
            confirm.Stop();
            confirm.PlayType = Effect2D.EffectManager.PlayType.ReverseOnce;
            confirm.Play();
        }
    }
}
