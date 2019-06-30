using FlowScriptEngine;
using PPDEditorCommon;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPDEditor.TypeSerializers
{
    public class EventDataTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(PPDEditorCommon.EventData); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var eventData = (EventData)value;
            AddNewElement(serializer, element, "DisplayState", eventData.DisplayState);
            AddNewElement(serializer, element, "MoveState", eventData.MoveState);
            AddNewElement(serializer, element, "NoteType", eventData.NoteType);
            AddNewElement(serializer, element, "SlideScale", eventData.SlideScale);
            AddNewElement(serializer, element, "BPM", eventData.BPM);
            AddNewElement(serializer, element, "BPMRapidChange", eventData.BPMRapidChange);
            AddNewElement(serializer, element, "VolumePercents", new object[]{
                        eventData.MovieVolumePercent,                        eventData.SquareVolumePercent,                        eventData.CrossVolumePercent,                        eventData.CircleVolumePercent,                        eventData.TriangleVolumePercent,                        eventData.LeftVolumePercent,                        eventData.DownVolumePercent,                        eventData.RightVolumePercent,                        eventData.UpVolumePercent,                        eventData.RVolumePercent,                        eventData.LVolumePercent                    });
            AddNewElement(serializer, element, "KeepPlayings", new object[]{
                        eventData.SquareKeepPlaying,                        eventData.CrossKeepPlaying,                        eventData.CircleKeepPlaying,                        eventData.TriangleKeepPlaying,                        eventData.LeftKeepPlaying,                        eventData.DownKeepPlaying,                        eventData.RightKeepPlaying,                        eventData.UpKeepPlaying,                        eventData.RKeepPlaying,                        eventData.LKeepPlaying                    });
            AddNewElement(serializer, element, "ReleaseSounds", new object[]{
                        eventData.SquareReleaseSound,                        eventData.CrossReleaseSound,                        eventData.CircleReleaseSound,                        eventData.TriangleReleaseSound,                        eventData.LeftReleaseSound,                        eventData.DownReleaseSound,                        eventData.RightReleaseSound,                        eventData.UpReleaseSound,                        eventData.RReleaseSound,                        eventData.LReleaseSound                    });
            AddNewElement(serializer, element, "InitializeOrder", eventData.InitializeOrder);
        }
    }
}
