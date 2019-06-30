using FlowScriptEngine;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.GameResult
{
    [ToolTipText("GameResult_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("GameResult_Value_Died")]
        public event FlowEventHandler Died;

        public override string Name
        {
            get { return "PPD.GameResult.Value"; }
        }

        protected GameResultManager gameResultManager;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("GameResultManager"))
            {
                gameResultManager = this.Manager.Items["GameResultManager"] as GameResultManager;
            }

            Manager.RegisterCallBack("Died", Died_EventHandler);
        }

        private void Died_EventHandler(object[] args)
        {
            FireEvent(Died);
        }

        [ToolTipText("GameResult_Value_CurrentScore")]
        public int CurrentScore
        {
            get
            {
                if (gameResultManager != null)
                {
                    return gameResultManager.CurrentScore;
                }
                return 0;
            }
        }

        [ToolTipText("GameResult_Value_CurrentLife")]
        public int CurrentLife
        {
            get
            {
                if (gameResultManager != null)
                {
                    return gameResultManager.CurrentLife;
                }
                return 0;
            }
        }

        [ToolTipText("GameResult_Value_MaxLife")]
        public int MaxLife
        {
            get
            {
                return GameResultManager.MAXLIFE;
            }
        }

        [ToolTipText("GameResult_Value_MinLife")]
        public int MinLife
        {
            get
            {
                return GameResultManager.MINLIFE;
            }
        }

        [ToolTipText("GameResult_Value_CurrentCombo")]
        public int CurrentCombo
        {
            get
            {
                if (gameResultManager != null)
                {
                    return gameResultManager.CurrentCombo;
                }
                return 0;
            }
        }

        [ToolTipText("GameResult_Value_MaxCombo")]
        public int MaxCombo
        {
            get
            {
                if (gameResultManager != null)
                {
                    return gameResultManager.MaxCombo;
                }
                return 0;
            }
        }

        [ToolTipText("GameResult_Value_CoolCount")]
        public int CoolCount
        {
            get
            {
                if (gameResultManager != null)
                {
                    return gameResultManager.CoolCount;
                }
                return 0;
            }
        }

        [ToolTipText("GameResult_Value_GoodCount")]
        public int GoodCount
        {
            get
            {
                if (gameResultManager != null)
                {
                    return gameResultManager.GoodCount;
                }
                return 0;
            }
        }

        [ToolTipText("GameResult_Value_SafeCount")]
        public int SafeCount
        {
            get
            {
                if (gameResultManager != null)
                {
                    return gameResultManager.SafeCount;
                }
                return 0;
            }
        }

        [ToolTipText("GameResult_Value_SadCount")]
        public int SadCount
        {
            get
            {
                if (gameResultManager != null)
                {
                    return gameResultManager.SadCount;
                }
                return 0;
            }
        }

        [ToolTipText("GameResult_Value_WorstCount")]
        public int WorstCount
        {
            get
            {
                if (gameResultManager != null)
                {
                    return gameResultManager.WorstCount;
                }
                return 0;
            }
        }

        [ToolTipText("GameResult_Value_HoldBonus")]
        public int HoldBonus
        {
            get
            {
                if (gameResultManager != null)
                {
                    return gameResultManager.HoldBonus;
                }
                return 0;
            }
        }

        [ToolTipText("GameResult_Value_SlideBonus")]
        public int SlideBonus
        {
            get
            {
                if (gameResultManager != null)
                {
                    return gameResultManager.SlideBonus;
                }
                return 0;
            }
        }

        [ToolTipText("GameResult_Value_ExpectedTotalSlideBonus")]
        public int ExpectedTotalSlideBonus
        {
            get
            {
                if (gameResultManager != null)
                {
                    return gameResultManager.ExpectedTotalSlideBonus;
                }
                return 0;
            }
        }

        [ToolTipText("GameResult_Value_IsRetrying")]
        public bool IsRetrying
        {
            get
            {
                if (gameResultManager != null)
                {
                    return gameResultManager.IsRetrying;
                }
                return false;
            }
        }

        [ToolTipText("GameResult_Value_IsReplaying")]
        public bool IsReplaying
        {
            get
            {
                if (gameResultManager != null)
                {
                    return gameResultManager.IsReplaying;
                }
                return false;
            }
        }
    }
}
