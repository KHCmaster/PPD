using System;

namespace FlowScriptDrawControl.Model
{
    public class SearchResult(Guid guid, Source[] sources, Comment[] comments)
    {
        public Guid Guid
        {
            get;
            private set;
        } = guid;

        public Source[] Sources
        {
            get;
            private set;
        } = sources;

        public Comment[] Comments
        {
            get;
            private set;
        } = comments;
    }
}
