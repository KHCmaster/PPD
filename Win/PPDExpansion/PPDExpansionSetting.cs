using PPDFramework;
using System.Windows;

namespace PPDExpansion
{
    class PPDExpansionSetting : SettingDataBase
    {
        const int defaultWidth = 800;
        const int defaultHeight = 450;
        const int defaultChartHeight = 300;

        private static PPDExpansionSetting setting = new PPDExpansionSetting();
        public override string Name
        {
            get { return "PPDExpansion.setting"; }
        }

        protected override void OnInitialize()
        {
            ShowMyHighScore = true;
            ShowWebHighScore = true;
            ShowCurrentStatus = true;
            ShowGridLines = true;
            Height = defaultHeight;
            Width = defaultWidth;
            SinglePlayChartHeight = new GridLength(defaultChartHeight);
            MultiPlayChartHeight = new GridLength(defaultChartHeight);
        }

        public static PPDExpansionSetting Setting
        {
            get
            {
                return setting;
            }
        }

        public bool ShowMyHighScore
        {
            get
            {
                return this["ShowMyHighScore"] == "1";
            }
            set
            {
                this["ShowMyHighScore"] = value ? "1" : "0";
            }
        }

        public bool ShowWebHighScore
        {
            get
            {
                return this["ShowWebHighScore"] == "1";
            }
            set
            {
                this["ShowWebHighScore"] = value ? "1" : "0";
            }
        }

        public bool ShowCurrentStatus
        {
            get
            {
                return this["ShowCurrentStatus"] == "1";
            }
            set
            {
                this["ShowCurrentStatus"] = value ? "1" : "0";
            }
        }

        public bool ShowIIDXStyle
        {
            get
            {
                return this["ShowIIDXStyle"] == "1";
            }
            set
            {
                this["ShowIIDXStyle"] = value ? "1" : "0";
            }
        }

        public bool ShowScoreDiff
        {
            get
            {
                return this["ShowScoreDiff"] == "1";
            }
            set
            {
                this["ShowScoreDiff"] = value ? "1" : "0";
            }
        }

        public int Height
        {
            get
            {
                if (!int.TryParse(this["Height"], out int val))
                {
                    val = defaultHeight;
                }
                return val;
            }
            set
            {
                this["Height"] = value.ToString();
            }
        }

        public int Width
        {
            get
            {
                if (!int.TryParse(this["Width"], out int val))
                {
                    val = defaultWidth;
                }
                return val;
            }
            set
            {
                this["Width"] = value.ToString();
            }
        }

        public GridLength SinglePlayChartHeight
        {
            get
            {
                if (!double.TryParse(this["SinglePlayChartHeight"], out double val))
                {
                    val = defaultChartHeight;
                }
                return new GridLength(val);
            }
            set
            {
                this["SinglePlayChartHeight"] = value.Value.ToString();
            }
        }

        public GridLength MultiPlayChartHeight
        {
            get
            {
                if (!double.TryParse(this["MultiPlayChartHeight"], out double val))
                {
                    val = defaultChartHeight;
                }
                return new GridLength(val);
            }
            set
            {
                this["MultiPlayChartHeight"] = value.Value.ToString();
            }
        }

        public bool ShowGridLines
        {
            get
            {
                return this["ShowGridLines"] == "1";
            }
            set
            {
                this["ShowGridLines"] = value ? "1" : "0";
            }
        }
    }
}