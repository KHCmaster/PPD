using PPDCore;
using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDMulti
{
    /// <summary>
    /// 停止中の画面UI
    /// </summary>
    public class PauseMenu : PauseMenuBase
    {
        enum PauseType
        {
            Resume = 0,
            Return = 1
        }
        public override event EventHandler Resumed;

        protected virtual void OnRetryed(EventArgs e)
        {
            Retryed?.Invoke(this, e);
        }

        public override event EventHandler Retryed;

        protected virtual void OnReplayed(EventArgs e)
        {
            Replayed?.Invoke(this, e);
        }

        public override event EventHandler Replayed;
        public override event EventHandler Returned;
        public override event ChangeLatencyEventHandler LatencyChanged;
        protected virtual void OnLatencyChanged(int sign)
        {
            LatencyChanged?.Invoke(sign);
        }

        Button[] buttons;
        PauseType pausetype = PauseType.Resume;

        public PauseMenu(PPDDevice device) : base(device)
        {
        }

        public override void Load()
        {
            buttons = new Button[2];
            var names = new string[] { "RESUME", "RETURN" };
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = new Button(device, ResourceManager, Utility.Path, names[i])
                {
                    Position = new Vector2(400, 185 + 80 * i)
                };
                buttons[i].Selected = false;
                this.AddChild(buttons[i]);
            }
            buttons[0].Selected = true;
            this.AddChild(new PictureObject(device, ResourceManager, Utility.Path.Combine("conftop.png"))
            {
                Position = new Vector2(266, 225 - 107)
            });
            this.AddChild(new PictureObject(device, ResourceManager, Utility.Path.Combine("confbottom.png"))
            {
                Position = new Vector2(266, 225 + 107 - 17)
            });
            this.AddChild(new PictureObject(device, ResourceManager, Utility.Path.Combine("confirmpause.png"))
            {
                Position = new Vector2(266, 118)
            });
            Hidden = false;
        }
        public override void Update(InputInfoBase inputInfo)
        {
            if (Disposed) return;
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
                // 再開
                if (Resumed != null) Resumed.Invoke(this, EventArgs.Empty);
            }
            else if (inputInfo.IsPressed(ButtonType.Circle))
            {
                // 再開
                if (pausetype == PauseType.Resume)
                {
                    if (Resumed != null) Resumed.Invoke(this, EventArgs.Empty);
                }
                // リターン
                else if (pausetype == PauseType.Return)
                {
                    if (Returned != null) Returned.Invoke(this, EventArgs.Empty);
                }
            }
            base.Update();
        }

        public override void Retry(bool canReplay)
        {
            Hidden = true;
        }
        private void ChangePauseType(int dn)
        {
            buttons[(int)pausetype].Selected = false;
            pausetype += dn;
            if (pausetype < PauseType.Resume) pausetype = PauseType.Return;
            if (pausetype > PauseType.Return) pausetype = PauseType.Resume;
            buttons[(int)pausetype].Selected = true;
        }
        public override void Draw()
        {
            if (!Hidden)
            {
                base.Draw();
            }
        }
    }
}
