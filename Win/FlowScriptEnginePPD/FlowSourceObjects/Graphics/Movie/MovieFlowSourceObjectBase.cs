using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Movie
{
    public abstract class MovieFlowSourceObjectBase : FlowSourceObjectBase
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
