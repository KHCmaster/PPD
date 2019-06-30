using System;
using System.Collections.Generic;
using System.Text;
using PPDFramework;

using SlimDX;
using SlimDX.Direct3D9;

namespace PPD
{
    class HoldManager : GameComponent
    {
        struct PressInfo
        {
            public long PressStartTime;
            public int Count;
        }
        public delegate void GainScoreEventHandler(int gain);
        public event GainScoreEventHandler ScoreGained;

        bool[] pressingButtons;
        PressInfo[] pressingInfos;


        PictureObject[] numbers;
        PictureObject[] bonuses;
        PictureObject[] marks;
        PictureObject holdback;
        PictureObject maxback;
        PictureObject holdplus;
        PictureObject maxplus;
        PictureObject holdbonus;
        NumberPictureObject holdnumber;
        NumberPictureObject maxnumber;

        EffectObject holdeffect;
        EffectObject maxeffect;

        bool holding = false;
        bool maxbonus = false;
        bool isPaused;
        int[] checkindex = new int[] { 9, 7, 4, 5, 6, 3, 0, 1, 2, 8 };

        Device device;

        public HoldManager(Device device, PPDFramework.Resource.ResourceManager resourceManager)
        {
            this.device = device;
            pressingButtons = new bool[10];
            pressingInfos = new PressInfo[10];

            numbers = new PictureObject[6];
            bonuses = new PictureObject[6];
            for (int i = 1; i <= 6; i++)
            {
                numbers[i - 1] = new PictureObject(String.Format("img\\default\\hold\\{0}.png", i), 195, 335, true, resourceManager, device);
                bonuses[i - 1] = new PictureObject(String.Format("img\\default\\hold\\{0}bonus.png", i), 210, 325, resourceManager, device);
                bonuses[i - 1].Position = new Vector2(210, (int)(337 - bonuses[i - 1].Height / 2));
                this.AddChild(numbers[i - 1]);
                this.AddChild(bonuses[i - 1]);
            }

            holdback = new PictureObject("img\\default\\hold\\holdback.png", 400, 335, true, resourceManager, device);
            holdeffect = new EffectObject("img\\default\\hold\\holdback.etd", 400, 335, resourceManager, device);
            holdeffect.PlayType = Effect2D.EffectManager.PlayType.Once;

            this.AddChild(holdback);
            this.AddChild(holdeffect);

            maxback = new PictureObject("img\\default\\hold\\maxback.png", 400, 360, true, resourceManager, device);
            maxeffect = new EffectObject("img\\default\\hold\\maxback.etd", 400, 360, resourceManager, device);
            maxeffect.PlayType = Effect2D.EffectManager.PlayType.Once;

            this.AddChild(maxback);
            this.AddChild(maxeffect);

            holdplus = new PictureObject("img\\default\\hold\\holdplus.png", 0, 327, resourceManager, device);
            this.AddChild(holdplus);

            maxplus = new PictureObject("img\\default\\hold\\maxplus.png", 0, 354, resourceManager, device);
            this.AddChild(maxplus);

            holdbonus = new PictureObject("img\\default\\hold\\holdbonus.png", 345, 359, true, resourceManager, device);
            this.AddChild(holdbonus);

            holdnumber = new NumberPictureObject("img\\default\\hold\\holdnumber.png", 600, 326, NumberPictureObject.Alignment.Right, -1, resourceManager, device);
            this.AddChild(holdnumber);

            maxnumber = new NumberPictureObject("img\\default\\hold\\maxnumber.png", 550, 352, NumberPictureObject.Alignment.Right, -1, resourceManager, device);
            this.AddChild(maxnumber);

            marks = new PictureObject[10];

            marks[0] = new PictureObject("img\\default\\hold\\sikakuc.png", 0, 0, true, resourceManager, device);

            marks[1] = new PictureObject("img\\default\\hold\\batuc.png", 0, 0, true, resourceManager, device);

            marks[2] = new PictureObject("img\\default\\hold\\maruc.png", 0, 0, true, resourceManager, device);

            marks[3] = new PictureObject("img\\default\\hold\\sankakuc.png", 0, 0, true, resourceManager, device);

            marks[4] = new PictureObject("img\\default\\hold\\hidaric.png", 0, 0, true, resourceManager, device);

            marks[5] = new PictureObject("img\\default\\hold\\sitac.png", 0, 0, true, resourceManager, device);

            marks[6] = new PictureObject("img\\default\\hold\\migic.png", 0, 0, true, resourceManager, device);

            marks[7] = new PictureObject("img\\default\\hold\\uec.png", 0, 0, true, resourceManager, device);

            marks[8] = new PictureObject("img\\default\\hold\\Rc.png", 0, 0, true, resourceManager, device);

            marks[9] = new PictureObject("img\\default\\hold\\Lc.png", 0, 0, true, resourceManager, device);

            for (int i = 0; i < marks.Length; i++)
            {
                marks[i].Position = new Vector2(385 + 17 * i, 336);
                this.AddChild(marks[i]);
            }

            Hide();
        }

