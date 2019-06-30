namespace FlowScriptControl.Classes
{
    public interface ILanguageProvider
    {
        string GetFlowSourceToolTipText(string key);
        string FormatError { get; }
        string AddNode { get; }
        string AddComment { get; }
        string Cut { get; }
        string Copy { get; }
        string Paste { get; }
        string PasteWithLinks { get; }
        string Delete { get; }
        string FitView { get; }
        string AddBoundComment { get; }
        string RemoveBoundComment { get; }
        string AddScope { get; }
        string RemoveScope { get; }
        string SelectInScope { get; }
        string SelectAllInScope { get; }
        string AddBreakPoint { get; }
        string RemoveBreakPoint { get; }
        string FindLinkedNode { get; }
        string CopyNodeName { get; }
        string CopyPropertyName { get; }
    }
}
