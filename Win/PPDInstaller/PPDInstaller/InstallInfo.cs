namespace PPDInstaller
{
    public class InstallInfo
    {

        public static string[] InstallationInfoData = {
            "PPD",
            "IPAFont",
            "BMSTOPPD",
            "LAVFilters",
            "RegisterStartMenu"
        };

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
            switch (key)
            {
                case "PPD":
                    return PPD;
                case "IPAFont":
                    return IPAFont;
                case "BMSTOPPD":
                    return BMSTOPPD;
                case "LAVFilters":
                    return LAVFilters;
                case "RegisterStartMenu":
                    return RegisterToMenu;
            }
            return false;
        }
    }
}
