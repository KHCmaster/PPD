using PPDCoreModel;
using PPDFramework.Mod;

namespace PPDCore
{
    class ScriptInfo(ScriptResourceManager resourceManager, ModInfo modInfo, string fileName)
    {
        public ScriptResourceManager ResourceManager
        {
            get;
            private set;
        } = resourceManager;

        public ModInfo ModInfo
        {
            get;
            private set;
        } = modInfo;

        public string FileName
        {
            get;
            private set;
        } = fileName;
    }
}
