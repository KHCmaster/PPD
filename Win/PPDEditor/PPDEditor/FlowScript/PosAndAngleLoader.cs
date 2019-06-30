using PPDEditorCommon;
using System.IO;

namespace PPDEditor.FlowScript
{
    class PosAndAngleLoader : IPosAndAngleLoader
    {
        private static PosAndAngleLoader instance = new PosAndAngleLoader();

        public static PosAndAngleLoader Instance
        {
            get
            {
                return instance;
            }
        }

        private PosAndAngleLoader()
        {

        }

        #region IPosAndAngleLoader メンバー

        public IPosAndAngle[] Load(string fileName)
        {
            var filePath = Path.Combine("posdat", Path.ChangeExtension(fileName, ".txt"));
            if (!File.Exists(filePath))
            {
                return null;
            }

            return PosAndAngleInfo.Load(filePath);
        }

        #endregion
    }
}
