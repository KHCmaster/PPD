using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SlimDX;
using SlimDX.Direct3D9;
using PPDFramework;

namespace testgame
{
    class GameResult : CGameResult
    {
        public override event EventHandler Returned;
        public override event EventHandler Retryed;
        enum State
        {
            waiting = -1,
            coolcounting = 0,
            goodcounting = 1,
            safecounting = 2,
            sadcounting = 3,
            worstcounting = 4,
            combocounting = 5,
            scorecounting = 6,
            done = 7,
            waitfadeforretry = 8,
            waitfadeforreturn = 9
        }
        State state = State.waiting;
        bool updatescore;
        bool highscore;
        EffectObject[] result;
        EffectObject high;
        EffectObject select;
        NumberPictureObject[] scoresmalls;
        NumberPictureObject scorebig;
        PictureObject scoreboard;
        PictureObject top;
        PictureObject bottom;
        PictureObject difficulty;
        PictureObject back;
        PictureObject[] selects;
        PictureObject black;
        StringObject songname;
        StringObject difficultstring;
        StringObject retry;
        StringObject retur;
        Dictionary<string, ImageResource> resource;
        int scorex = 388;
        int[] scorey = new int[] { 150, 180, 210, 240, 270, 300, 373 };
        int currentselect = 0;
        int score = 0;
        string dir = "";
        Difficulty diff = 0;
        string[] soundfilenames = new string[] { "sounds\\cursor1.wav", "sounds\\cursor2.wav", "sounds\\cursor3.wav", "sounds\\cursor4.wav" };
        public override void Retry()
        {
            foreach (NumberPictureObject npo in scoresmalls) npo.Value = 0;
            scorebig.Value = 0;
            currentselect = 0;
            highscore = false;
            //mistake = false;
        }
        public GameResult()
        {
        }
        public override void Load()
        {
            this.dir = PPDGameUtility.SongInformation.DirectoryPath;
            this.diff = PPDGameUtility.Difficulty;
            resource = new Dictionary<string, ImageResource>();
            result = new EffectObject[6];
            this.updatescore = !PPDGameUtility.Auto && PPDGameUtility.Profile.Index == 0;
            for (int i = 0; i < result.Length; i++)
            {
                string filename = "img\\default\\result\\";
                int x = 220;
                int y = 74;
                switch ((ResultEvaluateType)i)
                {
                    case ResultEvaluateType.Mistake:
                        filename += "mistake";
                        break;
                    case ResultEvaluateType.Cheap:
                        filename += "cheap";
                        break;
                    case ResultEvaluateType.Standard:
                        filename += "standard";
                        break;
                    case ResultEvaluateType.Great:
                        filename += "great";
                        break;
                    case ResultEvaluateType.Excellent:
                        filename += "excellent";
                        break;
                    case ResultEvaluateType.Perfect:
                        filename += "perfect";
                        break;
                }
                EffectObject anim = new EffectObject(filename + ".etd", x, y, resource, Device);
                anim.PlayType = Effect2D.EffectManager.PlayType.ReverseLoop;
                anim.Play();
                result[i] = anim;
            }
            string fn = "img\\default\\result\\";
            switch (diff)
            {
                case Difficulty.Easy:
                    fn += "easy.png";
                    break;
                case Difficulty.Normal:
                    fn += "normal.png";
                    break;
                case Difficulty.Hard:
                    fn += "hard.png";
                    break;
                case Difficulty.Extreme:
                    fn += "hard.png";
                    break;
                default:
                    fn += "normal.png";
                    break;
            }
            ImageResource temp = new ImageResource(fn, Device);
            resource.Add(fn, temp);
            difficulty = new PictureObject(fn, 45, 105, resource, Device);
            select = new EffectObject("img\\default\\difficulty.etd", 444, 332, resource, Device);
            select.PlayType = Effect2D.EffectManager.PlayType.Loop;
            select.Play();
            select.Alignment = EffectObject.EffectAlignment.TopLeft;
            high = new EffectObject("img\\default\\result\\high.etd", -25, 300, resource, Device);
            high.PlayType = Effect2D.EffectManager.PlayType.Loop;
            high.Play();
            high.Alignment = EffectObject.EffectAlignment.TopLeft;
            ImageResource pic = new ImageResource("img\\default\\result\\scoresmall.png", Device);
            resource.Add("img\\default\\result\\scoresmall.png", pic);
            scoresmalls = new NumberPictureObject[6];
            for (int i = 0; i < scoresmalls.Length; i++)
            {
                scoresmalls[i] = new NumberPictureObject("img\\default\\result\\scoresmall.png", scorex, scorey[i], NumberPictureObject.Alignment.Right, -1, resource, Device);
            }
            pic = new ImageResource("img\\default\\result\\scorebig.png", Device);
            resource.Add("img\\default\\result\\scorebig.png", pic);
            scorebig = new NumberPictureObject("img\\default\\result\\scorebig.png", scorex, scorey[6], NumberPictureObject.Alignment.Right, -1, resource, Device);
            pic = new ImageResource("img\\default\\result\\score.png", Device);
            resource.Add("img\\default\\result\\score.png", pic);
            scoreboard = new PictureObject("img\\default\\result\\score.png", 18, 52, resource, Device);
            pic = new ImageResource("img\\default\\result\\top.png", Device);
            resource.Add("img\\default\\result\\top.png", pic);
            top = new PictureObject("img\\default\\result\\top.png", 0, 0, resource, Device);
            pic = new ImageResource("img\\default\\result\\bottom.png", Device);
            resource.Add("img\\default\\result\\bottom.png", pic);
            bottom = new PictureObject("img\\default\\result\\bottom.png", 0, 405, false, resource, Device);
            pic = new ImageResource("img\\default\\back.png", Device);
            resource.Add("img\\default\\back.png", pic);
            back = new PictureObject("img\\default\\back.png", 0, 0, resource, Device);
            pic = new ImageResource("img\\default\\black.png", Device);
            resource.Add("img\\default\\black.png", pic);
            black = new PictureObject("img\\default\\black.png", 0, 0, resource, Device);
            black.Alpha = 0;
            pic = new ImageResource("img\\default\\difficulty.png", Device);
            resource.Add("img\\default\\difficulty.png", pic);
            selects = new PictureObject[2];
            for (int i = 0; i < selects.Length; i++)
            {
                PictureObject temppo = new PictureObject("img\\default\\difficulty.png", 520 + i * 130, 372, resource, Device);
                selects[i] = temppo;
            }
            select.Position = new Vector2(selects[currentselect].Position.X - 4, selects[currentselect].Position.Y - 4);
            songname = new StringObject(PPDGameUtility.SongInformation.DirectoryName, 35, 8, 20, 500, new Color4(1, 1, 1, 1), Device, Sprite);
            songname.Position = new Vector2(790 - songname.JustWidth, songname.Position.Y);
            difficultstring = new StringObject(PPDGameUtility.DifficultString, 35, 105, 28, new Color4(1, 1, 1, 1), Device, Sprite);
            difficultstring.Position = new Vector2(400 - difficultstring.Width, difficultstring.Position.Y);
            retry = new StringObject("RETRY", 535, 377, 20, new Color4(1, 1, 1, 1), Device, Sprite);
            retur = new StringObject("RETURN", 660, 377, 20, new Color4(1, 1, 1, 1), Device, Sprite);
            ResultSet += new EventHandler(GameResult_ResultSet);
        }

