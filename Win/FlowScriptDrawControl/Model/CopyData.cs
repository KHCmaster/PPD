using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Model
{
    class CopyData
    {
        public FlowAreaControl AreaControl
        {
            get;
            private set;
        }

        public Source[] Sources
        {
            get;
            private set;
        }

        public Connection[] Connections
        {
            get;
            private set;
        }

        public Connection[] ExternalConnections
        {
            get;
            private set;
        }

        public Comment[] Comments
        {
            get;
            private set;
        }

        public CopyData(FlowAreaControl areaControl, Source[] sources, Connection[] connections,
            Connection[] externalConnections, Comment[] comments)
        {
            AreaControl = areaControl;
            Sources = sources;
            Connections = connections;
            ExternalConnections = externalConnections;
            Comments = comments;
        }
    }
}
