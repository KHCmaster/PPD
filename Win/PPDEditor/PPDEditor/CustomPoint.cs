namespace PPDEditor
{
    public struct CustomPoint
    {
        float x;
        int y;
        public CustomPoint(float x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public float X
        {
            readonly get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }
        public int Y
        {
            readonly get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }
    }
}
