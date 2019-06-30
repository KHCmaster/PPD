using System;

namespace FlowScriptDrawControl.Model
{
    public class SearchResult
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

        public SearchResult(Guid guid, Source[] sources, Comment[] comments)
        {
            Guid = guid;
            Sources = sources;
            Comments = comments;
        }
    }
}
