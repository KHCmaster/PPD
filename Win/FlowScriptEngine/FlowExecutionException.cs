using System;

namespace FlowScriptEngine
{
    public class FlowExecutionException : Exception
    {
        public FlowSourceObjectBase SourceObject
        {
            get;
            private set;
        }

        public FlowExecutionException(Exception e, FlowSourceObjectBase sourceObject)
            : base("Error in flow execution", e)
        {
            SourceObject = sourceObject;
        }
    }
}
