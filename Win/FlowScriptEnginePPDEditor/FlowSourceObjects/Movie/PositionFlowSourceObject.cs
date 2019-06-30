namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Movie
{
    [ToolTipText("Movie_Position_Summary")]
    public partial class PositionFlowSourceObject : MovieFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Movie.Position"; }
        }

        [ToolTipText("Movie_Position_Value")]
        public float Value
        {
            get
            {
                return movie != null ? (float)movie.MoviePosition : 0;
            }
        }
    }
}
