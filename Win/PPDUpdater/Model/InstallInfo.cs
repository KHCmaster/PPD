namespace PPDUpdater.Model
{
    class InstallInfo
    {
        public InstallInfo()
        {
            PPDeditorVersion = VersionInfo.Zero;
            BMSTOPPDVersion = VersionInfo.Zero;
            Effect2DEditorVersion = VersionInfo.Zero;
            InstallVersion = VersionInfo.Zero;

            InstallLang = -1;
        }
        public bool PPD
        {
            get;
            set;
        }

        public bool SharpDX
        {
            get;
            set;
        }

        public bool DirectShowLib
        {
            get;
            set;
        }

        public bool IPAFont
        {
            get;
            set;
        }

        public bool PPDeditor
        {
            get;
            set;
        }

        public bool BMSTOPPD
        {
            get;
            set;
        }

        public bool ffdshow
        {
            get;
            set;
        }

        public bool MP4Splitter
        {
            get;
            set;
        }

        public bool FLVSplitter
        {
            get;
            set;
        }

        public bool Effect2DEditor
        {
            get;
            set;
        }

        public VersionInfo PPDVersion
        {
            get;
            set;
        }

        public VersionInfo PPDeditorVersion
        {
            get;
            set;
        }

        public VersionInfo BMSTOPPDVersion
        {
            get;
            set;
        }

        public VersionInfo Effect2DEditorVersion
        {
            get;
            set;
        }

        public VersionInfo InstallVersion
        {
            get;
            set;
        }

        public int InstallLang
        {
            get;
            set;
        }
    }
}
