namespace FlowScriptControl.Classes
{
    class DefaultLanguageProvider : ILanguageProvider
    {
        private DefaultLanguageProvider()
        {

        }

        static DefaultLanguageProvider()
        {
            Default = new DefaultLanguageProvider();
        }

        public static DefaultLanguageProvider Default
        {
            get;
            private set;
        }

        #region ILanguageProvider メンバ

        public string GetFlowSourceToolTipText(string key)
        {
            return "";
        }

        public string FormatError
        {
            get { return "フォーマットエラーです。以下のフォーマットを参考にしてください。"; }
        }

        public string AddNode
        {
            get { return "ノードを追加"; }
        }

        public string AddComment
        {
            get { return "コメントを追加"; }
        }

        public string Cut
        {
            get { return "切り取り"; }
        }

        public string Copy
        {
            get { return "コピー"; }
        }

        public string Paste
        {
            get { return "ペースト"; }
        }

        public string PasteWithLinks
        {
            get { return "リンクも貼り付け"; }
        }

        public string Delete
        {
            get { return "削除"; }
        }

        public string FitView
        {
            get { return "表示領域を合わせる"; }
        }

        public string AddBoundComment
        {
            get
            {
                return "バインドコメントを追加";
            }
        }

        public string RemoveBoundComment
        {
            get
            {
                return "バインドコメントを削除";
            }
        }

        public string AddScope
        {
            get
            {
                return "スコープを追加";
            }
        }

        public string RemoveScope
        {
            get
            {
                return "スコープを削除";
            }
        }

        public string SelectInScope
        {
            get
            {
                return "スコープ内の要素を選択";
            }
        }

        public string SelectAllInScope
        {
            get
            {
                return "スコープ内の全要素を選択";
            }
        }

        public string AddBreakPoint
        {
            get
            {
                return "ブレイクポイントを設定";
            }
        }

        public string RemoveBreakPoint
        {
            get
            {
                return "ブレイクポイントを解除";
            }
        }

        public string FindLinkedNode
        {
            get
            {
                return "リンクしているノードを検索";
            }
        }

        public string CopyNodeName
        {
            get
            {
                return "ノード名をコピー";
            }
        }

        public string CopyPropertyName
        {
            get
            {
                return "プロパティ名をコピー";
            }
        }

        #endregion
    }
}
