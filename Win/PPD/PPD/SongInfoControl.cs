using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D9;
using PPDFramework;

namespace testgame
{
    class SongInfoControl : GameComponent
    {
        enum State
        {
            normal = 0,
            focused = 1,
            fadeout = 2,
            fadein = 3
        }
        Difficulty selectdifficulty;
        SongInformation songinfo;
        State state = State.normal;
        StringObject bpm;
        StringObject easy;
        StringObject normal;
        StringObject hard;
        StringObject extreme;
        StringObject SA;
        StringObject focusedbpm;
        StringObject hiscore;
        StringObject difficulty;
        StringObject[] infos;
        StringObject[] difficulties;
        PictureObject[] infoboards;
        EffectObject anim;
        Dictionary<string, ImageResource> resource;
        List<PictureObject> pictureobjects;
        string[] score = new string[4];
        bool[] exist = new bool[6];
        const int xx = 610;
        const int xxx = 640;
        const int height = 220;
        const int secondheight = 250;
        public SongInfoControl(Device device, Sprite sprite)
        {
            state = State.normal;
            resource = new Dictionary<string, ImageResource>();
            pictureobjects = new List<PictureObject>();
            anim = new EffectObject("img\\default\\difficulty.etd", 0, 0, resource, device);
            anim.Alignment = EffectObject.EffectAlignment.TopLeft;
            anim.PlayType = Effect2D.EffectManager.PlayType.Loop;
            anim.Play();
            string filename = "img\\default\\difficulty.png";
            ImageResource p = new ImageResource(filename, device);
            resource.Add(filename, p);
            PictureObject po = new PictureObject(filename, 450, secondheight + 120, resource, device);
            pictureobjects.Add(po);
            po = new PictureObject(filename, 535, secondheight + 120, resource, device);
            pictureobjects.Add(po);
            po = new PictureObject(filename, 620, secondheight + 120, resource, device);
            pictureobjects.Add(po);
            po = new PictureObject(filename, 705, secondheight + 120, resource, device);
            pictureobjects.Add(po);
            infoboards = new PictureObject[2];
            p = new ImageResource("img\\default\\infoboard1.png", device);
            resource.Add("img\\default\\infoboard1.png", p);
            po = new PictureObject("img\\default\\infoboard1.png", 520, height - 14, resource, device);
            infoboards[0] = po;
            p = new ImageResource("img\\default\\infoboard2.png", device);
            resource.Add("img\\default\\infoboard2.png", p);
            po = new PictureObject("img\\default\\infoboard2.png", 500, 235, resource, device);
            infoboards[1] = po;
            infos = new StringObject[6];
            for (int i = 0; i < 6; i++)
            {
                StringObject st = new StringObject("", xx + 20, height + 30 * i, 20, new Color4(1, 1, 1, 1), device, sprite);
                infos[i] = st;
            }
            difficulties = new StringObject[4];
            for (int i = 0; i < 4; i++)
            {
                string fn = "";
                switch (i)
                {
                    case 0:
                        fn = "EASY";
                        break;
                    case 1:
                        fn = "NORMAL";
                        break;
                    case 2:
                        fn = "HARD";
                        break;
                    case 3:
                        fn = "EXTREME";
                        break;
                }
                StringObject st = new StringObject(fn, 0, secondheight + 125, 20, new Color4(1, 1, 1, 1), device, sprite);
                st.Position = new Vector2(490 + 85 * i - st.Width / 2, st.Position.Y);
                difficulties[i] = st;
            }
            bpm = new StringObject("BPM", 600, height, 20, new Color4(1, 1, 1, 1), device, sprite);
            easy = new StringObject("EASY", 600, height + 30, 20, new Color4(1, 1, 1, 1), device, sprite);
            normal = new StringObject("NORMAL", 600, height + 60, 20, new Color4(1, 1, 1, 1), device, sprite);
            hard = new StringObject("HARD", 600, height + 90, 20, new Color4(1, 1, 1, 1), device, sprite);
            extreme = new StringObject("EXTREME", 600, height + 120, 20, new Color4(1, 1, 1, 1), device, sprite);
            SA = new StringObject("S.A.", 600, height + 150, 20, new Color4(1, 1, 1, 1), device, sprite);
            bpm.Position = new Vector2(xx - bpm.Width, bpm.Position.Y);
            easy.Position = new Vector2(xx - easy.Width, easy.Position.Y);
            normal.Position = new Vector2(xx - normal.Width, normal.Position.Y);
            hard.Position = new Vector2(xx - hard.Width, hard.Position.Y);
            extreme.Position = new Vector2(xx - extreme.Width, extreme.Position.Y);
            SA.Position = new Vector2(xx - SA.Width, SA.Position.Y);
            focusedbpm = new StringObject("BPM", 600, secondheight, 20, new Color4(1, 1, 1, 1), device, sprite);
            hiscore = new StringObject("BPM", 600, secondheight + 40, 20, new Color4(1, 1, 1, 1), device, sprite);
            difficulty = new StringObject("BPM", 600, secondheight + 80, 20, new Color4(1, 1, 1, 1), device, sprite);
        }
        public void ChangeSongInfo(SongInformation songinfo)
        {
            this.songinfo = songinfo;
            if (!songinfo.IsPPDSong) return;
            exist = new bool[6];
            exist[0] = true;
            exist[1] = (songinfo.Difficulty & SongInformation.AvailableDifficulty.Easy) == SongInformation.AvailableDifficulty.Easy;
            exist[2] = (songinfo.Difficulty & SongInformation.AvailableDifficulty.Normal) == SongInformation.AvailableDifficulty.Normal;
            exist[3] = (songinfo.Difficulty & SongInformation.AvailableDifficulty.Hard) == SongInformation.AvailableDifficulty.Hard;
            exist[4] = (songinfo.Difficulty & SongInformation.AvailableDifficulty.Extreme) == SongInformation.AvailableDifficulty.Extreme;
            exist[5] = songinfo.IsPPDSong;
            infos[0].Text = songinfo.BPM.ToString();
            infos[5].Text = songinfo.ScoreAuthor;
            for (int i = 1; i <= 4; i++)
            {
                infos[i].Text = songinfo.GetDifficultyString((Difficulty)(i - 1));
                if (exist[i]) difficulties[i - 1].Color = new Color4(1, 1, 1, 1);
                else difficulties[i - 1].Color = new Color4(1, 0.3f, 0.3f, 0.4f);
                score[i - 1] = songinfo.GetScore((Difficulty)(i - 1));
            }
        }
        public void HideInfo()
        {
            exist = new bool[6];
        }
        public bool CanGoNext()
        {
            return selectdifficulty != Difficulty.Other;
        }
        public override void Update()
        {
            if (Hidden) return;
            if (state == State.focused)
            {
                anim.Update();
            }
            if (state == State.fadeout)
            {
                foreach (PictureObject po in pictureobjects)
                {
                    po.Alpha -= 0.1f;
                    if (po.Alpha <= 0)
                    {
                        po.Alpha = 0;
                    }
                }
                focusedbpm.Alpha -= 0.1f;
                if (focusedbpm.Alpha <= 0)
                {
                    focusedbpm.Alpha = 0;
                }
                difficulty.Alpha -= 0.1f;
                if (difficulty.Alpha <= 0)
                {
                    difficulty.Alpha = 0;
                }
                hiscore.Alpha -= 0.1f;
                if (hiscore.Alpha <= 0)
                {
                    hiscore.Alpha = 0;
                }
                foreach (StringObject st in difficulties)
                {
                    st.Alpha -= 0.1f;
                    if (st.Alpha <= 0)
                    {
                        st.Alpha = 0;
                    }
                }
            }
        }
        public override void Draw()
        {
            if (Hidden) return;
            if (state == State.normal)
            {
                infoboards[0].Draw();
                if (exist[0])
                {
                    bpm.Draw();
                }
                if (exist[1])
                {
                    easy.Draw();
                }
                if (exist[2])
                {
                    normal.Draw();
                }
                if (exist[3])
                {
                    hard.Draw();
                }
                if (exist[4])
                {
                    extreme.Draw();
                }
                if (songinfo != null && songinfo.IsPPDSong)
                {
                    SA.Draw();
                }
                for (int i = 0; i < infos.Length; i++)
                {
                    if (exist[i])
                    {
                        infos[i].Draw();
                    }
                }
            }
            else if (state == State.focused)
            {
                infoboards[1].Draw();
                focusedbpm.Draw();
                difficulty.Draw();
                hiscore.Draw();
                for (int i = 0; i < pictureobjects.Count; i++)
                {
                    if (selectdifficulty != Difficulty.Other)
                    {
                        pictureobjects[i].Draw();
                    }
                }
                if (selectdifficulty != Difficulty.Other)
                {
                    anim.Draw();
                }
                for (int i = 0; i < difficulties.Length; i++)
                {
                    difficulties[i].Draw();
                }
            }
            else if (state == State.fadeout)
            {
                focusedbpm.Draw();
                difficulty.Draw();
                hiscore.Draw();
                for (int i = 0; i < pictureobjects.Count; i++)
                {
                    pictureobjects[i].Draw();
                }
                for (int i = 0; i < difficulties.Length; i++)
                {
                    difficulties[i].Draw();
                }
            }
        }
        public void Focus()
        {
            state = State.focused;
            Focused = true;
            selectdifficulty = Difficulty.Normal;
            int count = 0;
            while (!exist[(int)selectdifficulty + 1] && count < 3)
            {
                selectdifficulty += 1;
                if (selectdifficulty > Difficulty.Extreme) selectdifficulty = Difficulty.Easy;
                if (selectdifficulty < 0) selectdifficulty = Difficulty.Extreme;
                count++;
            }
            if (count >= 4)
            {
                selectdifficulty = Difficulty.Other;
                return;
            }
            PictureObject po = pictureobjects[(int)selectdifficulty];
            anim.Position = new Vector2(po.Position.X - 4, po.Position.Y - 4);
            focusedbpm.Text = "BPM " + infos[0].Text;
            focusedbpm.Position = new Vector2(xxx - focusedbpm.Width / 2, focusedbpm.Position.Y);
            hiscore.Text = "ハイスコア " + score[(int)selectdifficulty];
            hiscore.Position = new Vector2(xxx - hiscore.Width / 2, hiscore.Position.Y);
            switch (selectdifficulty)
            {
                case Difficulty.Easy:
                    difficulty.Text = infos[1].Text;
                    break;
                case Difficulty.Normal:
                    difficulty.Text = infos[2].Text;
                    break;
                case Difficulty.Hard:
                    difficulty.Text = infos[3].Text;
                    break;
                case Difficulty.Extreme:
                    difficulty.Text = infos[4].Text;
                    break;
            }
            difficulty.Position = new Vector2(xxx - difficulty.Width / 2, difficulty.Position.Y);
            foreach (PictureObject p in pictureobjects)
            {
                p.Alpha = 1f;
            }
            focusedbpm.Alpha = 1f;
            difficulty.Alpha = 1f;
            hiscore.Alpha = 1f;
            foreach (StringObject st in difficulties)
            {
                st.Alpha = 1f;
            }
        }
        public void UnFocus()
        {
            state = State.normal;
            Focused = false;
        }
        public void ChangeDifficulty(int diff)
        {
            selectdifficulty += diff;
            if (selectdifficulty > Difficulty.Extreme) selectdifficulty = Difficulty.Easy;
            if (selectdifficulty < Difficulty.Easy) selectdifficulty = Difficulty.Extreme;
            int count = 0;
            while (!exist[(int)selectdifficulty + 1] && count < 3)
            {
                selectdifficulty += diff;
                if (selectdifficulty > Difficulty.Extreme) selectdifficulty = Difficulty.Easy;
                if (selectdifficulty < Difficulty.Easy) selectdifficulty = Difficulty.Extreme;
                count++;
            }
            if (count >= 3)
            {
                selectdifficulty = Difficulty.Other;
                return;
            }
            PictureObject po = pictureobjects[(int)selectdifficulty];
            anim.Position = new Vector2(po.Position.X - 4, po.Position.Y - 4);
            switch (selectdifficulty)
            {
                case Difficulty.Easy:
                    difficulty.Text = infos[1].Text;
                    break;
                case Difficulty.Normal:
                    difficulty.Text = infos[2].Text;
                    break;
                case Difficulty.Hard:
                    difficulty.Text = infos[3].Text;
                    break;
                case Difficulty.Extreme:
                    difficulty.Text = infos[4].Text;
                    break;
            }
            hiscore.Text = "ハイスコア " + score[(int)selectdifficulty];
            hiscore.Position = new Vector2(xxx - hiscore.Width / 2, hiscore.Position.Y);
            difficulty.Position = new Vector2(xxx - difficulty.Width / 2, difficulty.Position.Y);
        }
        public string Difficult
        {
            get
            {
                switch (selectdifficulty)
                {
                    case Difficulty.Easy:
                        return "EASY";
                    case Difficulty.Normal:
                        return "NORMAL";
                    case Difficulty.Hard:
                        return "HARD";
                    case Difficulty.Extreme:
                        return "EXTREME";
                    default:
                        return "";
                }
            }
        }
        public Difficulty Difficulty
        {
            get
            {
                return selectdifficulty;
            }
        }
        public string DiifficultyString
        {
            get
            {
                return songinfo.GetDifficultyString(selectdifficulty);
            }
        }
        public float BPM
        {
            get
            {
                float ret = 0;
                if (!float.TryParse(infos[0].Text, out ret))
                {
                    MessageBox.Show("BPMの値が不正です。100で開始します\nBPM's value format is incorrect.100 is set at BPM instead.");
                    ret = 100;
                }
                return ret;
            }
        }
        public void FadeOut()
        {
            state = State.fadeout;
        }
        protected override void DisposeResource()
        {
            foreach (ImageResource p in resource.Values)
            {
                p.Dispose();
            }
            bpm.Dispose();
            easy.Dispose();
            normal.Dispose();
            hard.Dispose();
            focusedbpm.Dispose();
            hiscore.Dispose();
            difficulty.Dispose();
            foreach (StringObject st in infos)
            {
                st.Dispose();
            }
            foreach (StringObject st in difficulties)
            {
                st.Dispose();
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
        public bool Focused
        {
            get;
            set;
        }
    }
}
