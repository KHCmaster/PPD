using PPDCoreModel.Data;
using PPDEditorCommon;
using System;
using System.Collections.Generic;

namespace PPDEditor.FlowScript
{
    class EditorMarkInfo : IEditorMarkInfo
    {
        public Mark Mark
        {
            get;
            private set;
        }

        public EditorMarkInfo(Mark mark, ILayer layer)
        {
            Mark = mark;
            Layer = layer;
            Position = mark.Position;
            Angle = mark.Rotation;
            Time = mark.Time;
            Type = (MarkType)mark.Type;
            if (mark is ExMark)
            {
                IsLong = true;
                ReleaseTime = ((ExMark)mark).EndTime;
            }
            else
            {
                IsLong = false;
                ReleaseTime = 0;
            }
            Parameters = new Dictionary<object, object>();
            foreach (var pair in mark.Parameters)
            {
                Parameters.Add(pair.Key, pair.Value);
            }
        }

        #region IEditorMarkInfo メンバー

        public ILayer Layer
        {
            get;
            private set;
        }

        #endregion

        #region IMarkInfo メンバー

        public SharpDX.Vector2 ColorPosition
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public SharpDX.Vector2 Position
        {
            get;
            set;
        }

        public float Angle
        {
            get;
            set;
        }

        public float Time
        {
            get;
            set;
        }

        public uint ID
        {
            get;
            set;
        }

        public PPDCoreModel.Data.MarkType Type
        {
            get;
            set;
        }

        public bool IsLong
        {
            get;
            private set;
        }

        public bool IsAC
        {
            get { throw new NotImplementedException(); }
        }

        public bool HasSameTimingMark
        {
            get { throw new NotImplementedException(); }
        }

        public float ReleaseTime
        {
            get;
            set;
        }

        public bool IsACFT
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsScratch
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsRight
        {
            get { throw new NotImplementedException(); }
        }

        public Dictionary<object, object> Parameters
        {
            get;
            private set;
        }

        public float SlideScale
        {
            get { throw new NotImplementedException(); }
        }

        public int SameTimingMarks
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
