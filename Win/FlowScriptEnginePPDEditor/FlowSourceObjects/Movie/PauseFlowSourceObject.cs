namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Movie
{
    [ToolTipText("Movie_Pause_Summary")]
    public partial class PauseFlowSourceObject : ExecutableMovieManagerFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Movie.Pause"; }
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (movieManager != null)
            {
                movieManager.Pause();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
