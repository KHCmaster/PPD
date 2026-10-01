namespace PPDMultiCommon.Web
{
    public abstract class HostExecutorBase(WebManager webManager) : ExecutorBase
    {
        protected WebManager webManager = webManager;
    }
}
