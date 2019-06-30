using FlowScriptEngine;
using PPDFramework.PPDStructure.PPDData;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Mark.Value"; }
        }

        [ToolTipText("Mark_Value_Value")]
        public MarkDataBase Value
        {
            private get;
            set;
        }

        [ToolTipText("Mark_Value_Time")]
        public float Time
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.Time;
                }
                return 0;
            }
        }

        [ToolTipText("Mark_Value_IsLong")]
        public bool IsLong
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null && Value is ExMarkData)
                {
                    return true;
                }
                return false;
            }
        }

        [ToolTipText("Mark_Value_ReleaseTime")]
        public float ReleaseTime
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null && Value is ExMarkData)
                {
                    return ((ExMarkData)Value).EndTime;
                }
                return 0;
            }
        }

        [ToolTipText("Mark_Value_Position")]
        public Vector2 Position
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return new Vector2(Value.X, Value.Y);
                }
                return Vector2.Zero;
            }
        }

        [ToolTipText("Mark_Value_Rotation")]
        public float Rotation
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.Angle;
                }
                return 0;
            }
        }

        [ToolTipText("Mark_Value_Type")]
        public PPDCoreModel.Data.MarkType Type
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return (PPDCoreModel.Data.MarkType)Value.ButtonType;
                }
                return 0;
            }
        }

        [ToolTipText("Mark_Value_ID")]
        public int ID
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return (int)Value.ID;
                }
                return 0;
            }
        }

        [ToolTipText("Mark_Value_Parameters")]
        public IEnumerable<object> Parameters
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.Parameters.Cast<object>();
                }
                return null;
            }
        }
    }
}
