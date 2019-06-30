using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using PPDFramework;

namespace testgame
{
    class PauseDisplay : CPauseMenu
    {
        enum PauseType
        {
            Resume = 0,
            Retry = 1,
            Return = 2
        }
        public override event EventHandler Resumed;
        public override event EventHandler Retryed;
        public override event EventHandler Returned;
        Dictionary<string, ImageResource> resource;
        PictureObject conf;
        PictureObject top;
        PictureObject bottom;
        StringObject resume;
        StringObject retry;
        StringObject retur;
        EffectObject select;
        PictureObject[] selects;
        PauseType pausetype = PauseType.Resume;
        string[] soundfilenames = new string[] { "sounds\\cursor1.wav", "sounds\\cursor2.wav", "sounds\\cursor3.wav", "sounds\\cursor4.wav" };
        public PauseDisplay()
        {
        }
        public override void Load()
        {
            resource = new Dictionary<string, ImageResource>();
            select = new EffectObject("img\\default\\difficulty.etd", 380, 150, resource, Device);
            select.Alignment = EffectObject.EffectAlignment.TopLeft;
            select.PlayType = Effect2D.EffectManager.PlayType.Loop;
            select.Play();
            ImageResource pic = new ImageResource("img\\default\\difficulty.png", Device);
            resource.Add("img\\default\\difficulty.png", pic);
            selects = new PictureObject[3];
            for (int i = 0; i < 3; i++)
            {
                PictureObject po = new PictureObject("img\\default\\difficulty.png", 380, 150 + 60 * i, resource, Device);
                po.Position = new Vector2(400 - po.Width / 2, po.Position.Y);
                selects[i] = po;
                switch (i)
                {
                    case 0:
                        resume = new StringObject("RESUME", 370, 150 + 60 * i + 5, 20, new Color4(1, 1, 1, 1), Device, Sprite);
                        break;
                    case 1:
                        retry = new StringObject("RETRY", 375, 150 + 60 * i + 5, 20, new Color4(1, 1, 1, 1), Device, Sprite);
                        break;
                    case 2:
                        retur = new StringObject("RETURN", 370, 150 + 60 * i + 5, 20, new Color4(1, 1, 1, 1), Device, Sprite);
                        break;
                }
            }
            pic = new ImageResource("img\\default\\confirmpause.png", Device);
            resource.Add("img\\default\\confirmpause.png", pic);
            conf = new PictureObject("img\\default\\confirmpause.png", 266, 118, resource, Device);
            pic = new ImageResource("img\\default\\conftop.png", Device);
            resource.Add("img\\default\\conftop.png", pic);
            top = new PictureObject("img\\default\\conftop.png", 266, 225 - 107, resource, Device);
            pic = new ImageResource("img\\default\\confbottom.png", Device);
            resource.Add("img\\default\\confbottom.png", pic);
            bottom = new PictureObject("img\\default\\confbottom.png", 266, 225 + 107 - 17, resource, Device);
            select.Position = new Vector2(selects[(int)pausetype].Position.X - 4, selects[(int)pausetype].Position.Y - 4);
            Hidden = false;
        }
        public override void Update(bool[] pressed)
        {
            select.Update();
            select.Position = new Vector2(selects[(int)pausetype].Position.X - 4, selects[(int)pausetype].Position.Y - 4);

            if (pressed[(int)ButtonType.Up])
            {
                ChangePauseType(-1);
                Sound.Play(soundfilenames[0], -1000);
            }
            else if (pressed[(int)ButtonType.Down])
            {
                ChangePauseType(1);
                Sound.Play(soundfilenames[0], -1000);
            }
            else if (pressed[(int)ButtonType.Cross] || pressed[(int)ButtonType.Start])
            {
                if (Resumed != null) Resumed.Invoke(this, EventArgs.Empty);
            }
            else if (pressed[(int)ButtonType.Circle])
            {
                if (pausetype == PauseType.Resume)
                {
                    if (Resumed != null) Resumed.Invoke(this, EventArgs.Empty);
                }
                else if (pausetype == PauseType.Retry)
                {
                    if (Retryed != null) Retryed.Invoke(this, EventArgs.Empty);
                }
                else if (pausetype == PauseType.Return)
                {
                    if (Returned != null) Returned.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public override void Retry()
        {
            pausetype = 0;
            Hidden = true;
        }
        private void ChangePauseType(int dn)
        {
            pausetype += dn;
            if (pausetype < PauseType.Resume) pausetype = PauseType.Return;
            if (pausetype > PauseType.Return) pausetype = PauseType.Resume;
        }
        public override void Draw()
        {
            if (!Hidden)
            {
                conf.Draw();
                top.Draw();
                bottom.Draw();
                for (int i = 0; i < selects.Length; i++)
                {
                    if ((int)pausetype == i)
                    {
                        select.Draw();
                    }
                    else
                    {
                        selects[i].Draw();
                    }
                }
                resume.Draw();
                retry.Draw();
                retur.Draw();
            }
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
