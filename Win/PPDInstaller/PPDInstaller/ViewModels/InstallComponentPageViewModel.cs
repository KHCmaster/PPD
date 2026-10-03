using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDInstaller.Models;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace PPDInstaller.ViewModels
{
    /// <summary>
    /// インストールするものを選ぶページです。
    /// </summary>
    public partial class InstallComponentPageViewModel : PageViewModel
    {
        const string ipaFontLicenseUrl = "http://ossipedia.ipa.go.jp/ipafont/index.html";

        readonly InstallFolderPageViewModel folderPage;
        readonly bool fontInstalled;

        [ObservableProperty]
        private bool installPPD = true;

        [ObservableProperty]
        private bool installLAVFilters = true;

        [ObservableProperty]
        private bool installIPAFont;

        [ObservableProperty]
        private bool installBMSTOPPD = true;

        internal InstallComponentPageViewModel(LanguageTexts texts, InstallFolderPageViewModel folderPage) : base(texts)
        {
            this.folderPage = folderPage;
            fontInstalled = IsIpaGothicInstalled();
            Versions = InstallVersions.Read();
        }

        internal InstallVersions Versions { get; }

        internal InstallInfo InstallInfo
        {
            get
            {
                return new InstallInfo
                {
                    PPD = InstallPPD,
                    IPAFont = InstallIPAFont,
                    BMSTOPPD = InstallBMSTOPPD,
                    LAVFilters = InstallLAVFilters
                };
            }
        }

        // インストール済みで同じバージョンのものはチェックを外す
        public override void OnShown()
        {
            var installed = InstallVersions.ReadInstalled(folderPage.InstallDirectory);
            if (installed != null)
            {
                if (installed.PPD != "") InstallPPD = Versions.PPD != installed.PPD;
                if (installed.BMSTOPPD != "") InstallBMSTOPPD = Versions.BMSTOPPD != installed.BMSTOPPD;
            }
            InstallIPAFont = !fontInstalled;
        }

        [RelayCommand]
        private static void OpenIpaFontLicense()
        {
            Process.Start(new ProcessStartInfo(ipaFontLicenseUrl) { UseShellExecute = true });
        }

        private static bool IsIpaGothicInstalled()
        {
            return Fonts.SystemFontFamilies.Any(ff => ff.FamilyNames.Values.Any(name => name == "IPAGothic" || name == "IPAゴシック"));
        }
    }
}
