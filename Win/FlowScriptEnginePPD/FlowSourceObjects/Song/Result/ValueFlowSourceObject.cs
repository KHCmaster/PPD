using FlowScriptEngine;
using PPDFramework;
using System;

namespace FlowScriptEnginePPD.FlowSourceObjects.Song.Result
{
    [ToolTipText("Song_Result_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Song.Result.Value"; }
        }

        [ToolTipText("Song_Result_Value_Value")]
        public ResultInfo Value
        {
            private get;
            set;
        }

        [ToolTipText("Song_Result_Value_ID")]
        public int ID
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.ID;
                }
                return 0;
            }
        }

        [ToolTipText("Song_Result_Value_Difficulty")]
        public PPDFrameworkCore.Difficulty Difficulty
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.Difficulty;
                }
                return PPDFrameworkCore.Difficulty.Other;
            }
        }

        [ToolTipText("Song_Result_Value_ResultType")]
        public ResultEvaluateType ResultType
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.ResultEvaluate;
                }
                return ResultEvaluateType.Mistake;
            }
        }

        [ToolTipText("Song_Result_Value_Score")]
        public int Score
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.Score;
                }
                return 0;
            }
        }

        [ToolTipText("Song_Result_Value_CoolCount")]
        public int CoolCount
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.CoolCount;
                }
                return 0;
            }
        }

        [ToolTipText("Song_Result_Value_GoodCount")]
        public int GoodCount
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.GoodCount;
                }
                return 0;
            }
        }

        [ToolTipText("Song_Result_Value_SafeCount")]
        public int SafeCount
        {
            get
            {

                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.SafeCount;
                }
                return 0;
            }
        }

        [ToolTipText("Song_Result_Value_SadCount")]
        public int SadCount
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.SadCount;
                }
                return 0;
            }
        }

        [ToolTipText("Song_Result_Value_WorstCount")]
        public int WorstCount
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.WorstCount;
                }
                return 0;
            }
        }

        [ToolTipText("Song_Result_Value_MaxCombo")]
        public int MaxCombo
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.MaxCombo;
                }
                return 0;
            }
        }

        [ToolTipText("Song_Result_Value_FinishTime")]
        public float FinishTime
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.FinishTime;
                }
                return 0;
            }
        }

        [ToolTipText("Song_Result_Value_PlayDateTime")]
        public DateTime PlayDateTime
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.Date;
                }
                return DateTime.MinValue;
            }
        }
    }
}
