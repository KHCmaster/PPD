using PPDConfiguration;
using System.IO;

namespace PPD.Update
{
    class Settings
    {
        private const string updaterFilePath = "PPDUpdater.ini";
        private const string installFilePath = "install.info";

        private static Settings setting = new Settings();
        private SettingReader updater;
        private SettingReader install;

        public static Settings Setting
        {
            get
            {
                return setting;
            }
        }

        public SettingReader Updater
        {
            get
            {
                return updater;
            }
        }

        public SettingReader Install
        {
            get
            {
                return install;
            }
        }

        private Settings()
        {
            string data = "";
            if (File.Exists(updaterFilePath))
            {
                data = File.ReadAllText(updaterFilePath);
            }
            updater = new SettingReader(data);
            data = "";
            if (File.Exists(installFilePath))
            {
                data = File.ReadAllText(installFilePath);
            }
            install = new SettingReader(data);
        }
    }
}
