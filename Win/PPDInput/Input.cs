using PPDFramework;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PPDInput
{
    public class Input : InputBase
    {
        int[] keyCount;
        List<int[]> buttonCount;

        public Input(Form window)
            : base(window)
        {
            buttonCount = new List<int[]>();
        }

        public override void Load()
        {
            base.Load();
            for (int i = 0; i < JoyStickCount; i++)
            {
                buttonCount.Add(null);
            }
        }

        protected override void ReleaseObjects()
        {
            if (loaded)
            {
                if (keyCount != null)
                {
                    Array.Clear(keyCount, 0, keyCount.Length);
                }
                if (buttonCount != null)
                {
                    buttonCount.Clear();
                }
            }
            base.ReleaseObjects();
        }

        public override bool GetInput(Key[] keys, int[] buttons, int joyStickIndex, out InputInfoBase inputInfo)
        {
            var outInputInfo = new InputInfo(false);

            var result = GetPressAndReleasedKey(keys, out int[] keyPressCount, out bool[] keyReleased);
            result |= GetPressAndReleasedButton(buttons, out int[] buttonPressCount, out bool[] buttonReleased, joyStickIndex);
            outInputInfo.Update(keyPressCount, keyReleased, buttonPressCount, buttonReleased);
            inputInfo = outInputInfo;
            return result;
        }

        private bool GetPressAndReleasedKey(Key[] keys, out int[] count, out bool[] released)
        {
            count = new int[keys.Length];
            released = new bool[keys.Length];
            try
            {
                if (keyCount == null)
                {
                    keyCount = new int[keys.Length];
                }
                if (keyboard == null || AssignMode) return false;
                this.keyboard.Acquire();
                this.keyboard.Poll();
                IEnumerable<KeyboardUpdate> bufferedData = keyboard.GetBufferedData();

                bool gotbuffer = false;
                bool[] gotbuffers = new bool[keys.Length];

                foreach (KeyboardUpdate keyboardUpdate in bufferedData)
                {
                    gotbuffer = true;
                    var index = Array.IndexOf(keys, keyboardUpdate.Key);
                    if (index >= 0)
                    {
                        gotbuffers[index] = true;
                        if (keyboardUpdate.IsPressed)
                        {
                            keyCount[index]++;
                        }
                        else
                        {
                            keyCount[index] = 0;
                            released[index] = true;
                        }
                    }
                }
                if (!gotbuffer)
                {
                    for (int i = 0; i < keyCount.Length; i++)
                    {
                        if (keyCount[i] > 0 && !released[i])
                        {
                            keyCount[i]++;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < keyCount.Length; i++)
                    {
                        if (keyCount[i] > 0 && !gotbuffers[i])
                        {
                            keyCount[i]++;
                        }
                    }
                }
                Array.Copy(keyCount, count, keyCount.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool GetPressAndReleasedButton(int[] buttons, out int[] count, out bool[] released, int joyStickIndex)
        {
            count = new int[buttons.Length];
            released = new bool[buttons.Length];
            try
            {
                if (joyStickIndex < 0 || joyStickIndex >= buttonCount.Count || AssignMode)
                {
                    count = new int[buttons.Length];
                    released = new bool[buttons.Length];
                    return false;
                }

                if (buttonCount[joyStickIndex] == null)
                {
                    buttonCount[joyStickIndex] = new int[buttons.Length];
                }
                var joystick = GetJoystickFromIndex(joyStickIndex);
                if (joystick == null)
                {
                    return false;
                }
                joystick.Acquire();
                joystick.Poll();

                var js = joystick.GetCurrentState();
                int[] povdata = js.PointOfViewControllers;
                bool[] povcome = new bool[4];
                bool[] axiscome = new bool[4];
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] < 0)
                    {
                        continue;
                    }

                    if (buttons[i] >= 1024)
                    {
                        for (int j = 0; j < povdata.Length; j++)
                        {
                            switch (buttons[i] - 1024)
                            {
                                case 0:
                                    if ((povdata[j] == 31500 || povdata[j] == 0 || povdata[j] == 4500))
                                    {
                                        povcome[0] = true;
                                    }

                                    break;
                                case 9000:
                                    if ((povdata[j] == 4500 || povdata[j] == 9000 || povdata[j] == 13500))
                                    {
                                        povcome[1] = true;
                                    }

                                    break;
                                case 18000:
                                    if ((povdata[j] == 13500 || povdata[j] == 18000 || povdata[j] == 22500))
                                    {
                                        povcome[2] = true;
                                    }

                                    break;
                                case 27000:
                                    if ((povdata[j] == 22500 || povdata[j] == 27000 || povdata[j] == 31500))
                                    {
                                        povcome[3] = true;
                                    }

                                    break;
                            }
                        }
                    }
                    else if (buttons[i] >= 512)
                    {
                        switch (buttons[i] - 512)
                        {
                            case 0:
                                //minus x
                                if (js.X < -threshold)
                                {
                                    axiscome[0] = true;
                                }
                                break;
                            case 100:
                                //plus x
                                if (js.X > threshold)
                                {
                                    axiscome[1] = true;
                                }
                                break;
                            case 200:
                                // minus y
                                if (js.Y < -threshold)
                                {
                                    axiscome[2] = true;
                                }
                                break;
                            case 300:
                                // plus y
                                if (js.Y > threshold)
                                {
                                    axiscome[3] = true;
                                }
                                break;
                        }
                    }
                    else
                    {
                        var pressed = false;
                        if (js.Buttons[buttons[i]])
                        {
                            pressed = true;
                        }
                        else
                        {
                            if (buttonCount[joyStickIndex][i] != 0)
                            {
                                released[i] = true;
                            }
                            buttonCount[joyStickIndex][i] = 0;
                        }
                        if (pressed || buttonCount[joyStickIndex][i] != 0)
                        {
                            count[i]++;
                        }
                    }
                }
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] >= 1024)
                    {
                        switch (buttons[i] - 1024)
                        {
                            case 0:
                                if (povcome[0])
                                {
                                    count[i]++;
                                }
                                else
                                {
                                    if (buttonCount[joyStickIndex][i] != 0)
                                    {
                                        released[i] = true;
                                    }
                                    buttonCount[joyStickIndex][i] = 0;
                                }

                                break;
                            case 9000:
                                if (povcome[1])
                                {
                                    count[i]++;
                                }
                                else
                                {
                                    if (buttonCount[joyStickIndex][i] != 0)
                                    {
                                        released[i] = true;
                                    }
                                    buttonCount[joyStickIndex][i] = 0;
                                }

                                break;
                            case 18000:
                                if (povcome[2])
                                {
                                    count[i]++;
                                }
                                else
                                {
                                    if (buttonCount[joyStickIndex][i] != 0)
                                    {
                                        released[i] = true;
                                    }
                                    buttonCount[joyStickIndex][i] = 0;
                                }

                                break;
                            case 27000:
                                if (povcome[3])
                                {
                                    count[i]++;
                                }
                                else
                                {
                                    if (buttonCount[joyStickIndex][i] != 0)
                                    {
                                        released[i] = true;
                                    }
                                    buttonCount[joyStickIndex][i] = 0;
                                }

                                break;
                        }
                    }
                    else if (buttons[i] >= 512)
                    {
                        switch (buttons[i] - 512)
                        {
                            case 0:
                                //minus x
                                if (axiscome[0])
                                {
                                    count[i]++;
                                }
                                else
                                {
                                    if (buttonCount[joyStickIndex][i] != 0)
                                    {
                                        released[i] = true;
                                    }
                                    buttonCount[joyStickIndex][i] = 0;
                                }
                                break;
                            case 100:
                                //plus x
                                if (axiscome[1])
                                {
                                    count[i]++;
                                }
                                else
                                {
                                    if (buttonCount[joyStickIndex][i] != 0)
                                    {
                                        released[i] = true;
                                    }
                                    buttonCount[joyStickIndex][i] = 0;
                                }
                                break;
                            case 200:
                                // minus y
                                if (axiscome[2])
                                {
                                    count[i]++;
                                }
                                else
                                {
                                    if (buttonCount[joyStickIndex][i] != 0)
                                    {
                                        released[i] = true;
                                    }
                                    buttonCount[joyStickIndex][i] = 0;
                                }
                                break;
                            case 300:
                                // plus y
                                if (axiscome[3])
                                {
                                    count[i]++;
                                }
                                else
                                {
                                    if (buttonCount[joyStickIndex][i] != 0)
                                    {
                                        released[i] = true;
                                    }
                                    buttonCount[joyStickIndex][i] = 0;
                                }
                                break;
                        }
                    }
                    count[i] = buttonCount[joyStickIndex][i] + count[i];
                }
                Array.Copy(count, buttonCount[joyStickIndex], count.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool GetPressedKey(out Key key)
        {
            try
            {
                key = Key.Unknown;
                if (keyboard == null) return false;
                this.keyboard.Acquire();
                this.keyboard.Poll();
                IEnumerable<KeyboardUpdate> bufferedData = keyboard.GetBufferedData();
                foreach (KeyboardUpdate keyboardUpdate in bufferedData)
                {
                    if (keyboardUpdate.IsPressed)
                    {
                        key = keyboardUpdate.Key;
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                key = Key.Unknown;
                return false;
            }
        }
        public override bool GetPressedButton(out int button)
        {
            try
            {
                button = -1;
                Joystick joystick = CurrentJoyStick;
                if (joystick == null) return false;
                joystick.Acquire();
                joystick.Poll();
                var state = joystick.GetCurrentState();
                int[] povdata = state.PointOfViewControllers;
                for (int i = 0; i < povdata.Length; i++)
                {
                    if ((ushort)povdata[i] != 0xffff)
                    {
                        button = 1024 + (ushort)povdata[i];
                        return true;
                    }
                }
                if (Math.Abs(state.X) >= 800)
                {
                    button = 512 + (state.X > 0 ? 100 : 0);
                    return true;
                }
                if (Math.Abs(state.Y) >= 800)
                {
                    button = 512 + (state.Y > 0 ? 300 : 200);
                    return true;
                }
                for (var i = 0; i < state.Buttons.Length; i++)
                {
                    if (state.Buttons[i])
                    {
                        button = i;
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                button = -1;
                return false;
            }
        }
    }
}
