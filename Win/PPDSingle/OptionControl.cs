using PPDFramework;
using PPDFramework.Web;
using PPDShareComponent;
using SharpDX;
using System;
using System.Linq;

namespace PPDSingle
{
    /// <summary>
    /// オプション用UI
    /// </summary>
    class OptionControl : FocusableGameComponent
    {
        public enum State
        {
            Auto,
            ExceptSlide,
            Random,
            MuteSE,
            Connect,
            PerfectTrial,
            RivalGhost,
            RivalGhostCount,
            Speed,
            Profile,
        }

        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;
        float[] speeds = { 0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 1.75f, 2.0f };
        PictureObject back;
        SelectableComponent[] selectables;
        State state = State.Auto;

        private float width;

        public AutoMode AutoMode
        {
            get
            {
                if (Auto)
                {
                    if (ExceptSlideAuto)
                    {
                        return AutoMode.ExceptSlide;
                    }
                    return AutoMode.All;
                }
                return AutoMode.None;
            }
            set
            {
                switch (value)
                {
                    case AutoMode.All:
                        Auto = true;
                        ExceptSlideAuto = false;
                        break;
                    case AutoMode.ExceptSlide:
                        Auto = true;
                        ExceptSlideAuto = true;
                        break;
                }
            }
        }

        private bool Auto
        {
            get;
            set;
        }

        private bool ExceptSlideAuto
        {
            get;
            set;
        }

        public bool Random
        {
            get;
            private set;
        }

        public bool MuteSE
        {
            get;
            private set;
        }

        public bool Connect
        {
            get;
            set;
        }

        public float SpeedScale
        {
            get { return speeds[((ListBoxComponent)selectables[(int)State.Speed]).SelectedIndex]; }
        }

        public bool PerfectTrial
        {
            get;
            set;
        }

        public bool RivalGhost
        {
            get;
            set;
        }

        public int RivalGhostCount
        {
            get { return (int)((ListBoxComponent)selectables[(int)State.RivalGhostCount]).SelectedItem; }
        }

        public OptionControl(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.resourceManager = resourceManager;
            this.sound = sound;

            back = new PictureObject(device, resourceManager, Utility.Path.Combine("option.png"));
            width = back.Width;
            Position = new Vector2(-width, 0);
            string[] langs = { "Auto", "ExceptSlide", "Random", "MuteSE", "Connect", "PerfectTrial", "RivalGhost", "RivalGhostCount", "Speed", "Profile" };
            selectables = new SelectableComponent[langs.Length];
            for (int i = 0; i < selectables.Length; i++)
            {
                if (i >= (int)State.RivalGhostCount)
                {
                    switch ((State)i)
                    {
                        case State.RivalGhostCount:
                            var counts = new int[SkinSetting.MaxRivalGhostCount];
                            for (var j = 0; j < counts.Length; j++)
                            {
                                counts[j] = j + 1;
                            }
                            this.AddChild(selectables[i] = new ListBoxComponent(device, resourceManager, Utility.Path, Utility.Language[langs[i]], counts)
                            {
                                Position = new Vector2(60, 80 + i * 30),
                                SelectedIndex = SkinSetting.Setting.RivalGhostCount - 1
                            });
                            break;
                        case State.Speed:
                            this.AddChild(selectables[i] = new ListBoxComponent(device, resourceManager, Utility.Path, Utility.Language[langs[i]],
                                speeds.Select(s => String.Format("x{0:F2}", s)).ToArray())
                            {
                                Position = new Vector2(60, 80 + i * 30),
                                SelectedIndex = 2
                            });
                            break;
                        case State.Profile:
                            this.AddChild(selectables[i] = new ListBoxComponent(device, resourceManager, Utility.Path, Utility.Language[langs[i]],
                                ProfileManager.Instance.Profiles.Select(p => p.DisplayText).ToArray())
                            {
                                Position = new Vector2(60, 80 + i * 30)
                            });
                            break;
                    }
                }
                else
                {
                    float xOffset = i == 1 ? 30 : 0;
                    this.AddChild(selectables[i] = new CheckBoxComponent(device, resourceManager, Utility.Path, Utility.Language[langs[i]])
                    {
                        Position = new Vector2(30 + xOffset, 80 + i * 30)
                    });
                }
            }
            selectables[0].Selected = true;

            this.AddChild(new TextureString(device, Utility.Language["Option"], 32, PPDColors.White)
            {
                Position = new Vector2(40, 30)
            });
            this.AddChild(back);

            Inputed += OptionControl_Inputed;
        }

