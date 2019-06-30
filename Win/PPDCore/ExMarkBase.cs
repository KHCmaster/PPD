using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using System.Collections.Generic;

namespace PPDCore
{
    abstract class ExMarkBase : Mark
    {
        public enum ExState
        {
            Waiting = 0,
            Pressed = 1,
            Released = 2
        }

        protected ExState exState = ExState.Waiting;
        protected float endTime;
        protected float length;
        protected float lastTime;

        protected ExMarkBase(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, MarkImagePathsBase filenames, ExMarkData emd, ButtonType type, float[] eval,
            float gap, DisplayState dState, NoteType noteType, int sameTimingMarks, KeyValuePair<string, string>[] parameters, PostDrawManager postDrawManager)
            : base(device, resourceManager, filenames, emd, type, eval, gap, dState, noteType, sameTimingMarks, parameters, postDrawManager)
        {
            this.endTime = emd.EndTime;
            this.length = this.endTime - Time;
        }

        public abstract bool ExUpdate(float currentTime, float bpm, ref bool[] button, ref bool[] released,
            MarkResults results, bool auto, ref int soundType, MarkManager.EvaluateEffectHandler effectCallback);

        public ExState ExMarkState
        {
            get
            {
                return exState;
            }
        }

        public override float ReleaseTime
        {
            get
            {
                return endTime;
            }
        }

        public override bool IsLong
        {
            get
            {
                return true;
            }
        }
    }
}
