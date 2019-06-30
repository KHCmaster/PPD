using System.Linq;

namespace PPDEditor
{
    public class TimeLineRowManager
    {
        private RowItem limitedRow;
        private RowItem normalRow;
        private RowItem currentRow;
        private bool isLimited;

        public TimeLineRowManager()
        {
            limitedRow = new RowItem();
            normalRow = new RowItem();
            currentRow = normalRow;
        }

        public int[] OrderedVisibleRows
        {
            get
            {
                return currentRow.RowOrders.Where(r => currentRow.RowVisibilities[r]).ToArray();
            }
        }

        public int[] InverseRowOrders
        {
            get
            {
                return currentRow.InverseRowOrders;
            }
        }

        public int OrderedVisibleRowsCount
        {
            get
            {
                return currentRow.RowOrders.Count(r => currentRow.RowVisibilities[r]);
            }
        }

        public bool[] Visibilities
        {
            get
            {
                return currentRow.RowVisibilities;
            }
        }

        public int[] RowOrders
        {
            get { return limitedRow.RowOrders; }
            set { limitedRow.RowOrders = value; }
        }

        public bool[] RowVisibilities
        {
            get { return limitedRow.RowVisibilities; }
            set { limitedRow.RowVisibilities = value; }
        }

        public bool IsLimited
        {
            get { return isLimited; }
            set
            {
                if (isLimited != value)
                {
                    isLimited = value;
                    currentRow = isLimited ? limitedRow : normalRow;
                }
            }
        }

        public void Restore()
        {
            currentRow = normalRow;
            for (int i = 0; i < currentRow.RowOrders.Length; i++)
            {
                limitedRow.RowOrders[i] = i;
                limitedRow.RowVisibilities[i] = true;
            }
        }

        public int GetTypeFromRowIndex(int rowIndex)
        {
            var iter = 0;
            for (var i = 0; i < currentRow.RowOrders.Length; i++)
            {
                if (currentRow.RowVisibilities[currentRow.RowOrders[i]])
                {
                    if (iter == rowIndex)
                    {
                        return currentRow.RowOrders[i];
                    }
                    iter++;
                }
            }
            return -1;
        }

        public int GetRowIndexFromType(int type)
        {
            var iter = 0;
            for (var i = 0; i < currentRow.RowOrders.Length; i++)
            {
                if (currentRow.RowOrders[i] == type)
                {
                    return iter;
                }
                if (currentRow.RowVisibilities[currentRow.RowOrders[i]])
                {
                    iter++;
                }
            }

            return -1;
        }

        public int GetNormalRowIndex(int y)
        {
            return (y - PPDEditorSkin.Skin.TimeLineHeight) / (PPDEditorSkin.Skin.TimeLineRowHeight);
        }

        public int GetRowIndex(int y)
        {
            if (y < PPDEditorSkin.Skin.TimeLineHeight)
            {
                return -1;
            }
            var index = (y - PPDEditorSkin.Skin.TimeLineHeight) / (PPDEditorSkin.Skin.TimeLineRowHeight);
            var rows = OrderedVisibleRows;
            if (index >= rows.Length)
            {
                return 11;
            }
            return rows[index];
        }

        class RowItem
        {
            private int[] rowOrders;
            private bool[] rowVisibilities;

            public int[] RowOrders
            {
                get { return rowOrders; }
                set { rowOrders = value; }
            }

            public int[] InverseRowOrders
            {
                get
                {
                    int[] ret = new int[rowOrders.Length];
                    for (int i = 0; i < rowOrders.Length; i++)
                    {
                        ret[rowOrders[i]] = i;
                    }
                    return ret;
                }
            }

            public bool[] RowVisibilities
            {
                get { return rowVisibilities; }
                set { rowVisibilities = value; }
            }

            public RowItem()
            {
                rowOrders = new int[10];
                for (int i = 0; i < rowOrders.Length; i++)
                {
                    rowOrders[i] = i;
                }
                rowVisibilities = new bool[10];
                for (int i = 0; i < rowVisibilities.Length; i++)
                {
                    rowVisibilities[i] = true;
                }
            }
        }
    }
}
