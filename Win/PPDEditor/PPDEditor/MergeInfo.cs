namespace PPDEditor
{
    public class MergeInfo
    {
        public MergeInfo(string projectPath, int version, AvailableDifficulty difficulty)
        {
            ProjectPath = projectPath;
            Difficulty = difficulty;
            ProjectVersion = version;
        }

        public string ProjectPath
        {
            get;
            private set;
        }

        public int ProjectVersion
        {
            get;
            private set;
        }

        public AvailableDifficulty Difficulty
        {
            get;
            private set;
        }
    }
}