        void OptionControl_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                Change(ButtonType.Circle);
                sound.Play(PPDSetting.DefaultSounds[3], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Triangle))
            {
                FocusManager.RemoveFocus();
            }
            else
            {
                if (args.InputInfo.IsPressed(ButtonType.Left))
                {
                    Change(ButtonType.Left);
                    sound.Play(PPDSetting.DefaultSounds[3], -1000);
                }
                if (args.InputInfo.IsPressed(ButtonType.Right))
                {
                    Change(ButtonType.Right);
                    sound.Play(PPDSetting.DefaultSounds[3], -1000);
                }
                if (args.InputInfo.IsPressed(ButtonType.Up))
                {
                    Change(ButtonType.Up);
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
                if (args.InputInfo.IsPressed(ButtonType.Down))
                {
                    Change(ButtonType.Down);
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
            }
        }

        public void Change(ButtonType button)
        {
            if (button == ButtonType.Down || button == ButtonType.Up)
            {
                selectables[(int)state].Selected = false;
                int result = (int)state + (button == ButtonType.Down ? 1 : -1);
                if (result < 0) result = (int)State.Profile;
                if (result > (int)State.Profile) result = 0;
                state = (State)result;
                selectables[(int)state].Selected = true;
            }
            else
            {
                bool isChange = button == ButtonType.Circle || button == ButtonType.Left || button == ButtonType.Right;
                switch (state)
                {
                    case State.Auto:
                        if (isChange) Auto = !Auto;
                        ExceptSlideAuto &= Auto;
                        break;
                    case State.ExceptSlide:
                        if (isChange) ExceptSlideAuto = !ExceptSlideAuto;
                        Auto |= ExceptSlideAuto;
                        break;
                    case State.Random:
                        if (isChange) Random = !Random;
                        break;
                    case State.MuteSE:
                        if (isChange) MuteSE = !MuteSE;
                        break;
                    case State.Connect:
                        if (isChange) Connect = !Connect;
                        break;
                    case State.PerfectTrial:
                        if (WebManager.Instance.IsLogined)
                        {
                            if (isChange) PerfectTrial = !PerfectTrial;
                        }
                        else
                        {
                            var drd = new GeneralDialog(device, resourceManager, sound, Utility.Language["LoginNecessaryForPerfectTrial"]);
                            FocusManager.Focus(drd);
                            this.InsertChild(drd, 0);
                            drd.LostFocused += drd_LostFocused;
                        }
                        break;
                    case State.RivalGhost:
                        if (WebManager.Instance.IsLogined)
                        {
                            if (isChange) RivalGhost = !RivalGhost;
                        }
                        else
                        {
                            var drd = new GeneralDialog(device, resourceManager, sound, Utility.Language["LoginNecessaryForRivalGhost"]);
                            FocusManager.Focus(drd);
                            this.InsertChild(drd, 0);
                            drd.LostFocused += drd_LostFocused;
                        }
                        break;
                    case State.RivalGhostCount:
                        if (isChange)
                        {
                            var isLeft = button == ButtonType.Left;
                            if (isLeft)
                            {
                                selectables[(int)State.RivalGhostCount].Left();
                            }
                            else
                            {
                                selectables[(int)State.RivalGhostCount].Right();
                            }
                        }
                        break;
                    case State.Speed:
                        if (isChange)
                        {
                            var isLeft = button == ButtonType.Left;
                            if (isLeft)
                            {
                                selectables[(int)State.Speed].Left();
                            }
                            else
                            {
                                selectables[(int)State.Speed].Right();
                            }
                        }
                        break;
                    case State.Profile:
                        if (isChange)
                        {
                            var isLeft = button == ButtonType.Left;
                            if (isLeft)
                            {
                                ProfileManager.Instance.Previous();
                                selectables[(int)State.Profile].Left();
                            }
                            else
                            {
                                selectables[(int)State.Profile].Right();
                                ProfileManager.Instance.Next();
                            }
                        }
                        break;
                }
            }
        }

        void drd_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            var baseDialog = sender as GeneralDialogBase;
            this.RemoveChild(baseDialog);
            baseDialog.Dispose();
        }

        protected override void UpdateImpl()
        {
            if (OverFocused)
            {
                Position = new Vector2(Position.X - (0 + Position.X) * 0.3f, 0);
            }
            else
            {
                Position = new Vector2(Position.X - (width + Position.X) * 0.3f, 0);
            }
            selectables[(int)State.Auto].Checked = Auto;
            selectables[(int)State.ExceptSlide].Checked = ExceptSlideAuto;
            selectables[(int)State.Random].Checked = Random;
            selectables[(int)State.MuteSE].Checked = MuteSE;
            selectables[(int)State.Connect].Checked = Connect;
            selectables[(int)State.PerfectTrial].Checked = PerfectTrial;
            selectables[(int)State.RivalGhost].Checked = RivalGhost;
        }
    }
}
