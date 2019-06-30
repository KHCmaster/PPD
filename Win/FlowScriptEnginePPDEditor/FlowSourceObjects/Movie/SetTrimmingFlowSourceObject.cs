using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Movie
{
    [ToolTipText("Movie_SetTrimming_Summary")]
    public partial class SetTrimmingFlowSourceObject : ExecutableMovieManagerFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPDEditor.Movie.SetTrimming"; }
        }

        [ToolTipText("Movie_SetTrimming_Left")]
        public float Left
        {
            private get;
            set;
        }

        [ToolTipText("Movie_SetTrimming_Right")]
        public float Right
        {
            private get;
            set;
        }

        [ToolTipText("Movie_SetTrimming_Top")]
        public float Top
        {
            private get;
            set;
        }

        [ToolTipText("Movie_SetTrimming_Bottom")]
        public float Bottom
        {
            private get;
            set;
        }

        [ToolTipText("Movie_SetTrimming_ApplyToIniFileWriter")]
        public bool ApplyToIniFileWriter
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            if (movieManager != null)
            {
                SetValue(nameof(Left));
                SetValue(nameof(Right));
                SetValue(nameof(Top));
                SetValue(nameof(Bottom));
                SetValue(nameof(ApplyToIniFileWriter));
                movieManager.SetTrimming(new MovieTrimmingData(Top, Left, Right, Bottom), ApplyToIniFileWriter);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
