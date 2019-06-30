using PPDFramework;


namespace PPDSingle
{
    class SongInfoTreeViewItem : TreeViewItem
    {

        public SongInformation SongInformation
        {
            get;
            set;
        }

        public TextureString TextureString
        {
            get;
            set;
        }

        protected override string Text
        {
            get { return TextureString.Text; }
        }

        protected override bool IsFolder
        {
            get { return !SongInformation.IsPPDSong; }
        }
    }
}
