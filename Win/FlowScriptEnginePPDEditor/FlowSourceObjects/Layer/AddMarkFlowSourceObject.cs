using PPDCoreModel.Data;
using PPDEditorCommon;
using SharpDX;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Layer
{
    [ToolTipText("Layer_AddMark_Summary")]
    public partial class AddMarkFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPDEditor.Layer.AddMark"; }
        }

        [ToolTipText("Layer_AddMark_Layer")]
        public ILayer Layer
        {
            private get;
            set;
        }

        [ToolTipText("Layer_AddMark_Position")]
        public Vector2 Position
        {
            private get;
            set;
        }

        [ToolTipText("Layer_AddMark_Rotation")]
        public float Rotation
        {
            private get;
            set;
        }

        [ToolTipText("Layer_AddMark_Time")]
        public float Time
        {
            private get;
            set;
        }

        [ToolTipText("Layer_AddMark_Type")]
        public MarkType Type
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Layer));
            if (Layer != null)
            {
                SetValue(nameof(Position));
                SetValue(nameof(Rotation));
                SetValue(nameof(Time));
                SetValue(nameof(Type));
                Layer.ChangeMarkPropertyManager.AddMark(Position, Rotation, Time, Type);
            }
        }
    }
}
