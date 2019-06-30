using System.Drawing;

namespace PPDEditor
{
    public class SquareGrid
    {
        public int Width
        {
            get;
            set;
        }

        public int Height
        {
            get;
            set;
        }

        public int OffsetX
        {
            get;
            set;
        }

        public int OffsetY
        {
            get;
            set;
        }

        public Color GridColor
        {
            get;
            set;
        }

        public int NormalizedOffsetX
        {
            get
            {
                return Normalize(OffsetX, Width);
            }
        }

        public int NormalizedOffsetY
        {
            get
            {
                return Normalize(OffsetY, Height);
            }
        }

        public SquareGrid()
        {
            Width = 5;
            Height = 5;
            OffsetX = 0;
            OffsetY = 0;
            GridColor = ColorTranslator.FromHtml("#FFFFFF");
        }

        private int Normalize(int num, int basenum)
        {
            int val = num % basenum;
            if (val < 0) val += basenum;
            return val;
        }

        public void CopyFrom(SquareGrid grid)
        {
            Width = grid.Width;
            Height = grid.Height;
            OffsetX = grid.OffsetX;
            OffsetY = grid.OffsetY;
            GridColor = grid.GridColor;
        }
    }
}
