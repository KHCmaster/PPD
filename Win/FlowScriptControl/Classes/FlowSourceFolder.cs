namespace FlowScriptControl.Classes
{
    class FlowSourceFolder : IToolTipText
    {
        private string toolTipText = "";
        private string toolTipTextKey = "";

        public FlowSourceFolder(string toolTipTextKey)
        {
            this.toolTipTextKey = toolTipTextKey;
            this.toolTipText = "";
        }

        #region IToolTipText メンバー

        public void UpdateLanguage(ILanguageProvider languageProvider)
        {
            toolTipText = languageProvider.GetFlowSourceToolTipText(toolTipTextKey);
        }

        public string ToolTipText
        {
            get { return toolTipText; }
        }

        #endregion
    }
}
