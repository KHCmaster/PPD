namespace PPDCore
{
    abstract class LatencyUpdaterBase(float baseLatency)
    {
        private readonly float baseLatency = baseLatency;

        public int BaseLantencyCount
        {
            get;
            private set;
        } = (int)(baseLatency * 1000);

        public int LatencyCount
        {
            get;
            set;
        }

        public float Latency
        {
            get
            {
                return baseLatency + 0.001f * LatencyCount;
            }
        }

        public abstract void SaveLatency();
    }
}
