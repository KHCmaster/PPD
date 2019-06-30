namespace PPDMultiCommon.Web
{
    public abstract class HostExecutorBase : ExecutorBase
    {
        protected WebManager webManager;
        protected HostExecutorBase(WebManager webManager)
        {
            this.webManager = webManager;
        }
    }
}
