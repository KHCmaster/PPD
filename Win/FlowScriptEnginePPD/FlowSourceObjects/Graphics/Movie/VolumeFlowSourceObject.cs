namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Movie
{
    [ToolTipText("Graphics_Movie_Volume_Summary")]
    public partial class VolumeFlowSourceObject : MovieFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Graphics.Movie.Volume"; }
        }

        [ToolTipText("Graphics_Movie_Volume_Value")]
        public int Value
        {
            get
            {
                return movie != null ? movie.Volume : 0;
            }
        }
    }
}
