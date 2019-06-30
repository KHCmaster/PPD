namespace PPDEditorCommon.Dialog.Model
{
    public class TimeLineInfo
    {
        public int[] RowOrders
        {
            get;
            private set;
        }

        public bool[] RowVisibilities
        {
            get;
            private set;
        }

        public bool RowLimited
        {
            get;
            set;
        }

        public TimeLineInfo()
        {
            RowOrders = new int[10];
            RowVisibilities = new bool[10];
            for (int i = 0; i < RowOrders.Length; i++)
            {
                RowOrders[i] = i;
                RowVisibilities[i] = true;
            }
        }
    }
}
