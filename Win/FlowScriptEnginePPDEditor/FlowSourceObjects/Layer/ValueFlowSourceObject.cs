using FlowScriptEngine;
using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Layer
{
    [ToolTipText("Layer_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Layer.Value"; }
        }

        [ToolTipText("Layer_Value_Layer")]
        public ILayer Layer
        {
            set;
            private get;
        }

        [ToolTipText("Layer_Value_SelectedMarks")]
        public object[] SelectedMarks
        {
            get
            {
                SetValue(nameof(Layer));
                if (Layer != null)
                {
                    return Layer.SelectedMarks;
                }
                return new object[0];
            }
        }

        [ToolTipText("Layer_Value_Marks")]
        public object[] Marks
        {
            get
            {
                SetValue(nameof(Layer));
                if (Layer != null)
                {
                    return Layer.Marks;
                }
                return new object[0];
            }
        }

        [ToolTipText("Layer_Value_SelectedMark")]
        public IEditorMarkInfo SelectedMark
        {
            get
            {
                SetValue(nameof(Layer));
                if (Layer != null)
                {
                    return Layer.SelectedMark;
                }
                return null;
            }
        }

        [ToolTipText("Layer_Value_IsSelected")]
        public bool IsSelected
        {
            get
            {
                SetValue(nameof(Layer));
                if (Layer != null)
                {
                    return Layer.IsSelected;
                }
                return false;
            }
        }
    }
}
