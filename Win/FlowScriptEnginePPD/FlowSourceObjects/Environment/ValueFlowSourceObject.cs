using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Environment
{
    [ToolTipText("Environment_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        private IGameHost gameHost;

        public override string Name
        {
            get { return "PPD.Environment.Value"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (Manager.Items.ContainsKey("GameHost"))
            {
                gameHost = Manager.Items["GameHost"] as IGameHost;
            }
        }

        [ToolTipText("Environment_Value_MonitorLatency")]
        public float MonitorLatency
        {
            get
            {
                return PPDFramework.PPDSetting.Setting.AdjustGapTime;
            }
        }

        [ToolTipText("Environment_Value_MovieLatency")]
        public float MovieLatency
        {
            get
            {
                return PPDFramework.PPDSetting.Setting.MovieLatency;
            }
        }

        [ToolTipText("Environment_Value_WindowWidth")]
        public int WindowWidth
        {
            get
            {
                return PPDFramework.PPDSetting.Setting.Width;
            }
        }

        [ToolTipText("Environment_Value_WindowHeight")]
        public int WindowHeight
        {
            get
            {
                return PPDFramework.PPDSetting.Setting.Height;
            }
        }

        [ToolTipText("Environment_Value_UserName")]
        public string UserName
        {
            get
            {
                return PPDFramework.Web.WebManager.Instance.CurrentUserName;
            }
        }

        [ToolTipText("Environment_Value_AccountId")]
        public string AccountId
        {
            get
            {
                return PPDFramework.Web.WebManager.Instance.CurrentAccountId;
            }
        }

        [ToolTipText("Environment_Value_IsLogined")]
        public bool IsLogined
        {
            get
            {
                return PPDFramework.Web.WebManager.Instance.IsLogined;
            }
        }

        [ToolTipText("Environment_Value_DrawSameColorAtSameTimingDisabled")]
        public bool DrawSameColorAtSameTimingDisabled
        {
            get
            {
                return PPDFramework.PPDSetting.Setting.DrawSameColorAtSameTimingDisabled;
            }
        }

        [ToolTipText("Environment_Value_FontSize")]
        public int FontSize
        {
            get
            {
                return PPDFramework.PPDSetting.Setting.FontSizeRatio;
            }
        }

        [ToolTipText("Environment_Value_FPS")]
        public float FPS
        {
            get
            {
                return gameHost != null ? gameHost.FPS : 0;
            }
        }

        [ToolTipText("Environment_Value_LastUpdateTime")]
        public float LastUpdateTime
        {
            get
            {
                return gameHost != null ? gameHost.LastUpdateTime : 0;
            }
        }

        [ToolTipText("Environment_Value_LastDrawTime")]
        public float LastDrawTime
        {
            get
            {
                return gameHost != null ? gameHost.LastDrawTime : 0;
            }
        }
    }
}
