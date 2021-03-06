﻿//--------------------------------------------------------
// This code is generated by AutoFastGenerator.
// You should not modify the code.
//--------------------------------------------------------

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    public partial class ReaderFlowSourceObject
    {
        public override object GetPropertyValue(string propertyName)
        {
            switch (propertyName)
            {
                case "Angle":
                    return Angle;
                case "EndOfStream":
                    return EndOfStream;
                case "EndTime":
                    return EndTime;
                case "IsLong":
                    return IsLong;
                case "MarkID":
                    return MarkID;
                case "MarkType":
                    return MarkType;
                case "Position":
                    return Position;
                case "Time":
                    return Time;
                default:
                    return null;
            }
        }

        protected override void SetPropertyValue(string propertyName, object value)
        {
            switch (propertyName)
            {
                case "Stream":
                    Stream = (System.IO.Stream)value;
                    break;
            }
        }

        public override void ConnectEvent(string eventName, FlowScriptEngine.FlowEventHandler eventHandler)
        {
            switch (eventName)
            {
                case "Failed":
                    Failed += new FlowScriptEngine.FlowEventHandler(eventHandler);
                    break;
                case "Success":
                    Success += new FlowScriptEngine.FlowEventHandler(eventHandler);
                    break;
            }
        }
    }
}
