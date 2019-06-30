namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Movie
{
    [ToolTipText("Movie_Seek_Summary")]
    public partial class SeekFlowSourceObject : ExecutableMovieManagerFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Movie.Seek"; }
        }

        [ToolTipText("Movie_Seek_Time")]
        public double Time
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (movieManager != null)
            {
                SetValue(nameof(Time));
                movieManager.Seek(Time);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
