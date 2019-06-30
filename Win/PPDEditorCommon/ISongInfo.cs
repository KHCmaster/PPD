namespace PPDEditorCommon
{
    public interface ISongInfo
    {
        float BPM { get; }
        float BPMOffset { get; }
        string CurrentProjectName { get; }
        string CurrentProjectFilePath { get; }
        string CurrentProjectDir { get; }
    }
}
