namespace PPDInstaller
{
    public class InstallInfo
    {

        public static readonly string[] InstallationInfoData = [
            "PPD",
            "IPAFont",
            "BMSTOPPD",
            "LAVFilters",
            "RegisterStartMenu"
        ];

        public bool PPD
        {
            get;
            set;
        }

        public bool IPAFont
        {
            get;
            set;
        }

        public bool BMSTOPPD
        {
            get;
            set;
        }

        public bool LAVFilters
        {
            get;
            set;
        }

        public bool RegisterToMenu
        {
            get;
            set;
        }

        public bool GetInstalled(string key)
        {
            return key switch
            {
                "PPD" => PPD,
                "IPAFont" => IPAFont,
                "BMSTOPPD" => BMSTOPPD,
                "LAVFilters" => LAVFilters,
                "RegisterStartMenu" => RegisterToMenu,
                _ => false,
            };
        }
    }
}
