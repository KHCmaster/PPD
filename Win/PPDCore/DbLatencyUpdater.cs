using PPDFramework;

namespace PPDCore
{
    class DbLatencyUpdater : LatencyUpdaterBase
    {
        PPDGameUtility ppdGameUtility;

        public DbLatencyUpdater(PPDGameUtility ppdGameUtility)
            : base(ppdGameUtility.SongInformation.Latency)
        {
            this.ppdGameUtility = ppdGameUtility;
        }

        public override void SaveLatency()
        {
            if (!ppdGameUtility.IsDebug)
            {
                ppdGameUtility.SongInformation.UpdateLatency(Latency);
            }
        }
    }
}
