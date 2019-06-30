using PPDCore;
using PPDFramework;
using SharpDX;
using System;

namespace PPDShareComponent
{
    /// <summary>
    /// 停止中の画面UI
    /// </summary>
    public class PauseMenu : PauseMenuBase
    {
        enum PauseType
        {
            Resume,
            Retry,
            Replay,
            Return,
        }
        public override event EventHandler Resumed;
        public override event EventHandler Retryed;
        public override event EventHandler Replayed;
        public override event EventHandler Returned;
        public override event ChangeLatencyEventHandler LatencyChanged;
        Button[] buttons;
        TextureString latencyString;
        PauseType pauseType = PauseType.Resume;
        PathManager pathManager;

        public PauseMenu(PPDDevice device, PathManager pathManager) : base(device)
        {
            this.pathManager = pathManager;
        }

        public override void Load()
        {
            buttons = new Button[4];
            var names = new string[] { "RESUME", "RETRY", "REPLAY", "RETURN" };
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = new Button(device, ResourceManager, pathManager, names[i])
                {
                    Position = new Vector2(400, 160 + 40 * i + 5)
                };
                buttons[i].Selected = false;
                this.AddChild(buttons[i]);
            }
            buttons[0].Selected = true;
            buttons[2].Enabled = false;
            latencyString = new TextureString(device, GetLatencyString(PPDGameUtility.SongInformation.Latency), 20, PPDColors.White);
            latencyString.Position = new Vector2(400 - latencyString.Width / 2, 330);
            this.AddChild(latencyString);
            this.AddChild(new PictureObject(device, ResourceManager, pathManager.Combine("conftop.png"))
            {
                Position = new Vector2(266, 225 - 107)
            });
            this.AddChild(new PictureObject(device, ResourceManager, pathManager.Combine("confbottom.png"))
            {
                Position = new Vector2(266, 225 + 107 - 17)
            });
            this.AddChild(new PictureObject(device, ResourceManager, pathManager.Combine("confirmpause.png"))
            {
                Position = new Vector2(266, 118)
            });
            Hidden = false;
        }

        private string GetLatencyString(float latency)
        {
            return String.Format("<L:-> Latency:{1}{0:F3} <+:R>", latency, latency >= 0 ? "+" : "");
        }

        public override void Update(InputInfoBase inputInfo)
        {
            if (Disposed) return;
            inputInfo = new PauseMenuInputInfo(inputInfo);

            if (inputInfo.IsPressed(ButtonType.Up))
            {
                ChangePauseType(-1);
                Sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (inputInfo.IsPressed(ButtonType.Down))
            {
                ChangePauseType(1);
                Sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (inputInfo.IsPressed(ButtonType.Cross) || inputInfo.IsPressed(ButtonType.Start))
            {
                OnResume();
            }
            else if (inputInfo.IsPressed(ButtonType.Circle))
            {
                switch (pauseType)
                {
                    case PauseType.Resume:
                        OnResume();
                        break;
                    case PauseType.Retry:
                        ChangePauseType(-1);
                        OnRetry();
                        break;
                    case PauseType.Replay:
                        ChangePauseType(-2);
                        OnReplay();
                        break;
                    case PauseType.Return:
                        ChangePauseType(-3);
                        OnReturn();
                        break;
                }
            }
            else if (inputInfo.IsPressed(ButtonType.Right))
            {
                ChangeLatency(((PauseMenuInputInfo)inputInfo).GetPressingInterval(ButtonType.Right));
            }
            else if (inputInfo.IsPressed(ButtonType.Left))
            {
                ChangeLatency(-((PauseMenuInputInfo)inputInfo).GetPressingInterval(ButtonType.Left));
            }
            else if (inputInfo.IsPressed(ButtonType.R))
            {
                ChangeLatency(10 * ((PauseMenuInputInfo)inputInfo).GetPressingInterval(ButtonType.R));
            }
            else if (inputInfo.IsPressed(ButtonType.L))
            {
                ChangeLatency(-10 * ((PauseMenuInputInfo)inputInfo).GetPressingInterval(ButtonType.L));
            }
            else if (inputInfo.IsPressed(ButtonType.Start))
            {
                OnResume();
            }
            else if (!Hidden)
            {
                ChangeLatency(0);
            }
            Update();
        }

        private void ChangeLatency(int diff)
        {
            if (LatencyChanged != null)
            {
                latencyString.Text = GetLatencyString(LatencyChanged.Invoke(diff));
                latencyString.Position = new Vector2(400 - latencyString.Width / 2, latencyString.Position.Y);
            }
        }

        protected void OnResume()
        {
            Resumed?.Invoke(this, EventArgs.Empty);
        }

        protected void OnRetry()
        {
            Retryed?.Invoke(this, EventArgs.Empty);
        }

        protected void OnReplay()
        {
            Replayed?.Invoke(this, EventArgs.Empty);
        }

        protected void OnReturn()
        {
            Returned?.Invoke(this, EventArgs.Empty);
        }

        public override void Retry(bool canReplay)
        {
            Hidden = true;
            buttons[2].Enabled = canReplay;
        }

        private void ChangePauseType(int dn)
        {
            buttons[(int)pauseType].Selected = false;
            while (true)
            {
                pauseType += dn;
                if (pauseType < PauseType.Resume)
                {
                    pauseType = PauseType.Return;
                }
                if (pauseType > PauseType.Return)
                {
                    pauseType = PauseType.Resume;
                }
                if (buttons[(int)pauseType].Enabled)
                {
                    break;
                }
            }
            buttons[(int)pauseType].Selected = true;
        }
    }
}
