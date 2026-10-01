using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Model
{
    class CopyData(FlowAreaControl areaControl, Source[] sources, Connection[] connections,
        Connection[] externalConnections, Comment[] comments)
    {
        public FlowAreaControl AreaControl
        {
            get;
            private set;
        } = areaControl;

        public Source[] Sources
        {
            get;
            private set;
        } = sources;

        public Connection[] Connections
        {
            get;
            private set;
        } = connections;

        public Connection[] ExternalConnections
        {
            get;
            private set;
        } = externalConnections;

        public Comment[] Comments
        {
            get;
            private set;
        } = comments;
    }
}
