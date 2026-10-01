namespace FlowScriptControl.Classes
{
    class FlowSourceFolder(string toolTipTextKey) : IToolTipText
    {
        private string toolTipText = "";
        private readonly string toolTipTextKey = toolTipTextKey;

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
