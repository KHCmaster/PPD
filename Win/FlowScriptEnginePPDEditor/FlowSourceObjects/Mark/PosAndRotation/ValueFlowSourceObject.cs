using FlowScriptEngine;
using PPDEditorCommon;
using SharpDX;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Mark.PosAndRotation
{
    [ToolTipText("Mark_PosAndRotation_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Mark.PosAndRotation.Value"; }
        }

        [ToolTipText("Mark_PosAndRotation_Value_Value")]
        public IPosAndAngle Value
        {
            private get;
            set;
        }

        [ToolTipText("Mark_PosAndRotation_Value_Position")]
        public Vector2 Position
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.Position.Value;
                }
                return Vector2.Zero;
            }
        }

        [ToolTipText("Mark_PosAndRotation_Value_Rotation")]
        public float Rotation
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.Rotation.Value;
                }
                return 0;
            }
        }

        [ToolTipText("Mark_PosAndRotation_Value_HasPosition")]
        public bool HasPosition
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.Position.HasValue;
                }
                return false;
            }
        }

        [ToolTipText("Mark_PosAndRotation_Value_HasRotation")]
        public bool HasRotation
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.Rotation.HasValue;
                }
                return false;
            }
        }
    }
}
