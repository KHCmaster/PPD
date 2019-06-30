using PPDFramework;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace PPDInput
{
    public class AccurateInput : Input
    {
        private Thread thread;
        private bool finish;

        private Key[] keys;
        private int[] buttons;
        private Stopwatch stopwatch;
        private Queue<InputAction> queue;
        private Dictionary<ButtonType, InputAction> lastPressInfos;
        private readonly object settingLock = new object();
        private int sleepTime;

        public AccurateInput(Form form, int sleepTime)
            : base(form)
        {
            this.sleepTime = sleepTime;
        }

        public override void Load()
        {
            base.Load();
            lastPressInfos = new Dictionary<ButtonType, InputAction>();
            queue = new Queue<InputAction>();
            stopwatch = new Stopwatch();
            stopwatch.Start();
            finish = false;
            thread = new Thread(WorkImpl);
            thread.Start();
        }

        protected override void ReleaseObjects()
        {
            if (loaded)
            {
                finish = true;
                if (thread != null)
                {
                    thread.Join(1000);
                    if (thread.IsAlive)
                    {
                        thread.Abort();
                        thread.Join(1000);
                        thread = null;
                    }
                }
                if (stopwatch != null)
                {
                    stopwatch.Stop();
                    stopwatch = null;
                }
            }
            base.ReleaseObjects();
        }

        private void WorkImpl()
        {
            while (!finish)
            {
                if (!loaded || keys == null || buttons == null)
                {
                    continue;
                }

                InputInfoBase inputInfo = null;
                bool result = false;
#if DEBUG
                if (Form.ActiveForm == window)
                {
#endif
                    lock (settingLock)
                    {
                        result = base.GetInput(keys, buttons, CurrentJoyStickIndex, out inputInfo);
                    }
#if DEBUG
                }
#endif
                if (result)
                {
                    double time = (double)stopwatch.ElapsedTicks / Stopwatch.Frequency;
                    foreach (ButtonType buttonType in ButtonUtility.Array)
                    {
                        if (inputInfo.IsPressed(buttonType))
                        {
                            var action = new InputAction(buttonType, time, true, stopwatch);
                            lock (queue)
                            {
                                queue.Enqueue(action);
                            }
                            lock (lastPressInfos)
                            {
                                lastPressInfos[buttonType] = action;
                            }
                        }
                        if (inputInfo.IsReleased(buttonType))
                        {
                            lock (queue)
                            {
                                queue.Enqueue(new InputAction(buttonType, time, false, stopwatch));
                            }
                            lock (lastPressInfos)
                            {
                                if (lastPressInfos.ContainsKey(buttonType))
                                {
                                    lastPressInfos.Remove(buttonType);
                                }
                            }
                        }
                    }
                }
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
            }
        }

        public override bool GetInput(Key[] keys, int[] buttons, int joyStickIndex, out PPDFramework.InputInfoBase inputInfo)
        {
            lock (settingLock)
            {
                if (this.keys == null)
                {
                    this.keys = new Key[keys.Length];
                }
                if (this.buttons == null)
                {
                    this.buttons = new int[buttons.Length];
                }

                this.keys = new Key[keys.Length];
                Array.Copy(keys, this.keys, keys.Length);
                Array.Copy(buttons, this.buttons, buttons.Length);
            }

            InputAction[] actions;
            Dictionary<ButtonType, InputAction> lastPress;
            lock (queue)
            {
                actions = queue.ToArray();
                queue.Clear();
            }
            lock (lastPressInfos)
            {
                lastPress = new Dictionary<ButtonType, InputAction>();
                foreach (KeyValuePair<ButtonType, InputAction> kvp in lastPressInfos)
                {
                    lastPress[kvp.Key] = kvp.Value;
                }
            }

            inputInfo = new AccurateInputInfo(actions, stopwatch, lastPress);

            return true;
        }

        public override bool GetPressedButton(out int button)
        {
            return base.GetPressedButton(out button);
        }

        public override bool GetPressedKey(out Key key)
        {
            return base.GetPressedKey(out key);
        }
    }
}
