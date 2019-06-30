namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Movie
{
    [ToolTipText("Graphics_Movie_Length_Summary")]
    public partial class LengthFlowSourceObject : MovieFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.Movie.Length"; }
        }

        [ToolTipText("Graphics_Movie_Length_Value")]
        public float Value
        {
            get
            {
                return movie != null ? (float)movie.Length : 0;
            }
        }
    }
}
