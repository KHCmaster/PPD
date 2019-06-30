namespace FlowScriptControl.Classes
{
    public interface IToolTipText
    {
        void UpdateLanguage(ILanguageProvider languageProvider);
        string ToolTipText { get; }
    }
}
