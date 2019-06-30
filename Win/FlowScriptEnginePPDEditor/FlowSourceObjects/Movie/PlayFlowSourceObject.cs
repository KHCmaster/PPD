namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Movie
{
    [ToolTipText("Movie_Play_Summary")]
    public partial class PlayFlowSourceObject : ExecutableMovieManagerFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Movie.Play"; }
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (movieManager != null)
            {
                movieManager.Play();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
