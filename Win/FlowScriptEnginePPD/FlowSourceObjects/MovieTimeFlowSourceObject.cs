namespace FlowScriptEnginePPD.FlowSourceObjects
{
    [ToolTipText("MovieTime_Summary")]
    public partial class MovieTimeFlowSourceObject : UpdatableFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.MovieTime"; }
        }

        public override int Priority
        {
            get
            {
                return int.MinValue;
            }
        }

        [ToolTipText("MovieTime_Time")]
        public float Time
        {
            get
            {
                return updateManager != null ? updateManager.MovieTime : 0;
            }
        }
    }
}
