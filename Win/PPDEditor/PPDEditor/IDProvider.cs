namespace PPDEditor
{
    public class IDProvider
    {
        uint nextID;
        public IDProvider(uint currentMax)
        {
            nextID = currentMax + 1;
        }

        public uint Next()
        {
            uint c = nextID;
            nextID++;
            return c;
        }

        public IDProvider Clone()
        {
            var ret = new IDProvider(0)
            {
                nextID = nextID
            };
            return ret;
        }
    }
}
