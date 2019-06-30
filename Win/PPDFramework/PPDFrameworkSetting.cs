namespace PPDFramework
{
    class PPDFrameworkSetting : SettingDataBase
    {
        private static PPDFrameworkSetting setting = new PPDFrameworkSetting();
        public override string Name
        {
            get { return "PPDFramework.setting"; }
        }

        public static PPDFrameworkSetting Setting
        {
            get
            {
                return setting;
            }
        }

        public int MasterVolume
        {
            get
            {
                if (int.TryParse(this["MasterVolume"], out int val))
                {
                    return val;
                }
                return 0;
            }
            set
            {
                this["MasterVolume"] = value.ToString();
            }
        }

        public int MovieVolume
        {
            get
            {
                if (int.TryParse(this["MovieVolume"], out int val))
                {
                    return val;
                }
                return 0;
            }
            set
            {
                this["MovieVolume"] = value.ToString();
            }
        }

        public int SeVolume
        {
            get
            {
                if (int.TryParse(this["SeVolume"], out int val))
                {
                    return val;
                }
                return 0;
            }
            set
            {
                this["SeVolume"] = value.ToString();
            }
        }
    }
}