        void GameResult_ResultSet(object sender, EventArgs e)
        {
            state = State.coolcounting;
        }
        public override void Update(bool[] b)
        {
            if (state == State.done)
            {
                if (b[(int)ButtonType.Left])
                {
                    currentselect--;
                    if (currentselect < 0)
                    {
                        currentselect = 1;
                    }
                    select.Position = new Vector2(selects[currentselect].Position.X - 4, selects[currentselect].Position.Y - 4);
                    Sound.Play(soundfilenames[0], -1000);
                }
                if (b[(int)ButtonType.Right])
                {
                    currentselect++;
                    if (currentselect > 1)
                    {
                        currentselect = 0;
                    }
                    select.Position = new Vector2(selects[currentselect].Position.X - 4, selects[currentselect].Position.Y - 4);
                    Sound.Play(soundfilenames[0], -1000);
                }
                if (b[(int)ButtonType.Circle])
                {
                    if (currentselect == 0)
                    {
                        state = State.waitfadeforretry;
                    }
                    else if (currentselect == 1)
                    {
                        state = State.waitfadeforreturn;
                    }
                }
            }
            if ((b[(int)ButtonType.Circle] || b[(int)ButtonType.Left] || b[(int)ButtonType.Right]) && state < State.done)
            {
                state = State.done;
                for (int i = 0; i < scoresmalls.Length; i++)
                {
                    if (i >= MarkEvals.Length) scoresmalls[i].Value = (uint)MaxCombo;
                    else scoresmalls[i].Value = (uint)MarkEvals[i];
                }
                scorebig.Value = (uint)score;
            }
            if (state >= State.waitfadeforretry)
            {
                black.Alpha += 0.04f;
                if (black.Alpha >= 1)
                {
                    black.Alpha = 1;
                    if (state == State.waitfadeforretry)
                    {
                        if (this.Retryed != null)
                        {
                            this.Retryed(this, new EventArgs());
                            state = State.waiting;
                            black.Alpha = 0;
                        }
                    }
                    else if (state == State.waitfadeforreturn)
                    {
                        if (this.Returned != null)
                        {
                            this.Returned(this, new EventArgs());
                            state = State.waiting;
                            black.Alpha = 0;
                        }
                    }
                }
            }
            if (state > State.waiting)
            {
                if (state < State.combocounting)
                {
                    int delta = (MarkEvals[(int)state] / 60 < 1 ? 1 : MarkEvals[(int)state] / 60);
                    scoresmalls[(int)state].Value += (uint)delta;
                    if (scoresmalls[(int)state].Value >= MarkEvals[(int)state])
                    {
                        scoresmalls[(int)state].Value = (uint)MarkEvals[(int)state];
                        state++;
                    }
                }
                if (state == State.combocounting)
                {
                    int delta = (MaxCombo / 60 < 1 ? 1 : MaxCombo / 60);
                    scoresmalls[(int)state].Value += (uint)delta;
                    if (scoresmalls[(int)state].Value >= MaxCombo)
                    {
                        scoresmalls[(int)state].Value = (uint)MaxCombo;
                        state = State.scorecounting;
                    }
                }
                if (state == State.scorecounting)
                {
                    int delta = (score / 60 > 99 ? score / 60 : 99);
                    scorebig.Value += (uint)delta;
                    if (scorebig.Value >= score)
                    {
                        scorebig.Value = (uint)score;
                        state = State.done;
                    }
                }
                for (int i = 0; i < result.Length; i++)
                {
                    result[i].Update();
                }
                select.Update();
                high.Update();
            }
        }
        public override void Draw()
        {
            back.Draw();
            scoreboard.Draw();
            difficulty.Draw();
            top.Draw();
            bottom.Draw();
            if (state >= State.done)
            {
                result[(int)Result].Draw();
                select.Draw();
                for (int i = 0; i < selects.Length; i++)
                {
                    if (currentselect == i) continue;
                    selects[i].Draw();
                }
            }
            if (state < State.waitfadeforretry)
            {
                songname.Draw();
                difficultstring.Draw();
            }
            if (state == State.done)
            {
                retry.Draw();
                retur.Draw();
                if (highscore) high.Draw();
            }
            for (int i = 0; i <= (int)state; i++)
            {
                if (i <= (int)State.combocounting)
                {
                    scoresmalls[i].Draw();
                }
                else if (i == (int)State.scorecounting)
                {
                    scorebig.Draw();
                    break;
                }
            }
            black.Draw();
        }
        protected override void DisposeResource()
        {
            foreach (ImageResource p in resource.Values)
            {
                p.Dispose();
            }
            songname.Dispose();
            difficultstring.Dispose();
            retry.Dispose();
            retur.Dispose();
        }
    }
}
