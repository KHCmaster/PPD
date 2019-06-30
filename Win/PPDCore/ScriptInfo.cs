using PPDCoreModel;
using PPDFramework.Mod;

namespace PPDCore
{
    class ScriptInfo
    {
        public ScriptResourceManager ResourceManager
        {
            get;
            private set;
        }

        public ModInfo ModInfo
        {
            get;
            private set;
        }

        public string FileName
        {
            get;
            private set;
        }

        public ScriptInfo(ScriptResourceManager resourceManager, ModInfo modInfo, string fileName)
        {
            ResourceManager = resourceManager;
            ModInfo = modInfo;
            FileName = fileName;
        }
    }
}
