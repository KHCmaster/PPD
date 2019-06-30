using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D9;
using PPDFramework;

namespace testgame
{
    class MenuSelectSong : GameComponent
    {
        enum State
        {
            NotAppeared = 0,
            Moving = 1,
            Appeared = 2,
            Still = 3
        }
        public event EventHandler DisapperFinish;

        const int defaultheight = 250;
        const int defaultwidth = 5;
        int height;
        int width;
        Dictionary<string, ImageResource> resource;
        ArrayList pictureobjects;
        PictureObject back;
        PictureObject folder;
        EffectObject anim;
        StringObject[] dxsts;
        SongInformation[] infos;
        private int maxnum;
        private int selectnum;
        private float currenty = 200;
        private float currentx = 0;
        State state;
        Menu m;
        Device device;
        Sprite sprite;
        public MenuSelectSong(Device device, Sprite sprite, Menu m, string directory)
        {
            this.device = device;
            this.sprite = sprite;
            this.m = m;
            state = State.NotAppeared;
            Focused = true;
            resource = new Dictionary<string, ImageResource>();
            pictureobjects = new ArrayList();
            anim = new EffectObject("img\\default\\selectback.etd", 0, 0, resource, device);
            anim.PlayType = Effect2D.EffectManager.PlayType.Loop;
            anim.Alignment = EffectObject.EffectAlignment.TopLeft;
            anim.Play();
            string filename = "img\\default\\selectback.png";
            ImageResource p = new ImageResource(filename, device);
            height = p.Height;
            width = p.Width;
            resource.Add(filename, p);
            back = new PictureObject(filename, 800, 0, resource, device);
            filename = "img\\default\\folder.png";
            p = new ImageResource(filename, device);
            resource.Add(filename, p);
            folder = new PictureObject(filename, 0, 0, resource, device);
        }
        public SongInformation[] SongInformations
        {
            set
            {
                infos = value;
                if (infos.Length == 0)
                {
                    MessageBox.Show("No song");
                    Application.Exit();
                }
                else
                {
                    dxsts = new StringObject[infos.Length];
                    for (int i = 0; i < infos.Length; i++)
                    {
                        string name = infos[i].DirectoryName;
                        dxsts[i] = new StringObject(name, 0, 0, 20, 390, new Color4(1, 1, 1, 1), device, sprite);
                        if (i == 0)
                        {
                            dxsts[i].AllowScroll = true;
                        }
                        else
                        {
                            dxsts[i].AllowScroll = false;
                        }
                    }
                    maxnum = infos.Length;
                }
            }
        }
        public override void Update()
        {
            if (Hidden) return;
            anim.Update();
            if (defaultheight != currenty)
            {
                currenty += (defaultheight - currenty) * 0.2f;
                if (Math.Abs(defaultheight - currenty) < 0.1f)
                {
                    currenty = defaultheight;
                }
            }
            if (state == State.Appeared)
            {
                currentx += (currentx - 0.001f) * 0.2f;
                if (currentx <= -(width + 100))
                {
                    currentx = -(width + 100);
                    if (this.DisapperFinish != null)
                    {
                        this.DisapperFinish(this, new EventArgs());
                        this.state = State.Still;
                    }
                }
            }
            else if (state == State.NotAppeared)
            {
                currentx += -currentx * 0.2f;
                if (currentx >= 0.001f)
                {
                    currentx = 0;
                    this.state = State.Still;
                }

            }
        }
        public override void Draw()
        {
            if (Hidden) return;
            for (int i = 0; i < dxsts.Length; i++)
            {
                int diff = Math.Abs(selectnum - i);
                if (diff <= 3)
                {
                    if (diff == 0)
                    {
                        anim.Position = new Vector2(currentx * (float)Math.Pow(4, diff), currenty - 5);
                        if (Focused)
                        {
                            anim.Draw();
                        }
                        else
                        {
                            back.Position = new Vector2(currentx * (float)Math.Pow(4, diff), currenty + (i - selectnum) * (height + 5));
                            back.Alpha = (float)1 / (diff + 1);
                            back.Draw();
                        }
                    }
                    else
                    {
                        back.Position = new Vector2(currentx * (float)Math.Pow(4, diff), currenty + (i - selectnum) * (height + 5));
                        back.Alpha = (float)1 / (diff + 1);
                        back.Draw();
                    }
                    dxsts[i].Position = new Vector2(40 + currentx * (float)Math.Pow(4, diff), currenty + (i - selectnum) * (height + 5) + 5);
                    dxsts[i].Alpha = (float)1 / (diff + 1);
                    dxsts[i].Draw();
                    if (!infos[i].IsPPDSong)
                    {
                        folder.Position = new Vector2(5 + currentx * (float)Math.Pow(4, diff), currenty + (i - selectnum) * (height + 5));
                        folder.Draw();
                    }
                }
            }
        }
        public int SelectedIndex
        {
            set
            {
                currenty += (selectnum - value) >= 1 ? -height : height;
                dxsts[selectnum].AllowScroll = false;
                selectnum = value;
                dxsts[selectnum].AllowScroll = true;
            }
        }
        public void Disappear()
        {
            state = State.Appeared;
        }
        public void Start()
        {
            state = State.NotAppeared;
        }
        protected override void DisposeResource()
        {
            foreach (ImageResource p in resource.Values)
            {
                p.Dispose();
            }
            foreach (StringObject st in dxsts)
            {
                st.Dispose();
            }
        }
        public override Vector2 Position
        {
            get;
            set;
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
        public bool Focused
        {
            get;
            set;
        }
    }
}
