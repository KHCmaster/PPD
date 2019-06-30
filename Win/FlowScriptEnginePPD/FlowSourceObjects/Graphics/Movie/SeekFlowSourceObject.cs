using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Movie
{
    [ToolTipText("Graphics_Movie_Seek_Summary")]
    [ModifyData]
    [ModifyDataWarning]
    public partial class SeekFlowSourceObject : ExecutableMovieFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.Movie.Seek"; }
        }

        [ToolTipText("Graphics_Movie_Seek_Time")]
        public double Time
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (movie != null)
            {
                SetValue(nameof(Time));
                movie.Seek(Time);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
