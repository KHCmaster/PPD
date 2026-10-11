using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;

namespace PPDInstaller.ViewModels
{
    /// <summary>
    /// インストールするフォルダーを選ぶページです。選んだフォルダーの下の KHC にインストールします。
    /// </summary>
    public partial class InstallFolderPageViewModel : PageViewModel
    {
        readonly IWindowService window;

        [ObservableProperty]
        public partial string InstallFolder { get; set; } = "";

        public InstallFolderPageViewModel(LanguageTexts texts, IWindowService window) : base(texts)
        {
            this.window = window;
            // 既定はシステムのドライブ（C: など）の KHC
            var drive = Environment.SystemDirectory.Length >= 2 ? Environment.SystemDirectory : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (drive.Length >= 2)
            {
                InstallFolder = drive[..2] + "\\KHC";
            }
        }

        /// <summary>
        /// インストールするフォルダーです（最後に区切り文字が付きます）。
        /// </summary>
        public string InstallDirectory
        {
            get { return InstallFolder + Path.DirectorySeparatorChar; }
        }

        [RelayCommand]
        private void Browse()
        {
            var folder = window.SelectFolder(InstallFolder);
            if (folder != null)
            {
                InstallFolder = folder + "\\KHC";
            }
        }
    }
}
