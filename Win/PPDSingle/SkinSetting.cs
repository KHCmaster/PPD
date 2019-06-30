using PPDFramework;

namespace PPDSingle
{
    class SkinSetting : SettingDataBase
    {
        public const int MaxRivalGhostCount = 7;

        private static SkinSetting setting = new SkinSetting();
        public override string Name
        {
            get { return "PPDSingle.setting"; }
        }

        public static SkinSetting Setting
        {
            get
            {
                return setting;
            }
        }

        public bool Connect
        {
            get
            {
                return this["Connect"] == "1";
            }
            set
            {
                this["Connect"] = value ? "1" : "0";
            }
        }

        public bool RivalGhost
        {
            get
            {
                return this["RivalGhost"] == "1";
            }
            set
            {
                this["RivalGhost"] = value ? "1" : "0";
            }
        }

        public SongSelectFilter.SortField SortField
        {
            get
            {
                if (this["SortField"] == null)
                {
                    return SongSelectFilter.SortField.Name;
                }
                return (SongSelectFilter.SortField)int.Parse(this["SortField"]);
            }
            set
            {
                this["SortField"] = ((int)value).ToString();
            }
        }

        public bool Desc
        {
            get
            {
                return this["Desc"] == "1";
            }
            set
            {
                this["Desc"] = value ? "1" : "0";
            }
        }

        public SongInformation.AvailableDifficulty Difficulty
        {
            get
            {
                if (this["Difficulty"] == null)
                {
                    return SongInformation.AvailableDifficulty.Easy | SongInformation.AvailableDifficulty.Normal |
                        SongInformation.AvailableDifficulty.Hard | SongInformation.AvailableDifficulty.Extreme;
                }
                return (SongInformation.AvailableDifficulty)int.Parse(this["Difficulty"]);
            }
            set
            {
                this["Difficulty"] = ((int)value).ToString();
            }
        }

        public SongSelectFilter.ScoreType ScoreType
        {
            get
            {
                if (this["ScoreType2"] == null)
                {
                    return SongSelectFilter.ScoreType.Normal | SongSelectFilter.ScoreType.AC | SongSelectFilter.ScoreType.ACFT;
                }
                return (SongSelectFilter.ScoreType)int.Parse(this["ScoreType2"]);
            }
            set
            {
                this["ScoreType2"] = ((int)value).ToString();
            }
        }

        public int RivalGhostCount
        {
            get
            {
                if (this["RivalGhostCount"] == null)
                {
                    return 1;
                }
                int.TryParse(this["RivalGhostCount"], out int val);
                if (val < 0)
                {
                    val = 1;
                }
                if (val > MaxRivalGhostCount)
                {
                    val = MaxRivalGhostCount;
                }
                return val;
            }
            set
            {
                this["RivalGhostCount"] = value.ToString();
            }
        }
    }
}
