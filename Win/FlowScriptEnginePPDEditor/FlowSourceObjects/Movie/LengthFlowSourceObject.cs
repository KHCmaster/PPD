namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Movie
{
    [ToolTipText("Movie_Length_Summary")]
    public partial class LengthFlowSourceObject : MovieFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Movie.Length"; }
        }

        [ToolTipText("Movie_Length_Value")]
        public float Value
        {
            get
            {
                return movie != null ? (float)movie.Length : 0;
            }
        }
    }
}
