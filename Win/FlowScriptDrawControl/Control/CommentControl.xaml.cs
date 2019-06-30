using FlowScriptDrawControl.Model;
using System.Windows;

namespace FlowScriptDrawControl.Control
{
    /// <summary>
    /// CommentControl.xaml の相互作用ロジック
    /// </summary>
    public partial class CommentControl : SelectableControl
    {
        public Comment CurrentComment
        {
            get
            {
                return (Comment)DataContext;
            }
        }

        private TextEditManager textEditManager;

        public CommentControl()
        {
            InitializeComponent();

            Loaded += CommentControl_Loaded;
        }

        void CommentControl_Loaded(object sender, RoutedEventArgs e)
        {
            textEditManager = new TextEditManager(this, textBlock, textBox);
            textEditManager.EnterEdit += textEditManager_EnterEdit;
        }

        bool textEditManager_EnterEdit(object arg)
        {
            return !IsMoved;
        }
    }
}