        public bool SetPressing(ButtonType buttontype, bool isPressing)
        {
            bool ret = false;
            holding = false;
            bool pressed = false;
            if (!isPressing)
            {
                if (pressingButtons[(int)buttontype])
                {
                    holding = false;
                    maxbonus = false;
                    Clear();
                    Hidden = true;
                }
            }
            else
            {
                if (maxbonus && !pressingButtons[(int)buttontype])
                {
                    ret = true;
                    maxbonus = false;
                    Clear();
                }
                if (PressingCount == 0)
                {
                    holdnumber.Value = 0;
                }
                Hidden = false;
                holding = true;
                if (!pressingButtons[(int)buttontype])
                {
                    maxbonus = false;
                    pressed = true;
                    pressingInfos[(int)buttontype].Count = 0;
                    pressingInfos[(int)buttontype].PressStartTime = Win32API.timeGetTime();
                    for (int i = 0; i < pressingInfos.Length; i++)
                    {
                        if (pressingButtons[i])
                        {
                            pressingInfos[i].Count = 0;
                            pressingInfos[i].PressStartTime = pressingInfos[(int)buttontype].PressStartTime;
                        }
                    }
                }
            }
            pressingButtons[(int)buttontype] = isPressing;
            if (pressed)
            {
                holdeffect.Stop();
                holdeffect.Play();
                UpdateMarksPos();
                Show();
            }
            return ret;
        }

        private void Show()
        {
            foreach (PictureObject p in numbers)
            {
                Show(p);
            }
            foreach (PictureObject p in bonuses)
            {
                Show(p);
            }

            Show(holdback);
            Show(maxback);

            Show(holdplus);
            Show(maxplus);
            Show(holdbonus);

            Show(holdnumber);
            Show(maxnumber);
        }

        private void Hide()
        {
            foreach (PictureObject p in numbers)
            {
                DecreaseAlpha(p, 1);
            }
            foreach (PictureObject p in bonuses)
            {
                DecreaseAlpha(p, 1);
            }
            foreach (PictureObject p in marks)
            {
                DecreaseAlpha(p, 1);
            }

            DecreaseAlpha(holdback, 1);
            DecreaseAlpha(maxback, 1);

            DecreaseAlpha(holdplus, 1);
            DecreaseAlpha(maxplus, 1);
            DecreaseAlpha(holdbonus, 1);

            DecreaseAlpha(holdnumber, 1);
            DecreaseAlpha(maxnumber, 1);
        }

        private void UpdateMarksPos()
        {
            int count = 0;
            for (int i = 0; i < checkindex.Length; i++)
            {
                int index = checkindex[i];
                if (pressingButtons[index] && count < 6)
                {
                    marks[index].Position = new Vector2(385 + 17 * count, 336);
                    Show(marks[index]);
                    count++;
                }
                else
                {
                    marks[index].Hidden = true;
                }
            }
        }

