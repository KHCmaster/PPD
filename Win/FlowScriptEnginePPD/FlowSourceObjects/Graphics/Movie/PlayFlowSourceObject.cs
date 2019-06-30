namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Movie
{
    [ToolTipText("Graphics_Movie_Play_Summary")]
    public partial class PlayFlowSourceObject : ExecutableMovieFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.Movie.Play"; }
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (movie != null)
            {
                movie.Play();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
