namespace FlowScriptDrawControl.Model
{
    public class Comment : ScopeChild
    {
        private string text;

        public string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        public Comment Clone()
        {
            return new Comment
            {
                Text = Text,
                Position = Position
            };
        }
    }
}
