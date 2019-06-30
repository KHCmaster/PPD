using PPDFramework;

namespace PPDShareComponent
{
    class SkinSetting : SettingDataBase
    {
        private static SkinSetting setting = new SkinSetting();
        public override string Name
        {
            get { return "PPDShareComponent.setting"; }
        }

        protected override void OnInitialize()
        {
            ThumbMode = false;
        }

        public static SkinSetting Setting
        {
            get
            {
                return setting;
            }
        }

        public bool ThumbMode
        {
            get
            {
                return this["ThumbMode"] == "1";
            }
            set
            {
                this["ThumbMode"] = value ? "1" : "0";
            }
        }
    }
}
