namespace PPDEditorCommon
{
    public interface IStorage
    {
        string GetValue(string storageKey, string key);
        void SetValue(string storageKey, string key, string value);
    }
}
