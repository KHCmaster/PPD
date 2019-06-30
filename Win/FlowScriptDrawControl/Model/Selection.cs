using System;

namespace FlowScriptDrawControl.Model
{
    class Selection
    {
        public Guid Guid
        {
            get;
            private set;
        }

        public Source[] Sources
        {
            get;
            private set;
        }

        public Comment[] Comments
        {
            get;
            private set;
        }

        public ConnectionInfo[] Connections
        {
            get;
            private set;
        }

        public ConnectionInfo[] ExternalConnections
        {
            get;
            private set;
        }

        public Scope[] Scopes
        {
            get;
            private set;
        }

        public Selection(Guid guid, Source[] sources, Comment[] comments)
        {
            Guid = guid;
            Sources = sources;
            Comments = comments;
        }

        public Selection(Guid guid, Source[] sources, Comment[] comments,
            ConnectionInfo[] connections, ConnectionInfo[] externalConnections, Scope[] scopes)
        {
            Guid = guid;
            Sources = sources;
            Comments = comments;
            Connections = connections;
            ExternalConnections = externalConnections;
            Scopes = scopes;
        }
    }
}
