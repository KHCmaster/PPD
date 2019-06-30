using PPDFramework;

namespace PPDTest
{
    class TestSetting : SettingDataBase
    {
        private static TestSetting setting = new TestSetting();
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
