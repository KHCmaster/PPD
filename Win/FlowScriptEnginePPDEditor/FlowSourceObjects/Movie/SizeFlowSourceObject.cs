namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Movie
{
    [ToolTipText("Movie_Size_Summary")]
    public partial class SizeFlowSourceObject : MovieFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Movie.Size"; }
        }

        [ToolTipText("Movie_Size_Width")]
        public int Width
        {
            get
            {
                if (movie != null)
                {
                    return movie.MovieWidth;
                }
                return 0;
            }
        }

        [ToolTipText("Movie_Size_Height")]
        public int Height
        {
            get
            {
                if (movie != null)
                {
                    return movie.MovieHeight;
                }
                return 0;
            }
        }
    }
}
