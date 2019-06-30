using PPDFramework;
using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PPDInput
{
    public abstract class InputBase : IDisposable
    {
        protected Form window;
        DirectInput di;
        protected Keyboard keyboard;
        List<Joystick> joysticks;
        List<string> joystickNames;
        bool disposed;
        protected const int threshold = 500;
        int currentJoyStickIndex = -1;

        protected bool loaded;
        private bool assignMode;

        protected InputBase(Form window)
        {
            this.window = window;
            joysticks = new List<Joystick>();
            joystickNames = new List<string>();
            //DirectInput の初期化
            di = new DirectInput();
        }

        public virtual void Load()
        {
            ReleaseObjects();
            //キーボードの初期化
            try
            {
                this.keyboard = new Keyboard(di);
                this.keyboard.SetCooperativeLevel(
                  window.Handle,
                  CooperativeLevel.Foreground |
                  CooperativeLevel.NonExclusive |
                  CooperativeLevel.NoWinKey);

                // バッファ方式を使う場合は、バッファサイズを指定しなければならない。
                this.keyboard.Properties.BufferSize = 8;
            }
            catch (SharpDXException e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            //joystick
            foreach (DeviceInstance d in di.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
            {
                // ジョイスティックデバイスの作成と協調レベルの設定
                try
                {
                    Joystick joystick;
                    joystick = new Joystick(di, d.InstanceGuid);
                    joystick.SetCooperativeLevel(window.Handle, CooperativeLevel.Foreground | CooperativeLevel.Exclusive);

                    // バッファ方式を使う場合は、バッファサイズを指定しなければならない。
                    joystick.Properties.BufferSize = 16;

                    // ボタンや軸のプロパティを設定する。
                    foreach (DeviceObjectInstance deviceObject in joystick.GetObjects(DeviceObjectTypeFlags.AbsoluteAxis))
                    {
                        if (deviceObject.ObjectId.Flags.HasFlag(DeviceObjectTypeFlags.AbsoluteAxis))
                        {
                            joystick.GetObjectPropertiesById(deviceObject.ObjectId).Range = new InputRange(-1000, 1000);
                        }
                    }
                    joysticks.Add(joystick);
                    joystickNames.Add(joystick.Information.InstanceName);
                }
                catch (SharpDXException e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }
            }

            if (joysticks.Count > 0)
            {
                currentJoyStickIndex = 0;
            }

            loaded = true;
        }

        protected Joystick CurrentJoyStick
        {
            get
            {
                if (currentJoyStickIndex >= 0 && currentJoyStickIndex < joysticks.Count)
                {
                    return joysticks[currentJoyStickIndex];
                }
                return null;
            }
        }

        public int JoyStickCount
        {
            get
            {
                return joysticks.Count;
            }
        }

        public int CurrentJoyStickIndex
        {
            get
            {
                return currentJoyStickIndex;
            }
            set
            {
                if (value >= 0 && value < joysticks.Count)
                {
                    currentJoyStickIndex = value;
                }
            }
        }

        public string[] JoyStickNames
        {
            get
            {
                return joystickNames.ToArray();
            }
        }

        public bool AssignMode
        {
            get { return assignMode; }
            set
            {
                assignMode = value;
                if (assignMode)
                {
                    Joystick joystick = CurrentJoyStick;
                    if (joystick != null)
                    {
                        joystick.Acquire();
                        joystick.Poll();
                        var state = joystick.GetCurrentState();
                        joystick.GetBufferedData();
                    }

                    if (keyboard != null)
                    {
                        keyboard.Acquire();
                        keyboard.Poll();
                        var bufferedData = keyboard.GetBufferedData();
                    }
                }
            }
        }

        protected virtual void ReleaseObjects()
        {
            if (loaded)
            {
                // キーボードデバイスの終了
                if (this.keyboard != null)
                {
                    this.keyboard.Unacquire();
                    this.keyboard.Dispose();
                }
                // ジョイスティックデバイスの終了
                foreach (Joystick joystick in joysticks)
                {
                    if (joystick != null)
                    {
                        joystick.Unacquire();
                        joystick.Dispose();
                    }
                }
                joysticks.Clear();
                joystickNames.Clear();
                currentJoyStickIndex = -1;

                loaded = false;
            }
        }

        protected Joystick GetJoystickFromIndex(int joyStickIndex)
        {
            return 0 <= joyStickIndex && joyStickIndex < joysticks.Count ? joysticks[joyStickIndex] : null;
        }
        public bool GetInput(Key[] keys, int[] buttons, out InputInfoBase inputInfo)
        {
            return GetInput(keys, buttons, CurrentJoyStickIndex, out inputInfo);
        }
        public abstract bool GetInput(Key[] keys, int[] buttons, int joyStickIndex, out InputInfoBase inputInfo);
        public abstract bool GetPressedKey(out Key key);
        public abstract bool GetPressedButton(out int button);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    ReleaseObjects();
                    // DirectInput 自身の終了
                    di.Dispose();
                }
            }
            disposed = true;
        }
    }

}
