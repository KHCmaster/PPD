﻿//--------------------------------------------------------
// This code is generated by AutoFastGenerator.
// You should not modify the code.
//--------------------------------------------------------

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Layer
{
    public partial class ValueFlowSourceObject
    {
        public override object GetPropertyValue(string propertyName)
        {
            switch (propertyName)
            {
                case "IsSelected":
                    return IsSelected;
                case "Marks":
                    return Marks;
                case "SelectedMark":
                    return SelectedMark;
                case "SelectedMarks":
                    return SelectedMarks;
                default:
                    return null;
            }
        }

        protected override void SetPropertyValue(string propertyName, object value)
        {
            switch (propertyName)
            {
                case "Layer":
                    Layer = (PPDEditorCommon.ILayer)value;
                    break;
            }
        }

    }
}
