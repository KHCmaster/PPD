namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Movie
{
    [ToolTipText("Movie_Stop_Summary")]
    public partial class StopFlowSourceObject : ExecutableMovieManagerFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Movie.Stop"; }
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (movieManager != null)
            {
                movieManager.Stop();
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
