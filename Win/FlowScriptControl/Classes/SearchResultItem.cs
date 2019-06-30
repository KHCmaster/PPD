using FlowScriptDrawControl.Model;
using System.Text;

namespace FlowScriptControl.Classes
{
    public class SearchResultItem
    {
        public SearchResult SearchResult
        {
            get;
            private set;
        }

        public Source Source
        {
            get;
            private set;
        }

        public Comment Comment
        {
            get;
            private set;
        }

        public SearchResultItem(SearchResult searchResult, Source source)
        {
            SearchResult = searchResult;
            Source = source;
        }

        public SearchResultItem(SearchResult searchResult, Comment comment)
        {
            SearchResult = searchResult;
            Comment = comment;
        }

        public override string ToString()
        {
            if (Source != null)
            {
                var sb = new StringBuilder();
                sb.AppendFormat("{0}", Source.Name);
                foreach (Item item in Source.InItems)
                {
                    if (item.Value != null)
                    {
                        sb.AppendFormat(",{0}={1}", item.Name, item.Value);
                    }
                }
                return sb.ToString();
            }
            if (Comment != null)
            {
                return Comment.Text;
            }

            return base.ToString();
        }
    }
}
