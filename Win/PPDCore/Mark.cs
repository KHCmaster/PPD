using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.Logger;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDCore
{
    public delegate bool CreateMarkFunc(IMarkInfo markInfo,
        out GameComponent mark, out GameComponent colorMark,
        out GameComponent axis,
        out GameComponent slideMark, out GameComponent slideColorMark,
        out PictureObject trace);
    public delegate bool GetScriptAllowedButtonsFunc(IMarkInfo markInfo, out ButtonType[] buttonTypes);

    class Mark : GameComponent, IMarkInfo
    {
        public delegate Vector2 ColorPositionDelegate(float scale, float timediff);
        public delegate bool CheckColorPositionDelegate(IMarkInfo markInfo, float currentTime, float bpm);
        public delegate bool CheckMarkEvaluateDelegate(IMarkInfo markInfo, EffectType effectType, bool missPress, bool release, Vector2 position);

        public event CheckColorPositionDelegate CheckColorPosition;
        public event ColorPositionDelegate ChangeColorPosition;
        public event CheckMarkEvaluateDelegate ChangeMarkEvaluate;

        protected enum State
        {
            Appearing = 0,
            Moving = 1,
            Disappering = 2,
            Cool = 3,
            Fine = 4,
            Safe = 5,
            Sad = 6,
            Worst = 7
        }

        protected int simultaneousType;
        protected State state = State.Appearing;
        protected const float DefaultBpm = 130;
        protected const float AutoWidth = 1 / 120f;
        protected DisplayState displayState;
        protected MarkImagePathsBase filenames;
        protected ColorMarkInfo colorMarkInfo;
        protected Matrix matrix;
        protected float[] eval;
        protected GameComponent mark;
        protected GameComponent axis;
        protected PictureObject trace;
        protected PictureObject simpic;
        protected float adjustGapTime;
        protected PPDFramework.Resource.ResourceManager resourceManager;
        protected bool isMarkCreated;
        protected float fixedBPM = -1;
        protected int sameTimingMarks;
        protected ButtonType usedButton;
        protected ButtonType[] scriptAllowedButtons;
        protected PostDrawManager postDrawManager;

        public int SimType
        {
            get
            {
                return simultaneousType;
            }
            set
            {
                simultaneousType = value;
                if (simultaneousType != 0)
                {
                    simpic = new PictureObject(device, resourceManager, filenames.GetTraceImagePath((ButtonType)(SimType - 1)));
                }
            }
        }

        public float Time
        {
            get;
            private set;
        }

        public float Angle
        {
            get;
            private set;
        }

        public ButtonType ButtonType
        {
            get;
            private set;
        }

        public Vector2 ColorPosition
        {
            get
            {
                return colorMarkInfo.ColorMark.Position;
            }
            set
            {
                colorMarkInfo.ColorMark.Position = value;
            }
        }

        public bool ColorHidden
        {
            get;
            protected set;
        }

        public NoteType NoteType
        {
            get;
            private set;
        }

        public uint ID
        {
            get;
            private set;
        }

        public bool IsAC
        {
            get
            {
                return NoteType == PPDFramework.NoteType.AC;
            }
        }

        public bool IsACFT
        {
            get
            {
                return NoteType == PPDFramework.NoteType.ACFT;
            }
        }


        public virtual bool IsLong
        {
            get
            {
                return false;
            }
        }

        public MarkType Type
        {
            get
            {
                return (MarkType)ButtonType;
            }
        }

        public bool HasSameTimingMark
        {
            get;
            set;
        }

        public int SameTimingMarks
        {
            get { return sameTimingMarks; }
        }

        public float FixedBPM
        {
            get
            {
                return fixedBPM;
            }
            set
            {
                fixedBPM = value;
            }
        }

        public virtual float ReleaseTime
        {
            get
            {
                return 0;
            }
        }

        public bool HasRLSameTiming
        {
            get
            {
                return ((sameTimingMarks >> (int)ButtonType.R) & 1) == 1 ||
                     ((sameTimingMarks >> (int)ButtonType.L) & 1) == 1;
            }
        }

        public virtual bool IsScratch
        {
            get
            {
                if (NoteType != PPDFramework.NoteType.ACFT)
                {
                    return false;
                }
                switch (ButtonType)
                {
                    case ButtonType.Triangle:
                    case ButtonType.Circle:
                        if (HasRLSameTiming)
                        {
                            return true;
                        }
                        break;
                    case ButtonType.R:
                        return true;
                    case ButtonType.L:
                        return true;
                }
                return false;
            }
        }

        public bool IsRight
        {
            get
            {
                return GetButtonType() == ButtonType.Start;
            }
        }

        public virtual bool DoNotDrawConnect
        {
            get
            {
                return false;
            }
        }

        public Dictionary<object, object> Parameters
        {
            get;
            private set;
        }

        public virtual float SlideScale
        {
            get { return 1; }
        }

        public Mark(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, MarkImagePathsBase filenames, MarkData md, ButtonType type, float[] eval,
            float gap, DisplayState dState, NoteType noteType, int sameTimingMarks, KeyValuePair<string, string>[] parameters, PostDrawManager postDrawManager) : base(device)
        {
            Position = new Vector2(md.X, md.Y);
            Angle = md.Angle;
            Time = md.Time;
            this.resourceManager = resourceManager;
            this.eval = eval;
            this.filenames = filenames;
            this.adjustGapTime = gap;
            this.sameTimingMarks = sameTimingMarks;
            this.ButtonType = type;
            this.displayState = dState;
            this.NoteType = noteType;
            this.ID = md.ID;
            Parameters = new Dictionary<object, object>();
            foreach (var parameter in parameters)
            {
                Parameters[parameter.Key] = parameter.Value;
            }
            this.postDrawManager = postDrawManager;
        }

        protected ButtonType GetButtonType()
        {
            if (NoteType == PPDFramework.NoteType.ACFT)
            {
                switch (ButtonType)
                {
                    case ButtonType.Triangle:
                        if (HasRLSameTiming)
                        {
                            goto case ButtonType.L;
                        }
                        break;
                    case ButtonType.Circle:
                        if (HasRLSameTiming)
                        {
                            goto case ButtonType.R;
                        }
                        break;
                    case ButtonType.R:
                        return ButtonType.Start;
                    case ButtonType.L:
                        return ButtonType.Start + 1;
                }
            }
            return ButtonType;
        }

        public virtual void Initialize(CreateMarkFunc createMark, GetScriptAllowedButtonsFunc getScriptAllowedButtons)
        {
            createMark(this, out mark, out GameComponent colorMark, out axis, out GameComponent slideMark, out GameComponent slideColorMark, out PictureObject trace);
            isMarkCreated = mark != null;
            mark = mark ?? new PictureObject(device, resourceManager, filenames.GetMarkImagePath(GetButtonType()), true);
            mark.Scale = Vector2.Zero;
            colorMark = colorMark ?? new PictureObject(device, resourceManager, filenames.GetMarkColorImagePath(GetButtonType()), true);
            axis = axis ?? new PictureObject(device, resourceManager, filenames.GetClockAxisImagePath(), true);
            this.trace = trace ?? new PictureObject(device, resourceManager, filenames.GetTraceImagePath(GetButtonType()), false);
            colorMarkInfo = new ColorMarkInfo(device, colorMark, Vector2.Zero, this.trace);
            colorMark.PostDrawManager = postDrawManager;
            colorMarkInfo.PostDrawManager = postDrawManager;
            getScriptAllowedButtons(this, out scriptAllowedButtons);
            this.matrix = Matrix.Transformation2D(Vector2.Zero, 0, Vector2.One, Vector2.Zero, -Angle, Vector2.Zero);
            Hidden = true;
            ColorHidden = true;
            this.AddChild(colorMark);
            this.AddChild(axis);
            this.AddChild(mark);
            this.AddChild(colorMarkInfo);
        }

        public EffectType Evaluate(float currentTime)
        {
            return EvaluateImpl(Time, currentTime - adjustGapTime);
        }

        protected EffectType EvaluateImpl(float basetime, float currentTime)
        {
            var difference = (float)Math.Abs(basetime - currentTime);
            Logger.Instance.AddLog("Diff:{0}", difference);
            if (difference <= eval[3])
            {
                if (difference <= eval[2])
                {
                    if (difference <= eval[1])
                    {
                        if (difference <= eval[0])
                        {
                            return EffectType.Cool;
                        }
                        return EffectType.Fine;
                    }
                    return EffectType.Safe;
                }
                return EffectType.Sad;
            }
            if (basetime - currentTime < 0)
            {
                return EffectType.Worst;
            }
            else
            {
                return EffectType.None;
            }
        }

        protected bool ProcessPressedButton(bool[] button)
        {
            if (scriptAllowedButtons != null)
            {
                return scriptAllowedButtons.Any(b => button[(int)b]);
            }
            else
            {
                if (!IsACFT || !IsScratch)
                {
                    return button[(int)ButtonType];
                }

                if (IsRight)
                {
                    return button[(int)ButtonType.Circle] || button[(int)ButtonType.R];
                }
                else
                {
                    return button[(int)ButtonType.Triangle] || button[(int)ButtonType.L];
                }
            }
        }

        protected bool ProcessReleasedButton(bool[] button)
        {
            return button[(int)usedButton];
        }

        protected void UnsetPressedButton(bool[] button)
        {
            if (scriptAllowedButtons != null)
            {
                usedButton = scriptAllowedButtons.FirstOrDefault(b => button[(int)b]);
            }
            else
            {
                if (!IsACFT || !IsScratch)
                {
                    usedButton = ButtonType;
                }
                else
                {
                    if (IsRight)
                    {
                        if (button[(int)ButtonType.R])
                        {
                            usedButton = ButtonType.R;
                        }
                        else if (button[(int)ButtonType.Circle])
                        {
                            usedButton = ButtonType.Circle;
                        }
                    }
                    else
                    {
                        if (button[(int)ButtonType.L])
                        {
                            usedButton = ButtonType.L;
                        }
                        else if (button[(int)ButtonType.Triangle])
                        {
                            usedButton = ButtonType.Triangle;
                        }
                    }
                }
            }
            button[(int)usedButton] = false;
        }

        protected bool UnsetReleasedButton(bool[] button)
        {
            if (!IsACFT || !IsScratch)
            {
                return button[(int)ButtonType];
            }

            return button[(int)usedButton] = false;
        }

        public virtual bool Update(float currentTime, float bpm, ref bool[] button, MarkResults results, bool auto, ref int soundType, MarkManager.EvaluateEffectHandler effectCallback)
        {
            if (fixedBPM >= 0)
            {
                bpm = fixedBPM;
            }
            bool ret = false;
            if (!auto)
            {
                if (ProcessPressedButton(button) && state < State.Cool)
                {
                    Logger.Instance.AddLog("Update:{0}, {1}, {2}", Time, currentTime, adjustGapTime);
                    var pressEval = EvaluateImpl(Time, currentTime - adjustGapTime);
                    Logger.Instance.AddLog("Evaluate:{0}", pressEval);
                    if (pressEval > EffectType.Worst)
                    {
                        UnsetPressedButton(button);
                    }
                    switch (pressEval)
                    {
                        case EffectType.Cool:
                            state = State.Cool;
                            OnPrcessEvaluate(false, false, EffectType.Cool, effectCallback);
                            results.Add(EffectType.Cool);
                            ret = true;
                            break;
                        case EffectType.Fine:
                            state = State.Fine;
                            OnPrcessEvaluate(false, false, EffectType.Fine, effectCallback);
                            results.Add(EffectType.Fine);
                            ret = true;
                            break;
                        case EffectType.Safe:
                            state = State.Safe;
                            OnPrcessEvaluate(false, false, EffectType.Safe, effectCallback);
                            results.Add(EffectType.Safe);
                            ret = true;
                            break;
                        case EffectType.Sad:
                            state = State.Sad;
                            OnPrcessEvaluate(false, false, EffectType.Sad, effectCallback);
                            results.Add(EffectType.Sad);
                            ret = true;
                            break;
                        case EffectType.Worst:
                            state = State.Worst;
                            OnPrcessEvaluate(false, false, EffectType.Worst, null);
                            results.Add(EffectType.Worst);
                            ret = true;
                            break;
                        case 0:
                            break;
                    }
                }
            }
            else
            {
                //when auto
                if (currentTime - Time >= -AutoWidth && state < State.Cool)
                {
                    state = State.Cool;
                    OnPrcessEvaluate(false, false, EffectType.Cool, effectCallback);
                    soundType = (int)ButtonType;
                    results.Add(EffectType.Cool);
                    ret = true;
                    Hidden = false;
                    ColorHidden = false;
                }
            }
            if (currentTime - Time >= eval[3] && state < State.Cool)
            {
                OnPrcessEvaluate(false, false, EffectType.Worst, null);
                results.Add(EffectType.Worst);
                ret = true;
            }
            if (state < State.Cool)
            {
                if (state == State.Moving && currentTime - Time >= 0)
                {
                    state = State.Disappering;
                }
                if (state == State.Disappering)
                {
                    mark.Scale = axis.Scale = colorMarkInfo.ColorMark.Scale = new Vector2((eval[3] + Time - currentTime) / eval[3]);
                    if (mark.Scale.X <= 0)
                    {
                        mark.Scale = axis.Scale = colorMarkInfo.Scale = Vector2.Zero;
                    }
                }
                float scale = (float)bpm / DefaultBpm;
                if (Hidden)
                {
                    if (Time - currentTime <= 0.5f / scale)
                    {
                        if (displayState == DisplayState.Sudden)
                        {
                            Hidden = false;
                            ColorHidden = false;
                            OnPrcessEvaluate(false, false, EffectType.Appear, effectCallback);
                            Array.Clear(colorMarkInfo.Pos1, 0, colorMarkInfo.Pos1.Length);
                            Array.Clear(colorMarkInfo.Pos2, 0, colorMarkInfo.Pos2.Length);
                            colorMarkInfo.VerticesCount = 0;
                        }
                    }
                    else if (Time - currentTime <= 2 / scale)
                    {
                        if (displayState == DisplayState.Normal || displayState == DisplayState.Hidden || displayState == DisplayState.HiddenColor || displayState == DisplayState.SuddenColor)
                        {
                            Hidden = false;
                            ColorHidden = false;
                            OnPrcessEvaluate(false, false, EffectType.Appear, effectCallback);
                            ColorHidden |= displayState == DisplayState.SuddenColor;
                        }
                    }
                }
                else
                {
                    if (Time - currentTime <= 0.25f / scale)
                    {
                        if (displayState == DisplayState.Hidden)
                        {
                            displayState = (DisplayState)(-1);
                            Hidden = true;
                        }
                        else ColorHidden |= displayState == DisplayState.HiddenColor;
                    }
                    else if (Time - currentTime <= 0.5f / scale)
                    {
                        if (ColorHidden && displayState == DisplayState.SuddenColor)
                        {
                            Array.Clear(colorMarkInfo.Pos1, 0, colorMarkInfo.Pos1.Length);
                            Array.Clear(colorMarkInfo.Pos2, 0, colorMarkInfo.Pos2.Length);
                            colorMarkInfo.VerticesCount = 0;
                            ColorHidden = false;
                        }
                    }
                }
                if (state == State.Appearing && !Hidden)
                {
                    float sx = mark.Scale.X + 0.05f;
                    mark.Scale = new Vector2(sx, sx);
                    //uzu.Scale = new Vector2(sx / 4f, sx / 4f);
                    if (mark.Scale.X > 1.2f)
                    {
                        mark.Scale = Vector2.One;
                        state = State.Moving;
                    }
                }
                UpdateColorPosition(currentTime, bpm);
                axis.Rotation = -1 * (float)Math.PI * (Time - currentTime) * scale;
                if (axis.Rotation >= 0)
                {
                    axis.Rotation = 0;
                }
            }
            colorMarkInfo.DrawTrace = state == State.Moving || state == State.Appearing;
            mark.Hidden = axis.Hidden = !(state < State.Cool);
            colorMarkInfo.ColorMark.Hidden = ColorHidden || !(state < State.Cool);
            colorMarkInfo.Hidden = ColorHidden;
            return ret;
        }

        protected virtual void OnPrcessEvaluate(bool missPress, bool released, EffectType effectType, MarkManager.EvaluateEffectHandler effectcallback)
        {
            OnPrcessEvaluate(missPress, released, effectType, effectcallback, Position);
        }

        protected virtual void OnPrcessEvaluate(bool missPress, bool released, EffectType effectType, MarkManager.EvaluateEffectHandler effectcallback, Vector2 position)
        {
            if (ChangeMarkEvaluate != null)
            {
                if (ChangeMarkEvaluate(this, effectType, missPress, released, position))
                {
                    return;
                }
            }

            if (effectcallback != null)
            {
                effectcallback.Invoke(effectType, position);
            }
        }

        public virtual void UpdateColorPosition(float currentTime, float bpm)
        {
            var vec = CalculateColorPosition(colorMarkInfo, this, Time, currentTime, bpm);
            UpdateColorMarkInfo(colorMarkInfo, this, Time, currentTime, bpm, vec);
        }

        public virtual Vector2 CalculateColorPosition(ColorMarkInfo colorMarkInfo, IMarkInfo markInfo, float markTime, float currentTime, float bpm)
        {
            float scale = (float)bpm / DefaultBpm;
            float timediff = markTime - currentTime;
            return OnChangeColorPosition(markInfo, scale, timediff, currentTime, bpm);
        }

        protected void UpdateColorMarkInfo(ColorMarkInfo colorMarkInfo, IMarkInfo markInfo, float markTime, float currentTime, float bpm, Vector2 vec)
        {
            var scale = (float)bpm / DefaultBpm;
            var timediff = markTime - currentTime;
            var rot = GetAngle(vec);
            vec += colorMarkInfo.BasePosition;
            var tempMat = Matrix.Transformation2D(Vector2.Zero, 0, Vector2.One, Vector2.Zero, timediff >= 0 ? rot : (float)(rot - Math.PI), Vector2.Zero);
            var vec1 = new Vector2(10, 4 * (float)Math.Cos((Time - currentTime) * scale * Math.PI));
            var vec2 = new Vector2(vec1.X, -vec1.Y);
            vec1 = Vector2.TransformCoordinate(vec1, tempMat) + vec;
            vec2 = Vector2.TransformCoordinate(vec2, tempMat) + vec;
            if (colorMarkInfo.VerticesCount == 40)
            {
                Array.ConstrainedCopy(colorMarkInfo.Pos1, 1, colorMarkInfo.Pos1, 0, 39);
                colorMarkInfo.Pos1[39] = vec1;
                Array.ConstrainedCopy(colorMarkInfo.Pos2, 1, colorMarkInfo.Pos2, 0, 39);
                colorMarkInfo.Pos2[39] = vec2;
            }
            else
            {
                colorMarkInfo.Pos1[colorMarkInfo.VerticesCount] = vec1;
                colorMarkInfo.Pos2[colorMarkInfo.VerticesCount] = vec2;
                colorMarkInfo.VerticesCount++;
            }
            colorMarkInfo.ColorMark.Position = vec;
        }

        protected float GetAngle(Vector2 vec)
        {
            var length = vec.Length();

            if (length == 0)
            {
                return 0;
            }

            var d = (float)Math.Acos(vec.X / length);

            if (vec.Y < 0)
            {
                d = (float)(Math.PI * 2 - d);
            }

            if (float.IsNaN(d))
            {
                return 0;
            }

            return d;
        }

        protected Vector2 OnChangeColorPosition(IMarkInfo markInfo, float scale, float timediff, float currentTime, float bpm)
        {
            if (CheckColorPosition != null)
            {
                if (CheckColorPosition.Invoke(markInfo, currentTime, bpm))
                {
                    return markInfo.ColorPosition;
                }
            }

            Vector2 vec;
            if (ChangeColorPosition != null)
            {
                vec = ChangeColorPosition.Invoke(scale, timediff);
            }
            else
            {
                vec = new Vector2(300 * timediff * scale, -30 * (float)Math.Sin(timediff * scale * Math.PI));
            }

            vec = Vector2.TransformCoordinate(vec, matrix);
            return vec;
        }
    }
}