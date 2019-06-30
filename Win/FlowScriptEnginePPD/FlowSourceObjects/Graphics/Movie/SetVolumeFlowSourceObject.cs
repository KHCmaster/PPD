namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Movie
{
    [ToolTipText("Graphics_Movie_SetVolume_Summary")]
    public partial class SetVolumeFlowSourceObject : ExecutableMovieFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.Movie.SetVolume"; }
        }

        [ToolTipText("Graphics_Movie_SetVolume_Volume")]
        public int Volume
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (movie != null)
            {
                SetValue(nameof(Volume));
                movie.Volume = Volume;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
