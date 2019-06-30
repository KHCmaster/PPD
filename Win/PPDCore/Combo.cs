using PPDFramework;
using SharpDX;

namespace PPDCore
{
    /// <summary>
    /// コンボ表示用コンポーネント
    /// </summary>
    class Combo : GameComponent
    {
        ComboInternal comboInternal;

        public Combo(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            comboInternal = new ComboInternal(device, resourceManager);
            Retry();
        }

        public void Retry()
        {
            comboInternal.Retry();
            this.ClearChildren();
            this.AddChild(comboInternal);
            SetDefault();
        }

        public void ChangeState(MarkEvaluateType type, bool isMissPress)
        {
            comboInternal.ChangeState(type, isMissPress);
        }

        public void ChangeCombo(int currentCombo, Vector2 pos)
        {
            comboInternal.ChangeCombo(currentCombo, pos);
        }

        class ComboInternal : GameComponent
        {
            NumberPictureObject display;
            int comboCount;
            int displayCount;

            enum State
            {
                Cool = 0,
                Good = 1,
                Safe = 2,
                Sad = 3,
                Worst = 4,
                MissCool = 5,
                MissGood = 6,
                MissSafe = 7,
                MissSad = 8,
                Nothing = 10
            }
            State state = State.Nothing;
            PictureObject[] evals;

            const float MinScale = 0.6f;

            public ComboInternal(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
            {
                display = new NumberPictureObject(device, resourceManager, Utility.Path.Combine("num.png"))
                {
                    Position = Vector2.Zero,
                    Alignment = PPDFramework.Alignment.Left,
                    MaxDigit = -1
                };
                display.Hidden = true;

                var filenames = new string[] {
                    "cool.png",
                    "good.png",
                    "safe.png",
                    "sad.png",
                    "worst.png",
                    "misscool.png",
                    "missgood.png",
                    "misssafe.png",
                    "misssad.png"
                };
                evals = new PictureObject[filenames.Length];
                for (int i = 0; i < filenames.Length; i++)
                {
                    evals[i] = new PictureObject(device, resourceManager, Utility.Path.Combine("eva", filenames[i]), true)
                    {
                        Hidden = true
                    };
                    this.AddChild(evals[i]);
                }

                this.AddChild(display);
            }

            /// <summary>
            /// コンボ調整
            /// </summary>
            /// <param name="currentCombo"></param>
            /// <param name="pos">マークの場所</param>
            public void ChangeCombo(int currentCombo, Vector2 pos)
            {
                if (currentCombo != 0)
                {
                    comboCount = currentCombo;
                    displayCount = 0;
                    display.Alpha = 1;
                    display.Hidden = comboCount <= 4;
                }
                else
                {
                    comboCount = 0;
                    display.Hidden = true;
                }

                pos = new Vector2(pos.X, pos.Y - 50);

                if (pos.X <= 50) pos.X = 100;
                if (pos.X >= 720) pos.X = 720;
                if (pos.Y <= 50) pos.Y = 50;
                this.Position = pos;
                display.Value = (uint)comboCount;
            }

            /// <summary>
            /// リトライ
            /// </summary>
            public void Retry()
            {
                displayCount = 0;
                comboCount = 0;
                display.Hidden = true;
                state = State.Nothing;
                for (int i = 0; i < evals.Length; i++)
                {
                    evals[i].Hidden = true;
                }
                SetDefault();
            }

            protected override void UpdateImpl()
            {
                displayCount++;
                display.Scale = new Vector2(MinScale, MinScale);


                display.Hidden |= displayCount > 60;

                if (state != State.Nothing)
                {
                    evals[(int)state].Scale = new Vector2(MinScale, MinScale);
                    display.Position = new Vector2((evals[(int)state].Width / 2) * evals[(int)state].Scale.X, -evals[(int)state].Height * evals[(int)state].Scale.Y / 2 + -display.Height * display.Scale.Y / 2);
                    if (displayCount > 60)
                    {
                        evals[(int)state].Hidden = true;
                        state = State.Nothing;
                    }
                }
            }

            public void ChangeState(MarkEvaluateType type, bool isMissPress)
            {
                displayCount = 0;
#pragma warning disable RECS0033 // Convert 'if' to '||' expression
                if (state != State.Nothing)
#pragma warning restore RECS0033 // Convert 'if' to '||' expression
                {
                    evals[(int)state].Hidden = true;
                }
                state = State.Cool + (int)type;
                if (isMissPress && type != MarkEvaluateType.Worst)
                {
                    state += 5;
                }
                if (state != State.Nothing)
                {
                    evals[(int)state].Hidden = false;
                    display.Position = new Vector2((evals[(int)state].Width / 2) * evals[(int)state].Scale.X, -evals[(int)state].Height * evals[(int)state].Scale.Y / 2 + -display.Height * display.Scale.Y / 2);
                }
            }
        }
    }
}