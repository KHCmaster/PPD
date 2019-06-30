using FlowScriptEngine;
using PPDFramework;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics
{
    [ToolTipText("Graphics_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        GameComponent gameComponent;

        public override string Name
        {
            get { return "PPD.Graphics.Value"; }
        }

        [ToolTipText("Graphics_Value_Value")]
        public GameComponent Value
        {
            get
            {
                SetValue(nameof(Value));
                return gameComponent;
            }
            set { gameComponent = value; }
        }

        [ToolTipText("Graphics_Value_Position")]
        public Vector2 Position
        {
            get
            {
                SetValue(nameof(Value));
                return gameComponent != null ? gameComponent.Position : Vector2.Zero;
            }
        }

        [ToolTipText("Graphics_Value_Alpha")]
        public float Alpha
        {
            get
            {
                SetValue(nameof(Value));
                return gameComponent != null ? gameComponent.Alpha : 0;
            }
        }

        [ToolTipText("Graphics_Value_Hidden")]
        public bool Hidden
        {
            get
            {
                SetValue(nameof(Value));
                return gameComponent != null && gameComponent.Hidden;
            }
        }

        [ToolTipText("Graphics_Value_Scale")]
        public Vector2 Scale
        {
            get
            {
                SetValue(nameof(Value));
                return gameComponent != null ? gameComponent.Scale : Vector2.Zero;
            }
        }

        [ToolTipText("Graphics_Value_Rotation")]
        public float Rotation
        {
            get
            {
                SetValue(nameof(Value));
                return gameComponent != null ? gameComponent.Rotation : 0;
            }
        }

        [ToolTipText("Graphics_Value_ScaleCenter")]
        public Vector2 ScaleCenter
        {
            get
            {
                SetValue(nameof(Value));
                return gameComponent != null ? gameComponent.ScaleCenter : Vector2.Zero;
            }
        }

        [ToolTipText("Graphics_Value_RotationCenter")]
        public Vector2 RotationCenter
        {
            get
            {
                SetValue(nameof(Value));
                return gameComponent != null ? gameComponent.RotationCenter : Vector2.Zero;
            }
        }
    }
}
