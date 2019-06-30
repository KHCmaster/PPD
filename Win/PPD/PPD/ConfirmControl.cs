using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using PPDFramework;

namespace testgame
{
    class ConfirmControl : GameComponent
    {
        enum State
        {
            notappeared = 0,
            appeared = 1,
            vanishing = 2,
            next = 3
        }
        StringObject songname;
        StringObject difficulty;
        StringObject play;
        EffectObject confirm;
        PictureObject black;
        Dictionary<string, ImageResource> resource;
        State state;
        public ConfirmControl(Device device, Sprite sprite)
        {
            state = State.notappeared;
            resource = new Dictionary<string, ImageResource>();
            confirm = new EffectObject("img\\default\\confirm.etd", 400, 225, resource, device);
            confirm.Finish += new EventHandler(confirm_Finish);
            ImageResource p = new ImageResource("img\\default\\black.png", device);
            resource.Add("img\\default\\black.png", p);
            black = new PictureObject("img\\default\\black.png", 0, 0, resource, device);
            black.Alpha = 0;
            songname = new StringObject("", 0, 160, 20, 210, new Color4(1, 1, 1, 1), device, sprite);
            difficulty = new StringObject("", 0, 195, 20, 210, new Color4(1, 1, 1, 1), device, sprite);
            play = new StringObject("をプレイします", 0, 230, 20, new Color4(1, 1, 1, 1), device, sprite);
        }

        void confirm_Finish(object sender, EventArgs e)
        {
            if (state == State.vanishing) Focused = false;
            else if (state == State.notappeared) Show();
            else if (state == State.next) confirm.Alpha = 0;
        }
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
            difficulty.Position = new Vector2(400 - difficulty.Width / 2, difficulty.Position.Y);
            play.Position = new Vector2(400 - play.Width / 2, play.Position.Y);
        }
        public override void Update()
        {
            if (Hidden || !Focused) return;
            confirm.Update();
            if (Focused)
            {
                if (state == State.notappeared)
                {
                    black.Alpha += 0.1f;
                    if (black.Alpha >= 0.5f) black.Alpha = 0.5f;
                    songname.Alpha = black.Alpha * 2;
                    difficulty.Alpha = black.Alpha * 2;
                    play.Alpha = black.Alpha * 2;
                }
                else if (state == State.vanishing)
                {
                    black.Alpha -= 0.1f;
                    if (black.Alpha < 0) black.Alpha = 0;
                }
                else if (state == State.next)
                {
                    black.Alpha += 0.1f;
                    if (black.Alpha >= 1) black.Alpha = 1;
                }
            }
        }
        public override void Draw()
        {
            if (Hidden || !Focused) return;
            if (Focused)
            {
                black.Draw();
                confirm.Draw();
                songname.Draw();
                difficulty.Draw();
                play.Draw();

            }
        }
        public void Focus(object sender, EventArgs e)
        {
            this.Focused = true;
            state = State.notappeared;
            confirm.Stop();
            confirm.PlayType = Effect2D.EffectManager.PlayType.Once;
            confirm.Play();
        }
        public void Vanish()
        {
            state = State.vanishing;
            songname.Alpha = 0;
            difficulty.Alpha = 0;
            play.Alpha = 0;
            confirm.Stop();
            confirm.PlayType = Effect2D.EffectManager.PlayType.ReverseOnce;
            confirm.Play();
        }
        public void Show()
        {
            black.Alpha = 0.5f;
            state = State.appeared;
            songname.Alpha = 1;
            difficulty.Alpha = 1;
            play.Alpha = 1;
        }
        public bool Appeared
        {
            get
            {
                return (state == State.appeared ? true : false);
            }
        }
        public void Next()
        {
            state = State.next;
            songname.Alpha = 0;
            difficulty.Alpha = 0;
            play.Alpha = 0;
            confirm.Stop();
            confirm.PlayType = Effect2D.EffectManager.PlayType.ReverseOnce;
            confirm.Play();
        }
        protected override void DisposeResource()
        {
            foreach (ImageResource p in resource.Values)
            {
                p.Dispose();
            }
            songname.Dispose();
            difficulty.Dispose();
            play.Dispose();
        }
        public override float Alpha
        {
            get;
            set;
        }
        public override bool Hidden
        {
            get;
            set;
        }
        public override Vector2 Position
        {
            get;
            set;
        }
        public bool Focused
        {
            get;
            set;
        }
    }
}
