using FlowScriptEngine;
using PPDEditorCommon;
using SharpDX;
using System.Collections.Generic;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Mark.Value"; }
        }

        [ToolTipText("Mark_Value_Mark")]
        public IEditorMarkInfo Mark
        {
            private get;
            set;
        }

        [ToolTipText("Mark_Value_Position")]
        public Vector2 Position
        {
            get
            {
                SetValue(nameof(Mark));
                if (Mark != null)
                {
                    return Mark.Position;
                }
                return Vector2.Zero;
            }
        }

        [ToolTipText("Mark_Value_Rotation")]
        public float Rotation
        {
            get
            {
                SetValue(nameof(Mark));
                if (Mark != null)
                {
                    return Mark.Angle;
                }
                return 0;
            }
        }

        [ToolTipText("Mark_Value_Time")]
        public float Time
        {
            get
            {
                SetValue(nameof(Mark));
                if (Mark != null)
                {
                    return Mark.Time;
                }
                return 0;
            }
        }

        [ToolTipText("Mark_Value_ID")]
        public int ID
        {
            get
            {
                SetValue(nameof(Mark));
                if (Mark != null)
                {
                    return (int)Mark.ID;
                }
                return 0;
            }
        }

        [ToolTipText("Mark_Value_Type")]
        public PPDCoreModel.Data.MarkType Type
        {
            get
            {
                SetValue(nameof(Mark));
                if (Mark != null)
                {
                    return Mark.Type;
                }
                return 0;
            }
        }

        [ToolTipText("Mark_Value_IsLong")]
        public bool IsLong
        {
            get
            {
                SetValue(nameof(Mark));
                if (Mark != null)
                {
                    return Mark.IsLong;
                }
                return false;
            }
        }

        [ToolTipText("Mark_Value_ReleaseTime")]
        public float ReleaseTime
        {
            get
            {
                SetValue(nameof(Mark));
                if (Mark != null)
                {
                    return Mark.ReleaseTime;
                }
                return 0;
            }
        }

        [ToolTipText("Mark_Value_Parameters")]
        public Dictionary<object, object> Parameters
        {
            get
            {
                SetValue(nameof(Mark));
                if (Mark != null)
                {
                    return Mark.Parameters;
                }
                return null;
            }
        }
    }
}
