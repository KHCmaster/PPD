namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Movie
{
    [ToolTipText("Graphics_Movie_Position_Summary")]
    public partial class PositionFlowSourceObject : MovieFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.Movie.Position"; }
        }

        [ToolTipText("Graphics_Movie_Position_Value")]
        public float Value
        {
            get
            {
                return movie != null ? (float)movie.MoviePosition : 0;
            }
        }
    }
}
