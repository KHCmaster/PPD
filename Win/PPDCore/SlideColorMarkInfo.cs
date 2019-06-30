using PPDFramework;
using SharpDX;

namespace PPDCore
{
    class SlideColorMarkInfo : ColorMarkInfo
    {
        public enum State
        {
            None,
            Appearing,
            Moving,
            Disappearing
        }

        public bool IsSlided
        {
            get;
            set;
        }

        public State ShowState
        {
            get;
            set;
        }

        public float MarkTime
        {
            get;
            private set;
        }

        public SlideMarkInfo SlideMarkInfo
        {
            get;
            private set;
        }

        public SlideColorMarkInfo(PPDDevice device, SlideExMark exMark, GameComponent colorMark, Vector2 basePosition, PictureObject trace, float markTime)
            : base(device, colorMark, basePosition, trace)
        {
            MarkTime = markTime;
            SlideMarkInfo = new SlideMarkInfo(exMark, this);
        }
    }
}