        public override void Update()
        {
            if (holding && !IsPaused)
            {
                long currentTime = Win32API.timeGetTime();
                for (int i = 0; i < pressingInfos.Length; i++)
                {
                    if (pressingButtons[i])
                    {
                        int num = (int)((currentTime - pressingInfos[i].PressStartTime) * 60 / 1000);
                        if (num > 300)
                        {
                            num = 300;
                            if (!maxbonus)
                            {
                                maxbonus = true;
                                maxeffect.Stop();
                                maxeffect.Play();
                                int gain = PressingCount * 1500;
                                maxnumber.Value = (uint)(gain);
                                GainScore(gain);
                            }
                        }
                        if (num > pressingInfos[i].Count)
                        {
                            int gain = (int)(10 * (num - pressingInfos[i].Count));
                            GainScore(gain);
                            holdnumber.Value += (uint)gain;
                            pressingInfos[i].Count = num;
                        }
                    }
                }
            }
            holdplus.Position = new Vector2(holdnumber.Position.X - holdnumber.DigitWidth * holdnumber.Value.ToString().Length - holdplus.Width, holdplus.Position.Y);
            maxplus.Position = new Vector2(maxnumber.Position.X - maxnumber.DigitWidth * maxnumber.Value.ToString().Length - maxplus.Width, maxplus.Position.Y);
            if (maxbonus)
            {
                maxback.Hidden = !maxback.Hidden;
            }
            if (Hidden)
            {
                foreach (PictureObject p in numbers)
                {
                    DecreaseAlpha(p);
                }
                foreach (PictureObject p in bonuses)
                {
                    DecreaseAlpha(p);
                }
                foreach (PictureObject p in marks)
                {
                    DecreaseAlpha(p);
                }

                DecreaseAlpha(holdback);
                DecreaseAlpha(maxback);

                DecreaseAlpha(holdplus);
                DecreaseAlpha(maxplus);
                DecreaseAlpha(holdbonus);

                DecreaseAlpha(holdnumber);
                DecreaseAlpha(maxnumber);
            }
            base.Update();
        }

        private void DecreaseAlpha(IDrawable id)
        {
            DecreaseAlpha(id, 0.1f);
        }

        private void DecreaseAlpha(IDrawable id, float alpha)
        {
            id.Alpha -= alpha;
            if (id.Alpha <= 0) id.Alpha = 0;
        }

        private void Show(IDrawable id)
        {
            id.Alpha = 1;
            id.Hidden = false;
        }

        public override void Draw()
        {
            holdback.Draw();
            holdeffect.Draw();
            int pressCount = PressingCount;
            if (pressCount > 0)
            {
                bonuses[pressCount - 1].Draw();
                numbers[pressCount - 1].Draw();
            }
            holdnumber.Draw();
            holdplus.Draw();
            if (maxbonus)
            {
                maxback.Draw();
                maxeffect.Draw();
                holdbonus.Draw();
                maxnumber.Draw();
                maxplus.Draw();
            }
            for (int i = 0; i < marks.Length; i++)
            {
                marks[i].Draw();
            }
        }

        private int PressingCount
        {
            get
            {
                int ret = 0;
                Array.ForEach(pressingButtons, (b => { ret += b ? 1 : 0; }));
                return ret >= 7 ? 6 : ret;
            }
        }

        public bool IsPaused
        {
            get
            {
                return isPaused;
            }
            set
            {
                isPaused = value;
                if (!isPaused)
                {
                    long currentTime = Win32API.timeGetTime();
                    for (int i = 0; i < pressingInfos.Length; i++)
                    {
                        pressingInfos[i].PressStartTime = currentTime - pressingInfos[i].Count * 1000 / 60;
                    }
                }
            }
        }

        private void GainScore(int gain)
        {
            if (ScoreGained != null)
            {
                ScoreGained.Invoke(gain);
            }
        }

        public void Retry()
        {
            Clear();
            Hide();
        }

        private void Clear()
        {
            Array.Clear(pressingButtons, 0, pressingButtons.Length);
            Array.Clear(pressingInfos, 0, pressingInfos.Length);
        }

    }
}
