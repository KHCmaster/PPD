namespace PPDEditor
{
    public class MergeInfo(string projectPath, int version, AvailableDifficulty difficulty)
    {
        public string ProjectPath
        {
            get;
            private set;
        } = projectPath;

        public int ProjectVersion
        {
            get;
            private set;
        } = version;

        public AvailableDifficulty Difficulty
        {
            get;
            private set;
        } = difficulty;
    }
}
