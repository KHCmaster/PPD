using PPDFramework;

namespace PPDCore
{
    class DbLatencyUpdater(PPDGameUtility ppdGameUtility) : LatencyUpdaterBase(ppdGameUtility.SongInformation.Latency)
    {
        readonly PPDGameUtility ppdGameUtility = ppdGameUtility;

        public override void SaveLatency()
        {
            if (!ppdGameUtility.IsDebug)
            {
                ppdGameUtility.SongInformation.UpdateLatency(Latency);
            }
        }
    }
}
