using PPDFramework;

namespace PPDTest
{
    class TestSetting : SettingDataBase
    {
        private static readonly TestSetting setting = new();
        public override string Name
        {
            get { return "Test.setting"; }
        }

        public static TestSetting Setting
        {
            get
            {
                return setting;
            }
        }
    }
}
