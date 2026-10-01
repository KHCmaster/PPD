using SharpGen.Runtime;
using PPDFramework;
using Vortice.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PPDInput
{
    public abstract class InputBase : IDisposable
    {
        protected Form window;
        readonly IDirectInput8 di;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Dispose から呼ばれる ReleaseObjects で破棄する")]
        protected IDirectInputDevice8 keyboard;
        readonly List<IDirectInputDevice8> joysticks;
        readonly List<string> joystickNames;
        bool disposed;
        protected const int threshold = 500;
        int currentJoyStickIndex = -1;

        protected bool loaded;
        private bool assignMode;

        protected InputBase(Form window)
        {
            this.window = window;
            joysticks = [];
            joystickNames = [];
            //DirectInput の初期化
            di = DInput.DirectInput8Create();
        }

        public virtual void Load()
        {
            ReleaseObjects();
            //キーボードの初期化
            try
            {
                this.keyboard = di.CreateDevice(PredefinedDevice.SysKeyboard);
                this.keyboard.SetDataFormat<RawKeyboardState>().CheckError();
                this.keyboard.SetCooperativeLevel(
                  window.Handle,
                  CooperativeLevel.Foreground |
                  CooperativeLevel.NonExclusive |
                  CooperativeLevel.NoWinKey).CheckError();

                // バッファ方式を使う場合は、バッファサイズを指定しなければならない。
                this.keyboard.Properties.BufferSize = 8;
            }
            catch (SharpGenException e)
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
                    IDirectInputDevice8 joystick;
                    joystick = di.CreateDevice(d.InstanceGuid);
                    joystick.SetDataFormat<RawJoystickState>().CheckError();
                    joystick.SetCooperativeLevel(window.Handle, CooperativeLevel.Foreground | CooperativeLevel.Exclusive).CheckError();

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
                    joystickNames.Add(joystick.DeviceInfo.InstanceName);
                }
                catch (SharpGenException e)
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

        protected IDirectInputDevice8 CurrentJoyStick
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
                    IDirectInputDevice8 joystick = CurrentJoyStick;
                    if (joystick != null)
                    {
                        joystick.Acquire().CheckError();
                        joystick.Poll().CheckError();
                        var state = joystick.GetCurrentJoystickState();
                        joystick.GetBufferedJoystickData();
                    }

                    if (keyboard != null)
                    {
                        keyboard.Acquire().CheckError();
                        keyboard.Poll().CheckError();
                        var bufferedData = keyboard.GetBufferedKeyboardData();
                    }
                }
            }
        }

        protected virtual void ReleaseObjects()
        {
            // Load が途中で失敗した（loaded が false の）場合も、作成済みのデバイスを解放する
            // キーボードデバイスの終了
            if (this.keyboard != null)
            {
                this.keyboard.Unacquire();
                this.keyboard.Dispose();
                this.keyboard = null;
            }
            // ジョイスティックデバイスの終了
            foreach (IDirectInputDevice8 joystick in joysticks)
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

        protected IDirectInputDevice8 GetJoystickFromIndex(int joyStickIndex)
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

        protected virtual void Dispose(bool disposing)
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
