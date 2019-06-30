namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Movie
{
    [ToolTipText("Graphics_Movie_Pause_Summary")]
    public partial class PauseFlowSourceObject : ExecutableMovieFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.Movie.Pause"; }
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (movie != null)
            {
                movie.Pause();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
