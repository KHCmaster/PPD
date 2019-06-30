namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Movie
{
    [ToolTipText("Graphics_Movie_Stop_Summary")]
    public partial class StopFlowSourceObject : ExecutableMovieFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.Movie.Stop"; }
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (movie != null)
            {
                movie.Stop();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
