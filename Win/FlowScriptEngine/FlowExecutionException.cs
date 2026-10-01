using System;

namespace FlowScriptEngine
{
    public class FlowExecutionException(Exception e, FlowSourceObjectBase sourceObject) : Exception("Error in flow execution", e)
    {
        public FlowSourceObjectBase SourceObject
        {
            get;
            private set;
        } = sourceObject;
    }
}
