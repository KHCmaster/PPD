namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Movie
{
    [ToolTipText("Movie_Trimming_Summary")]
    public partial class TrimmingFlowSourceObject : MovieManagerFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Movie.Trimming"; }
        }

        [ToolTipText("Movie_Trimming_Left")]
        public float Left
        {
            get
            {
                if (movieManager != null)
                {
                    return movieManager.MovieTrimmingData.Left;
                }
                return 0;
            }
        }

        [ToolTipText("Movie_Trimming_Right")]
        public float Right
        {
            get
            {
                if (movieManager != null)
                {
                    return movieManager.MovieTrimmingData.Right;
                }
                return 0;
            }
        }

        [ToolTipText("Movie_Trimming_Top")]
        public float Top
        {
            get
            {
                if (movieManager != null)
                {
                    return movieManager.MovieTrimmingData.Top;
                }
                return 0;
            }
        }

        [ToolTipText("Movie_Trimming_Bottom")]
        public float Bottom
        {
            get
            {
                if (movieManager != null)
                {
                    return movieManager.MovieTrimmingData.Bottom;
                }
                return 0;
            }
        }
    }
}
