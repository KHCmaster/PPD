using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SlimDX;
using SlimDX.Direct3D9;
using PPDFramework;

namespace testgame
{
    class Moviethumb : GameComponent
    {
        const string noimage = "img\\default\\noimage.png";
        const string folder = "img\\default\\bigfolder.png";
        const int space = 20;
        const int startx = 400;
        const int starty = 265;
        const int width = 480;
        //const int starty = 100;
        Dictionary<string, ImageResource> resource;
        EffectObject flare;
        EffectObject gloss;
        PictureObject[] pics = new PictureObject[0];
        SongInformation[] infos = new SongInformation[0];
        Reflection refrect;
        Device device;
        Sprite sprite;
        float Minumumscale = 0.3f;
        float Maximumscale = 0.6f;
        int selectnum = 0;
        public Moviethumb(Device device, Sprite sprite)
        {
            resource = new Dictionary<string, ImageResource>();
            this.device = device;
            this.sprite = sprite;
            flare = new EffectObject("img\\default\\lightmodeflare.etd", 0, 0, resource, device);
            flare.PlayType = Effect2D.EffectManager.PlayType.ReverseLoop;
            flare.Play();
            gloss = new EffectObject("img\\default\\lightmodegloss.etd", 0, 0, resource, device);
            gloss.PlayType = Effect2D.EffectManager.PlayType.Loop;
            gloss.Play();
            ImageResource pic = new ImageResource(noimage, device);
            resource.Add(noimage, pic);
            pic = new ImageResource(folder, device);
            resource.Add(folder, pic);
            refrect = new Reflection(device);
        }
        public override void Update()
        {
            gloss.Update();
            flare.Update();
            for (int j = 0; j < pics.Length; j++)
            {
                if (j == selectnum)
                {
                    Vector2 sc = pics[j].Scale;
                    if (sc.X < Maximumscale)
                    {
                        sc.X += 0.01f;
                        sc.Y += 0.01f;
                        if (sc.X >= Maximumscale)
                        {
                            sc.X = Maximumscale;
                            sc.Y = Maximumscale;
                        }
                        pics[j].Scale = sc;
                    }
                }
                else
                {
                    Vector2 sc = pics[j].Scale;
                    if (sc.X > Minumumscale)
                    {
                        sc.X -= 0.01f;
                        sc.Y -= 0.01f;
                        if (sc.X <= Minumumscale)
                        {
                            sc.X = Minumumscale;
                            sc.Y = Minumumscale;
                        }
                        pics[j].Scale = sc;
                    }
                }
            }
            gloss.Scale = pics[selectnum].Scale;
            flare.Scale = pics[selectnum].Scale;
            Vector2 pos = pics[selectnum].Position;
            if (pos.X != startx)
            {
                pics[selectnum].Position = new Vector2(pos.X + (startx - pos.X) * 0.2f, pos.Y);
                if (Math.Abs(pics[selectnum].Position.X - startx) < 0.001f)
                {
                    pics[selectnum].Position = new Vector2(startx, pos.Y);
                }
                gloss.Position = new Vector2(pics[selectnum].Position.X, starty);
                flare.Position = new Vector2(pics[selectnum].Position.X, starty);
                for (int i = 0; i < pics.Length; i++)
                {
                    if (i != selectnum)
                    {
                        pics[i].Position = new Vector2(pics[selectnum].Position.X - (space + Minumumscale * width) * (selectnum - i)
                            - pics[selectnum].Scale.X * width * (selectnum - i > 0 ? 1 : -1) / 2 + (selectnum - i > 0 ? 1 : -1) * (space + Minumumscale * width), pics[i].Position.Y);
                    }
                }
            }
        }
        public override void Draw()
        {
            if (infos[selectnum].IsPPDSong)
            {
                flare.Draw();
            }
            PictureObject[] temps = new PictureObject[pics.Length];
            float[] xs = new float[pics.Length];
            for (int i = 0; i < pics.Length; i++)
            {
                temps[i] = pics[i];
                xs[i] = pics[i].Scale.X;
            }
            Array.Sort(xs, temps);
            for (int i = 0; i < pics.Length; i++)
            {
                temps[i].Draw();
                refrect.Draw(temps[i]);
            }
            if (infos[selectnum].IsPPDSong)
            {
                gloss.Draw();
            }
        }
        public void ChangeDirectory(SongInformation[] infos, int selectnum)
        {
            pics = new PictureObject[infos.Length];
            this.infos = infos;
            this.selectnum = selectnum;
            for (int i = 0; i < infos.Length; i++)
            {
                if (!infos[i].IsPPDSong)
                {
                    PictureObject po = new PictureObject(folder, startx + (i - selectnum) * (width * Minumumscale + space), starty, true, resource, device);
                    po.Scale = new Vector2(Minumumscale, Minumumscale);
                    pics[i] = po;
                }
                else
                {
                    string thumbname = Path.Combine(infos[i].DirectoryPath, "thumb.png");
                    if (File.Exists(thumbname))
                    {
                        if (!resource.ContainsKey(thumbname))
                        {
                            ImageResource p = new ImageResource(thumbname, device);
                            resource.Add(thumbname, p);
                        }
                        PictureObject po = new PictureObject(thumbname, startx + (i - selectnum) * (width * Minumumscale + space), starty, true, resource, device);
                        pics[i] = po;
                        po.Scale = new Vector2(Minumumscale, Minumumscale);
                    }
                    else
                    {
                        PictureObject po = new PictureObject(noimage, startx + (i - selectnum) * (width * Minumumscale + space), starty, true, resource, device);
                        pics[i] = po;
                        po.Scale = new Vector2(Minumumscale, Minumumscale);
                    }
                }
            }
            gloss.Scale = new Vector2(Minumumscale, Minumumscale);
            flare.Scale = new Vector2(Minumumscale, Minumumscale);
            gloss.Position = new Vector2(startx, starty);
            flare.Position = new Vector2(startx, starty);
        }
        public void ChangeSelectIndex(int dn)
        {
            selectnum += dn;
            if (selectnum < 0) selectnum = pics.Length - 1;
            if (selectnum >= pics.Length) selectnum = 0;
            gloss.Scale = new Vector2(Minumumscale, Minumumscale);
            flare.Scale = new Vector2(Minumumscale, Minumumscale);
            gloss.Position = new Vector2(startx + space * selectnum, starty);
            flare.Position = new Vector2(startx + space * selectnum, starty);
        }
        protected override void DisposeResource()
        {
            foreach (ImageResource p in resource.Values)
            {
                p.Dispose();
            }
        }
        public override float Alpha
        {
            get;
            set;
        }
        public override Vector2 Position
        {
            get;
            set;
        }
        public override bool Hidden
        {
            get;
            set;
        }
    }
}
