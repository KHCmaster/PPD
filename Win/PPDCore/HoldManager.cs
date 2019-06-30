using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using SharpDX;
using System;
using System.Collections.Generic;

namespace PPDCore
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

        public event Action<HoldInfo> HoldStart;
        public event Action<HoldInfo> HoldChange;
        public event Action<HoldInfo> HoldEnd;
        public event Action<HoldInfo> HoldMaxHold;

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

        int notMaxBonusCount;
        int[] checkindex = { 9, 7, 4, 5, 6, 3, 0, 1, 2, 8 };

        bool isDrawn;
        int maxHoldFrames;
        bool isMaxHolding;

        public bool IsHolding
        {
            get;
            private set;
        }

        public bool IsMaxHolding
        {
            get { return isMaxHolding; }
            private set
            {
                if (isMaxHolding != value)
                {
                    if (isMaxHolding)
                    {
                        maxback.Alpha = 1;
                    }
                    isMaxHolding = value;
                }
            }
        }

        public HoldManager(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            pressingButtons = new bool[10];
            pressingInfos = new PressInfo[10];

            marks = new PictureObject[10];
            marks[0] = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "sikakuc.png"), true);
            marks[1] = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "batuc.png"), true);
            marks[2] = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "maruc.png"), true);
            marks[3] = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "sankakuc.png"), true);
            marks[4] = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "hidaric.png"), true);
            marks[5] = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "sitac.png"), true);
            marks[6] = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "migic.png"), true);
            marks[7] = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "uec.png"), true);
            marks[8] = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "Rc.png"), true);
            marks[9] = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "Lc.png"), true);
            for (int i = 0; i < marks.Length; i++)
            {
                marks[i].Position = new Vector2(385 + 17 * i, 336);
                this.AddChild(marks[i]);
            }
            this.AddChild(maxplus = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "maxplus.png"))
            {
                Position = new Vector2(0, 354)
            });
            this.AddChild(maxnumber = new NumberPictureObject(device, resourceManager, Utility.Path.Combine("hold", "maxnumber.png"))
            {
                Position = new Vector2(550, 352),
                Alignment = Alignment.Right,
                MaxDigit = -1
            });
            this.AddChild(holdbonus = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "holdbonus.png"), true)
            {
                Position = new Vector2(345, 359)
            });
            this.AddChild(maxeffect = new EffectObject(device, resourceManager, Utility.Path.Combine("hold\\maxback.etd"))
            {
                Position = new Vector2(400, 360)
            });
            maxeffect.PlayType = Effect2D.EffectManager.PlayType.Once;
            this.AddChild(maxback = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "maxback.png"), true)
            {
                Position = new Vector2(400, 360)
            });
            this.AddChild(holdplus = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "holdplus.png"))
            {
                Position = new Vector2(0, 327)
            });
            this.AddChild(holdnumber = new NumberPictureObject(device, resourceManager, Utility.Path.Combine("hold", "holdnumber.png"))
            {
                Position = new Vector2(600, 326),
                Alignment = Alignment.Right,
                MaxDigit = -1
            });
            numbers = new PictureObject[6];
            bonuses = new PictureObject[6];
            for (int i = 1; i <= 6; i++)
            {
                numbers[i - 1] = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", String.Format("{0}.png", i)), true)
                {
                    Position = new Vector2(195, 335)
                };
                bonuses[i - 1] = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", String.Format("{0}bonus.png", i)))
                {
                    Position = new Vector2(210, 325)
                };
                bonuses[i - 1].Position = new Vector2(210, (int)(337 - bonuses[i - 1].Height / 2));
                this.AddChild(numbers[i - 1]);
                this.AddChild(bonuses[i - 1]);
            }
            this.AddChild(holdeffect = new EffectObject(device, resourceManager, Utility.Path.Combine("hold\\holdback.etd"))
            {
                Position = new Vector2(400, 335)
            });
            holdeffect.PlayType = Effect2D.EffectManager.PlayType.Once;
            this.AddChild(holdback = new PictureObject(device, resourceManager, Utility.Path.Combine("hold", "holdback.png"), true)
            {
                Position = new Vector2(400, 335)
            });

            Hide();
        }

        public bool SetPressing(ButtonType buttonType, bool isPressing, long currentTime)
        {
            bool ret = false;
            IsHolding = false;
            bool pressed = false;
            if (!isPressing)
            {
                IsHolding = false;
                IsMaxHolding = false;
                Clear();
                isDrawn = true;
                OnHoldEnd(new HoldInfo(0, 0, GetPressingMarkTypes()));
            }
            else
            {
                if (IsMaxHolding && !pressingButtons[(int)buttonType])
                {
                    ret = true;
                    IsMaxHolding = false;
                    Clear();
                    OnHoldEnd(new HoldInfo(0, 0, GetPressingMarkTypes()));
                }
                if (PressingCount == 0)
                {
                    holdnumber.Value = 0;
                }
                isDrawn = false;
                IsHolding = true;
                if (!pressingButtons[(int)buttonType])
                {
                    IsMaxHolding = false;
                    pressed = true;
                    pressingInfos[(int)buttonType].Count = 0;
                    pressingInfos[(int)buttonType].PressStartTime = currentTime;
                    for (int i = 0; i < pressingInfos.Length; i++)
                    {
                        if (pressingButtons[i])
                        {
                            pressingInfos[i].Count = 0;
                            pressingInfos[i].PressStartTime = pressingInfos[(int)buttonType].PressStartTime;
                        }
                    }
                    pressingButtons[(int)buttonType] = isPressing;
                    OnHoldStart(new HoldInfo((int)holdnumber.Value, 0, GetPressingMarkTypes()));
                }
            }
            pressingButtons[(int)buttonType] = isPressing;
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
            Show(holdplus);
            Show(holdnumber);
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

        public void Update(long currentTime)
        {
            if (IsHolding)
            {
                for (int i = 0; i < pressingInfos.Length; i++)
                {
                    if (pressingButtons[i])
                    {
                        var num = (int)((currentTime - pressingInfos[i].PressStartTime) * 60 / 1000);
                        if (num >= 300)
                        {
                            num = 300;
                            if (!IsMaxHolding)
                            {
                                IsMaxHolding = true;
                                maxeffect.Stop();
                                maxeffect.Play();
                                int gain = PressingCount * 1500;
                                maxnumber.Value = (uint)(gain);
                                GainScore(gain);
                                OnHoldMaxHold(new HoldInfo((int)holdnumber.Value, gain, GetPressingMarkTypes()));
                            }
                        }
                        if (num > pressingInfos[i].Count)
                        {
                            var gain = (int)(10 * (num - pressingInfos[i].Count));
                            GainScore(gain);
                            holdnumber.Value += (uint)gain;
                            pressingInfos[i].Count = num;
                        }
                    }
                }
                OnHoldChange(new HoldInfo((int)holdnumber.Value, 0, GetPressingMarkTypes()));
            }
            holdplus.Position = new Vector2(holdnumber.Position.X - holdnumber.DigitWidth * holdnumber.Value.ToString().Length - holdplus.Width, holdplus.Position.Y);
            maxplus.Position = new Vector2(maxnumber.Position.X - maxnumber.DigitWidth * maxnumber.Value.ToString().Length - maxplus.Width, maxplus.Position.Y);
            if (IsMaxHolding)
            {
                notMaxBonusCount = 0;
                maxback.Alpha = (maxHoldFrames++ % 3) == 0 ? 1 : 0.5f;
                maxplus.Alpha = holdbonus.Alpha = maxnumber.Alpha = 1;
            }
            else
            {
                notMaxBonusCount++;
                if (notMaxBonusCount >= 60)
                {
                    DecreaseAlpha(maxback, 0.1f);
                    DecreaseAlpha(maxplus, 0.1f);
                    DecreaseAlpha(holdbonus, 0.1f);
                    DecreaseAlpha(maxnumber, 0.1f);
                }
            }
            if (isDrawn)
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

                DecreaseAlpha(holdplus);

                DecreaseAlpha(holdnumber);
            }
            for (var i = 0; i < bonuses.Length; i++)
            {
                bonuses[i].Hidden = numbers[i].Hidden = true;
            }
            var pressCount = PressingCount;
            if (pressCount > 0)
            {
                bonuses[pressCount - 1].Hidden = numbers[pressCount - 1].Hidden = false;

            }
            Update();
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

        private int PressingCount
        {
            get
            {
                int ret = 0;
                Array.ForEach(pressingButtons, (b => { ret += b ? 1 : 0; }));
                return ret >= 7 ? 6 : ret;
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
            IsHolding = IsMaxHolding = false;
            SetDefault();
        }

        private void Clear()
        {
            Array.Clear(pressingButtons, 0, pressingButtons.Length);
            Array.Clear(pressingInfos, 0, pressingInfos.Length);
        }

        private void OnHoldStart(HoldInfo holdInfo)
        {
            HoldStart?.Invoke(holdInfo);
        }

        private void OnHoldChange(HoldInfo holdInfo)
        {
            HoldChange?.Invoke(holdInfo);
        }

        private void OnHoldEnd(HoldInfo holdInfo)
        {
            HoldEnd?.Invoke(holdInfo);
        }

        private void OnHoldMaxHold(HoldInfo holdInfo)
        {
            if (holdInfo != null)
            {
                HoldMaxHold(holdInfo);
            }
        }

        private MarkType[] GetPressingMarkTypes()
        {
            var ret = new List<MarkType>();
            for (int i = 0; i < pressingButtons.Length; i++)
            {
                if (pressingButtons[i])
                {
                    ret.Add((MarkType)i);
                }
            }
            return ret.ToArray();
        }
    }
}
