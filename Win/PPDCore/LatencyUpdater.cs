namespace PPDCore
{
    abstract class LatencyUpdaterBase
    {
        private float baseLatency;

        public int BaseLantencyCount
        {
            get;
            private set;
        }

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

        protected LatencyUpdaterBase(float baseLatency)
        {
            this.baseLatency = baseLatency;
            BaseLantencyCount = (int)(baseLatency * 1000);
        }

        public abstract void SaveLatency();
    }
}
