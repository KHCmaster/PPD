using PPDFramework;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Movie
{
    public abstract class ExecutableMovieFlowSourceObjectBase : ExecutableFlowSourceObject
    {
        protected IMovie movie;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.TryGetValue("Movie", out object value))
            {
                movie = value as IMovie;
            }
        }
    }
}
