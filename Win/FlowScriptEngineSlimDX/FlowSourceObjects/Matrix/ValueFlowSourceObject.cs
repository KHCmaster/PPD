using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        private SharpDX.Matrix value;
        public override string Name
        {
            get { return "Matrix.Value"; }
        }

        [ToolTipText("Matrix_Value_Value_Summary")]
        public SharpDX.Matrix Value
        {
            get
            {
                SetValue(nameof(Value));
                return value;
            }
            set { this.value = value; }
        }

        [ToolTipText("Matrix_Value_M11_Summary")]
        public float M11
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M11;
            }
        }

        [ToolTipText("Matrix_Value_M12_Summary")]
        public float M12
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M12;
            }
        }

        [ToolTipText("Matrix_Value_M13_Summary")]
        public float M13
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M13;
            }
        }

        [ToolTipText("Matrix_Value_M14_Summary")]
        public float M14
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M14;
            }
        }

        [ToolTipText("Matrix_Value_M21_Summary")]
        public float M21
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M21;
            }
        }

        [ToolTipText("Matrix_Value_M22_Summary")]
        public float M22
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M22;
            }
        }

        [ToolTipText("Matrix_Value_M23_Summary")]
        public float M23
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M23;
            }
        }

        [ToolTipText("Matrix_Value_M24_Summary")]
        public float M24
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M24;
            }
        }

        [ToolTipText("Matrix_Value_M31_Summary")]
        public float M31
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M31;
            }
        }

        [ToolTipText("Matrix_Value_M32_Summary")]
        public float M32
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M32;
            }
        }

        [ToolTipText("Matrix_Value_M33_Summary")]
        public float M33
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M33;
            }
        }

        [ToolTipText("Matrix_Value_M34_Summary")]
        public float M34
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M34;
            }
        }

        [ToolTipText("Matrix_Value_M41_Summary")]
        public float M41
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M41;
            }
        }

        [ToolTipText("Matrix_Value_M42_Summary")]
        public float M42
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M42;
            }
        }

        [ToolTipText("Matrix_Value_M43_Summary")]
        public float M43
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M43;
            }
        }

        [ToolTipText("Matrix_Value_M44_Summary")]
        public float M44
        {
            get
            {
                SetValue(nameof(Value));
                return Value.M44;
            }
        }
    }
}
