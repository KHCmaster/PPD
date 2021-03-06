﻿//--------------------------------------------------------
// This code is generated by AutoFastGenerator.
// You should not modify the code.
//--------------------------------------------------------

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Dialog
{
    public partial class ShowFlowSourceObject
    {
        public override object GetPropertyValue(string propertyName)
        {
            switch (propertyName)
            {
                case "Result":
                    return Result;
                default:
                    return null;
            }
        }

        protected override void SetPropertyValue(string propertyName, object value)
        {
            switch (propertyName)
            {
                case "Setting":
                    Setting = (PPDEditorCommon.Dialog.ViewModel.SettingWindowViewModel)value;
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
