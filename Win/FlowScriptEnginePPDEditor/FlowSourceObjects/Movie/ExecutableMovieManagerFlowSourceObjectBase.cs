using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Movie
{
    public abstract class ExecutableMovieManagerFlowSourceObjectBase : ExecutableFlowSourceObject
    {
        protected IMovieManager movieManager;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (Manager.Items.TryGetValue("MovieManager", out object value))
            {
                movieManager = value as IMovieManager;
            }
        }
    }
}
