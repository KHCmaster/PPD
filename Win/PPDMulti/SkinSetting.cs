using PPDFramework;

namespace PPDMulti
{
    class SkinSetting : SettingDataBase
    {
        private static SkinSetting setting = new SkinSetting();
        public override string Name
        {
            get { return "PPDMulti.setting"; }
        }

        public static SkinSetting Setting
        {
            get
            {
                return setting;
            }
        }

        public string AutoUseItemTypes
        {
            get
            {
                string val;
                return (val = setting["AutoUseItemTypes"]) == null ? "" : val;
            }
            set
            {
                setting["AutoUseItemTypes"] = value;
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

        public string LastCreatedRoom
        {
            get { return this["LastCreatedRoom"]; }
            set { this["LastCreatedRoom"] = value; }
        }
    }
}
