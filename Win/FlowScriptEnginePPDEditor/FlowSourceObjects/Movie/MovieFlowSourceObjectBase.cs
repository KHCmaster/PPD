using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Movie
{
    public abstract class MovieFlowSourceObjectBase : FlowSourceObjectBase
    {
        protected IMovie movie;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("Movie"))
            {
                movie = this.Manager.Items["Movie"] as IMovie;
            }
        }
    }
}
