namespace FlowScriptEngineBasic.FlowSourceObjects.Random
{
    class RandomUtility
    {
        private static System.Random rand;
        static RandomUtility()
        {
            rand = new System.Random();
        }

        public static System.Random Rand
        {
            get
            {
                return rand;
            }
        }
    }
}
