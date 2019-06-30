using PPDFramework;
using System.Globalization;
using System.IO;

namespace PPDCore
{
    class FileLatencyUpdater : LatencyUpdaterBase
    {
        PPDGameUtility ppdGameUtility;
        public FileLatencyUpdater(PPDGameUtility ppdGameUtility) :
            base(GetLatency(ppdGameUtility))
        {
            this.ppdGameUtility = ppdGameUtility;
        }

        public override void SaveLatency()
        {
            var path = Path.Combine(ppdGameUtility.SongInformation.DirectoryPath, "Latency");
            File.WriteAllText(path, Latency.ToString(CultureInfo.InvariantCulture));
        }

        private static float GetLatency(PPDGameUtility ppdGameUtility)
        {
            var path = Path.Combine(ppdGameUtility.SongInformation.DirectoryPath, "Latency");
            if (!File.Exists(path))
            {
                return 0;
            }

            if (!float.TryParse(File.ReadAllText(path), NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
            {
                return 0;
            }
            return val;
        }
    }
}
