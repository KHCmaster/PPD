using FlowScriptEngine;
using PPDEditorCommon;
using System;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Event
{
    [ToolTipText("Event_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Event.Value"; }
        }

        [ToolTipText("Event_Value_Event")]
        public EventData Event
        {
            private get;
            set;
        }

        [ToolTipText("Event_Value_DisplayState")]
        public PPDFramework.PPDStructure.EVDData.DisplayState DisplayState
        {
            get
            {
                SetValue(nameof(Event));
                if (Event != null)
                {
                    return Event.DisplayState;
                }
                return 0;
            }
        }

        [ToolTipText("Event_Value_MoveState")]
        public PPDFramework.PPDStructure.EVDData.MoveState MoveState
        {
            get
            {
                SetValue(nameof(Event));
                if (Event != null)
                {
                    return Event.MoveState;
                }
                return 0;
            }
        }

        [ToolTipText("Event_Value_NoteType")]
        public PPDFramework.NoteType NoteType
        {
            get
            {
                SetValue(nameof(Event));
                if (Event != null)
                {
                    return Event.NoteType;
                }
                return 0;
            }
        }

        [ToolTipText("Event_Value_SlideScale")]
        public float SlideScale
        {
            get
            {
                SetValue(nameof(Event));
                if (Event != null)
                {
                    return Event.SlideScale;
                }
                return 1;
            }
        }

        [ToolTipText("Event_Value_BPM")]
        public float BPM
        {
            get
            {
                SetValue(nameof(Event));
                if (Event != null)
                {
                    return Event.BPM;
                }
                return 0;
            }
        }

        [ToolTipText("Event_Value_BPMRapidChange")]
        public bool BPMRapidChange
        {
            get
            {
                SetValue(nameof(Event));
                if (Event != null)
                {
                    return Event.BPMRapidChange;
                }
                return false;
            }
        }

        [ToolTipText("Event_Value_VolumePercents")]
        public object[] VolumePercents
        {
            get
            {
                SetValue(nameof(Event));
                if (Event != null)
                {
                    return new object[]{
                        Event.MovieVolumePercent,                        Event.SquareVolumePercent,                        Event.CrossVolumePercent,                        Event.CircleVolumePercent,                        Event.TriangleVolumePercent,                        Event.LeftVolumePercent,                        Event.DownVolumePercent,                        Event.RightVolumePercent,                        Event.UpVolumePercent,                        Event.RVolumePercent,                        Event.LVolumePercent                    };
                }
                return null;
            }
        }

        [ToolTipText("Event_Value_KeepPlayings")]
        public object[] KeepPlayings
        {
            get
            {
                SetValue(nameof(Event));
                if (Event != null)
                {
                    return new object[]{
                        Event.SquareKeepPlaying,                        Event.CrossKeepPlaying,                        Event.CircleKeepPlaying,                        Event.TriangleKeepPlaying,                        Event.LeftKeepPlaying,                        Event.DownKeepPlaying,                        Event.RightKeepPlaying,                        Event.UpKeepPlaying,                        Event.RKeepPlaying,                        Event.LKeepPlaying                    };
                }
                return null;
            }
        }

        [ToolTipText("Event_Value_ReleaseSounds")]
        public object[] ReleaseSounds
        {
            get
            {
                SetValue(nameof(Event));
                if (Event != null)
                {
                    return new object[]{
                        Event.SquareReleaseSound,                        Event.CrossReleaseSound,                        Event.CircleReleaseSound,                        Event.TriangleReleaseSound,                        Event.LeftReleaseSound,                        Event.DownReleaseSound,                        Event.RightReleaseSound,                        Event.UpReleaseSound,                        Event.RReleaseSound,                        Event.LReleaseSound                    };
                }
                return null;
            }
        }

        [ToolTipText("Event_Value_InitializeOrder")]
        public object[] InitializeOrder
        {
            get
            {
                SetValue(nameof(Event));
                if (Event != null)
                {
                    var ret = new object[Event.InitializeOrder.Length];
                    Array.Copy(Event.InitializeOrder, ret, ret.Length);
                    return ret;
                }
                return null;
            }
        }
    }
}
