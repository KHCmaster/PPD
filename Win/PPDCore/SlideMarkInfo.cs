using PPDCoreModel;
using System.Collections.Generic;

namespace PPDCore
{
    class SlideMarkInfo : IMarkInfo
    {
        SlideExMark mark;
        SlideColorMarkInfo slideColorMarkInfo;

        public SlideMarkInfo(SlideExMark mark, SlideColorMarkInfo slideColorMarkInfo)
        {
            this.mark = mark;
            this.slideColorMarkInfo = slideColorMarkInfo;
        }

        #region IMarkInfo メンバー

        public SharpDX.Vector2 ColorPosition
        {
            get;
            set;
        }

        public SharpDX.Vector2 Position
        {
            get { return slideColorMarkInfo.BasePosition; }
        }

        public float Angle
        {
            get { return mark.Angle; }
        }

        public float Time
        {
            get { return slideColorMarkInfo.MarkTime; }
        }

        public uint ID
        {
            get { return mark.ID; }
        }

        public PPDCoreModel.Data.MarkType Type
        {
            get { return mark.Type; }
        }

        public bool IsLong
        {
            get { return true; }
        }

        public bool IsAC
        {
            get { return false; }
        }

        public bool HasSameTimingMark
        {
            get { return mark.HasRLSameTiming; }
        }

        public float ReleaseTime
        {
            get { return mark.ReleaseTime; }
        }

        public bool IsACFT
        {
            get { return true; }
        }

        public bool IsScratch
        {
            get { return mark.IsScratch; }
        }

        public bool IsRight
        {
            get { return mark.IsRight; }
        }

        public Dictionary<object, object> Parameters
        {
            get { return mark.Parameters; }
        }

        public float SlideScale
        {
            get { return mark.SlideScale; }
        }

        public int SameTimingMarks
        {
            get { return mark.SameTimingMarks; }
        }

        #endregion
    }
}
